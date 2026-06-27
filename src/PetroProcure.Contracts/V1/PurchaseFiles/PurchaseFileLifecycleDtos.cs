namespace PetroProcure.Contracts.V1.PurchaseFiles;

/// <summary>
/// One related business entity shown in the purchase-file lifecycle summary
/// (e.g. an inquiry, tender, contract, purchase order, or warehouse receipt).
/// Carries display-friendly values only — never raw GUIDs for the UI to show.
/// </summary>
public sealed record PurchaseFileLifecycleRelatedEntityDto(
    Guid Id,
    string Number,
    string? Title,
    string Status,
    DateTime? Date,
    string DetailRoute);

/// <summary>
/// A single stage of the procurement lifecycle (Indent, Inquiry, Tender, Commission,
/// Contract, Purchase Order, Warehouse Receipt, Documents, Legal/AI evaluations).
/// </summary>
public sealed record PurchaseFileLifecycleStepDto(
    string Stage,
    string Title,
    bool IsComplete,
    int Count,
    IReadOnlyList<PurchaseFileLifecycleRelatedEntityDto> Items);

/// <summary>
/// Lightweight read-model for <c>GET /api/purchase-files/{id}/lifecycle</c>. Aggregates the
/// related chain around a purchase file using existing per-entity "by purchase file" queries.
/// Intentionally summary-only (no heavy object graphs).
/// </summary>
public sealed record PurchaseFileLifecycleDto(
    Guid PurchaseFileId,
    string FileNumber,
    string Title,
    string Status,
    Guid? SourceIndentId,
    string? SourceIndentNumber,
    Guid? MaterialNeedId,
    Guid? ShortageAlertId,
    int DocumentsCount,
    int ReportsCount,
    DateTime? LatestActionDate,
    string CurrentStage,
    IReadOnlyList<PurchaseFileLifecycleStepDto> Steps);
