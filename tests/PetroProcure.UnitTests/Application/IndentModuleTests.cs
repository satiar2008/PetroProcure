using PetroProcure.Application.Indents;
using PetroProcure.Application.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.UnitTests.Application;

public sealed class IndentModuleTests
{
    [Theory]
    [InlineData(3, "2630001", IndentType.Manual)]
    [InlineData(5, "2650001", IndentType.SystemGenerated)]
    [InlineData(0, "2600001", IndentType.DirectPurchase)]
    public async Task GeneratesValidIndentNumber(int typeDigit, string expected, IndentType expectedType)
    {
        var service = new IndentNumberService(new FakeIndentRepository());

        var number = await service.GenerateNextIndentNumber(26, typeDigit);
        var parts = service.ParseIndentNumber(number);

        Assert.Equal(expected, number);
        Assert.Equal(expectedType, parts.IndentType);
    }

    [Fact]
    public void RejectsInvalidTypeDigit()
    {
        var service = new IndentNumberService(new FakeIndentRepository());

        Assert.Throws<ArgumentOutOfRangeException>(() => service.ResolveIndentType(10));
    }

    [Fact]
    public async Task RejectsDuplicateSequence()
    {
        var repository = new FakeIndentRepository { ExistingNumber = "2630001" };
        var handler = new IndentCommandHandler(repository, new IndentNumberService(repository),
            new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

        await Assert.ThrowsAsync<IndentConflictException>(() => handler.Handle(new CreateIndentCommand(
            26, 3, "Duplicate", Guid.NewGuid(), null, null)));
    }

    [Fact]
    public async Task ItemsAreGroupedByMescGeneralGroup()
    {
        var repository = new FakeIndentRepository();
        var indent = CreateIndent();
        indent.AddItem(CreateItem(indent.Id, "1234560002", "123456", "Pipes", "Elbow"));
        indent.AddItem(CreateItem(indent.Id, "1234560001", "123456", "Pipes", "Pipe"));
        repository.Indent = indent;

        var result = await new IndentQueryHandler(repository, new IndentNumberService(repository))
            .Handle(new GetIndentItemsGroupedByMescGeneralGroupQuery(indent.Id));

        var group = Assert.Single(result);
        Assert.Equal("123456", group.MescGeneralGroupCode);
        Assert.Equal("Pipes", group.GeneralDescription);
        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public async Task SnapshotDescriptionsAreSavedCorrectly()
    {
        var repository = new FakeIndentRepository();
        var indent = CreateIndent();
        repository.Indent = indent;
        repository.Snapshot = new(
            Guid.NewGuid(), "1234560001", "123456", "Original general",
            "Original specific", repository.UnitId, true);
        var handler = new IndentCommandHandler(repository, new IndentNumberService(repository),
            new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

        var item = await handler.Handle(new AddIndentItemCommand(
            indent.Id, repository.Snapshot.Id, repository.UnitId, 12, "Technical", null));

        repository.Snapshot = repository.Snapshot with
        {
            GeneralDescription = "Changed general",
            SpecificDescription = "Changed specific"
        };
        Assert.Equal("Original general", item.GeneralDescription);
        Assert.Equal("Original specific", item.SpecificDescription);
        Assert.Equal("Original general", indent.Items.Single().GeneralDescription);
    }

    [Fact]
    public async Task SendingIndentToPurchaseCreatesWorkflowAndInboxTask()
    {
        var repository = new FakeIndentRepository();
        var workflow = new FakeWorkflowRepository();
        var indent = CreateIndent();
        indent.AddItem(CreateItem(indent.Id, "1234560001", "123456", "Pipes", "Pipe"));
        indent.Submit();
        indent.Approve();
        repository.Indent = indent;
        var handler = new IndentCommandHandler(repository, new IndentNumberService(repository),
            new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true), workflow);

        await handler.Handle(new SendIndentToPurchaseDepartmentCommand(indent.Id));

        Assert.Equal(IndentStatus.SentToPurchaseDepartment, indent.Status);
        Assert.NotNull(workflow.Workflow);
        Assert.Equal("Indent", workflow.Workflow!.EntityType);
        Assert.Equal(indent.Id, workflow.Workflow.EntityId);
        var task = Assert.Single(workflow.Tasks);
        Assert.Equal(indent.Id, task.IndentId);
        Assert.Equal(repository.PurchaseDepartmentId, task.AssignedDepartmentId);
    }

    private static Indent CreateIndent() => new(
        Guid.NewGuid(), "2630001", 26, 3, 1, "Test indent",
        Guid.NewGuid(), null, Guid.NewGuid());

    private static IndentItem CreateItem(
        Guid indentId, string code, string groupCode, string general, string specific) => new(
        Guid.NewGuid(), indentId, Guid.NewGuid(), code, groupCode, general, specific,
        Guid.NewGuid(), 1);

    private sealed class FakeIndentRepository : IIndentRepository
    {
        private readonly Dictionary<(int Year, int Type), int> _sequences = [];
        public Indent? Indent { get; set; }
        public string? ExistingNumber { get; set; }
        public Guid UnitId { get; } = Guid.NewGuid();
        public Guid PurchaseDepartmentId { get; } = Guid.NewGuid();
        public MescItemSnapshot? Snapshot { get; set; }

        public Task<string> GenerateNextIndentNumberAsync(int yearPart, int typeDigit, CancellationToken cancellationToken)
        {
            var key = (yearPart, typeDigit);
            var next = _sequences.GetValueOrDefault(key) + 1;
            _sequences[key] = next;
            return Task.FromResult(PetroProcure.Domain.Modules.Indents.Indent.BuildIndentNumber(yearPart, typeDigit, next));
        }

        public Task<bool> IndentNumberExistsAsync(string indentNumber, CancellationToken cancellationToken) =>
            Task.FromResult(indentNumber == ExistingNumber);
        public Task<Indent?> FindAsync(Guid id, bool includeItems, CancellationToken cancellationToken) =>
            Task.FromResult(Indent?.Id == id ? Indent : null);
        public Task<Indent?> FindByNumberAsync(string indentNumber, CancellationToken cancellationToken) =>
            Task.FromResult(Indent?.IndentNumber == indentNumber ? Indent : null);
        public Task<MescItemSnapshot?> GetMescItemSnapshotAsync(Guid mescItemId, CancellationToken cancellationToken) =>
            Task.FromResult(Snapshot?.Id == mescItemId ? Snapshot : null);
        public Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(id == UnitId);
        public Task<Guid?> GetDepartmentIdByTypeAsync(DepartmentType type, CancellationToken cancellationToken) =>
            Task.FromResult<Guid?>(type == DepartmentType.PurchaseDepartment ? PurchaseDepartmentId : null);
        public Task AddAsync(Indent indent, CancellationToken cancellationToken)
        {
            Indent = indent;
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<IndentListDto>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<IndentListDto>>([]);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeWorkflowRepository : IWorkflowRepository
    {
        public WorkflowInstance? Workflow { get; private set; }
        public List<InboxTask> Tasks { get; } = [];

        public Task<WorkflowInstance?> FindWorkflowAsync(Guid id, bool includeSteps, CancellationToken ct) =>
            Task.FromResult(Workflow?.Id == id ? Workflow : null);
        public Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid purchaseFileId, CancellationToken ct) =>
            Task.FromResult<WorkflowInstance?>(null);
        public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<PurchaseFile?>(null);
        public Task<InboxTask?> FindTaskAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(Tasks.SingleOrDefault(task => task.Id == id));
        public Task<bool> UserHasDepartmentAccessAsync(Guid userId, Guid departmentId, CancellationToken ct) =>
            Task.FromResult(true);
        public Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct)
        {
            Workflow = workflow;
            return Task.CompletedTask;
        }
        public Task AddTaskAsync(InboxTask task, CancellationToken ct)
        {
            Tasks.Add(task);
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<InboxTaskDto>> GetMyTasksAsync(Guid userId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<InboxTaskDto>>([]);
        public Task<IReadOnlyList<InboxTaskDto>> GetDepartmentTasksAsync(Guid departmentId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<InboxTaskDto>>([]);
        public Task<InboxTaskDto?> GetTaskDtoAsync(Guid taskId, CancellationToken ct) =>
            Task.FromResult<InboxTaskDto?>(null);
        public Task<IReadOnlyList<WorkflowStepDto>> GetWorkflowTimelineAsync(Guid workflowInstanceId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<WorkflowStepDto>>([]);
        public Task<IReadOnlyList<WorkflowStepDto>> GetTimelineAsync(Guid purchaseFileId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<WorkflowStepDto>>([]);
        public Task<IReadOnlyList<InboxTaskDto>> GetOpenPurchaseFileTasksAsync(Guid purchaseFileId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<InboxTaskDto>>([]);
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
