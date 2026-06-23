using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseFiles;

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

    private static PurchaseFileCommandHandler Handler(FakeRepository repository) =>
        new(repository, new PurchaseFileNumberService(repository), new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

    private static PurchaseFile CreateFile() => new(
        Guid.NewGuid(), "PF-2026-000001", "Test file", null, PurchaseFilePriority.Normal,
        null, Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid());

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
        public PurchaseFile? File { get; set; }
        public Indent? Indent { get; set; }
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
        public Task<Indent?> FindApprovedIndentAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Indent?.Id == id && Indent.Status == IndentStatus.Approved ? Indent : null);
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
    }
}
