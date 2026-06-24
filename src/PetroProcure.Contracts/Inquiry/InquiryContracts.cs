using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Inquiry;

public sealed record InquiryDto(Guid Id, string InquiryNumber, Guid PurchaseFileId, string Title, string? Description,
    InquiryStatus Status, InquiryType InquiryType, DateTime IssueDate, DateTime? DeadlineDate, DateTime CreatedAt,
    Guid CreatedByUserId, DateTime? SentAt, Guid? SentByUserId, DateTime? ClosedAt, Guid? ClosedByUserId,
    DateTime? CancelledAt, Guid? CancelledByUserId, string? CancellationReason);

public sealed record InquirySummaryDto(Guid Id, string InquiryNumber, Guid PurchaseFileId, string? PurchaseFileNumber,
    string Title, InquiryStatus Status, InquiryType InquiryType, int ItemCount, int SupplierCount,
    DateTime? DeadlineDate, DateTime CreatedAt);

public sealed record InquiryDetailDto(InquiryDto Inquiry, IReadOnlyList<InquiryItemDto> Items,
    IReadOnlyList<InquirySupplierDto> Suppliers, IReadOnlyList<SupplierQuoteDto> Quotes,
    IReadOnlyList<InquiryDocumentDto> Documents);

public sealed record InquiryItemDto(Guid Id, Guid InquiryId, Guid? PurchaseFileItemId, Guid MescItemId,
    string MescCode, string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription,
    Guid UnitOfMeasureId, decimal RequestedQuantity, string? TechnicalDescription);

public sealed record InquiryItemsGroupedDto(string MescGeneralGroupCode, string GeneralDescription,
    IReadOnlyList<InquiryItemDto> Items);

public sealed record InquirySupplierDto(Guid Id, Guid InquiryId, Guid SupplierId, string SupplierCode,
    string SupplierName, Guid? ContactId, string? ContactName, string? ContactEmail,
    InquirySupplierStatus Status, DateTime? InvitedAt, Guid? InvitedByUserId,
    DateTime? RespondedAt, DateTime? DeclinedAt, string? DeclineReason);

public sealed record SupplierQuoteDto(Guid Id, Guid InquiryId, Guid SupplierId, Guid InquirySupplierId,
    string? QuoteNumber, DateTime? QuoteDate, DateTime? ValidUntil, string Currency, string? DeliveryTerms,
    string? PaymentTerms, DateTime? DeliveryDate, decimal TotalAmount, decimal? TaxAmount,
    decimal? DiscountAmount, decimal FinalAmount, string? TechnicalNote, string? CommercialNote,
    SupplierQuoteStatus Status, DateTime ReceivedAt, Guid ReceivedByUserId, bool IsSelected,
    string? SelectionReason, IReadOnlyList<SupplierQuoteItemDto> Items);

public sealed record SupplierQuoteItemDto(Guid Id, Guid SupplierQuoteId, Guid InquiryItemId,
    string MescCode, string MescGeneralGroupCode, string GeneralDescription, string SpecificDescription,
    decimal Quantity, decimal UnitPrice, decimal TotalPrice, DateTime? DeliveryDate,
    TechnicalComplianceStatus TechnicalComplianceStatus, string? TechnicalNote);

public sealed record InquiryDocumentDto(Guid Id, Guid InquiryId, Guid? FileDocumentId, string DocumentType,
    string? OriginalFileName, string? Description, DateTime UploadedAt, Guid UploadedByUserId);

public sealed record InquiryComparisonDto(Guid InquiryId, string InquiryNumber,
    IReadOnlyList<InquiryComparisonSupplierDto> Suppliers, IReadOnlyList<InquiryComparisonItemDto> Items);

public sealed record InquiryComparisonSupplierDto(Guid SupplierId, string SupplierCode, string SupplierName,
    Guid QuoteId, DateTime? QuoteDate, DateTime? ValidUntil, string Currency, decimal TotalAmount,
    decimal FinalAmount, DateTime? DeliveryDate, string? PaymentTerms, string? DeliveryTerms,
    bool IsSelected, SupplierQuoteStatus Status);

public sealed record InquiryComparisonItemDto(Guid InquiryItemId, string MescGeneralGroupCode,
    string GeneralDescription, string MescCode, string SpecificDescription,
    IReadOnlyList<SupplierQuoteItemDto> QuoteItems);

public sealed record InquiryLookupDto(Guid Id, string InquiryNumber, string Title, InquiryStatus Status);

public sealed record InquiryListRequest(string? SearchTerm = null, string? InquiryNumber = null,
    string? PurchaseFileNumber = null, InquiryStatus? Status = null, InquiryType? InquiryType = null,
    Guid? SupplierId = null, DateTime? CreatedDateFrom = null, DateTime? CreatedDateTo = null,
    DateTime? DeadlineDateFrom = null, DateTime? DeadlineDateTo = null, string SortBy = "CreatedAt",
    bool SortDescending = true, int PageNumber = 1, int PageSize = 20);

public sealed record CreateInquiryRequest(Guid PurchaseFileId, string Title, InquiryType InquiryType,
    DateTime? DeadlineDate, string? Description);

public sealed record CreateInquiryFromPurchaseFileRequest(string Title, InquiryType InquiryType,
    DateTime? DeadlineDate, string? Description, Guid[] PurchaseFileItemIds, Guid[] SupplierIds);

public sealed record UpdateInquiryRequest(string Title, InquiryType InquiryType, DateTime? DeadlineDate, string? Description);
public sealed record AddInquiryItemRequest(Guid PurchaseFileItemId);
public sealed record RemoveInquiryItemRequest(Guid ItemId);
public sealed record AddInquirySupplierRequest(Guid SupplierId, Guid? ContactId = null);
public sealed record RemoveInquirySupplierRequest(Guid InquirySupplierId);
public sealed record SendInquiryRequest(string? Comment = null);
public sealed record CancelInquiryRequest(string Reason);
public sealed record AddSupplierQuoteRequest(Guid InquirySupplierId, string? QuoteNumber, DateTime? QuoteDate,
    DateTime? ValidUntil, string Currency, string? DeliveryTerms, string? PaymentTerms, DateTime? DeliveryDate,
    decimal TotalAmount, decimal? TaxAmount, decimal? DiscountAmount, string? TechnicalNote, string? CommercialNote);
public sealed record UpdateSupplierQuoteRequest(string? QuoteNumber, DateTime? QuoteDate, DateTime? ValidUntil,
    string Currency, string? DeliveryTerms, string? PaymentTerms, DateTime? DeliveryDate, decimal TotalAmount,
    decimal? TaxAmount, decimal? DiscountAmount, string? TechnicalNote, string? CommercialNote);
public sealed record AddSupplierQuoteItemRequest(Guid InquiryItemId, decimal Quantity, decimal UnitPrice,
    DateTime? DeliveryDate, TechnicalComplianceStatus TechnicalComplianceStatus, string? TechnicalNote);
public sealed record SelectSupplierQuoteRequest(string? Reason);
public sealed record RejectSupplierQuoteRequest(string? Reason);
