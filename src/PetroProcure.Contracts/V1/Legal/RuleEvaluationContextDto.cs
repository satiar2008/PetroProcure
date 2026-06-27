namespace PetroProcure.Contracts.V1.Legal;

/// <summary>
/// Flat, serializable view of the procurement rule-evaluation context for API/UI inspection
/// (AI-RAG-01). Mirrors the application-layer RuleEvaluationContext. Collections are never null.
/// </summary>
public sealed record RuleEvaluationContextDto(
    string EntityType,
    Guid EntityId,
    Guid? PurchaseFileId,
    Guid? TenderId,
    string? FileNumber,
    string? Status,
    Guid? CurrentDepartmentId,
    Guid? RequestingDepartmentId,
    Guid? ApplicantDepartmentId,
    decimal? EstimatedAmount,
    decimal? FinalAmount,
    string? Currency,
    int ItemCount,
    decimal TotalRequestedQuantity,
    bool HasTender,
    string? TenderType,
    int SupplierCount,
    int OfferCount,
    IReadOnlyList<string> DocumentTypes,
    int ExistingDocumentCount,
    DateTime? CreatedAt,
    DateTime? InquiryDeadline,
    DateTime? TenderDeadline,
    DateTime? TechnicalReviewDeadline,
    IReadOnlyList<string> ApprovalStatuses,
    IReadOnlyList<string> WorkflowStatuses,
    IReadOnlyList<string> LegalReferences,
    IReadOnlyList<Guid> UserDepartmentIds);
