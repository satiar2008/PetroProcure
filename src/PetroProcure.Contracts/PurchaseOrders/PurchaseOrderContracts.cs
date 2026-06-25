using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.PurchaseOrders;

public sealed record PurchaseOrderDto(
    Guid Id,
    string PurchaseOrderNumber,
    Guid PurchaseFileId,
    string? PurchaseFileNumber,
    Guid SupplierId,
    string? SupplierName,
    Guid? ContractId,
    string? ContractNumber,
    Guid? TenderId,
    string? TenderNumber,
    Guid? TenderBidId,
    string Title,
    string? Description,
    PurchaseOrderStatus Status,
    PurchaseOrderType PurchaseOrderType,
    string Currency,
    decimal? TotalAmount,
    decimal? TaxAmount,
    decimal? DiscountAmount,
    decimal? FinalAmount,
    DateTime? OrderDate,
    DateTime? ExpectedDeliveryDate,
    string? DeliveryLocation,
    string? DeliveryTerms,
    string? PaymentTerms,
    string? WarrantyTerms,
    string? Notes,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    DateTime? SubmittedAt,
    Guid? SubmittedByUserId,
    DateTime? ApprovedAt,
    Guid? ApprovedByUserId,
    DateTime? IssuedAt,
    Guid? IssuedByUserId,
    DateTime? CompletedAt,
    Guid? CompletedByUserId,
    DateTime? CancelledAt,
    Guid? CancelledByUserId,
    string? CancellationReason);

public sealed record PurchaseOrderSummaryDto(
    Guid Id,
    string PurchaseOrderNumber,
    Guid PurchaseFileId,
    string PurchaseFileNumber,
    Guid SupplierId,
    string SupplierName,
    Guid? ContractId,
    string? ContractNumber,
    string Title,
    PurchaseOrderType PurchaseOrderType,
    PurchaseOrderStatus Status,
    decimal? FinalAmount,
    string Currency,
    DateTime? OrderDate,
    DateTime? ExpectedDeliveryDate,
    DateTime CreatedAt);

public sealed record PurchaseOrderDetailDto(
    PurchaseOrderDto PurchaseOrder,
    IReadOnlyList<PurchaseOrderItemDto> Items,
    IReadOnlyList<PurchaseOrderApprovalDto> Approvals,
    IReadOnlyList<PurchaseOrderDocumentDto> Documents);

public sealed record PurchaseOrderItemDto(
    Guid Id,
    Guid PurchaseOrderId,
    Guid? PurchaseFileItemId,
    Guid? ContractItemId,
    Guid? TenderBidItemId,
    Guid MescItemId,
    string MescCode,
    string MescGeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    Guid UnitOfMeasureId,
    decimal OrderedQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    decimal? UnitPrice,
    decimal? TotalPrice,
    DateTime? ExpectedDeliveryDate,
    string? TechnicalDescription,
    string? Notes);

public sealed record PurchaseOrderApprovalDto(
    Guid Id,
    Guid PurchaseOrderId,
    string ApprovalStep,
    Guid? DepartmentId,
    Guid? ApproverUserId,
    PurchaseOrderApprovalStatus Status,
    string? Comment,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt);

public sealed record PurchaseOrderDocumentDto(
    Guid Id,
    Guid PurchaseOrderId,
    Guid? FileDocumentId,
    string DocumentType,
    string? OriginalFileName,
    string? Description,
    DateTime UploadedAt,
    Guid UploadedByUserId);

public sealed record PurchaseOrderLookupDto(Guid Id, string PurchaseOrderNumber, string Title, PurchaseOrderStatus Status);

public sealed record PurchaseOrderListRequest(
    string? SearchTerm = null,
    PurchaseOrderStatus? Status = null,
    PurchaseOrderType? PurchaseOrderType = null,
    Guid? SupplierId = null,
    string? ContractNumber = null,
    string? PurchaseFileNumber = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record CreatePurchaseOrderRequest(
    Guid PurchaseFileId,
    Guid SupplierId,
    Guid? ContractId,
    Guid? TenderId,
    Guid? TenderBidId,
    string Title,
    PurchaseOrderType PurchaseOrderType,
    string Currency,
    decimal? TotalAmount,
    decimal? TaxAmount,
    decimal? DiscountAmount,
    decimal? FinalAmount,
    DateTime? OrderDate,
    DateTime? ExpectedDeliveryDate,
    string? DeliveryLocation,
    string? DeliveryTerms,
    string? PaymentTerms,
    string? WarrantyTerms,
    string? Notes,
    string? Description);

public sealed record CreatePurchaseOrderFromContractRequest(
    string? Title = null,
    DateTime? OrderDate = null,
    DateTime? ExpectedDeliveryDate = null,
    string? DeliveryLocation = null,
    string? Notes = null);

public sealed record CreatePurchaseOrderFromPurchaseFileRequest(
    Guid SupplierId,
    string Title,
    PurchaseOrderType PurchaseOrderType = PurchaseOrderType.DirectPurchase,
    string Currency = "IRR",
    DateTime? OrderDate = null,
    DateTime? ExpectedDeliveryDate = null,
    string? DeliveryLocation = null,
    string? DeliveryTerms = null,
    string? PaymentTerms = null,
    string? WarrantyTerms = null,
    string? Notes = null,
    IReadOnlyList<AddPurchaseOrderItemRequest>? Items = null);

public sealed record UpdatePurchaseOrderRequest(
    string Title,
    string? Description,
    PurchaseOrderType PurchaseOrderType,
    string Currency,
    decimal? TotalAmount,
    decimal? TaxAmount,
    decimal? DiscountAmount,
    decimal? FinalAmount,
    DateTime? OrderDate,
    DateTime? ExpectedDeliveryDate,
    string? DeliveryLocation,
    string? DeliveryTerms,
    string? PaymentTerms,
    string? WarrantyTerms,
    string? Notes);

public sealed record AddPurchaseOrderItemRequest(
    Guid? PurchaseFileItemId,
    Guid? ContractItemId,
    Guid? TenderBidItemId,
    Guid MescItemId,
    string MescCode,
    string MescGeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    Guid UnitOfMeasureId,
    decimal OrderedQuantity,
    decimal? UnitPrice,
    DateTime? ExpectedDeliveryDate,
    string? TechnicalDescription,
    string? Notes);

public sealed record RemovePurchaseOrderItemRequest(Guid ItemId);
public sealed record SubmitPurchaseOrderRequest(string? Comment);
public sealed record ApprovePurchaseOrderRequest(string? Comment);
public sealed record RejectPurchaseOrderRequest(string Comment);
public sealed record IssuePurchaseOrderRequest(string? Comment);
public sealed record CancelPurchaseOrderRequest(string Reason);
public sealed record UpdatePurchaseOrderReceivedQuantityRequest(Guid ItemId, decimal ReceivedQuantity);
