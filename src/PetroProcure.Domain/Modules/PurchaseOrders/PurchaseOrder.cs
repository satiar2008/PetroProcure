using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.PurchaseOrders;

public sealed class PurchaseOrder : Entity<Guid>
{
    private readonly List<PurchaseOrderItem> _items = [];
    private readonly List<PurchaseOrderApproval> _approvals = [];
    private readonly List<PurchaseOrderDocument> _documents = [];

    private PurchaseOrder() : base(Guid.Empty)
    {
        PurchaseOrderNumber = Title = Currency = string.Empty;
    }

    public PurchaseOrder(Guid id, string purchaseOrderNumber, Guid purchaseFileId, Guid supplierId,
        Guid? contractId, Guid? tenderId, Guid? tenderBidId, string title, PurchaseOrderType purchaseOrderType,
        string currency, Guid createdByUserId, string? description = null, decimal? totalAmount = null,
        decimal? taxAmount = null, decimal? discountAmount = null, decimal? finalAmount = null,
        DateTime? orderDate = null, DateTime? expectedDeliveryDate = null, string? deliveryLocation = null,
        string? deliveryTerms = null, string? paymentTerms = null, string? warrantyTerms = null, string? notes = null) : base(id)
    {
        PurchaseOrderNumber = Required(purchaseOrderNumber, nameof(purchaseOrderNumber));
        PurchaseFileId = purchaseFileId == Guid.Empty ? throw new ArgumentException("Purchase file is required.", nameof(purchaseFileId)) : purchaseFileId;
        SupplierId = supplierId == Guid.Empty ? throw new ArgumentException("Supplier is required.", nameof(supplierId)) : supplierId;
        ContractId = contractId;
        TenderId = tenderId;
        TenderBidId = tenderBidId;
        Title = Required(title, nameof(title));
        Description = Trim(description);
        Status = PurchaseOrderStatus.Draft;
        PurchaseOrderType = purchaseOrderType;
        Currency = Required(currency, nameof(currency));
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdateFinancials(totalAmount, taxAmount, discountAmount, finalAmount);
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DeliveryLocation = Trim(deliveryLocation);
        DeliveryTerms = Trim(deliveryTerms);
        PaymentTerms = Trim(paymentTerms);
        WarrantyTerms = Trim(warrantyTerms);
        Notes = Trim(notes);
    }

