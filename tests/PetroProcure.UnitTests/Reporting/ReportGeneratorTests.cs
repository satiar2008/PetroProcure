using PetroProcure.Application.Documents;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Storage;
using PetroProcure.Reporting;

namespace PetroProcure.UnitTests.Reporting;

public sealed class ReportGeneratorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "PetroProcureReportTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ReportGeneratorCreatesPdfBytes()
    {
        var generator = new ReportGenerator(new FakeDataProvider(), new CapturingStorage(),
            new TestCurrentUser(Guid.NewGuid()));

        var bytes = await generator.GeneratePdfAsync(ReportNames.PurchaseFileSummary,
            new Dictionary<string, object?> { ["PurchaseFileId"] = FakeDataProvider.PurchaseFileId });

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task GeneratedReportCanBeSavedIntoRootFolderRepository()
    {
        var purchaseFile = new PurchaseFile(
            FakeDataProvider.PurchaseFileId, "PF-2026-000001", "Test", null, PurchaseFilePriority.Normal,
            null, Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid());
        var repository = new FakeFileRepository { PurchaseFile = purchaseFile };
        var storage = new FileStorageService(repository,
            Microsoft.Extensions.Options.Options.Create(new FileStorageOptions
            {
                RootPath = _root, QuarantinePath = Path.Combine(_root, ".quarantine"),
                AllowedExtensions = [".pdf"], AllowedMimeTypes = ["application/pdf"]
            }), new NoOpFileScanner());
        var generator = new ReportGenerator(new FakeDataProvider(), storage,
            new TestCurrentUser(Guid.NewGuid()));

        var document = await generator.SaveGeneratedReportToPurchaseFileAsync(
            purchaseFile.Id, ReportNames.PurchaseFileSummary,
            new Dictionary<string, object?> { ["PurchaseFileId"] = purchaseFile.Id });

        Assert.False(Path.IsPathRooted(document.RelativePath));
        Assert.Contains("/10-Final/", document.RelativePath);
        Assert.True(File.Exists(Path.Combine(_root, document.RelativePath.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task PurchaseFileReportDataContainsGroupedMescItems()
    {
        var data = await new FakeDataProvider().GetPurchaseFileAsync(FakeDataProvider.PurchaseFileId, default);

        var group = Assert.Single(data!.Groups);
        Assert.Equal("123456", group.GeneralGroupCode);
        Assert.Equal(2, group.Items.Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private sealed class FakeDataProvider : IReportDataProvider
    {
        public static readonly Guid PurchaseFileId = Guid.NewGuid();
        public Task<PurchaseFileReportData?> GetPurchaseFileAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<PurchaseFileReportData?>(new(
                PurchaseFileId, "PF-2026-000001", "خرید لوله", "در واحد خرید", "واحد خرید",
                DateTime.UtcNow, "2630001",
                [new("123456", "لوله و اتصالات",
                    [new("1234560001","123456","لوله و اتصالات","لوله فولادی","متر",10),
                     new("1234560002","123456","لوله و اتصالات","زانو فولادی","عدد",4)])]));
        public Task<IndentReportData?> GetIndentAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<IndentReportData?>(new(id, "2630001", "دستی", "متقاضی", []));
    }

    private sealed class CapturingStorage : IFileStorageService
    {
        public Task EnsurePurchaseFileFoldersAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<FileDocumentDto> SaveFileAsync(Guid purchaseFileId, DocumentType documentType, string originalFileName, Stream stream, Guid uploadedByUserId, Guid? departmentId = null, string? mimeType = null, string? description = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new FileDocumentDto(Guid.NewGuid(),purchaseFileId,null,documentType,originalFileName,"stored.pdf","PurchaseFiles/test.pdf",".pdf","application/pdf",stream.Length,"hash",1,uploadedByUserId,DateTime.UtcNow,false,description));
        public Task<StoredFileContent> OpenFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<FileDocumentDto> CreateNewVersionAsync(Guid fileDocumentId, Stream stream, Guid createdByUserId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public string GetRelativePath(PurchaseFile purchaseFile, DocumentType documentType, string storedFileName) => "";
        public Task<IReadOnlyList<FileDocumentDto>> GetPurchaseFileDocumentsAsync(Guid purchaseFileId, bool includeDeleted = false, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FileDocumentDto>>([]);
    }

    private sealed class FakeFileRepository : IFileDocumentRepository
    {
        public PurchaseFile? PurchaseFile { get; set; }
        public FileDocument? Document { get; private set; }
        public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(PurchaseFile);
        public Task<FileDocument?> FindDocumentAsync(Guid id, bool includeVersions, CancellationToken cancellationToken) => Task.FromResult(Document);
        public Task AddDocumentAsync(FileDocument document, CancellationToken cancellationToken) { Document=document; return Task.CompletedTask; }
        public Task<IReadOnlyList<FileDocumentDto>> GetDocumentsAsync(Guid purchaseFileId, bool includeDeleted, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<FileDocumentDto>>([]);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
