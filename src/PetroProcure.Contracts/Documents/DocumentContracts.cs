using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Documents;

public sealed record FileDocumentDto(
    Guid Id, Guid PurchaseFileId, Guid? DepartmentId, DocumentType DocumentType,
    string OriginalFileName, string StoredFileName, string RelativePath, string Extension,
    string MimeType, long Size, string Hash, int VersionNo, Guid UploadedByUserId,
    DateTime UploadedAt, bool IsDeleted, string? Description);
public sealed record DocumentVersionDto(
    Guid Id, Guid FileDocumentId, int VersionNo, string StoredFileName,
    string RelativePath, long Size, string Hash, Guid CreatedByUserId, DateTime CreatedAt);
public sealed record UploadDocumentRequest(
    DocumentType DocumentType, Guid? DepartmentId, string? Description);
public sealed record DocumentUploadLimitsDto(
    int MaxFileSizeMb, IReadOnlyList<string> AllowedExtensions, IReadOnlyList<string> AllowedMimeTypes);
public sealed record DocumentListRequest(bool IncludeDeleted = false, DocumentType? DocumentType = null);
