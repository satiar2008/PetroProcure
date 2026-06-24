using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Tenders;

public sealed class Tender : AuditableEntity<Guid>
{
    private readonly List<TenderItem> _items = [];
    private readonly List<TenderParticipant> _participants = [];
    private readonly List<TenderStage> _stages = [];
    private readonly List<TenderBid> _bids = [];
    private readonly List<TenderEvaluation> _evaluations = [];
    private readonly List<TenderDecision> _decisions = [];
    private readonly List<TenderDocument> _documents = [];

    private Tender() : base(Guid.Empty)
    {
        TenderNumber = string.Empty;
        Title = string.Empty;
    }

    public Tender(Guid id, string tenderNumber, Guid purchaseFileId, Guid? sourceInquiryId, string title,
        TenderType tenderType, DateTime issueDate, DateTime? submissionDeadline, DateTime? openingDate,
        string? description, Guid createdByUserId, DateTime? createdAt = null) : base(id)
    {
        TenderNumber = Required(tenderNumber, nameof(tenderNumber));
        PurchaseFileId = purchaseFileId;
        SourceInquiryId = sourceInquiryId;
        Title = Required(title, nameof(title));
        TenderType = tenderType;
        Status = TenderStatus.Draft;
        IssueDate = issueDate;
        SubmissionDeadline = submissionDeadline;
        OpeningDate = openingDate;
        Description = Trim(description);
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt ?? DateTime.UtcNow;

        foreach (TenderStageType stageType in Enum.GetValues<TenderStageType>())
            _stages.Add(new TenderStage(Guid.NewGuid(), id, stageType));
    }

    public string TenderNumber { get; private set; }
    public Guid PurchaseFileId { get; private set; }
    public Guid? SourceInquiryId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public TenderType TenderType { get; private set; }
    public TenderStatus Status { get; private set; }
    public DateTime IssueDate { get; private set; }
    public DateTime? SubmissionDeadline { get; private set; }
    public DateTime? OpeningDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public Guid? PublishedByUserId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ClosedByUserId { get; private set; }

    public IReadOnlyCollection<TenderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<TenderParticipant> Participants => _participants.AsReadOnly();
    public IReadOnlyCollection<TenderStage> Stages => _stages.AsReadOnly();
    public IReadOnlyCollection<TenderBid> Bids => _bids.AsReadOnly();
    public IReadOnlyCollection<TenderEvaluation> Evaluations => _evaluations.AsReadOnly();
    public IReadOnlyCollection<TenderDecision> Decisions => _decisions.AsReadOnly();
    public IReadOnlyCollection<TenderDocument> Documents => _documents.AsReadOnly();

    public void Update(string title, TenderType tenderType, DateTime? submissionDeadline, DateTime? openingDate, string? description)
    {
        EnsureEditable();
        Title = Required(title, nameof(title));
        TenderType = tenderType;
        SubmissionDeadline = submissionDeadline;
        OpeningDate = openingDate;
        Description = Trim(description);
    }

    public void AddItem(TenderItem item)
    {
        EnsureEditable();
        if (item.TenderId != Id) throw new InvalidOperationException("Tender item belongs to another tender.");
        if (_items.Any(x => x.PurchaseFileItemId.HasValue && x.PurchaseFileItemId == item.PurchaseFileItemId))
            throw new InvalidOperationException("Purchase file item is already added to tender.");
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        EnsureEditable();
        var item = _items.SingleOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Tender item was not found.");
        _items.Remove(item);
    }

    public void AddParticipant(TenderParticipant participant)
    {
        EnsureEditable();
        if (participant.TenderId != Id) throw new InvalidOperationException("Tender participant belongs to another tender.");
        if (_participants.Any(x => x.SupplierId == participant.SupplierId))
            throw new InvalidOperationException("Supplier is already added to tender.");
        _participants.Add(participant);
    }

    public void RemoveParticipant(Guid participantId)
    {
        EnsureEditable();
        var participant = _participants.SingleOrDefault(x => x.Id == participantId)
            ?? throw new InvalidOperationException("Tender participant was not found.");
        _participants.Remove(participant);
    }

    public void Publish(Guid userId)
    {
        EnsureEditable();
        if (_items.Count == 0) throw new InvalidOperationException("Tender cannot be published without at least one item.");
        if (_participants.Count == 0) throw new InvalidOperationException("Tender cannot be published without at least one participant.");
        Status = TenderStatus.Published;
        PublishedAt = DateTime.UtcNow;
        PublishedByUserId = userId;
        foreach (var participant in _participants.Where(x => x.Status == TenderParticipantStatus.Draft))
            participant.Invite(userId);
        StartStage(TenderStageType.SupplierInvitation, userId);
    }

