using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.PurchaseFiles;

public sealed record PurchaseFileItemDto(
    Guid Id, Guid MescItemId, string MescCode, string MescGeneralGroupCode,
    string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId,
    decimal RequestedQuantity, decimal ApprovedQuantity, string? TechnicalDescription,
    Guid? SourceIndentItemId);
public sealed record PurchaseFileStatusHistoryDto(
    Guid Id, PurchaseFileStatus FromStatus, PurchaseFileStatus ToStatus,
    Guid ChangedByUserId, DateTime ChangedAt, string? Reason, Guid? DepartmentId);
public sealed record PurchaseFileNoteDto(
    Guid Id, Guid DepartmentId, Guid UserId, string NoteText, DateTime CreatedAt, bool IsInternal);
public sealed record PurchaseFileDto(
    Guid Id, string FileNumber, string Title, string? Description, PurchaseFileStatus Status,
    PurchaseFilePriority Priority, Guid? SourceIndentId, Guid PurchaseDepartmentId,
    Guid CurrentDepartmentId, Guid? ResponsibleUserId, Guid CreatedByUserId,
    DateTime CreatedAt, DateTime? CompletedAt, DateTime? ArchivedAt,
    IReadOnlyList<PurchaseFileItemDto> Items,
    IReadOnlyList<PurchaseFileStatusHistoryDto> StatusHistory,
    IReadOnlyList<PurchaseFileNoteDto> Notes);
public sealed record PurchaseFileSummaryDto(
    Guid Id, string FileNumber, string Title, PurchaseFileStatus Status,
    PurchaseFilePriority Priority, Guid CurrentDepartmentId, Guid? ResponsibleUserId,
    DateTime CreatedAt, int ItemCount, string? SourceIndentNumber);
public sealed record PurchaseFileDetailDto(
    PurchaseFileDto PurchaseFile, IReadOnlyList<PurchaseFileGroupedItemsDto> ItemGroups);
public sealed record PurchaseFileGroupedItemsDto(
    string MescGeneralGroupCode, string GeneralDescription, IReadOnlyList<PurchaseFileItemDto> Items);
public sealed record PurchaseFileTimelineDto(
    IReadOnlyList<PurchaseFileStatusHistoryDto> StatusChanges,
    IReadOnlyList<PurchaseFileNoteDto> Notes);
public sealed record CreatePurchaseFileRequest(
    int Year, string Title, string? Description, PurchaseFilePriority Priority,
    Guid PurchaseDepartmentId, Guid CurrentDepartmentId, Guid? ResponsibleUserId);
public sealed record CreatePurchaseFileFromIndentRequest(
    int Year, Guid PurchaseDepartmentId, Guid? ResponsibleUserId, PurchaseFilePriority Priority);
public sealed record AddPurchaseFileItemRequest(
    Guid MescItemId, Guid UnitOfMeasureId, decimal RequestedQuantity,
    decimal ApprovedQuantity, string? TechnicalDescription);
public sealed record ChangePurchaseFileStatusRequest(
    PurchaseFileStatus Status, string? Reason, Guid? DepartmentId);
public sealed record AssignPurchaseFileToDepartmentRequest(
    Guid DepartmentId, Guid? ResponsibleUserId, string? Reason);
public sealed record AddPurchaseFileNoteRequest(Guid DepartmentId, string NoteText, bool IsInternal);
public sealed record PurchaseFileLifecycleRequest(string? Reason);
public sealed record PurchaseFileListRequest(
    string? FileNumber = null, string? Title = null, PurchaseFileStatus? Status = null,
    Guid? CurrentDepartmentId = null, DateTime? CreatedDateFrom = null, DateTime? CreatedDateTo = null,
    string? SourceIndentNumber = null, string SortBy = "CreatedAt", bool SortDescending = true,
    int PageNumber = 1, int PageSize = 25);

public sealed record RequestTechnicalReviewRequest(Guid? DepartmentId, string? Comment, DateOnly? DueDate);
public sealed record SubmitTechnicalReviewRequest(
    PurchaseFileTechnicalReviewDecision Decision,
    string Comments,
    string? RecommendationNotes);
public sealed record TechnicalReviewClarificationRequest(string Comments, string? RecommendationNotes);
public sealed record CancelTechnicalReviewRequest(string? Comments);
public sealed record PurchaseFileTechnicalReviewDto(
    Guid Id,
    Guid PurchaseFileId,
    string PurchaseFileNumber,
    string PurchaseFileTitle,
    Guid DepartmentId,
    string DepartmentName,
    Guid RequestedByUserId,
    Guid? ReviewedByUserId,
    PurchaseFileTechnicalReviewStatus Status,
    PurchaseFileTechnicalReviewDecision? Decision,
    string? RequestComment,
    string? Comments,
    string? RecommendationNotes,
    DateTime RequestedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<PurchaseFileItemDto> Items);
public sealed record ApplicantDashboardDto(
    int PendingTechnicalReviews,
    int InProgressTechnicalReviews,
    int ClarificationRequests,
    int CompletedThisMonth,
    double? AverageResponseHours,
    IReadOnlyList<PurchaseFileTechnicalReviewDto> RecentReviews);
public sealed record DepartmentDashboardDto(
    string DepartmentKey,
    string Title,
    int InboxTasks,
    int PendingActions,
    int OverdueTasks,
    IReadOnlyDictionary<string, int> StatusCounts,
    IReadOnlyList<PurchaseFileSummaryDto> RecentPurchaseFiles);
