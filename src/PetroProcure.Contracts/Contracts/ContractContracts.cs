using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Contracts;

public sealed record PurchaseContractDto(
    Guid Id,
    string ContractNumber,
    Guid PurchaseFileId,
    string? PurchaseFileNumber,
    Guid SupplierId,
    string? SupplierName,
    Guid? TenderId,
    string? TenderNumber,
    Guid? TenderBidId,
    Guid? CommissionDecisionId,
    Guid? ContractTemplateId,
    string Title,
    string Subject,
    ContractStatus Status,
    ContractType ContractType,
    string Currency,
    decimal? TotalAmount,
    decimal? TaxAmount,
    decimal? FinalAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? DeliveryDeadline,
    string? PaymentTerms,
    string? DeliveryTerms,
    string? WarrantyTerms,
    string? PenaltyTerms,
    string? Description,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    DateTime? SubmittedAt,
    Guid? SubmittedByUserId,
    DateTime? ApprovedAt,
    Guid? ApprovedByUserId,
    DateTime? SignedAt,
    Guid? SignedByUserId,
    DateTime? CancelledAt,
    Guid? CancelledByUserId,
    string? CancellationReason);

public sealed record PurchaseContractSummaryDto(
    Guid Id,
    string ContractNumber,
    Guid PurchaseFileId,
    string PurchaseFileNumber,
    Guid SupplierId,
    string SupplierName,
    string Title,
    ContractType ContractType,
    ContractStatus Status,
    decimal? FinalAmount,
    string Currency,
    DateTime CreatedAt);

public sealed record PurchaseContractDetailDto(
    PurchaseContractDto Contract,
    IReadOnlyList<PurchaseContractItemDto> Items,
    IReadOnlyList<ContractClauseDto> Clauses,
    IReadOnlyList<ContractApprovalDto> Approvals,
    IReadOnlyList<ContractDocumentDto> Documents);

public sealed record PurchaseContractItemDto(
    Guid Id,
    Guid ContractId,
    Guid? PurchaseFileItemId,
    Guid? TenderBidItemId,
    Guid MescItemId,
    string MescCode,
    string MescGeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    Guid UnitOfMeasureId,
    decimal Quantity,
    decimal? UnitPrice,
    decimal? TotalPrice,
    DateTime? DeliveryDate,
    string? TechnicalDescription);

public sealed record ContractClauseDto(
    Guid Id,
    Guid ContractId,
    int OrderNo,
    string Title,
    string Body,
    ContractClauseType ClauseType,
    bool IsRequired,
    bool IsEditable,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    DateTime? UpdatedAt,
    Guid? UpdatedByUserId);

public sealed record ContractTemplateDto(
    Guid Id,
    string TemplateCode,
    string Title,
    string? Description,
    ContractType ContractType,
    bool IsActive,
    DateTime CreatedAt,
    Guid CreatedByUserId,
    IReadOnlyList<ContractTemplateClauseDto> Clauses);

public sealed record ContractTemplateClauseDto(
    Guid Id,
    Guid TemplateId,
    int OrderNo,
    string Title,
    string Body,
    ContractClauseType ClauseType,
    bool IsRequired,
    bool IsEditable);

public sealed record ContractApprovalDto(
    Guid Id,
    Guid ContractId,
    string ApprovalStep,
    Guid? DepartmentId,
    Guid? ApproverUserId,
    ContractApprovalStatus Status,
    string? Comment,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt);

public sealed record ContractDocumentDto(
    Guid Id,
    Guid ContractId,
    Guid? FileDocumentId,
    string DocumentType,
    string? OriginalFileName,
    string? Description,
    DateTime UploadedAt,
    Guid UploadedByUserId);

public sealed record ContractLookupDto(Guid Id, string ContractNumber, string Title, ContractStatus Status);

