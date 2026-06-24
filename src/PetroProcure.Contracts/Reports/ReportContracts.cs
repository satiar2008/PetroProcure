namespace PetroProcure.Contracts.V1.Reports;

public sealed record ReportRequest(string ReportName, IReadOnlyDictionary<string, object?> Parameters);
public sealed record ReportResultDto(
    string ReportName, string ContentType, string FileName, byte[] Content, DateTime GeneratedAt);
public sealed record SaveReportToFileRequest(string ReportName, IReadOnlyDictionary<string, object?> Parameters);
public sealed record SaveGeneratedReportRequest(string? Description = null);
public sealed record GeneratedReportResultDto(
    Guid FileDocumentId, Guid PurchaseFileId, string FileName, string RelativePath,
    string ReportName, DateTime GeneratedAt);
public sealed record ReportPreviewDto(string ReportName, string Title, string PreviewUrl);
