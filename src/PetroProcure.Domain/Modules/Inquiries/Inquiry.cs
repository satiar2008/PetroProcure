using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Inquiries;

public sealed class Inquiry : Entity<Guid>
{
    private readonly List<InquiryItem> _items = [];
    private readonly List<InquirySupplier> _suppliers = [];
    private readonly List<SupplierQuote> _quotes = [];
    private readonly List<InquiryDocument> _documents = [];

    private Inquiry() : base(Guid.Empty)
    {
        InquiryNumber = string.Empty;
        Title = string.Empty;
    }

    public Inquiry(Guid id, string inquiryNumber, Guid purchaseFileId, string title, InquiryType inquiryType,
        DateTime issueDate, DateTime? deadlineDate, string? description, Guid createdByUserId)
        : base(id)
    {
        InquiryNumber = Required(inquiryNumber, nameof(inquiryNumber));
        PurchaseFileId = purchaseFileId == Guid.Empty ? throw new ArgumentException("Purchase file is required.", nameof(purchaseFileId)) : purchaseFileId;
        Title = Required(title, nameof(title));
        InquiryType = inquiryType;
        IssueDate = issueDate;
        DeadlineDate = deadlineDate;
        Description = Trim(description);
        Status = InquiryStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public string InquiryNumber { get; private set; }
    public Guid PurchaseFileId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public InquiryStatus Status { get; private set; }
    public InquiryType InquiryType { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime? DeadlineDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? SentAt { get; private set; }
    public Guid? SentByUserId { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ClosedByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }
    public IReadOnlyCollection<InquiryItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<InquirySupplier> Suppliers => _suppliers.AsReadOnly();
    public IReadOnlyCollection<SupplierQuote> Quotes => _quotes.AsReadOnly();
    public IReadOnlyCollection<InquiryDocument> Documents => _documents.AsReadOnly();

    public void Update(string title, InquiryType inquiryType, DateTime? deadlineDate, string? description)
    {
        EnsureEditable();
        Title = Required(title, nameof(title));
        InquiryType = inquiryType;
        DeadlineDate = deadlineDate;
        Description = Trim(description);
    }

    public void AddItem(InquiryItem item)
    {
        EnsureEditable();
        if (item.InquiryId != Id) throw new InvalidOperationException("Item belongs to another inquiry.");
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Inquiry item was not found.");
        _items.Remove(item);
    }

    public void AddSupplier(InquirySupplier supplier)
    {
        EnsureEditable();
        if (supplier.InquiryId != Id) throw new InvalidOperationException("Supplier belongs to another inquiry.");
        if (_suppliers.Any(x => x.SupplierId == supplier.SupplierId && x.Status != InquirySupplierStatus.Excluded))
            throw new InvalidOperationException("Supplier is already assigned to this inquiry.");
        _suppliers.Add(supplier);
    }

    public void RemoveSupplier(Guid inquirySupplierId)
    {
        EnsureEditable();
        var supplier = _suppliers.SingleOrDefault(x => x.Id == inquirySupplierId) ?? throw new InvalidOperationException("Inquiry supplier was not found.");
        supplier.Exclude();
    }

    public void Send(Guid sentByUserId)
    {
        EnsureEditable();
        if (!_items.Any()) throw new InvalidOperationException("Inquiry cannot be sent without items.");
        if (!_suppliers.Any(x => x.Status != InquirySupplierStatus.Excluded)) throw new InvalidOperationException("Inquiry cannot be sent without suppliers.");
        Status = InquiryStatus.Sent;
        SentAt = DateTime.UtcNow;
        SentByUserId = sentByUserId;
        foreach (var supplier in _suppliers.Where(x => x.Status == InquirySupplierStatus.Draft))
            supplier.Invite(sentByUserId);
    }

    public void MarkQuoteReceived()
    {
        if (Status is InquiryStatus.Sent or InquiryStatus.PartiallyResponded)
            Status = _suppliers.All(x => x.Status is InquirySupplierStatus.Responded or InquirySupplierStatus.Excluded)
                ? InquiryStatus.FullyResponded
                : InquiryStatus.PartiallyResponded;
    }

    public void SelectQuote(Guid quoteId, string? reason)
    {
        EnsureNotClosed();
        foreach (var quote in _quotes)
            quote.Unselect();
        var selected = _quotes.SingleOrDefault(x => x.Id == quoteId) ?? throw new InvalidOperationException("Quote was not found.");
        selected.Select(reason);
        Status = InquiryStatus.SupplierSelected;
    }

    public void AddQuote(SupplierQuote quote)
    {
        EnsureNotClosed();
        if (quote.InquiryId != Id) throw new InvalidOperationException("Quote belongs to another inquiry.");
        _quotes.Add(quote);
        MarkQuoteReceived();
    }

    public void Close(Guid userId)
    {
        EnsureNotClosed();
        Status = InquiryStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedByUserId = userId;
    }

    public void Cancel(string reason, Guid userId)
    {
        EnsureNotClosed();
        Status = InquiryStatus.Cancelled;
        CancellationReason = Required(reason, nameof(reason));
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }

    public void EnsureEditable()
    {
        EnsureNotClosed();
        if (Status is not InquiryStatus.Draft and not InquiryStatus.ReadyToSend)
            throw new InvalidOperationException("Inquiry cannot be edited in the current status.");
    }

    private void EnsureNotClosed()
    {
        if (Status is InquiryStatus.Closed or InquiryStatus.Cancelled)
            throw new InvalidOperationException("Closed or cancelled inquiries are read-only.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
