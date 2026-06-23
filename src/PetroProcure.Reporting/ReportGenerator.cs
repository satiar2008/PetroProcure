using DevExpress.XtraReports.UI;
using PetroProcure.Application.Documents;
using PetroProcure.Domain.Enums;
using PetroProcure.Reporting.Reports;
using PetroProcure.Application.Security;

namespace PetroProcure.Reporting;

public sealed class ReportGenerator(
    IReportDataProvider dataProvider,
    IFileStorageService storage,
    ICurrentUserService currentUser) : IReportGenerator
{
    public async Task<byte[]> GeneratePdfAsync(string reportName, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        using var report = await CreateReportAsync(reportName, parameters, cancellationToken);
        await using var stream = new MemoryStream();
        await report.ExportToPdfAsync(stream, new DevExpress.XtraPrinting.PdfExportOptions(), cancellationToken);
        return stream.ToArray();
    }

    public Task<ReportPreviewModel> GeneratePreviewModelAsync(string reportName, IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        var (key, value) = parameters.First();
        var url = reportName switch
        {
            ReportNames.Indent => $"/api/reports/indent/{value}/pdf",
            _ => $"/api/reports/purchase-file-summary/{value}/pdf"
        };
        return Task.FromResult(new ReportPreviewModel(reportName, url, PersianTitle(reportName)));
    }

    public async Task<FileDocumentDto> SaveGeneratedReportToPurchaseFileAsync(Guid purchaseFileId, string reportName,
        IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        var bytes = await GeneratePdfAsync(reportName, parameters, cancellationToken);
        await using var stream = new MemoryStream(bytes);
        return await storage.SaveFileAsync(purchaseFileId, DocumentType.FinalReport,
            $"{reportName}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf", stream, currentUser.UserId,
            mimeType: "application/pdf", description: PersianTitle(reportName), cancellationToken: cancellationToken);
    }

    private async Task<XtraReport> CreateReportAsync(string name, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct)
    {
        return name switch
        {
            ReportNames.PurchaseFileSummary => new PurchaseFileSummaryReport(await RequiredPurchaseFile(parameters, ct)),
            ReportNames.PurchaseFileItemsGroupedByMesc => new PurchaseFileItemsGroupedByMescReport(await RequiredPurchaseFile(parameters, ct)),
            ReportNames.Indent => new IndentReport(await RequiredIndent(parameters, ct)),
            _ => throw new ArgumentException($"Unknown report '{name}'.", nameof(name))
        };
    }

    private async Task<PurchaseFileReportData> RequiredPurchaseFile(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "PurchaseFileId");
        return await dataProvider.GetPurchaseFileAsync(id, ct) ?? throw new InvalidOperationException("Purchase file was not found.");
    }
    private async Task<IndentReportData> RequiredIndent(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "IndentId");
        return await dataProvider.GetIndentAsync(id, ct) ?? throw new InvalidOperationException("Indent was not found.");
    }
    private static Guid RequiredGuid(IReadOnlyDictionary<string, object?> p, string key) =>
        p.TryGetValue(key, out var value) && value is Guid id ? id : throw new ArgumentException($"Parameter '{key}' is required.");
    private static string PersianTitle(string name) => name switch
    {
        ReportNames.PurchaseFileSummary => "خلاصه پرونده خرید",
        ReportNames.Indent => "گزارش درخواست خرید",
        ReportNames.PurchaseFileItemsGroupedByMesc => "اقلام پرونده به تفکیک گروه MESC",
        _ => name
    };
}
