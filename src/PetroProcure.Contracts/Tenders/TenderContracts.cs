using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Tenders;

public sealed record TenderDto(Guid Id, string TenderNumber, Guid PurchaseFileId, string? PurchaseFileNumber,
    Guid? SourceInquiryId, string? SourceInquiryNumber, string Title, string? Description, TenderType TenderType,
    TenderStatus Status, DateTime IssueDate, DateTime? SubmissionDeadline, DateTime? OpeningDate, DateTime CreatedAt,
    Guid CreatedByUserId, DateTime? PublishedAt, Guid? PublishedByUserId, DateTime? CancelledAt,
    Guid? CancelledByUserId, string? CancellationReason, DateTime? ClosedAt, Guid? ClosedByUserId);

public sealed record TenderSummaryDto(Guid Id, string TenderNumber, Guid PurchaseFileId, string? PurchaseFileNumber,
    string Title, TenderStatus Status, TenderType TenderType, int ItemCount, int ParticipantCount,
    DateTime? SubmissionDeadline, DateTime CreatedAt);

public sealed record TenderDetailDto(TenderDto Tender, IReadOnlyList<TenderItemDto> Items,
    IReadOnlyList<TenderParticipantDto> Participants, IReadOnlyList<TenderStageDto> Stages,
    IReadOnlyList<TenderBidDto> Bids, IReadOnlyList<TenderEvaluationDto> Evaluations,
    IReadOnlyList<TenderDecisionDto> Decisions, IReadOnlyList<TenderDocumentDto> Documents);

public sealed record TenderItemDto(Guid Id, Guid TenderId, Guid? PurchaseFileItemId, Guid MescItemId,
    string MescCode, string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription,
    Guid UnitOfMeasureId, decimal Quantity, string? TechnicalDescription);

public sealed record TenderParticipantDto(Guid Id, Guid TenderId, Guid SupplierId, string SupplierCode,
    string SupplierName, Guid? ContactId, string? ContactName, string? ContactEmail, TenderParticipantStatus Status,
    DateTime? InvitedAt, Guid? InvitedByUserId, DateTime? SubmittedAt, DateTime? DeclinedAt, string? DeclineReason);

public sealed record TenderStageDto(Guid Id, Guid TenderId, TenderStageType StageType, TenderStageStatus Status,
    DateTime? StartedAt, Guid? StartedByUserId, DateTime? CompletedAt, Guid? CompletedByUserId, string? Notes);

public sealed record TenderBidDto(Guid Id, Guid TenderId, Guid TenderParticipantId, Guid SupplierId,
    string? SupplierName, string? BidNumber, DateTime? SubmittedAt, DateTime? ReceivedAt, Guid? ReceivedByUserId,
    TenderBidStatus Status, decimal? TechnicalScore, decimal? CommercialScore, decimal? FinalScore, string? Currency,
    decimal? TotalAmount, decimal? FinalAmount, string? DeliveryTerms, string? PaymentTerms, DateTime? ValidUntil,
    string? Notes, IReadOnlyList<TenderBidItemDto> Items);

public sealed record TenderBidItemDto(Guid Id, Guid TenderBidId, Guid TenderItemId, string MescCode,
    string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription, decimal Quantity,
    decimal? UnitPrice, decimal? TotalPrice, TechnicalComplianceStatus TechnicalComplianceStatus, string? TechnicalNote);

public sealed record TenderEvaluationDto(Guid Id, Guid TenderId, Guid TenderBidId, TenderEvaluationType EvaluationType,
    Guid EvaluatorUserId, DateTime EvaluationDate, decimal? Score, TenderEvaluationResult Result, string? Notes);

public sealed record TenderDecisionDto(Guid Id, Guid TenderId, TenderDecisionType DecisionType, DateTime DecisionDate,
    Guid DecidedByUserId, Guid? SelectedTenderBidId, Guid? SelectedSupplierId, string? Reason, string? Notes);

