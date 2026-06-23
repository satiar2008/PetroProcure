namespace PetroProcure.Contracts.V1.Reports;

public sealed record ReportRequest(string ReportName, IReadOnlyDictionary<string, object?> Parameters);
public sealed record ReportResultDto(
    string ReportName, string ContentType, string FileName, byte[] Content, DateTime GeneratedAt);
public sealed record SaveReportToFileRequest(string ReportName, IReadOnlyDictionary<string, object?> Parameters);
