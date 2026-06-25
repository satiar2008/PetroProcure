using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Contracts;

public sealed class PurchaseContract : Entity<Guid>
{
    private readonly List<PurchaseContractItem> _items = [];
    private readonly List<ContractClause> _clauses = [];
    private readonly List<ContractApproval> _approvals = [];
    private readonly List<ContractDocument> _documents = [];

    private PurchaseContract() : base(Guid.Empty)
    {
        ContractNumber = Title = Subject = Currency = string.Empty;
    }

    public PurchaseContract(
        Guid id,
        string contractNumber,
        Guid purchaseFileId,
        Guid supplierId,
        Guid? tenderId,
        Guid? tenderBidId,
        Guid? commissionDecisionId,
        Guid? contractTemplateId,
        string title,
        string subject,
        ContractType contractType,
        string currency,
        Guid createdByUserId,
        decimal? totalAmount = null,
        decimal? taxAmount = null,
        decimal? finalAmount = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        DateTime? deliveryDeadline = null,
        string? paymentTerms = null,
        string? deliveryTerms = null,
        string? warrantyTerms = null,
        string? penaltyTerms = null,
        string? description = null) : base(id)
    {
        ContractNumber = Required(contractNumber, nameof(contractNumber));
        PurchaseFileId = purchaseFileId;
        SupplierId = supplierId;
        TenderId = tenderId;
        TenderBidId = tenderBidId;
        CommissionDecisionId = commissionDecisionId;
        ContractTemplateId = contractTemplateId;
        Title = Required(title, nameof(title));
        Subject = Required(subject, nameof(subject));
        Status = ContractStatus.Draft;
        ContractType = contractType;
        Currency = Required(currency, nameof(currency));
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        UpdateFinancials(totalAmount, taxAmount, finalAmount);
        StartDate = startDate;
        EndDate = endDate;
        DeliveryDeadline = deliveryDeadline;
        PaymentTerms = Trim(paymentTerms);
        DeliveryTerms = Trim(deliveryTerms);
        WarrantyTerms = Trim(warrantyTerms);
        PenaltyTerms = Trim(penaltyTerms);
        Description = Trim(description);
    }