    public void Cancel(string reason, Guid userId)
    {
        EnsureNotFinal();
        Status = TenderStatus.Cancelled;
        CancellationReason = Required(reason, nameof(reason));
        CancelledAt = DateTime.UtcNow;
        CancelledByUserId = userId;
    }

    public void Close(Guid userId)
    {
        EnsureNotFinal();
        Status = TenderStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedByUserId = userId;
    }

    public void AddBid(TenderBid bid)
    {
        EnsureNotFinal();
        if (bid.TenderId != Id) throw new InvalidOperationException("Tender bid belongs to another tender.");
        _bids.Add(bid);
        Status = TenderStatus.ReceivingBids;
    }

    public void AddEvaluation(TenderEvaluation evaluation)
    {
        EnsureNotFinal();
        if (evaluation.TenderId != Id) throw new InvalidOperationException("Tender evaluation belongs to another tender.");
        _evaluations.Add(evaluation);
        Status = evaluation.EvaluationType switch
        {
            TenderEvaluationType.Qualification => TenderStatus.UnderQualification,
            TenderEvaluationType.Technical => TenderStatus.UnderTechnicalEvaluation,
            TenderEvaluationType.Commercial => TenderStatus.UnderCommercialEvaluation,
            _ => TenderStatus.UnderFinalReview
        };
    }

    public void SelectWinner(Guid bidId, Guid userId, string? reason, string? notes)
    {
        EnsureNotFinal();
        if (_decisions.Any(x => x.DecisionType == TenderDecisionType.SelectWinner))
            throw new InvalidOperationException("Only one selected winner is allowed per tender.");
        var bid = _bids.SingleOrDefault(x => x.Id == bidId) ?? throw new InvalidOperationException("Tender bid was not found.");
        foreach (var existing in _bids.Where(x => x.Status == TenderBidStatus.Selected))
            existing.Unselect();
        bid.Select();
        _decisions.Add(new TenderDecision(Guid.NewGuid(), Id, TenderDecisionType.SelectWinner, userId, bid.Id, bid.SupplierId, reason, notes));
        Status = TenderStatus.WinnerSelected;
    }

    public void AddDocument(TenderDocument document)
    {
        if (document.TenderId != Id) throw new InvalidOperationException("Tender document belongs to another tender.");
        _documents.Add(document);
    }

    private void StartStage(TenderStageType stageType, Guid userId)
    {
        var stage = _stages.SingleOrDefault(x => x.StageType == stageType);
        stage?.Start(userId);
    }

    private void EnsureEditable()
    {
        EnsureNotFinal();
        if (Status is not TenderStatus.Draft and not TenderStatus.ReadyToPublish)
            throw new InvalidOperationException("Tender cannot be edited in the current status.");
    }

