using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Suppliers;

public sealed class SupplierDocument : Entity<Guid>
{
    private SupplierDocument()
        : base(Guid.Empty)
    {
        DocumentType = string.Empty;
        OriginalFileName = string.Empty;
    }

    public SupplierDocument(Guid id, Guid supplierId, string documentType, Guid? fileDocumentId, string originalFileName, string? description, Guid uploadedByUserId)
        : base(id)
    {
        SupplierId = supplierId;
        DocumentType = Required(documentType, nameof(documentType));
        FileDocumentId = fileDocumentId;
        OriginalFileName = Required(originalFileName, nameof(originalFileName));
        Description = Trim(description);
        UploadedAt = DateTime.UtcNow;
        UploadedByUserId = uploadedByUserId;
    }

    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public string DocumentType { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
