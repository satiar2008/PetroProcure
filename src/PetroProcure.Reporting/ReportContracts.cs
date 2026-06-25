using PetroProcure.Application.Documents;

namespace PetroProcure.Reporting;

public static class ReportNames
{
    public const string PurchaseFileSummary = "PurchaseFileSummaryReport";
    public const string Indent = "IndentReport";
    public const string PurchaseFileItemsGroupedByMesc = "PurchaseFileItemsGroupedByMescReport";
    public const string TenderSummary = "TenderSummaryReport";
    public const string TenderComparison = "TenderComparisonReport";
    public const string TenderWinnerDecision = "TenderWinnerDecisionReport";
    public const string CommissionSessionMinutes = "CommissionSessionMinutesReport";
    public const string CommissionDecision = "CommissionDecisionReport";
    public const string Contract = "ContractReport";
    public const string PurchaseOrder = "PurchaseOrderReport";
}

public sealed record ReportPreviewModel(string ReportName, string PreviewUrl, string Title);

public interface IReportGenerator
{
    Task<byte[]> GeneratePdfAsync(string reportName, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default);
    Task<ReportPreviewModel> GeneratePreviewModelAsync(string reportName, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default);
    Task<FileDocumentDto> SaveGeneratedReportToPurchaseFileAsync(Guid purchaseFileId, string reportName,
        IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default);
}

public interface IReportDataProvider
{
    Task<PurchaseFileReportData?> GetPurchaseFileAsync(Guid id, CancellationToken cancellationToken);
    Task<IndentReportData?> GetIndentAsync(Guid id, CancellationToken cancellationToken);
    Task<TenderReportData?> GetTenderAsync(Guid id, CancellationToken cancellationToken);
    Task<TenderComparisonReportData?> GetTenderComparisonAsync(Guid id, CancellationToken cancellationToken);
    Task<TenderWinnerDecisionReportData?> GetTenderWinnerDecisionAsync(Guid id, CancellationToken cancellationToken);
    Task<CommissionSessionReportData?> GetCommissionSessionAsync(Guid id, CancellationToken cancellationToken);
    Task<CommissionDecisionReportData?> GetCommissionDecisionAsync(Guid sessionId, Guid decisionId, CancellationToken cancellationToken);
    Task<ContractReportData?> GetContractAsync(Guid id, CancellationToken cancellationToken);
    Task<PurchaseOrderReportData?> GetPurchaseOrderAsync(Guid id, CancellationToken cancellationToken);
}

public sealed record ReportItemData(string MescCode, string GeneralGroupCode, string GeneralDescription,
    string SpecificDescription, string Unit, decimal Quantity);
public sealed record ReportItemGroupData(string GeneralGroupCode, string GeneralDescription, IReadOnlyList<ReportItemData> Items);
public sealed record PurchaseFileReportData(Guid Id, string FileNumber, string Title, string Status,
    string CurrentDepartment, DateTime CreatedAt, string? IndentNumber, IReadOnlyList<ReportItemGroupData> Groups);
public sealed record IndentReportData(Guid Id, string IndentNumber, string IndentType,
    string RequestingDepartment, IReadOnlyList<ReportItemGroupData> Groups);

public sealed record TenderReportData(Guid Id, string TenderNumber, Guid PurchaseFileId, string PurchaseFileNumber,
    string PurchaseFileTitle, string? SourceInquiryNumber, string Title, string TenderType, string Status,
    string IssueDate, string SubmissionDeadline, string OpeningDate, string CreatedBy, string PublishedBy,
    IReadOnlyList<TenderParticipantReportData> Participants, IReadOnlyList<ReportItemGroupData> Groups);
public sealed record TenderParticipantReportData(string SupplierCode, string SupplierName, string Status, string InvitedAt);

public sealed record TenderComparisonReportData(Guid TenderId, string TenderNumber, Guid PurchaseFileId,
    string PurchaseFileNumber, string PurchaseFileTitle, IReadOnlyList<TenderComparisonSupplierReportData> Suppliers,
    IReadOnlyList<TenderComparisonItemGroupData> Groups, string SelectedWinner);
public sealed record TenderComparisonSupplierReportData(string SupplierName, string BidNumber, string TechnicalScore,
    string CommercialScore, string FinalScore, string TotalAmount, string FinalAmount, string DeliveryTerms,
    string PaymentTerms, bool IsSelected);
public sealed record TenderComparisonBidItemReportData(string SupplierName, string UnitPrice, string TotalPrice,
    string TechnicalComplianceStatus, string TechnicalNote);
public sealed record TenderComparisonItemReportData(string MescCode, string SpecificDescription, decimal Quantity,
    IReadOnlyList<TenderComparisonBidItemReportData> SupplierItems);
public sealed record TenderComparisonItemGroupData(string GeneralGroupCode, string GeneralDescription,
    IReadOnlyList<TenderComparisonItemReportData> Items);

public sealed record TenderWinnerDecisionReportData(Guid TenderId, string TenderNumber, Guid PurchaseFileId,
    string PurchaseFileNumber, string SelectedSupplier, string SelectedBid, string DecisionDate,
    string DecisionReason, string CommissionDecisionReference, string FinalAmount, string Notes);

public sealed record CommissionSessionReportData(Guid Id, string SessionNumber, Guid TenderId, string TenderNumber,
    Guid PurchaseFileId, string PurchaseFileNumber, string Title, string SessionDate, string Location, string Status,
    string Chairperson, string Secretary, IReadOnlyList<CommissionMemberReportData> Members,
    IReadOnlyList<CommissionAgendaReportData> AgendaItems, IReadOnlyList<CommissionMinuteReportData> Minutes,
    IReadOnlyList<CommissionDecisionReportData> Decisions);
public sealed record CommissionMemberReportData(string FullName, string Role, string AttendanceStatus, string VoteStatus);
public sealed record CommissionAgendaReportData(int OrderNo, string Title, string Description, string Status, string Notes);
public sealed record CommissionMinuteReportData(string AgendaTitle, string Text, string CreatedAt);
public sealed record CommissionDecisionReportData(Guid Id, string SessionNumber, string TenderNumber, string PurchaseFileNumber,
    string DecisionType, string Status, string SelectedSupplier, string SelectedBid, string DecisionText, string Reason,
    string CreatedBy, string ApprovedBy, string ApprovedAt);

public sealed record ContractReportData(Guid Id, string ContractNumber, string PurchaseFileNumber, string SupplierName,
    string TenderNumber, string CommissionDecisionReference, string Title, string Subject, string Status,
    string ContractType, string Currency, string TotalAmount, string TaxAmount, string FinalAmount,
    string StartDate, string EndDate, string DeliveryDeadline, string PaymentTerms, string DeliveryTerms,
    string WarrantyTerms, string PenaltyTerms, IReadOnlyList<ReportItemGroupData> Groups,
    IReadOnlyList<ContractClauseReportData> Clauses);

public sealed record ContractClauseReportData(int OrderNo, string Title, string Body, string ClauseType, bool IsRequired);

public sealed record PurchaseOrderReportData(Guid Id, string PurchaseOrderNumber, string PurchaseFileNumber,
    string ContractNumber, string SupplierName, string SupplierCode, string Title, string Status,
    string PurchaseOrderType, string Currency, string TotalAmount, string TaxAmount, string DiscountAmount,
    string FinalAmount, string OrderDate, string ExpectedDeliveryDate, string DeliveryLocation,
    string DeliveryTerms, string PaymentTerms, string WarrantyTerms, string Notes,
    IReadOnlyList<ReportItemGroupData> Groups);
