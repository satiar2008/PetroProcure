using PetroProcure.Domain.Enums;

namespace PetroProcure.Application.PurchaseFiles;

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
    IReadOnlyList<PurchaseFileItemDto> Items, IReadOnlyList<PurchaseFileStatusHistoryDto> StatusHistory,
    IReadOnlyList<PurchaseFileNoteDto> Notes);

public sealed record PurchaseFileListDto(
    Guid Id, string FileNumber, string Title, PurchaseFileStatus Status,
    PurchaseFilePriority Priority, Guid CurrentDepartmentId, Guid? ResponsibleUserId,
    DateTime CreatedAt, int ItemCount, string? SourceIndentNumber = null);

public sealed record PurchaseFileItemsGroupedDto(
    string MescGeneralGroupCode, string GeneralDescription, IReadOnlyList<PurchaseFileItemDto> Items);

public sealed record PurchaseFileTimelineDto(
    IReadOnlyList<PurchaseFileStatusHistoryDto> StatusChanges,
    IReadOnlyList<PurchaseFileNoteDto> Notes);

public sealed record PurchaseFileMescSnapshot(
    Guid Id, string Code, string GeneralGroupCode, string GeneralDescription,
    string SpecificDescription, bool IsActive);
