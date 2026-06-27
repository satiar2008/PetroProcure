using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Contracts.V1.Common;
using PurchaseFileListRequest = PetroProcure.Contracts.V1.PurchaseFiles.PurchaseFileListRequest;

namespace PetroProcure.UnitTests.Application;

public sealed class PurchaseFileModuleTests
{
    [Fact]
    public async Task CreatesPurchaseFileManually()
    {
        var repository = new FakeRepository();
        var departmentId = Guid.NewGuid();
        var result = await Handler(repository).Handle(new CreatePurchaseFileCommand(
            2026, "Manual file", "Description", PurchaseFilePriority.High,
            departmentId, departmentId, null));

        Assert.Equal("PF-2026-000001", result.FileNumber);
        Assert.Equal(PurchaseFileStatus.Draft, result.Status);
        Assert.Null(result.SourceIndentId);
    }

    [Fact]
    public async Task CreatesPurchaseFileFromApprovedIndentAndCopiesSnapshots()
    {
        var repository = new FakeRepository { Indent = CreateApprovedIndent() };

        var result = await Handler(repository).Handle(new CreatePurchaseFileFromIndentCommand(
            repository.Indent.Id, 2026, Guid.NewGuid(), null));

        var item = Assert.Single(result.Items);
        Assert.Equal(repository.Indent.Items.Single().Id, item.SourceIndentItemId);
        Assert.Equal("Original general", item.GeneralDescription);
        Assert.Equal("Original specific", item.SpecificDescription);
    }