    private void EnsureNotFinal()
    {
        if (Status is TenderStatus.Closed or TenderStatus.Cancelled)
            throw new InvalidOperationException("Closed or cancelled tenders are read-only.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderItem : AuditableEntity<Guid>
{
    private TenderItem() : base(Guid.Empty)
    {
        MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty;
    }

    public TenderItem(Guid id, Guid tenderId, Guid? purchaseFileItemId, Guid mescItemId, string mescCode,
        string mescGeneralGroupCode, string generalDescription, string specificDescription, Guid unitOfMeasureId,
        decimal quantity, string? technicalDescription) : base(id)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        TenderId = tenderId;
        PurchaseFileItemId = purchaseFileItemId;
        MescItemId = mescItemId;
        MescCode = Required(mescCode, nameof(mescCode));
        MescGeneralGroupCode = Required(mescGeneralGroupCode, nameof(mescGeneralGroupCode));
        GeneralDescription = Required(generalDescription, nameof(generalDescription));
        SpecificDescription = Required(specificDescription, nameof(specificDescription));
        UnitOfMeasureId = unitOfMeasureId;
        Quantity = quantity;
        TechnicalDescription = Trim(technicalDescription);
    }

    public Guid TenderId { get; private set; }
    public Guid? PurchaseFileItemId { get; private set; }
    public Guid MescItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
    public string? TechnicalDescription { get; private set; }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderParticipant : AuditableEntity<Guid>
{
    private TenderParticipant() : base(Guid.Empty) { SupplierCode = SupplierName = string.Empty; }
    public TenderParticipant(Guid id, Guid tenderId, Guid supplierId, string supplierCode, string supplierName,
        Guid? contactId, string? contactName, string? contactEmail) : base(id)
    {
        TenderId = tenderId; SupplierId = supplierId; SupplierCode = Required(supplierCode, nameof(supplierCode));
        SupplierName = Required(supplierName, nameof(supplierName)); ContactId = contactId; ContactName = Trim(contactName);
        ContactEmail = Trim(contactEmail); Status = TenderParticipantStatus.Draft;
    }
    public Guid TenderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string SupplierCode { get; private set; }
    public string SupplierName { get; private set; }
    public Guid? ContactId { get; private set; }
    public string? ContactName { get; private set; }
    public string? ContactEmail { get; private set; }
    public TenderParticipantStatus Status { get; private set; }
    public DateTime? InvitedAt { get; private set; }
    public Guid? InvitedByUserId { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? DeclinedAt { get; private set; }
    public string? DeclineReason { get; private set; }
    public void Invite(Guid userId) { Status = TenderParticipantStatus.Invited; InvitedAt = DateTime.UtcNow; InvitedByUserId = userId; }
    public void MarkSubmitted() { Status = TenderParticipantStatus.Submitted; SubmittedAt = DateTime.UtcNow; }
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderStage : AuditableEntity<Guid>
{
    private TenderStage() : base(Guid.Empty) { }
    public TenderStage(Guid id, Guid tenderId, TenderStageType stageType) : base(id)
    { TenderId = tenderId; StageType = stageType; Status = TenderStageStatus.NotStarted; }
    public Guid TenderId { get; private set; }
    public TenderStageType StageType { get; private set; }
    public TenderStageStatus Status { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public Guid? StartedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public string? Notes { get; private set; }
    public void Start(Guid userId) { Status = TenderStageStatus.InProgress; StartedAt ??= DateTime.UtcNow; StartedByUserId ??= userId; }
    public void Complete(Guid userId, string? notes) { Status = TenderStageStatus.Completed; CompletedAt = DateTime.UtcNow; CompletedByUserId = userId; Notes = notes; }
}

public sealed class TenderBid : AuditableEntity<Guid>
{
    private readonly List<TenderBidItem> _items = [];
    private TenderBid() : base(Guid.Empty) { }
    public TenderBid(Guid id, Guid tenderId, Guid tenderParticipantId, Guid supplierId, string? bidNumber,
        string? currency, decimal? totalAmount, decimal? finalAmount, string? deliveryTerms, string? paymentTerms,
        DateTime? validUntil, string? notes, Guid receivedByUserId) : base(id)
    {
        TenderId = tenderId; TenderParticipantId = tenderParticipantId; SupplierId = supplierId; BidNumber = Trim(bidNumber);
        Currency = Trim(currency); TotalAmount = totalAmount; FinalAmount = finalAmount; DeliveryTerms = Trim(deliveryTerms);
        PaymentTerms = Trim(paymentTerms); ValidUntil = validUntil; Notes = Trim(notes); Status = TenderBidStatus.Received;
        ReceivedAt = DateTime.UtcNow; ReceivedByUserId = receivedByUserId;
    }
    public Guid TenderId { get; private set; }
    public Guid TenderParticipantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string? BidNumber { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public Guid? ReceivedByUserId { get; private set; }
    public TenderBidStatus Status { get; private set; }
    public decimal? TechnicalScore { get; private set; }
    public decimal? CommercialScore { get; private set; }
    public decimal? FinalScore { get; private set; }
    public string? Currency { get; private set; }
    public decimal? TotalAmount { get; private set; }
    public decimal? FinalAmount { get; private set; }
    public string? DeliveryTerms { get; private set; }
    public string? PaymentTerms { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<TenderBidItem> Items => _items.AsReadOnly();
    public void Update(string? bidNumber, string? currency, decimal? totalAmount, decimal? finalAmount, string? deliveryTerms, string? paymentTerms, DateTime? validUntil, string? notes)
    { BidNumber = Trim(bidNumber); Currency = Trim(currency); TotalAmount = totalAmount; FinalAmount = finalAmount; DeliveryTerms = Trim(deliveryTerms); PaymentTerms = Trim(paymentTerms); ValidUntil = validUntil; Notes = Trim(notes); }
    public void AddItem(TenderBidItem item) { if (item.TenderBidId != Id) throw new InvalidOperationException("Bid item belongs to another bid."); _items.Add(item); }
    public void ApplyEvaluation(TenderEvaluationType type, decimal? score)
    { if (type == TenderEvaluationType.Technical) TechnicalScore = score; if (type == TenderEvaluationType.Commercial) CommercialScore = score; FinalScore = new[] { TechnicalScore, CommercialScore }.Where(x => x.HasValue).Select(x => x!.Value).DefaultIfEmpty().Average(); }
    public void Select() => Status = TenderBidStatus.Selected;
    public void Unselect() { if (Status == TenderBidStatus.Selected) Status = TenderBidStatus.Accepted; }
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class TenderBidItem : AuditableEntity<Guid>
{
    private TenderBidItem() : base(Guid.Empty) { MescCode = MescGeneralGroupCode = GeneralDescription = SpecificDescription = string.Empty; }
    public TenderBidItem(Guid id, Guid tenderBidId, Guid tenderItemId, string mescCode, string mescGeneralGroupCode,
        string generalDescription, string specificDescription, decimal quantity, decimal? unitPrice,
        TechnicalComplianceStatus technicalComplianceStatus, string? technicalNote) : base(id)
    {
        TenderBidId = tenderBidId; TenderItemId = tenderItemId; MescCode = mescCode; MescGeneralGroupCode = mescGeneralGroupCode;
        GeneralDescription = generalDescription; SpecificDescription = specificDescription; Quantity = quantity; UnitPrice = unitPrice;
        TotalPrice = unitPrice.HasValue ? unitPrice.Value * quantity : null; TechnicalComplianceStatus = technicalComplianceStatus; TechnicalNote = technicalNote;
    }
    public Guid TenderBidId { get; private set; }
    public Guid TenderItemId { get; private set; }
    public string MescCode { get; private set; }
    public string MescGeneralGroupCode { get; private set; }
    public string GeneralDescription { get; private set; }
    public string SpecificDescription { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? TotalPrice { get; private set; }
    public TechnicalComplianceStatus TechnicalComplianceStatus { get; private set; }
    public string? TechnicalNote { get; private set; }
}

public sealed class TenderEvaluation : AuditableEntity<Guid>
{
    private TenderEvaluation() : base(Guid.Empty) { }
    public TenderEvaluation(Guid id, Guid tenderId, Guid tenderBidId, TenderEvaluationType evaluationType, Guid evaluatorUserId, decimal? score, TenderEvaluationResult result, string? notes) : base(id)
    { TenderId = tenderId; TenderBidId = tenderBidId; EvaluationType = evaluationType; EvaluatorUserId = evaluatorUserId; EvaluationDate = DateTime.UtcNow; Score = score; Result = result; Notes = notes; }
    public Guid TenderId { get; private set; }
    public Guid TenderBidId { get; private set; }
    public TenderEvaluationType EvaluationType { get; private set; }
    public Guid EvaluatorUserId { get; private set; }
    public DateTime EvaluationDate { get; private set; }
    public decimal? Score { get; private set; }
    public TenderEvaluationResult Result { get; private set; }
    public string? Notes { get; private set; }
}

public sealed class TenderDecision : AuditableEntity<Guid>
{
    private TenderDecision() : base(Guid.Empty) { }
    public TenderDecision(Guid id, Guid tenderId, TenderDecisionType decisionType, Guid decidedByUserId, Guid? selectedTenderBidId, Guid? selectedSupplierId, string? reason, string? notes) : base(id)
    { TenderId = tenderId; DecisionType = decisionType; DecisionDate = DateTime.UtcNow; DecidedByUserId = decidedByUserId; SelectedTenderBidId = selectedTenderBidId; SelectedSupplierId = selectedSupplierId; Reason = reason; Notes = notes; }
    public Guid TenderId { get; private set; }
    public TenderDecisionType DecisionType { get; private set; }
    public DateTime DecisionDate { get; private set; }
    public Guid DecidedByUserId { get; private set; }
    public Guid? SelectedTenderBidId { get; private set; }
    public Guid? SelectedSupplierId { get; private set; }
    public string? Reason { get; private set; }
    public string? Notes { get; private set; }
}

public sealed class TenderDocument : AuditableEntity<Guid>
{
    private TenderDocument() : base(Guid.Empty) { DocumentType = string.Empty; }
    public TenderDocument(Guid id, Guid tenderId, Guid? fileDocumentId, string documentType, string? originalFileName, string? description, Guid uploadedByUserId) : base(id)
    { TenderId = tenderId; FileDocumentId = fileDocumentId; DocumentType = documentType; OriginalFileName = originalFileName; Description = description; UploadedAt = DateTime.UtcNow; UploadedByUserId = uploadedByUserId; }
    public Guid TenderId { get; private set; }
    public Guid? FileDocumentId { get; private set; }
    public string DocumentType { get; private set; }
    public string? OriginalFileName { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public Guid UploadedByUserId { get; private set; }
}

public sealed class TenderSequence : AuditableEntity<Guid>
{
    private TenderSequence() : base(Guid.Empty) { }
    public TenderSequence(Guid id, int year, int lastSequence) : base(id) { Year = year; LastSequence = lastSequence; }
    public int Year { get; private set; }
    public int LastSequence { get; private set; }
    public int Next() => ++LastSequence;
}
