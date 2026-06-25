using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Warehouse;

public sealed record WarehouseDto(Guid Id, string Code, string Name, string? Location, bool IsActive, string? Description);
public sealed record WarehouseLookupDto(Guid Id, string Code, string Name);
public sealed record WarehouseReceiptLookupDto(Guid Id, string ReceiptNumber, WarehouseReceiptStatus Status);

public sealed record WarehouseReceiptDto(Guid Id, string ReceiptNumber, Guid PurchaseOrderId, string? PurchaseOrderNumber,
    Guid PurchaseFileId, string? PurchaseFileNumber, Guid WarehouseId, string? WarehouseName, Guid SupplierId,
    string? SupplierName, WarehouseReceiptStatus Status, DateTime ReceiptDate, string? DeliveryNoteNumber,
    string? CarrierName, string? VehicleNumber, Guid ReceivedByUserId, string? Description, DateTime CreatedAt,
    Guid CreatedByUserId, DateTime? ApprovedAt, Guid? ApprovedByUserId, DateTime? CancelledAt,
    Guid? CancelledByUserId, string? CancellationReason);

public sealed record WarehouseReceiptSummaryDto(Guid Id, string ReceiptNumber, Guid PurchaseOrderId,
    string PurchaseOrderNumber, Guid PurchaseFileId, string PurchaseFileNumber, Guid WarehouseId,
    string WarehouseName, Guid SupplierId, string SupplierName, WarehouseReceiptStatus Status,
    DateTime ReceiptDate, DateTime CreatedAt);

public sealed record WarehouseReceiptDetailDto(WarehouseReceiptDto Receipt,
    IReadOnlyList<WarehouseReceiptItemDto> Items,
    IReadOnlyList<WarehouseReceiptDocumentDto> Documents,
    IReadOnlyList<InventoryTransactionDto> InventoryTransactions);

public sealed record WarehouseReceiptItemDto(Guid Id, Guid WarehouseReceiptId, Guid PurchaseOrderItemId,
    Guid MescItemId, string MescCode, string MescGeneralGroupCode, string GeneralDescription,
    string SpecificDescription, Guid UnitOfMeasureId, decimal OrderedQuantity, decimal PreviouslyReceivedQuantity,
    decimal ReceivedQuantity, decimal? AcceptedQuantity, decimal? RejectedQuantity,
    decimal RemainingQuantityAfterReceipt, string? BatchNumber, string? SerialNumber, DateTime? ExpiryDate,
    WarehouseReceiptQualityStatus QualityStatus, string? Notes);

public sealed record WarehouseReceiptDocumentDto(Guid Id, Guid WarehouseReceiptId, Guid? FileDocumentId,
    string DocumentType, string? OriginalFileName, string? Description, DateTime UploadedAt, Guid UploadedByUserId);

public sealed record InventoryTransactionDto(Guid Id, string TransactionNumber, Guid MescItemId, string MescCode,
    string SpecificDescription, Guid WarehouseId, string WarehouseName, InventoryTransactionType TransactionType,
    InventoryTransactionReferenceType ReferenceType, Guid ReferenceId, decimal Quantity, Guid UnitOfMeasureId,
    DateTime TransactionDate, DateTime CreatedAt, Guid CreatedByUserId, string? Description);

public sealed record StockBalanceDto(Guid Id, Guid MescItemId, Guid? WarehouseId, string MescCode,
    string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription, string? WarehouseName,
    decimal AvailableQuantity, decimal ReservedQuantity, decimal OnOrderQuantity, DateTime LastUpdatedAt);

public sealed record WarehouseListRequest(string? SearchTerm = null, bool IncludeInactive = false,
    int PageNumber = 1, int PageSize = 20);
public sealed record CreateWarehouseRequest(string Code, string Name, string? Location, string? Description);
public sealed record UpdateWarehouseRequest(string Name, string? Location, bool IsActive, string? Description);

public sealed record WarehouseReceiptListRequest(string? SearchTerm = null, WarehouseReceiptStatus? Status = null,
    string? ReceiptNumber = null, string? PurchaseOrderNumber = null, string? PurchaseFileNumber = null,
    Guid? SupplierId = null, Guid? WarehouseId = null, DateTime? ReceiptDateFrom = null,
    DateTime? ReceiptDateTo = null, string? SortBy = "CreatedAt", bool SortDescending = true,
    int PageNumber = 1, int PageSize = 20);

public sealed record CreateWarehouseReceiptRequest(Guid PurchaseOrderId, Guid WarehouseId, DateTime ReceiptDate,
    string? DeliveryNoteNumber, string? CarrierName, string? VehicleNumber, string? Description);

public sealed record CreateWarehouseReceiptFromPurchaseOrderRequest(Guid WarehouseId, DateTime ReceiptDate,
    string? DeliveryNoteNumber, string? CarrierName, string? VehicleNumber, string? Description,
    IReadOnlyList<AddWarehouseReceiptItemRequest>? Items = null);

public sealed record UpdateWarehouseReceiptRequest(DateTime ReceiptDate, string? DeliveryNoteNumber,
    string? CarrierName, string? VehicleNumber, string? Description);

public sealed record AddWarehouseReceiptItemRequest(Guid PurchaseOrderItemId, decimal ReceivedQuantity,
    decimal? AcceptedQuantity = null, decimal? RejectedQuantity = null,
    WarehouseReceiptQualityStatus QualityStatus = WarehouseReceiptQualityStatus.NotInspected,
    string? BatchNumber = null, string? SerialNumber = null, DateTime? ExpiryDate = null, string? Notes = null);

public sealed record RemoveWarehouseReceiptItemRequest(Guid ItemId);
public sealed record SubmitWarehouseReceiptRequest(string? Comment);
public sealed record ApproveWarehouseReceiptRequest(string? Comment);
public sealed record CancelWarehouseReceiptRequest(string Reason);

public sealed record InventoryTransactionListRequest(string? SearchTerm = null, Guid? WarehouseId = null,
    InventoryTransactionType? TransactionType = null, DateTime? DateFrom = null, DateTime? DateTo = null,
    int PageNumber = 1, int PageSize = 20);

public sealed record StockBalanceListRequest(string? SearchTerm = null, Guid? WarehouseId = null,
    bool LowStockOnly = false, string? MescGeneralGroupCode = null, int PageNumber = 1, int PageSize = 20);
