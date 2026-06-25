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
            ReportNames.TenderSummary => $"/api/tenders/{value}/reports/summary/pdf",
            ReportNames.TenderComparison => $"/api/tenders/{value}/reports/comparison/pdf",
            ReportNames.TenderWinnerDecision => $"/api/tenders/{value}/reports/winner-decision/pdf",
            ReportNames.CommissionSessionMinutes => $"/api/commission/sessions/{value}/reports/minutes/pdf",
            ReportNames.Contract => $"/api/contracts/{value}/reports/contract/pdf",
            _ => $"/api/reports/purchase-file-summary/{value}/pdf"
        };
        return Task.FromResult(new ReportPreviewModel(reportName, url, PersianTitle(reportName)));
    }

    public async Task<FileDocumentDto> SaveGeneratedReportToPurchaseFileAsync(Guid purchaseFileId, string reportName,
        IReadOnlyDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
    {
        var bytes = await GeneratePdfAsync(reportName, parameters, cancellationToken);
        await using var stream = new MemoryStream(bytes);
        return await storage.SaveFileAsync(purchaseFileId, DocumentTypeFor(reportName),
            FileNameFor(reportName, parameters), stream, currentUser.UserId,
            mimeType: "application/pdf", description: PersianTitle(reportName), cancellationToken: cancellationToken);
    }

    private async Task<XtraReport> CreateReportAsync(string name, IReadOnlyDictionary<string, object?> parameters, CancellationToken ct)
    {
        return name switch
        {
            ReportNames.PurchaseFileSummary => new PurchaseFileSummaryReport(await RequiredPurchaseFile(parameters, ct)),
            ReportNames.PurchaseFileItemsGroupedByMesc => new PurchaseFileItemsGroupedByMescReport(await RequiredPurchaseFile(parameters, ct)),
            ReportNames.Indent => new IndentReport(await RequiredIndent(parameters, ct)),
            ReportNames.TenderSummary => new TenderSummaryReport(await RequiredTender(parameters, ct)),
            ReportNames.TenderComparison => new TenderComparisonReport(await RequiredTenderComparison(parameters, ct)),
            ReportNames.TenderWinnerDecision => new TenderWinnerDecisionReport(await RequiredTenderWinnerDecision(parameters, ct)),
            ReportNames.CommissionSessionMinutes => new CommissionSessionMinutesReport(await RequiredCommissionSession(parameters, ct)),
            ReportNames.CommissionDecision => new CommissionDecisionReport(await RequiredCommissionDecision(parameters, ct)),
            ReportNames.Contract => new ContractReport(await RequiredContract(parameters, ct)),
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
    private async Task<TenderReportData> RequiredTender(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "TenderId");
        return await dataProvider.GetTenderAsync(id, ct) ?? throw new InvalidOperationException("Tender was not found.");
    }
    private async Task<TenderComparisonReportData> RequiredTenderComparison(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "TenderId");
        return await dataProvider.GetTenderComparisonAsync(id, ct) ?? throw new InvalidOperationException("Tender was not found.");
    }
    private async Task<TenderWinnerDecisionReportData> RequiredTenderWinnerDecision(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "TenderId");
        return await dataProvider.GetTenderWinnerDecisionAsync(id, ct) ?? throw new InvalidOperationException("Tender was not found.");
    }
    private async Task<CommissionSessionReportData> RequiredCommissionSession(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "SessionId");
        return await dataProvider.GetCommissionSessionAsync(id, ct) ?? throw new InvalidOperationException("Commission session was not found.");
    }
    private async Task<CommissionDecisionReportData> RequiredCommissionDecision(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var sessionId = RequiredGuid(p, "SessionId");
        var decisionId = RequiredGuid(p, "DecisionId");
        return await dataProvider.GetCommissionDecisionAsync(sessionId, decisionId, ct) ?? throw new InvalidOperationException("Commission decision was not found.");
    }
    private async Task<ContractReportData> RequiredContract(IReadOnlyDictionary<string, object?> p, CancellationToken ct)
    {
        var id = RequiredGuid(p, "ContractId");
        return await dataProvider.GetContractAsync(id, ct) ?? throw new InvalidOperationException("Contract was not found.");
    }
    private static Guid RequiredGuid(IReadOnlyDictionary<string, object?> p, string key) =>
        p.TryGetValue(key, out var value) && value is Guid id ? id : throw new ArgumentException($"Parameter '{key}' is required.");
    private static string PersianTitle(string name) => name switch
    {
        ReportNames.PurchaseFileSummary => "خلاصه پرونده خرید",
        ReportNames.Indent => "گزارش درخواست خرید",
        ReportNames.PurchaseFileItemsGroupedByMesc => "اقلام پرونده به تفکیک گروه MESC",
        ReportNames.TenderSummary => "خلاصه مناقصه",
        ReportNames.TenderComparison => "مقایسه پیشنهادهای مناقصه",
        ReportNames.TenderWinnerDecision => "تصمیم برنده مناقصه",
        ReportNames.CommissionSessionMinutes => "صورتجلسه کمیسیون",
        ReportNames.CommissionDecision => "تصمیم کمیسیون",
        ReportNames.Contract => "قرارداد خرید",
        _ => name
    };

    private static DocumentType DocumentTypeFor(string reportName) => reportName switch
    {
        ReportNames.TenderSummary or ReportNames.TenderComparison or ReportNames.TenderWinnerDecision => DocumentType.TenderDocument,
        ReportNames.CommissionSessionMinutes or ReportNames.CommissionDecision => DocumentType.TenderCommissionMinutes,
        ReportNames.Contract => DocumentType.Contract,
        _ => DocumentType.FinalReport
    };

    private static string FileNameFor(string reportName, IReadOnlyDictionary<string, object?> parameters)
    {
        var suffix = parameters.TryGetValue("TenderNumber", out var tenderNumber) ? tenderNumber?.ToString()
            : parameters.TryGetValue("SessionNumber", out var sessionNumber) ? sessionNumber?.ToString()
            : DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var raw = reportName switch
        {
            ReportNames.TenderSummary => $"TenderSummary-{suffix}.pdf",
            ReportNames.TenderComparison => $"TenderComparison-{suffix}.pdf",
            ReportNames.TenderWinnerDecision => $"TenderWinnerDecision-{suffix}.pdf",
            ReportNames.CommissionSessionMinutes => $"CommissionMinutes-{suffix}.pdf",
            ReportNames.CommissionDecision => $"CommissionDecision-{suffix}-{Short(parameters, "DecisionId")}.pdf",
            ReportNames.Contract => $"Contract-{(parameters.TryGetValue("ContractNumber", out var contractNumber) ? contractNumber : suffix)}.pdf",
            _ => $"{reportName}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
        };
        return Safe(raw);
    }

    private static string Short(IReadOnlyDictionary<string, object?> parameters, string key) =>
        parameters.TryGetValue(key, out var value) && value is Guid id ? id.ToString("N")[..8] : "report";

    private static string Safe(string fileName)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalid, '-');
        return fileName.Replace(' ', '-');
    }
}
