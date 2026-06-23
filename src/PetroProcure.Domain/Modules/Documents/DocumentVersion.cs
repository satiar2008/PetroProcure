using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Documents;

public sealed class DocumentVersion : Entity<Guid>
{
    public DocumentVersion(
        Guid id, Guid fileDocumentId, int versionNo, string storedFileName,
        string relativePath, long size, string hash, Guid createdByUserId)
        : base(id)
    {
        if (versionNo < 2) throw new ArgumentOutOfRangeException(nameof(versionNo));
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        FileDocumentId = fileDocumentId;
        VersionNo = versionNo;
        StoredFileName = Required(storedFileName, nameof(storedFileName));
        RelativePath = FileDocument.ValidateRelativePath(relativePath);
        Size = size;
        Hash = Required(hash, nameof(hash));
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid FileDocumentId { get; private set; }
    public int VersionNo { get; private set; }
    public string StoredFileName { get; private set; }
    public string RelativePath { get; private set; }
    public long Size { get; private set; }
    public string Hash { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
