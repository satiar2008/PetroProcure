using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class InquiryDocument : Entity<Guid>
{
    private InquiryDocument() : base(Guid.Empty) { DocumentType = string.Empty; }
    public InquiryDocument(Guid id, Guid inquiryId, Guid? fileDocumentId, string documentType, string? originalFileName, string? description, Guid uploadedByUserId) : base(id)
    {
        InquiryId = inquiryId; FileDocumentId = fileDocumentId; DocumentType = string.IsNullOrWhiteSpace(documentType) ? throw new ArgumentException("Document type is required.") : documentType.Trim();
        OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? null : originalFileName.Trim(); Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UploadedAt = DateTime.UtcNow; UploadedByUserId = uploadedByUserId;
    }
    public Guid InquiryId { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }
}
