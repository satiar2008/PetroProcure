using System.Text;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Documents;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Infrastructure.Storage;

namespace PetroProcure.UnitTests.Infrastructure;

public sealed class FileStorageServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "PetroProcureTests", Guid.NewGuid().ToString("N"));
    private readonly FakeRepository _repository = new();

    [Fact]
    public async Task RootFolderPathIsReadFromConfiguration()
    {
        var file = CreatePurchaseFile();
        _repository.PurchaseFile = file;
        var service = CreateService();

        await service.EnsurePurchaseFileFoldersAsync(file);

        Assert.True(Directory.Exists(Path.Combine(_root, "PurchaseFiles", "2026", file.FileNumber)));
    }

    [Fact]
    public async Task PurchaseFileFoldersAreCreated()
    {
        var file = CreatePurchaseFile();
        _repository.PurchaseFile = file;
        var service = CreateService();

        await service.EnsurePurchaseFileFoldersAsync(file);

        foreach (var folder in FileStorageService.StandardFolders)
            Assert.True(Directory.Exists(Path.Combine(_root, "PurchaseFiles", "2026", file.FileNumber, folder)));
    }

    [Fact]
    public async Task FileIsSavedWithRelativePathAndHash()
    {
        var file = CreatePurchaseFile();
        _repository.PurchaseFile = file;
        var service = CreateService();
        var bytes = Encoding.UTF8.GetBytes("PetroProcure document");

        var document = await service.SaveFileAsync(
            file.Id, DocumentType.TechnicalSpecification, "specification.txt",
            new MemoryStream(bytes), Guid.NewGuid());

        Assert.False(Path.IsPathRooted(document.RelativePath));
        Assert.StartsWith("PurchaseFiles/2026/PF-2026-000001/02-Technical/", document.RelativePath);
        Assert.Equal("3912bd04e65862ba9498979e16868021d42956898115bf5bc55246e4cdd4f7c1", document.Hash);
        Assert.True(File.Exists(ToPhysical(document.RelativePath)));
    }

    [Fact]
    public async Task SoftDeleteDoesNotRemovePhysicalFile()
    {
        var file = CreatePurchaseFile();
        _repository.PurchaseFile = file;
        var service = CreateService();
        var document = await service.SaveFileAsync(
            file.Id, DocumentType.Indent, "indent.pdf",
            new MemoryStream([1, 2, 3]), Guid.NewGuid());
        var physicalPath = ToPhysical(document.RelativePath);

        await service.DeleteFileAsync(document.Id);

        Assert.True(_repository.Document!.IsDeleted);
        Assert.True(File.Exists(physicalPath));
    }

    [Fact]
    public async Task NewVersionIncrementsVersionNumber()
    {
        var file = CreatePurchaseFile();
        _repository.PurchaseFile = file;
        var service = CreateService();
        var original = await service.SaveFileAsync(
            file.Id, DocumentType.Contract, "contract.pdf",
            new MemoryStream([1, 2, 3]), Guid.NewGuid());

        var version = await service.CreateNewVersionAsync(
            original.Id, new MemoryStream([4, 5, 6]), Guid.NewGuid());

        Assert.Equal(2, version.VersionNo);
        Assert.Contains("-v2.pdf", version.StoredFileName);
        Assert.Single(_repository.Document!.Versions);
    }

    [Fact]
    public async Task RejectsDisallowedExtension()
    {
        _repository.PurchaseFile = CreatePurchaseFile();
        await Assert.ThrowsAsync<FileStorageValidationException>(() => CreateService().SaveFileAsync(
            _repository.PurchaseFile.Id, DocumentType.TechnicalSpecification, "script.exe",
            new MemoryStream([1]), Guid.NewGuid(), mimeType: "application/octet-stream"));
    }

    [Fact]
    public async Task RejectsOversizedFile()
    {
        _repository.PurchaseFile = CreatePurchaseFile();
        await Assert.ThrowsAsync<FileStorageValidationException>(() => CreateService().SaveFileAsync(
            _repository.PurchaseFile.Id, DocumentType.TechnicalSpecification, "large.pdf",
            new MemoryStream(new byte[1024 * 1024 + 1]), Guid.NewGuid(), mimeType: "application/pdf"));
    }

    [Fact]
    public async Task RejectsPathTraversalFileName()
    {
        _repository.PurchaseFile = CreatePurchaseFile();
        await Assert.ThrowsAsync<FileStorageValidationException>(() => CreateService().SaveFileAsync(
            _repository.PurchaseFile.Id, DocumentType.TechnicalSpecification, "../secret.pdf",
            new MemoryStream([1]), Guid.NewGuid(), mimeType: "application/pdf"));
    }

    [Fact]
    public async Task PhysicalFileIsRemovedWhenDatabaseSaveFails()
    {
        _repository.PurchaseFile = CreatePurchaseFile();
        _repository.FailSave = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService().SaveFileAsync(
            _repository.PurchaseFile.Id, DocumentType.TechnicalSpecification, "failure.pdf",
            new MemoryStream([1, 2, 3]), Guid.NewGuid(), mimeType: "application/pdf"));
        var files = Directory.Exists(_root)
            ? Directory.GetFiles(_root, "*.pdf", SearchOption.AllDirectories)
            : [];
        Assert.Empty(files);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private FileStorageService CreateService() =>
        new(_repository, Options.Create(TestOptions()), new NoOpFileScanner());
    private FileStorageOptions TestOptions() => new()
    {
        RootPath = _root, QuarantinePath = Path.Combine(_root, ".quarantine"), MaxFileSizeMb = 1,
        AllowedExtensions = [".pdf", ".txt"], AllowedMimeTypes = ["application/pdf", "text/plain"]
    };

    private string ToPhysical(string relativePath) =>
        Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static PurchaseFile CreatePurchaseFile() => new(
        Guid.NewGuid(), "PF-2026-000001", "Test", null, PurchaseFilePriority.Normal,
        null, Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid());

    private sealed class FakeRepository : IFileDocumentRepository
    {
        public PurchaseFile? PurchaseFile { get; set; }
        public FileDocument? Document { get; private set; }
        public bool FailSave { get; set; }
        public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(PurchaseFile?.Id == id ? PurchaseFile : null);
        public Task<FileDocument?> FindDocumentAsync(Guid id, bool includeVersions, CancellationToken cancellationToken) =>
            Task.FromResult(Document?.Id == id ? Document : null);
        public Task AddDocumentAsync(FileDocument document, CancellationToken cancellationToken)
        {
            Document = document;
            return Task.CompletedTask;
        }
        public Task<IReadOnlyList<FileDocumentDto>> GetDocumentsAsync(Guid purchaseFileId, bool includeDeleted, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<FileDocumentDto>>([]);
        public Task SaveChangesAsync(CancellationToken cancellationToken) =>
            FailSave ? Task.FromException(new InvalidOperationException("DB failure")) : Task.CompletedTask;
    }
}
