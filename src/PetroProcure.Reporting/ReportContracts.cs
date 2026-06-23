using PetroProcure.Application.Documents;

namespace PetroProcure.Reporting;

public static class ReportNames
{
    public const string PurchaseFileSummary = "PurchaseFileSummaryReport";
    public const string Indent = "IndentReport";
    public const string PurchaseFileItemsGroupedByMesc = "PurchaseFileItemsGroupedByMescReport";
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
}

public sealed record ReportItemData(string MescCode, string GeneralGroupCode, string GeneralDescription,
    string SpecificDescription, string Unit, decimal Quantity);
public sealed record ReportItemGroupData(string GeneralGroupCode, string GeneralDescription, IReadOnlyList<ReportItemData> Items);
public sealed record PurchaseFileReportData(Guid Id, string FileNumber, string Title, string Status,
    string CurrentDepartment, DateTime CreatedAt, string? IndentNumber, IReadOnlyList<ReportItemGroupData> Groups);
public sealed record IndentReportData(Guid Id, string IndentNumber, string IndentType,
    string RequestingDepartment, IReadOnlyList<ReportItemGroupData> Groups);
