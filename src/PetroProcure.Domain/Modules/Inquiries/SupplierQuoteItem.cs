using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class SupplierQuoteItem : Entity<Guid>
{
    private SupplierQuoteItem() : base(Guid.Empty) { MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty; }
    public SupplierQuoteItem(Guid id, Guid supplierQuoteId, Guid inquiryItemId, string mescCode, string mescGeneralGroupCode,
        string generalDescription, string specificDescription, decimal quantity, decimal unitPrice, DateTime? deliveryDate,
        TechnicalComplianceStatus technicalComplianceStatus, string? technicalNote) : base(id)
    {
        SupplierQuoteId = supplierQuoteId; InquiryItemId = inquiryItemId; MescCode = mescCode; MescGeneralGroupCode = mescGeneralGroupCode;
        GeneralDescription = generalDescription; SpecificDescription = specificDescription; Quantity = quantity; UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice; DeliveryDate = deliveryDate; TechnicalComplianceStatus = technicalComplianceStatus;
        TechnicalNote = string.IsNullOrWhiteSpace(technicalNote) ? null : technicalNote.Trim();
    }
    public Guid SupplierQuoteId { get; private set; }
    public Guid InquiryItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public TechnicalComplianceStatus TechnicalComplianceStatus { get; private set; }
    public string? TechnicalNote { get; private set; }
}