    [Fact]
    public async Task CreatesPurchaseFileFromIndentSentToPurchaseDepartment()
    {
        var indent = CreateApprovedIndent();
        indent.SendToPurchaseDepartment();
        var repository = new FakeRepository { Indent = indent };

        var result = await Handler(repository).Handle(new CreatePurchaseFileFromIndentCommand(
            indent.Id, 2026, Guid.NewGuid(), null));

        Assert.Equal(indent.Id, result.SourceIndentId);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task CreatingPurchaseFileFromSameIndentReturnsExistingFile()
    {
        var indent = CreateApprovedIndent();
        var existing = CreateFile();
        typeof(PurchaseFile).GetProperty(nameof(PurchaseFile.SourceIndentId))!
            .SetValue(existing, indent.Id);
        var repository = new FakeRepository { Indent = indent, File = existing };

        var result = await Handler(repository).Handle(new CreatePurchaseFileFromIndentCommand(
            indent.Id, 2026, Guid.NewGuid(), null));

        Assert.Equal(existing.Id, result.Id);
        Assert.Equal(existing.FileNumber, result.FileNumber);
    }


    [Fact]
    public async Task FileNumberIsGeneratedCorrectly()
    {
        var service = new PurchaseFileNumberService(new FakeRepository());

        Assert.Equal("PF-2026-000001", await service.GenerateNextFileNumber(2026));
    }

    [Fact]
    public async Task ItemsAreGroupedByMescGeneralGroup()
    {
        var repository = new FakeRepository();
        var file = CreateFile();
        file.AddItem(CreateItem(file.Id, "1234560002", "123456", "Pipes", "Elbow"));
        file.AddItem(CreateItem(file.Id, "1234560001", "123456", "Pipes", "Pipe"));
        repository.File = file;

        var result = await new PurchaseFileQueryHandler(repository)
            .Handle(new GetPurchaseFileItemsGroupedByMescGeneralGroupQuery(file.Id));

        var group = Assert.Single(result);
        Assert.Equal("Pipes", group.GeneralDescription);
        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public void StatusChangeCreatesHistory()
    {
        var file = CreateFile();
        var userId = Guid.NewGuid();

        file.ChangeStatus(PurchaseFileStatus.WaitingForPurchaseDepartment, userId, "Ready");

        var history = Assert.Single(file.StatusHistory);
        Assert.Equal(PurchaseFileStatus.Draft, history.FromStatus);
        Assert.Equal(PurchaseFileStatus.WaitingForPurchaseDepartment, history.ToStatus);
        Assert.Equal(userId, history.ChangedByUserId);
    }

    [Fact]
    public void ArchivedFileCannotBeEdited()
    {
        var file = CreateFile();
        var userId = Guid.NewGuid();
        file.Complete(userId);
        file.Archive(userId);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            file.AddItem(CreateItem(file.Id, "1234560001", "123456", "Pipes", "Pipe"), true));

        Assert.Contains("read-only", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RequestTechnicalReviewCreatesApplicantInboxTask()
    {
        var purchaseDepartmentId = Guid.NewGuid();
        var applicantDepartmentId = Guid.NewGuid();
        var repository = new FakeRepository
        {
            File = CreateFile(purchaseDepartmentId),
            ApplicantDepartmentId = applicantDepartmentId
        };
        var userId = Guid.NewGuid();
        var handler = new PurchaseFileTechnicalReviewHandler(repository,
            new TestCurrentUser(userId, [purchaseDepartmentId], isSystemAdmin: true));

        var result = await handler.Handle(new RequestPurchaseFileTechnicalReviewCommand(
            repository.File.Id, null, "لطفاً مشخصات فنی کنترل شود.", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))));

        Assert.Equal(applicantDepartmentId, result.DepartmentId);
        Assert.Equal(PurchaseFileTechnicalReviewStatus.Requested, result.Status);
        var task = Assert.Single(repository.InboxTasks);
        Assert.Equal(applicantDepartmentId, task.AssignedDepartmentId);
        Assert.Equal(repository.File.Id, task.PurchaseFileId);
        Assert.Contains(repository.File.FileNumber, task.Title, StringComparison.Ordinal);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task SubmitTechnicalReviewReturnsInboxTaskToPurchaseDepartment()
    {
        var purchaseDepartmentId = Guid.NewGuid();
        var applicantDepartmentId = Guid.NewGuid();
        var repository = new FakeRepository
        {
            File = CreateFile(purchaseDepartmentId),
            ApplicantDepartmentId = applicantDepartmentId
        };
        var requester = new PurchaseFileTechnicalReviewHandler(repository,
            new TestCurrentUser(Guid.NewGuid(), [purchaseDepartmentId], isSystemAdmin: true));
        var requested = await requester.Handle(new RequestPurchaseFileTechnicalReviewCommand(
            repository.File.Id, applicantDepartmentId, "بررسی فنی", null));
        var applicantUserId = Guid.NewGuid();
        var reviewer = new PurchaseFileTechnicalReviewHandler(repository,
            new TestCurrentUser(applicantUserId, [applicantDepartmentId]));

        var result = await reviewer.Handle(new SubmitPurchaseFileTechnicalReviewCommand(
            requested.Id, PurchaseFileTechnicalReviewDecision.TechnicallyAccepted, "از نظر فنی قابل قبول است.", null));

        Assert.Equal(PurchaseFileTechnicalReviewStatus.Approved, result.Status);
        Assert.Equal(PurchaseFileTechnicalReviewDecision.TechnicallyAccepted, result.Decision);
        Assert.Contains(repository.InboxTasks, x => x.AssignedDepartmentId == applicantDepartmentId && x.Status == WorkflowStatus.Completed);
        var returnTask = Assert.Single(repository.InboxTasks, x => x.AssignedDepartmentId == purchaseDepartmentId);
        Assert.Equal(WorkflowStatus.Pending, returnTask.Status);
        Assert.Contains("نتیجه بررسی فنی", returnTask.Title, StringComparison.Ordinal);
    }

    private static PurchaseFileCommandHandler Handler(FakeRepository repository) =>
        new(repository, new PurchaseFileNumberService(repository), new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

    private static PurchaseFile CreateFile() => CreateFile(Guid.NewGuid());

    private static PurchaseFile CreateFile(Guid purchaseDepartmentId) => new(
        Guid.NewGuid(), "PF-2026-000001", "Test file", null, PurchaseFilePriority.Normal,
        null, purchaseDepartmentId, purchaseDepartmentId, null, Guid.NewGuid());

    private static PurchaseFileItem CreateItem(
        Guid fileId, string code, string groupCode, string general, string specific) => new(
        Guid.NewGuid(), fileId, Guid.NewGuid(), code, groupCode, general, specific,
        Guid.NewGuid(), 5, 5, null, null);

    private static Indent CreateApprovedIndent()
    {
        var indent = new Indent(
            Guid.NewGuid(), "2630001", 26, 3, 1, "Approved indent",
            Guid.NewGuid(), null, Guid.NewGuid(), "Indent description");
        indent.AddItem(new IndentItem(
            Guid.NewGuid(), indent.Id, Guid.NewGuid(), "1234560001", "123456",
            "Original general", "Original specific", Guid.NewGuid(), 10, "Technical"));
        indent.Submit();
        indent.Approve();
        return indent;
    }

    private sealed class FakeRepository : IPurchaseFileRepository
    {
        private readonly Dictionary<int, int> _sequences = [];
        private readonly List<PurchaseFileTechnicalReview> _technicalReviews = [];
        private WorkflowInstance? _workflow;
        public PurchaseFile? File { get; set; }
        public Indent? Indent { get; set; }
        public Guid? ApplicantDepartmentId { get; set; }
        public List<InboxTask> InboxTasks { get; } = [];
        public Task<string> GenerateNextFileNumberAsync(int year, CancellationToken cancellationToken)
        {
            var next = _sequences.GetValueOrDefault(year) + 1;
            _sequences[year] = next;
            return Task.FromResult($"PF-{year:0000}-{next:000000}");
        }
        public Task<bool> FileNumberExistsAsync(string fileNumber, CancellationToken cancellationToken) =>
            Task.FromResult(File?.FileNumber == fileNumber);
        public Task<bool> SourceIndentAlreadyUsedAsync(Guid indentId, CancellationToken cancellationToken) =>
            Task.FromResult(File?.SourceIndentId == indentId);
        public Task<PurchaseFile?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken) =>
            Task.FromResult(File?.Id == id ? File : null);
        public Task<PurchaseFile?> FindByNumberAsync(string fileNumber, CancellationToken cancellationToken) =>
            Task.FromResult(File?.FileNumber == fileNumber ? File : null);
        public Task<PurchaseFile?> FindBySourceIndentAsync(Guid indentId, bool includeDetails, CancellationToken cancellationToken) =>
            Task.FromResult(File?.SourceIndentId == indentId ? File : null);
        public Task<Indent?> FindApprovedIndentAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Indent?.Id == id
                && (Indent.Status is IndentStatus.Approved or IndentStatus.SentToPurchaseDepartment)
                    ? Indent
                    : null);
        public Task<string?> GetDepartmentNameAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(id == ApplicantDepartmentId ? "واحد متقاضی" : "واحد خرید");
        public Task<Guid?> GetDepartmentIdByTypeAsync(DepartmentType type, CancellationToken cancellationToken) =>
            Task.FromResult(type == DepartmentType.Applicant ? ApplicantDepartmentId : null);
        public Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid purchaseFileId, CancellationToken cancellationToken) =>
            Task.FromResult(_workflow?.EntityId == purchaseFileId ? _workflow : null);
        public Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken cancellationToken)
        {
            _workflow = workflow;
            return Task.CompletedTask;
        }
        public Task AddInboxTaskAsync(InboxTask task, CancellationToken cancellationToken)
        {
            InboxTasks.Add(task);
            return Task.CompletedTask;
        }
        public Task<InboxTask?> FindInboxTaskAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(InboxTasks.FirstOrDefault(x => x.Id == id));
        public Task<bool> HasActiveTechnicalReviewAsync(Guid purchaseFileId, Guid departmentId, CancellationToken cancellationToken) =>
            Task.FromResult(_technicalReviews.Any(x => x.PurchaseFileId == purchaseFileId
                && x.DepartmentId == departmentId
                && x.Status is not PurchaseFileTechnicalReviewStatus.Approved
                    and not PurchaseFileTechnicalReviewStatus.Rejected
                    and not PurchaseFileTechnicalReviewStatus.Cancelled));
        public Task AddTechnicalReviewAsync(PurchaseFileTechnicalReview review, CancellationToken cancellationToken)
        {
            _technicalReviews.Add(review);
            return Task.CompletedTask;
        }
        public Task<PurchaseFileTechnicalReview?> FindTechnicalReviewAsync(Guid id, bool includeFile, CancellationToken cancellationToken) =>
            Task.FromResult(_technicalReviews.FirstOrDefault(x => x.Id == id));
        public Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> GetTechnicalReviewsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PurchaseFileTechnicalReviewDto>>(_technicalReviews
                .Where(x => x.PurchaseFileId == purchaseFileId)
                .Select(ToDto)
                .ToArray());
        public Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> GetTechnicalReviewsByDepartmentsAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PurchaseFileTechnicalReviewDto>>(_technicalReviews
                .Where(x => departmentIds.Count == 0 || departmentIds.Contains(x.DepartmentId))
                .Select(ToDto)
                .ToArray());
        public Task<PurchaseFileTechnicalReviewDto?> GetTechnicalReviewDtoAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_technicalReviews.FirstOrDefault(x => x.Id == id) is { } review ? ToDto(review) : null);
        public Task<ApplicantDashboardDto> GetApplicantDashboardAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken) =>
            Task.FromResult(new ApplicantDashboardDto(0, 0, 0, 0, null, []));
        public Task<DepartmentDashboardDto> GetDepartmentDashboardAsync(string departmentKey, IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken) =>
            Task.FromResult(new DepartmentDashboardDto(departmentKey, "داشبورد", 0, 0, 0, new Dictionary<string, int>(), []));
        public Task<PurchaseFileMescSnapshot?> GetMescSnapshotAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<PurchaseFileMescSnapshot?>(null);
        public Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task AddAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken)
        {
            File = purchaseFile;
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<PurchaseFileListDto>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<PurchaseFileListDto>>([]);
        public Task<PagedResult<PurchaseFileListDto>> GetPagedAsync(PurchaseFileListRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(new PagedResult<PurchaseFileListDto>([], request.PageNumber, request.PageSize, 0));
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private PurchaseFileTechnicalReviewDto ToDto(PurchaseFileTechnicalReview review) =>
            new(
                review.Id,
                review.PurchaseFileId,
                File?.FileNumber ?? "PF-2026-000001",
                File?.Title ?? "Test file",
                review.DepartmentId,
                review.DepartmentId == ApplicantDepartmentId ? "واحد متقاضی" : "واحد خرید",
                review.RequestedByUserId,
                review.ReviewedByUserId,
                review.Status,
                review.Decision,
                review.RequestComment,
                review.Comments,
                review.RecommendationNotes,
                review.RequestedAt,
                review.StartedAt,
                review.CompletedAt,
                File?.Items.Select(item => new PurchaseFileItemDto(
                    item.Id,
                    item.MescItemId,
                    item.MescCode,
                    item.MescGeneralGroupCode,
                    item.GeneralDescription,
                    item.SpecificDescription,
                    item.UnitOfMeasureId,
                    item.RequestedQuantity,
                    item.ApprovedQuantity,
                    item.TechnicalDescription,
                    item.SourceIndentItemId)).ToArray() ?? []);
    }
}
