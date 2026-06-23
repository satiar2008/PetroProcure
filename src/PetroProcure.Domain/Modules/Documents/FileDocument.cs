using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Documents;

public sealed class FileDocument : AuditableEntity<Guid>
{
    private readonly List<DocumentVersion> _versions = [];

    public FileDocument(
        Guid id, Guid purchaseFileId, Guid? departmentId, DocumentType documentType,
        string originalFileName, string storedFileName, string relativePath, string extension,
        string mimeType, long size, string hash, int versionNo, Guid uploadedByUserId,
        string? description = null)
        : base(id)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        if (versionNo < 1) throw new ArgumentOutOfRangeException(nameof(versionNo));
        PurchaseFileId = purchaseFileId;
        DepartmentId = departmentId;
        DocumentType = documentType;
        OriginalFileName = Required(originalFileName, nameof(originalFileName));
        StoredFileName = Required(storedFileName, nameof(storedFileName));
        RelativePath = ValidateRelativePath(relativePath);
        Extension = extension?.Trim() ?? string.Empty;
        MimeType = Required(mimeType, nameof(mimeType));
        Size = size;
        Hash = Required(hash, nameof(hash));
        VersionNo = versionNo;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
        Description = description?.Trim();
    }

    public Guid PurchaseFileId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string OriginalFileName { get; private set; }
    public string StoredFileName { get; private set; }
    public string RelativePath { get; private set; }
    public string Extension { get; private set; }
    public string MimeType { get; private set; }
    public long Size { get; private set; }
    public string Hash { get; private set; }
    public int VersionNo { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();

    public void AddVersion(DocumentVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        if (version.FileDocumentId != Id || version.VersionNo != VersionNo + 1)
            throw new InvalidOperationException("Document version sequence is invalid.");
        _versions.Add(version);
        VersionNo = version.VersionNo;
        StoredFileName = version.StoredFileName;
        RelativePath = version.RelativePath;
        Size = version.Size;
        Hash = version.Hash;
    }

    public void SoftDelete() => IsDeleted = true;

    public static string ValidateRelativePath(string path)
    {
        var value = Required(path, nameof(path)).Replace('\\', '/');
        if (Path.IsPathRooted(value) || value.Split('/').Any(part => part == ".."))
            throw new ArgumentException("Only a safe relative path may be stored.", nameof(path));
        return value;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
