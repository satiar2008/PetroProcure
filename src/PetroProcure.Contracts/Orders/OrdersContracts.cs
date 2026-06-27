using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Orders;

public sealed record InventoryControlItemDto(Guid Id, Guid MescItemId, string MescCode, string MescGeneralGroupCode,
    string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId, decimal MinimumStockLevel,
    decimal ReorderPoint, decimal? MaximumStockLevel, decimal? SafetyStock, bool IsStockControlled, bool IsActive,
    string? Notes, decimal CurrentStock, decimal OnOrderQuantity, DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record StockBalanceDto(Guid Id, Guid MescItemId, Guid? WarehouseId, string MescCode,
    string GeneralDescription, decimal AvailableQuantity, decimal ReservedQuantity, decimal OnOrderQuantity,
    DateTime LastUpdatedAt);

public sealed record MaterialNeedDto(Guid Id, string NeedNumber, Guid MescItemId, string MescCode,
    string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId,
    decimal RequestedQuantity, DateOnly? NeededByDate, Guid SourceDepartmentId, Guid? ApplicantDepartmentId,
    Guid RequestedByUserId, MaterialNeedStatus Status, MaterialNeedPriority Priority, string? Description,
    DateTime CreatedAt, DateTime? SubmittedAt, DateTime? ReviewedAt, Guid? ReviewedByUserId,
    Guid? RelatedIndentId, string? RejectionReason);

public sealed record MaterialNeedDetailDto(MaterialNeedDto Need);

public sealed record ShortageAlertDto(Guid Id, Guid MescItemId, string MescCode, string MescGeneralGroupCode,
    string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId, decimal CurrentStock,
    decimal ReorderPoint, decimal ShortageQuantity, ShortageAlertStatus Status, DateTime DetectedAt,
    DateTime? ResolvedAt, Guid? RelatedIndentId, string? ResolutionNote);

public sealed record OrdersDashboardDto(long OpenMaterialNeeds, long ApprovedNeedsWaitingForIndent,
    long OpenShortageAlerts, long IndentsWaitingForApproval, long IndentsSentToPurchaseDepartment,
    IReadOnlyList<InventoryControlItemDto> CriticalItems, IReadOnlyList<MaterialNeedDto> RecentMaterialNeeds);

public sealed record MaterialNeedsGroupedByMescDto(string MescGeneralGroupCode, string GeneralDescription,
    IReadOnlyList<MaterialNeedDto> Items);

public sealed record InventoryControlListRequest(string? SearchTerm = null, bool IncludeInactive = false,
    int PageNumber = 1, int PageSize = 20);
public sealed record UpdateInventoryControlItemRequest(decimal MinimumStockLevel, decimal ReorderPoint,
    decimal? MaximumStockLevel, decimal? SafetyStock, bool IsStockControlled, bool IsActive, string? Notes);
public sealed record CreateStockAdjustmentRequest(Guid WarehouseId, decimal Quantity, string? Description);
public sealed record CreateInventoryControlItemRequest(Guid MescItemId, Guid WarehouseId, decimal InitialQuantity,
    decimal MinimumStockLevel, decimal ReorderPoint, decimal? MaximumStockLevel, decimal? SafetyStock,
    bool IsStockControlled, string? Notes);
public sealed record StockBalanceListRequest(string? SearchTerm = null, Guid? WarehouseId = null,
    int PageNumber = 1, int PageSize = 20);
public sealed record MaterialNeedListRequest(MaterialNeedStatus? Status = null, MaterialNeedPriority? Priority = null,
    string? MescCode = null, Guid? ApplicantDepartmentId = null, DateTime? CreatedDateFrom = null,
    DateTime? CreatedDateTo = null, int PageNumber = 1, int PageSize = 20);
public sealed record CreateMaterialNeedRequest(Guid MescItemId, decimal RequestedQuantity, DateOnly? NeededByDate,
    Guid SourceDepartmentId, Guid? ApplicantDepartmentId, MaterialNeedPriority Priority, string? Description);
public sealed record SubmitMaterialNeedRequest;
public sealed record ReviewMaterialNeedRequest(string? Comment = null);
public sealed record ApproveMaterialNeedRequest(string? Comment = null);
public sealed record RejectMaterialNeedRequest(string Reason);
public sealed record ConvertMaterialNeedToIndentRequest(int YearPart, int TypeDigit, string? Title = null);
public sealed record ConvertMaterialNeedsToIndentRequest(IReadOnlyList<Guid> MaterialNeedIds, int YearPart, int TypeDigit,
    string? Title = null);
public sealed record ShortageAlertListRequest(ShortageAlertStatus? Status = null, string? MescCode = null,
    int PageNumber = 1, int PageSize = 20);
public sealed record DetectShortageAlertsRequest(bool IncludeExistingOpen = false);
public sealed record ConvertShortageToIndentRequest(int YearPart, int TypeDigit, Guid RequestingDepartmentId,
    string? Title = null);
public sealed record ResolveShortageAlertRequest(string? ResolutionNote);
