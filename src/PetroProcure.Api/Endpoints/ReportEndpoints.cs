using PetroProcure.Reporting;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;

namespace PetroProcure.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/reports/purchase-file-summary/{purchaseFileId:guid}/pdf", async (
            Guid purchaseFileId, IReportGenerator generator, CancellationToken ct) =>
        {
            var bytes = await generator.GeneratePdfAsync(ReportNames.PurchaseFileSummary,
                new Dictionary<string, object?> { ["PurchaseFileId"] = purchaseFileId }, ct);
            return Results.File(bytes, "application/pdf", $"purchase-file-{purchaseFileId}.pdf");
        }).RequirePermission(ApplicationPermissions.ReportExportPdf);
        app.MapPost("/api/reports/purchase-file-summary/{purchaseFileId:guid}/save-to-file", async (
            Guid purchaseFileId, IReportGenerator generator, CancellationToken ct) =>
            Results.Ok(await generator.SaveGeneratedReportToPurchaseFileAsync(purchaseFileId,
                ReportNames.PurchaseFileSummary, new Dictionary<string, object?>
                {
                    ["PurchaseFileId"] = purchaseFileId
                }, ct))).RequirePermission(ApplicationPermissions.ReportExportPdf);
        app.MapGet("/api/reports/indent/{indentId:guid}/pdf", async (
            Guid indentId, IReportGenerator generator, CancellationToken ct) =>
        {
            var bytes = await generator.GeneratePdfAsync(ReportNames.Indent,
                new Dictionary<string, object?> { ["IndentId"] = indentId }, ct);
            return Results.File(bytes, "application/pdf", $"indent-{indentId}.pdf");
        }).RequirePermission(ApplicationPermissions.ReportExportPdf);
        return app;
    }
}
