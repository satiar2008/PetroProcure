using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Documents;
using PetroProcure.Domain.Modules.PurchaseFiles;

namespace PetroProcure.Application.Documents;

public sealed class FileStorageOptions
{
    public const string SectionName = "PetroProcure:FileStorage";
    public string RootPath { get; set; } = string.Empty;
    public int MaxFileSizeMb { get; set; } = 25;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".txt"];
    public string[] AllowedMimeTypes { get; set; } =
    [
        "application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "image/png", "image/jpeg", "text/plain"
    ];
    public string QuarantinePath { get; set; } = string.Empty;
    public bool EnableAntivirusScan { get; set; }
}

public enum FileScanStatus { Clean = 1, Suspicious, Infected, Failed }
public sealed record FileScanResult(FileScanStatus Status, string? Details = null);
public interface IFileScanner
{
    Task<FileScanResult> ScanAsync(string filePath, CancellationToken cancellationToken = default);
}
public interface IOrphanFileCleanupService
{
    Task<int> CleanupAsync(CancellationToken cancellationToken = default);
}

public sealed record FileDocumentDto(
    Guid Id, Guid PurchaseFileId, Guid? DepartmentId, DocumentType DocumentType,
    string OriginalFileName, string StoredFileName, string RelativePath, string Extension,
    string MimeType, long Size, string Hash, int VersionNo, Guid UploadedByUserId,
    DateTime UploadedAt, bool IsDeleted, string? Description);

public sealed record StoredFileContent(Stream Stream, string MimeType, string OriginalFileName);

public interface IFileStorageService
{
    Task EnsurePurchaseFileFoldersAsync(PurchaseFile purchaseFile, CancellationToken cancellationToken = default);
    Task<FileDocumentDto> SaveFileAsync(
        Guid purchaseFileId, DocumentType documentType, string originalFileName, Stream stream,
        Guid uploadedByUserId, Guid? departmentId = null, string? mimeType = null,
        string? description = null, CancellationToken cancellationToken = default);
    Task<StoredFileContent> OpenFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(Guid fileDocumentId, CancellationToken cancellationToken = default);
    Task<FileDocumentDto> CreateNewVersionAsync(
        Guid fileDocumentId, Stream stream, Guid createdByUserId,
        CancellationToken cancellationToken = default);
    string GetRelativePath(PurchaseFile purchaseFile, DocumentType documentType, string storedFileName);
    Task<IReadOnlyList<FileDocumentDto>> GetPurchaseFileDocumentsAsync(
        Guid purchaseFileId, bool includeDeleted = false, CancellationToken cancellationToken = default);
}

public interface IFileDocumentRepository
{
    Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken cancellationToken);
    Task<FileDocument?> FindDocumentAsync(Guid id, bool includeVersions, CancellationToken cancellationToken);
    Task AddDocumentAsync(FileDocument document, CancellationToken cancellationToken);
    Task<IReadOnlyList<FileDocumentDto>> GetDocumentsAsync(Guid purchaseFileId, bool includeDeleted, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