public sealed record ContractListRequest(
    string? SearchTerm = null,
    ContractStatus? Status = null,
    ContractType? ContractType = null,
    Guid? SupplierId = null,
    string? PurchaseFileNumber = null,
    string? TenderNumber = null,
    DateTime? CreatedDateFrom = null,
    DateTime? CreatedDateTo = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record CreateContractRequest(
    Guid PurchaseFileId,
    Guid SupplierId,
    Guid? TenderId,
    Guid? TenderBidId,
    Guid? CommissionDecisionId,
    Guid? ContractTemplateId,
    string Title,
    string Subject,
    ContractType ContractType,
    string Currency,
    decimal? TotalAmount,
    decimal? TaxAmount,
    decimal? FinalAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? DeliveryDeadline,
    string? PaymentTerms,
    string? DeliveryTerms,
    string? WarrantyTerms,
    string? PenaltyTerms,
    string? Description);

public sealed record CreateContractFromTenderRequest(Guid? SupplierId, Guid? TenderBidId, Guid? ContractTemplateId, string? Title, string? Subject);
public sealed record CreateContractFromTenderBidRequest(Guid? ContractTemplateId, string? Title, string? Subject);
public sealed record CreateContractFromPurchaseFileRequest(Guid SupplierId, Guid? ContractTemplateId, string Title, string Subject, ContractType ContractType = ContractType.DirectPurchase);

public sealed record UpdateContractRequest(
    string Title,
    string Subject,
    ContractType ContractType,
    string Currency,
    decimal? TotalAmount,
    decimal? TaxAmount,
    decimal? FinalAmount,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime? DeliveryDeadline,
    string? PaymentTerms,
    string? DeliveryTerms,
    string? WarrantyTerms,
    string? PenaltyTerms,
    string? Description);

public sealed record AddContractItemRequest(
    Guid? PurchaseFileItemId,
    Guid? TenderBidItemId,
    Guid MescItemId,
    string MescCode,
    string MescGeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    Guid UnitOfMeasureId,
    decimal Quantity,
    decimal? UnitPrice,
    DateTime? DeliveryDate,
    string? TechnicalDescription);

public sealed record RemoveContractItemRequest(Guid ItemId);

public sealed record AddContractClauseRequest(
    int OrderNo,
    string Title,
    string Body,
    ContractClauseType ClauseType,
    bool IsRequired,
    bool IsEditable);

public sealed record UpdateContractClauseRequest(
    int OrderNo,
    string Title,
    string Body,
    ContractClauseType ClauseType,
    bool IsRequired,
    bool IsEditable);

public sealed record RemoveContractClauseRequest(Guid ClauseId);
public sealed record ApplyContractTemplateRequest(Guid TemplateId);
public sealed record SubmitContractRequest(string? Comment = null);
public sealed record ApproveContractRequest(string? Comment = null);
public sealed record RejectContractRequest(string Comment);
public sealed record SignContractRequest(string? Comment = null);
public sealed record CancelContractRequest(string Reason);

public sealed record CreateContractTemplateRequest(
    string TemplateCode,
    string Title,
    string? Description,
    ContractType ContractType,
    IReadOnlyList<AddContractTemplateClauseRequest> Clauses);

public sealed record UpdateContractTemplateRequest(
    string Title,
    string? Description,
    ContractType ContractType,
    bool IsActive);

public sealed record AddContractTemplateClauseRequest(
    int OrderNo,
    string Title,
    string Body,
    ContractClauseType ClauseType,
    bool IsRequired,
    bool IsEditable);

public sealed record UpdateContractTemplateClauseRequest(
    int OrderNo,
    string Title,
    string Body,
    ContractClauseType ClauseType,
    bool IsRequired,
    bool IsEditable);

public sealed record ContractReportResultDto(Guid ContractId, string ContractNumber, string FileName);

public sealed record ContractGroupedItemsDto(string GeneralGroupCode, string GeneralDescription, IReadOnlyList<PurchaseContractItemDto> Items);

public static class ContractContractExtensions
{
    public static PagedResult<T> EmptyPage<T>(int pageNumber = 1, int pageSize = 20) => new([], pageNumber, pageSize, 0);
}
