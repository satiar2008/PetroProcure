using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class InquirySupplier : Entity<Guid>
{
    private InquirySupplier() : base(Guid.Empty)
    {
        SupplierCode = SupplierName = string.Empty;
    }

    public InquirySupplier(Guid id, Guid inquiryId, Guid supplierId, string supplierCode, string supplierName,
        Guid? contactId, string? contactName, string? contactEmail)
        : base(id)
    {
        InquiryId = inquiryId;
        SupplierId = supplierId;
        SupplierCode = Required(supplierCode, nameof(supplierCode));
        SupplierName = Required(supplierName, nameof(supplierName));
        ContactId = contactId;
        ContactName = Trim(contactName);
        ContactEmail = Trim(contactEmail);
        Status = InquirySupplierStatus.Draft;
    }

    public Guid InquiryId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string SupplierCode { get; private set; }
    public string SupplierName { get; private set; }
    public Guid? ContactId { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactEmail { get; private set; }
    public InquirySupplierStatus Status { get; private set; }
    public DateTime? InvitedAt { get; private set; }
    public Guid? InvitedByUserId { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public DateTime? DeclinedAt { get; private set; }
    public string? DeclineReason { get; private set; }

    public void Invite(Guid userId) { Status = InquirySupplierStatus.Invited; InvitedAt = DateTime.UtcNow; InvitedByUserId = userId; }
    public void MarkResponded() { Status = InquirySupplierStatus.Responded; RespondedAt = DateTime.UtcNow; }
    public void Decline(string reason) { Status = InquirySupplierStatus.Declined; DeclinedAt = DateTime.UtcNow; DeclineReason = Required(reason, nameof(reason)); }
    public void Exclude() => Status = InquirySupplierStatus.Excluded;

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