    public string PurchaseOrderNumber { get; private set; }
    public Guid PurchaseFileId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid? ContractId { get; private set; }
    public Guid? TenderId { get; private set; }
    public Guid? TenderBidId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public PurchaseOrderType PurchaseOrderType { get; private set; }
    public string Currency { get; private set; }
    public decimal? TotalAmount { get; private set; }
    public decimal? TaxAmount { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal? FinalAmount { get; private set; }
    public DateTime? OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public string? DeliveryLocation { get; private set; }
    public string? DeliveryTerms { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? WarrantyTerms { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public Guid? SubmittedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public Guid? IssuedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<PurchaseOrderApproval> Approvals => _approvals.AsReadOnly();
    public IReadOnlyCollection<PurchaseOrderDocument> Documents => _documents.AsReadOnly();

    public void Update(string title, string? description, PurchaseOrderType purchaseOrderType, string currency,
        decimal? totalAmount, decimal? taxAmount, decimal? discountAmount, decimal? finalAmount,
        DateTime? orderDate, DateTime? expectedDeliveryDate, string? deliveryLocation, string? deliveryTerms,
        string? paymentTerms, string? warrantyTerms, string? notes, bool adminOverride = false)
    {
        EnsureEditable(adminOverride);
        Title = Required(title, nameof(title));
        Description = Trim(description);
        PurchaseOrderType = purchaseOrderType;
        Currency = Required(currency, nameof(currency));
        UpdateFinancials(totalAmount, taxAmount, discountAmount, finalAmount);
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DeliveryLocation = Trim(deliveryLocation);
        DeliveryTerms = Trim(deliveryTerms);
        PaymentTerms = Trim(paymentTerms);
        WarrantyTerms = Trim(warrantyTerms);
        Notes = Trim(notes);
    }

    public void AddItem(PurchaseOrderItem item, bool adminOverride = false)
    {
        EnsureEditable(adminOverride);
        if (item.PurchaseOrderId != Id) throw new InvalidOperationException("Purchase order item belongs to another order.");
        _items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId, bool adminOverride = false)
    {
        EnsureEditable(adminOverride);
        var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Purchase order item was not found.");
        _items.Remove(item);
        RecalculateTotals();
    }

    public void AddApproval(PurchaseOrderApproval approval)
    {
        if (approval.PurchaseOrderId != Id) throw new InvalidOperationException("Approval belongs to another purchase order.");
        _approvals.Add(approval);
    }

    public void AddDocument(PurchaseOrderDocument document)
    {
        if (document.PurchaseOrderId != Id) throw new InvalidOperationException("Document belongs to another purchase order.");
        _documents.Add(document);
    }

    public void Submit(Guid userId)
    {
        EnsureEditable();
        if (SupplierId == Guid.Empty) throw new InvalidOperationException("Purchase order cannot be submitted without supplier.");
        if (_items.Count == 0) throw new InvalidOperationException("Purchase order cannot be submitted without at least one item.");
        Status = PurchaseOrderStatus.UnderReview;
        SubmittedAt = DateTime.UtcNow;
        SubmittedByUserId = userId;
    }

    public void Approve(Guid userId, string? comment = null)
    {
        if (Status is not PurchaseOrderStatus.UnderReview and not PurchaseOrderStatus.WaitingForApproval)
            throw new InvalidOperationException("Purchase order is not waiting for approval.");
        Status = PurchaseOrderStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = userId;
        var approval = new PurchaseOrderApproval(Guid.NewGuid(), Id, "PurchaseOrderApproval", null, userId, comment);
        approval.Approve(comment);
        _approvals.Add(approval);
    }

    public void Reject(Guid userId, string comment)
    {
        if (Status is not PurchaseOrderStatus.UnderReview and not PurchaseOrderStatus.WaitingForApproval)
            throw new InvalidOperationException("Purchase order is not waiting for approval.");
        Status = PurchaseOrderStatus.Draft;
        var approval = new PurchaseOrderApproval(Guid.NewGuid(), Id, "PurchaseOrderRejection", null, userId, comment);
        approval.Reject(comment);
        _approvals.Add(approval);
    }

    public void Issue(Guid userId)
    {
        if (Status != PurchaseOrderStatus.Approved)
            throw new InvalidOperationException("Purchase order cannot be issued unless approved.");
        if (_items.Count == 0) throw new InvalidOperationException("Purchase order cannot be issued without at least one item.");
        Status = PurchaseOrderStatus.Issued;
        IssuedAt = DateTime.UtcNow;
        IssuedByUserId = userId;
        OrderDate ??= DateTime.UtcNow;
    }

    public void Complete(Guid userId)
    {
        if (Status is not PurchaseOrderStatus.Issued and not PurchaseOrderStatus.PartiallyReceived and not PurchaseOrderStatus.FullyReceived)
            throw new InvalidOperationException("Only issued or received purchase orders can be completed.");
        Status = PurchaseOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }

    public void ApplyReceipt(Guid itemId, decimal receivedQuantity)
    {
        if (Status is not PurchaseOrderStatus.Issued and not PurchaseOrderStatus.PartiallyReceived)
            throw new InvalidOperationException("Only issued or partially received purchase orders can receive goods.");
        var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Purchase order item was not found.");
        item.Receive(receivedQuantity);
        Status = _items.All(x => x.RemainingQuantity == 0)
            ? PurchaseOrderStatus.FullyReceived
            : PurchaseOrderStatus.PartiallyReceived;
    }

    public void Cancel(string reason, Guid userId)
    {
        if (Status is PurchaseOrderStatus.Completed or PurchaseOrderStatus.Archived)
            throw new InvalidOperationException("Completed or archived purchase orders cannot be cancelled.");
        Status = PurchaseOrderStatus.Cancelled;
        CancellationReason = Required(reason, nameof(reason));
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }

    public void EnsureEditable(bool adminOverride = false)
    {
        if (!adminOverride && Status is PurchaseOrderStatus.Issued or PurchaseOrderStatus.Completed
            or PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Archived)
            throw new InvalidOperationException("Issued, completed, cancelled, or archived purchase orders are read-only.");
    }

    private void RecalculateTotals()
    {
        var total = _items.Where(x => x.TotalPrice.HasValue).Select(x => x.TotalPrice!.Value).DefaultIfEmpty().Sum();
        if (total > 0) TotalAmount = total;
        FinalAmount = TotalAmount.HasValue || TaxAmount.HasValue || DiscountAmount.HasValue
            ? (TotalAmount ?? 0) + (TaxAmount ?? 0) - (DiscountAmount ?? 0)
            : FinalAmount;
    }

    private void UpdateFinancials(decimal? totalAmount, decimal? taxAmount, decimal? discountAmount, decimal? finalAmount)
    {
        if (totalAmount < 0 || taxAmount < 0 || discountAmount < 0 || finalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount));
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        DiscountAmount = discountAmount;
        FinalAmount = finalAmount ?? (totalAmount.HasValue || taxAmount.HasValue || discountAmount.HasValue
            ? (totalAmount ?? 0) + (taxAmount ?? 0) - (discountAmount ?? 0)
            : null);
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseOrderItem : Entity<Guid>
{
    private PurchaseOrderItem() : base(Guid.Empty)
    {
        MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty;
    }

    public PurchaseOrderItem(Guid id, Guid purchaseOrderId, Guid? purchaseFileItemId, Guid? contractItemId,
        Guid? tenderBidItemId, Guid mescItemId, string mescCode, string mescGeneralGroupCode,
        string generalDescription, string specificDescription, Guid unitOfMeasureId, decimal orderedQuantity,
        decimal? unitPrice = null, DateTime? expectedDeliveryDate = null, string? technicalDescription = null,
        string? notes = null) : base(id)
    {
        if (orderedQuantity <= 0) throw new ArgumentOutOfRangeException(nameof(orderedQuantity));
        PurchaseOrderId = purchaseOrderId;
        PurchaseFileItemId = purchaseFileItemId;
        ContractItemId = contractItemId;
        TenderBidItemId = tenderBidItemId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        OrderedQuantity = orderedQuantity;
        ReceivedQuantity = 0;
        RemainingQuantity = orderedQuantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.HasValue ? unitPrice.Value * orderedQuantity : null;
        ExpectedDeliveryDate = expectedDeliveryDate;
        TechnicalDescription = Trim(technicalDescription);
        Notes = Trim(notes);
    }

    public Guid PurchaseOrderId { get; private set; }
    public Guid? PurchaseFileItemId { get; private set; }
    public Guid? ContractItemId { get; private set; }
    public Guid? TenderBidItemId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal RemainingQuantity { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? TotalPrice { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public string? TechnicalDescription { get; private set; }
    public string? Notes { get; private set; }

    public void UpdateReceivedQuantity(decimal receivedQuantity)
    {
        if (receivedQuantity < 0 || receivedQuantity > OrderedQuantity) throw new ArgumentOutOfRangeException(nameof(receivedQuantity));
        ReceivedQuantity = receivedQuantity;
        RemainingQuantity = OrderedQuantity - receivedQuantity;
    }

    public void Receive(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > RemainingQuantity) throw new InvalidOperationException("Received quantity cannot exceed remaining quantity.");
        ReceivedQuantity += quantity;
        RemainingQuantity -= quantity;
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseOrderApproval : Entity<Guid>
{
    private PurchaseOrderApproval() : base(Guid.Empty) { ApprovalStep = string.Empty; }
    public PurchaseOrderApproval(Guid id, Guid purchaseOrderId, string approvalStep, Guid? departmentId,
        Guid? approverUserId, string? comment = null) : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        ApprovalStep = string.IsNullOrWhiteSpace(approvalStep) ? "Approval" : approvalStep.Trim();
        DepartmentId = departmentId;
        ApproverUserId = approverUserId;
        Status = PurchaseOrderApprovalStatus.Pending;
        Comment = Trim(comment);
        CreatedAt = DateTime.UtcNow;
    }

    public Guid PurchaseOrderId { get; private set; }
    public string ApprovalStep { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? ApproverUserId { get; private set; }
    public PurchaseOrderApprovalStatus Status { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public void Approve(string? comment = null) { Status = PurchaseOrderApprovalStatus.Approved; Comment = Trim(comment) ?? Comment; ApprovedAt = DateTime.UtcNow; }
    public void Reject(string? comment) { Status = PurchaseOrderApprovalStatus.Rejected; Comment = Trim(comment); RejectedAt = DateTime.UtcNow; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseOrderDocument : Entity<Guid>
{
    private PurchaseOrderDocument() : base(Guid.Empty) { DocumentType = string.Empty; }
    public PurchaseOrderDocument(Guid id, Guid purchaseOrderId, Guid? fileDocumentId, string documentType,
        string? originalFileName, string? description, Guid uploadedByUserId) : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        FileDocumentId = fileDocumentId;
        DocumentType = string.IsNullOrWhiteSpace(documentType) ? "PurchaseOrderDocument" : documentType.Trim();
        OriginalFileName = Trim(originalFileName);
        Description = Trim(description);
        UploadedAt = DateTime.UtcNow;
        UploadedByUserId = uploadedByUserId;
    }

    public Guid PurchaseOrderId { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseOrderSequence : Entity<Guid>
{
    private PurchaseOrderSequence() : base(Guid.Empty) { }
    public PurchaseOrderSequence(Guid id, int year, int lastNumber) : base(id)
    {
        Year = year;
        LastNumber = lastNumber;
    }
    public int Year { get; private set; }
    public int LastNumber { get; private set; }
    public int Next() => ++LastNumber;
}