    public string ContractNumber { get; private set; }
    public Guid PurchaseFileId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid? TenderId { get; private set; }
    public Guid? TenderBidId { get; private set; }
    public Guid? CommissionDecisionId { get; private set; }
    public Guid? ContractTemplateId { get; private set; }
    public string Title { get; private set; }
    public string Subject { get; private set; }
    public ContractStatus Status { get; private set; }
    public ContractType ContractType { get; private set; }
    public string Currency { get; private set; }
    public decimal? TotalAmount { get; private set; }
    public decimal? TaxAmount { get; private set; }
    public decimal? FinalAmount { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? DeliveryDeadline { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? DeliveryTerms { get; private set; }
    public string? WarrantyTerms { get; private set; }
    public string? PenaltyTerms { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public Guid? SubmittedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? SignedAt { get; private set; }
    public Guid? SignedByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<PurchaseContractItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<ContractClause> Clauses => _clauses.AsReadOnly();
    public IReadOnlyCollection<ContractApproval> Approvals => _approvals.AsReadOnly();
    public IReadOnlyCollection<ContractDocument> Documents => _documents.AsReadOnly();

    public void Update(
        string title,
        string subject,
        ContractType contractType,
        string currency,
        decimal? totalAmount,
        decimal? taxAmount,
        decimal? finalAmount,
        DateTime? startDate,
        DateTime? endDate,
        DateTime? deliveryDeadline,
        string? paymentTerms,
        string? deliveryTerms,
        string? warrantyTerms,
        string? penaltyTerms,
        string? description)
    {
        EnsureEditable();
        Title = Required(title, nameof(title));
        Subject = Required(subject, nameof(subject));
        ContractType = contractType;
        Currency = Required(currency, nameof(currency));
        UpdateFinancials(totalAmount, taxAmount, finalAmount);
        StartDate = startDate;
        EndDate = endDate;
        DeliveryDeadline = deliveryDeadline;
        PaymentTerms = Trim(paymentTerms);
        DeliveryTerms = Trim(deliveryTerms);
        WarrantyTerms = Trim(warrantyTerms);
        PenaltyTerms = Trim(penaltyTerms);
        Description = Trim(description);
    }

    public void ApplyTemplate(Guid templateId)
    {
        EnsureEditable();
        ContractTemplateId = templateId;
    }

    public void AddItem(PurchaseContractItem item)
    {
        EnsureEditable();
        if (item.ContractId != Id) throw new InvalidOperationException("Contract item belongs to another contract.");
        _items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Contract item was not found.");
        _items.Remove(item);
        RecalculateTotals();
    }

    public void AddClause(ContractClause clause)
    {
        EnsureEditable();
        if (clause.ContractId != Id) throw new InvalidOperationException("Clause belongs to another contract.");
        _clauses.Add(clause);
    }

    public void RemoveClause(Guid clauseId)
    {
        EnsureEditable();
        var clause = _clauses.SingleOrDefault(x => x.Id == clauseId) ?? throw new InvalidOperationException("Clause was not found.");
        if (clause.IsRequired) throw new InvalidOperationException("Required clauses cannot be removed.");
        _clauses.Remove(clause);
    }

    public void AddApproval(ContractApproval approval)
    {
        if (approval.ContractId != Id) throw new InvalidOperationException("Approval belongs to another contract.");
        _approvals.Add(approval);
    }

    public void AddDocument(ContractDocument document)
    {
        if (document.ContractId != Id) throw new InvalidOperationException("Document belongs to another contract.");
        _documents.Add(document);
    }

    public void Submit(Guid userId)
    {
        EnsureEditable();
        if (SupplierId == Guid.Empty) throw new InvalidOperationException("Contract cannot be submitted without supplier.");
        if (_items.Count == 0) throw new InvalidOperationException("Contract cannot be submitted without at least one item.");
        if (_clauses.Any(x => x.IsRequired && string.IsNullOrWhiteSpace(x.Body)))
            throw new InvalidOperationException("Contract cannot be submitted without required clauses.");
        if (!_clauses.Any(x => x.IsRequired))
            throw new InvalidOperationException("Contract cannot be submitted without required clauses.");
        Status = ContractStatus.UnderReview;
        SubmittedAt = DateTime.UtcNow;
        SubmittedByUserId = userId;
    }

    public void Approve(Guid userId)
    {
        if (Status is not ContractStatus.UnderReview and not ContractStatus.WaitingForApproval)
            throw new InvalidOperationException("Contract is not waiting for approval.");
        Status = ContractStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = userId;
    }

    public void Reject(Guid userId, string? comment)
    {
        if (Status is not ContractStatus.UnderReview and not ContractStatus.WaitingForApproval)
            throw new InvalidOperationException("Contract is not waiting for approval.");
        Status = ContractStatus.Draft;
        _approvals.Add(new ContractApproval(Guid.NewGuid(), Id, "Rejection", null, userId, comment));
        _approvals[^1].Reject(comment);
    }

    public void Sign(Guid userId)
    {
        if (Status != ContractStatus.Approved) throw new InvalidOperationException("Only approved contracts can be signed.");
        Status = ContractStatus.Signed;
        SignedAt = DateTime.UtcNow;
        SignedByUserId = userId;
    }

    public void Cancel(string reason, Guid userId)
    {
        if (Status is ContractStatus.Completed or ContractStatus.Archived)
            throw new InvalidOperationException("Completed or archived contracts cannot be cancelled.");
        Status = ContractStatus.Cancelled;
        CancellationReason = Required(reason, nameof(reason));
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }

    public void EnsureEditable(bool adminOverride = false)
    {
        if (!adminOverride && Status is ContractStatus.Approved or ContractStatus.Signed or ContractStatus.Active
            or ContractStatus.Completed or ContractStatus.Archived)
            throw new InvalidOperationException("Approved, signed, active, completed, or archived contracts are read-only.");
        if (Status == ContractStatus.Cancelled)
            throw new InvalidOperationException("Cancelled contracts are read-only.");
    }

    private void RecalculateTotals()
    {
        var total = _items.Where(x => x.TotalPrice.HasValue).Select(x => x.TotalPrice!.Value).DefaultIfEmpty().Sum();
        TotalAmount = total == 0 ? TotalAmount : total;
        FinalAmount = TotalAmount.HasValue || TaxAmount.HasValue ? (TotalAmount ?? 0) + (TaxAmount ?? 0) : FinalAmount;
    }

    private void UpdateFinancials(decimal? totalAmount, decimal? taxAmount, decimal? finalAmount)
    {
        if (totalAmount < 0 || taxAmount < 0 || finalAmount < 0) throw new ArgumentOutOfRangeException(nameof(totalAmount));
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        FinalAmount = finalAmount ?? (totalAmount.HasValue || taxAmount.HasValue ? (totalAmount ?? 0) + (taxAmount ?? 0) : null);
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class PurchaseContractItem : Entity<Guid>
{
    private PurchaseContractItem() : base(Guid.Empty)
    {
        MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty;
    }

    public PurchaseContractItem(Guid id, Guid contractId, Guid? purchaseFileItemId, Guid? tenderBidItemId,
        Guid mescItemId, string mescCode, string mescGeneralGroupCode, string generalDescription,
        string specificDescription, Guid unitOfMeasureId, decimal quantity, decimal? unitPrice,
        DateTime? deliveryDate, string? technicalDescription) : base(id)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        ContractId = contractId;
        PurchaseFileItemId = purchaseFileItemId;
        TenderBidItemId = tenderBidItemId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = unitPrice.HasValue ? unitPrice.Value * quantity : null;
        DeliveryDate = deliveryDate;
        TechnicalDescription = Trim(technicalDescription);
    }

    public Guid ContractId { get; private set; }
    public Guid? PurchaseFileItemId { get; private set; }
    public Guid? TenderBidItemId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? TotalPrice { get; private set; }
    public DateTime? DeliveryDate { get; private set; }
    public string? TechnicalDescription { get; private set; }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ContractClause : Entity<Guid>
{
    private ContractClause() : base(Guid.Empty) { Title = Body = string.Empty; }

    public ContractClause(Guid id, Guid contractId, int orderNo, string title, string body,
        ContractClauseType clauseType, bool isRequired, bool isEditable, Guid createdByUserId) : base(id)
    {
        ContractId = contractId;
        OrderNo = orderNo;
        Title = Required(title, nameof(title));
        Body = Required(body, nameof(body));
        ClauseType = clauseType;
        IsRequired = isRequired;
        IsEditable = isEditable;
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public Guid ContractId { get; private set; }
    public int OrderNo { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public ContractClauseType ClauseType { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsEditable { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public void Update(int orderNo, string title, string body, ContractClauseType clauseType,
        bool isRequired, bool isEditable, Guid updatedByUserId)
    {
        if (!IsEditable) throw new InvalidOperationException("Clause is not editable.");
        OrderNo = orderNo;
        Title = Required(title, nameof(title));
        Body = Required(body, nameof(body));
        ClauseType = clauseType;
        IsRequired = isRequired;
        IsEditable = isEditable;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}

public sealed class ContractTemplate : Entity<Guid>
{
    private readonly List<ContractTemplateClause> _clauses = [];
    private ContractTemplate() : base(Guid.Empty) { TemplateCode = Title = string.Empty; }

    public ContractTemplate(Guid id, string templateCode, string title, string? description,
        ContractType contractType, Guid createdByUserId) : base(id)
    {
        TemplateCode = Required(templateCode, nameof(templateCode));
        Title = Required(title, nameof(title));
        Description = Trim(description);
        ContractType = contractType;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
    }

    public string TemplateCode { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public ContractType ContractType { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public IReadOnlyCollection<ContractTemplateClause> Clauses => _clauses.AsReadOnly();

    public void Update(string title, string? description, ContractType contractType, bool isActive)
    {
        Title = Required(title, nameof(title));
        Description = Trim(description);
        ContractType = contractType;
        IsActive = isActive;
    }

    public void AddClause(ContractTemplateClause clause)
    {
        if (clause.TemplateId != Id) throw new InvalidOperationException("Template clause belongs to another template.");
        _clauses.Add(clause);
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ContractTemplateClause : Entity<Guid>
{
    private ContractTemplateClause() : base(Guid.Empty) { Title = Body = string.Empty; }

    public ContractTemplateClause(Guid id, Guid templateId, int orderNo, string title, string body,
        ContractClauseType clauseType, bool isRequired, bool isEditable) : base(id)
    {
        TemplateId = templateId;
        OrderNo = orderNo;
        Title = Required(title, nameof(title));
        Body = Required(body, nameof(body));
        ClauseType = clauseType;
        IsRequired = isRequired;
        IsEditable = isEditable;
    }

    public Guid TemplateId { get; private set; }
    public int OrderNo { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public ContractClauseType ClauseType { get; private set; }
    public bool IsRequired { get; private set; }
    public bool IsEditable { get; private set; }

    public void Update(int orderNo, string title, string body, ContractClauseType clauseType, bool isRequired, bool isEditable)
    {
        OrderNo = orderNo;
        Title = Required(title, nameof(title));
        Body = Required(body, nameof(body));
        ClauseType = clauseType;
        IsRequired = isRequired;
        IsEditable = isEditable;
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}

public sealed class ContractApproval : Entity<Guid>
{
    private ContractApproval() : base(Guid.Empty) { ApprovalStep = string.Empty; }
    public ContractApproval(Guid id, Guid contractId, string approvalStep, Guid? departmentId,
        Guid? approverUserId, string? comment = null) : base(id)
    {
        ContractId = contractId;
        ApprovalStep = string.IsNullOrWhiteSpace(approvalStep) ? "Approval" : approvalStep.Trim();
        DepartmentId = departmentId;
        ApproverUserId = approverUserId;
        Status = ContractApprovalStatus.Pending;
        Comment = Trim(comment);
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ContractId { get; private set; }
    public string ApprovalStep { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? ApproverUserId { get; private set; }
    public ContractApprovalStatus Status { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }

    public void Approve(string? comment = null)
    {
        Status = ContractApprovalStatus.Approved;
        Comment = Trim(comment) ?? Comment;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Reject(string? comment)
    {
        Status = ContractApprovalStatus.Rejected;
        Comment = Trim(comment);
        RejectedAt = DateTime.UtcNow;
    }

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ContractDocument : Entity<Guid>
{
    private ContractDocument() : base(Guid.Empty) { DocumentType = string.Empty; }
    public ContractDocument(Guid id, Guid contractId, Guid? fileDocumentId, string documentType,
        string? originalFileName, string? description, Guid uploadedByUserId) : base(id)
    {
        ContractId = contractId;
        FileDocumentId = fileDocumentId;
        DocumentType = string.IsNullOrWhiteSpace(documentType) ? "ContractDocument" : documentType.Trim();
        OriginalFileName = Trim(originalFileName);
        Description = Trim(description);
        UploadedAt = DateTime.UtcNow;
        UploadedByUserId = uploadedByUserId;
    }

    public Guid ContractId { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ContractSequence : Entity<Guid>
{
    private ContractSequence() : base(Guid.Empty) { }
    public ContractSequence(Guid id, int year, int lastNumber) : base(id)
    {
        Year = year;
        LastNumber = lastNumber;
    }

    public int Year { get; private set; }
    public int LastNumber { get; private set; }
    public int Next() => ++LastNumber;
}