public sealed record TenderDocumentDto(Guid Id, Guid TenderId, Guid? FileDocumentId, string DocumentType,
    string? OriginalFileName, string? Description, DateTime UploadedAt, Guid UploadedByUserId);

public sealed record TenderItemsGroupedDto(string MescGeneralGroupCode, string GeneralDescription,
    IReadOnlyList<TenderItemDto> Items);

public sealed record TenderComparisonDto(Guid TenderId, string TenderNumber,
    IReadOnlyList<TenderComparisonSupplierDto> Suppliers, IReadOnlyList<TenderComparisonItemDto> Items);

public sealed record TenderComparisonSupplierDto(Guid SupplierId, string SupplierCode, string SupplierName,
    Guid TenderBidId, string? Currency, decimal? TotalAmount, decimal? FinalAmount, string? DeliveryTerms,
    string? PaymentTerms, decimal? TechnicalScore, decimal? CommercialScore, decimal? FinalScore, bool IsSelected);

public sealed record TenderComparisonItemDto(Guid TenderItemId, string MescGeneralGroupCode, string GeneralDescription,
    string MescCode, string SpecificDescription, IReadOnlyList<TenderBidItemDto> SupplierItems);

public sealed record TenderLookupDto(Guid Id, string TenderNumber, string Title, TenderStatus Status);

public sealed record TenderListRequest(string? SearchTerm = null, string? TenderNumber = null,
    string? PurchaseFileNumber = null, TenderStatus? Status = null, TenderType? TenderType = null,
    Guid? SupplierId = null, DateTime? CreatedDateFrom = null, DateTime? CreatedDateTo = null,
    DateTime? SubmissionDeadlineFrom = null, DateTime? SubmissionDeadlineTo = null, string? SortBy = "CreatedAt",
    bool SortDescending = true, int PageNumber = 1, int PageSize = 20);

public sealed record CreateTenderRequest(Guid PurchaseFileId, string Title, TenderType TenderType,
    DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description);
public sealed record CreateTenderFromPurchaseFileRequest(string Title, TenderType TenderType,
    DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description, Guid[] PurchaseFileItemIds, Guid[] SupplierIds);
public sealed record CreateTenderFromInquiryRequest(string Title, TenderType TenderType,
    DateTime? SubmissionDeadline, DateTime? OpeningDate, string? Description, Guid[] InquirySupplierIds);
public sealed record UpdateTenderRequest(string Title, TenderType TenderType, DateTime? SubmissionDeadline,
    DateTime? OpeningDate, string? Description);
public sealed record AddTenderItemRequest(Guid PurchaseFileItemId);
public sealed record RemoveTenderItemRequest(Guid ItemId);
public sealed record AddTenderParticipantRequest(Guid SupplierId, Guid? ContactId = null);
public sealed record RemoveTenderParticipantRequest(Guid ParticipantId);
public sealed record PublishTenderRequest;
public sealed record CancelTenderRequest(string Reason);
public sealed record AddTenderBidRequest(Guid TenderParticipantId, string? BidNumber, string? Currency,
    decimal? TotalAmount, decimal? FinalAmount, string? DeliveryTerms, string? PaymentTerms, DateTime? ValidUntil, string? Notes);
public sealed record UpdateTenderBidRequest(string? BidNumber, string? Currency, decimal? TotalAmount,
    decimal? FinalAmount, string? DeliveryTerms, string? PaymentTerms, DateTime? ValidUntil, string? Notes);
public sealed record AddTenderBidItemRequest(Guid TenderItemId, decimal Quantity, decimal? UnitPrice,
    TechnicalComplianceStatus TechnicalComplianceStatus, string? TechnicalNote);
public sealed record AddTenderEvaluationRequest(Guid TenderBidId, TenderEvaluationType EvaluationType,
    decimal? Score, TenderEvaluationResult Result, string? Notes);
public sealed record SelectTenderWinnerRequest(Guid TenderBidId, string? Reason, string? Notes);
public sealed record CloseTenderRequest;
