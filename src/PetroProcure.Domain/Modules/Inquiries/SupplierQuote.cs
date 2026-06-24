using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class SupplierQuote : Entity<Guid>
{
    private readonly List<SupplierQuoteItem> _items = [];
    private SupplierQuote() : base(Guid.Empty) { Currency = "IRR"; }
    public SupplierQuote(Guid id, Guid inquiryId, Guid supplierId, Guid inquirySupplierId, string? quoteNumber,
        DateTime? quoteDate, DateTime? validUntil, string currency, string? deliveryTerms, string? paymentTerms,
        DateTime? deliveryDate, decimal totalAmount, decimal? taxAmount, decimal? discountAmount,
        string? technicalNote, string? commercialNote, Guid receivedByUserId)
        : base(id)
    {
        InquiryId = inquiryId; SupplierId = supplierId; InquirySupplierId = inquirySupplierId;
        QuoteNumber = Trim(quoteNumber); QuoteDate = quoteDate; ValidUntil = validUntil;
        Currency = string.IsNullOrWhiteSpace(currency) ? "IRR" : currency.Trim();
        DeliveryTerms = Trim(deliveryTerms); PaymentTerms = Trim(paymentTerms); DeliveryDate = deliveryDate;
        TotalAmount = totalAmount; TaxAmount = taxAmount; DiscountAmount = discountAmount;
        FinalAmount = totalAmount + (taxAmount ?? 0) - (discountAmount ?? 0);
        TechnicalNote = Trim(technicalNote); CommercialNote = Trim(commercialNote);
        Status = SupplierQuoteStatus.Received; ReceivedAt = DateTime.UtcNow; ReceivedByUserId = receivedByUserId;
    }
    public Guid InquiryId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid InquirySupplierId { get; private set; }
    public string? QuoteNumber { get; private set; }
    public DateTime? QuoteDate { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public string Currency { get; private set; }
    public string? DeliveryTerms { get; private set; }
    public string? PaymentTerms { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal? TaxAmount { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal FinalAmount { get; private set; }
    public string? TechnicalNote { get; private set; }
    public string? CommercialNote { get; private set; }
    public SupplierQuoteStatus Status { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public Guid ReceivedByUserId { get; private set; }
    public bool IsSelected { get; private set; }
    public string? SelectionReason { get; private set; }
    public IReadOnlyCollection<SupplierQuoteItem> Items => _items.AsReadOnly();
    public void AddItem(SupplierQuoteItem item) { if (item.SupplierQuoteId != Id) throw new InvalidOperationException("Quote item belongs to another quote."); _items.Add(item); }
    public void Select(string? reason) { IsSelected = true; SelectionReason = Trim(reason); Status = SupplierQuoteStatus.Selected; }
    public void Unselect() { IsSelected = false; if (Status == SupplierQuoteStatus.Selected) Status = SupplierQuoteStatus.Received; }
    public void Reject(string? reason) { IsSelected = false; SelectionReason = Trim(reason); Status = SupplierQuoteStatus.Rejected; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
