using Microsoft.AspNetCore.Mvc;
using PetroProcure.Api.Security;
using PetroProcure.Application.Legal;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Legal;

namespace PetroProcure.Api.Endpoints;

public static class LegalRuleEndpoints
{
    public static IEndpointRouteBuilder MapLegalRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var legal = app.MapGroup("/api/legal").WithTags("Legal Documents");

        legal.MapGet("/documents", async ([AsParameters] LegalDocumentListRequest request,
            LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetLegalDocumentsQuery(request), ct))
            .RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapGet("/documents/{id:guid}", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetLegalDocumentByIdQuery(id), ct) is { } document
                ? Results.Ok(document)
                : Results.NotFound())
            .RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapPost("/documents/upload", async ([FromForm] IFormFile file, [FromForm] string title,
            [FromForm] string? description, [FromForm] string? sourceDocumentTitle,
            [FromForm] string? sourceDocumentNumber, [FromForm] DateTime? sourceDocumentDate,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            if (file.Length == 0)
                return Results.BadRequest("Uploaded legal document is empty.");

            await using var stream = file.OpenReadStream();
            var result = await handler.Handle(new UploadLegalDocumentCommand(title, file.FileName, stream,
                file.ContentType, description, sourceDocumentTitle, sourceDocumentNumber, sourceDocumentDate), ct);
            return Results.Created($"/api/legal/documents/{result.Id}", result);
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.LegalDocumentManage);

        legal.MapGet("/documents/{id:guid}/download", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
        {
            var file = await handler.Handle(new DownloadLegalDocumentQuery(id), ct);
            return Results.File(file.Stream, file.MimeType, file.OriginalFileName);
        }).RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapDelete("/documents/{id:guid}", async (Guid id, LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new DeleteLegalDocumentCommand(id), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.LegalDocumentManage);

        legal.MapGet("/documents/{id:guid}/articles", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetArticlesByDocumentQuery(id), ct))
            .RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapGet("/articles/search", async ([AsParameters] LegalArticleSearchRequest request,
            LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new SearchLegalArticlesQuery(request), ct))
            .RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapGet("/clauses/search", async ([AsParameters] LegalClauseSearchRequest request,
            LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new SearchLegalClausesQuery(request), ct))
            .RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapGet("/clauses/{id:guid}/context", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetLegalClauseContextQuery(id), ct) is { } context ? Results.Ok(context) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.LegalDocumentView);

        legal.MapPost("/articles", async (CreateLegalArticleRequest request, LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateLegalArticleCommand(request), ct);
            return Results.Created($"/api/legal/documents/{result.LegalDocumentId}/articles", result);
        }).RequirePermission(ApplicationPermissions.LegalDocumentManage);

        legal.MapPost("/clauses", async (CreateLegalClauseRequest request, LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateLegalClauseCommand(request), ct);
            return Results.Created($"/api/legal/articles/{result.LegalArticleId}/clauses", result);
        }).RequirePermission(ApplicationPermissions.LegalDocumentManage);

        var rules = app.MapGroup("/api/procurement-rules").WithTags("Procurement Rules");

        rules.MapGet("/", async ([AsParameters] ProcurementRuleListRequest request,
            LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetProcurementRulesQuery(request), ct))
            .RequirePermission(ApplicationPermissions.ProcurementRuleView);

        rules.MapGet("/{id:guid}/versions", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetRuleVersionsQuery(id), ct))
            .RequirePermission(ApplicationPermissions.ProcurementRuleView);

        rules.MapPost("/", async (CreateProcurementRuleRequest request, LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateProcurementRuleCommand(request), ct);
            return Results.Created($"/api/procurement-rules/{result.Id}", result);
        }).RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        rules.MapPost("/{id:guid}/clone-draft", async (Guid id, LegalRuleCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new CloneRuleAsDraftCommand(id), ct)))
            .RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        rules.MapPut("/{id:guid}/draft", async (Guid id, UpdateRuleDraftRequest request,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new UpdateRuleDraftCommand(id, request), ct)))
            .RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        rules.MapPost("/{id:guid}/submit", async (Guid id, SubmitRuleForApprovalRequest? request,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new SubmitRuleForApprovalCommand(id, request?.Comment), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        rules.MapPost("/{id:guid}/approve", async (Guid id, ApproveRuleVersionRequest? request,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ApproveRuleVersionCommand(id, request?.Comment), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ProcurementRuleApprove);

        rules.MapPost("/{id:guid}/deprecate", async (Guid id, DeprecateRuleRequest request,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new DeprecateRuleCommand(id, request.Reason), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        rules.MapPost("/evaluate/purchase-file/{purchaseFileId:guid}", async (Guid purchaseFileId,
            LegalRuleEvaluationHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new EvaluatePurchaseFileRulesCommand(purchaseFileId), ct)))
            .RequireAnyPermission(ApplicationPermissions.ProcurementRuleEvaluate, ApplicationPermissions.AiAgentEvaluatePurchaseRules);

        rules.MapPost("/evaluate/tender/{tenderId:guid}", async (Guid tenderId,
            LegalRuleEvaluationHandler handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new EvaluateTenderRulesCommand(tenderId), ct)))
            .RequireAnyPermission(ApplicationPermissions.ProcurementRuleEvaluate, ApplicationPermissions.AiAgentEvaluatePurchaseRules);

        rules.MapGet("/evaluations/purchase-file/{purchaseFileId:guid}", async (Guid purchaseFileId,
            LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetPurchaseFileRuleEvaluationsQuery(purchaseFileId), ct))
            .RequireAnyPermission(ApplicationPermissions.ProcurementRuleView, ApplicationPermissions.AiAgentUse);

        rules.MapPost("/gates/purchase-files/{purchaseFileId:guid}/override", async (
            Guid purchaseFileId,
            OverrideProcurementRuleGateRequest request,
            IProcurementRuleGateService gate,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
            Results.Ok(await gate.OverrideTransitionAsync(
                purchaseFileId,
                string.IsNullOrWhiteSpace(request.TransitionName)
                    ? ProcurementRuleGateTransitions.PurchaseFileComplete
                    : request.TransitionName,
                request.Reason,
                new ProcurementRuleGateUserContext(
                    currentUser.UserId,
                    currentUser.IsSystemAdmin,
                    currentUser.Permissions),
                ct)))
            .RequirePermission(ApplicationPermissions.LegalRuleOverrideBlockingFinding);

        // AI-RAG-03: admin import + management of versioned procurement rules.
        var adminRules = app.MapGroup("/api/admin/procurement-rules").WithTags("Procurement Rules Admin");

        adminRules.MapPost("/import/json", async (ProcurementRuleImportRequest request,
            IProcurementRuleImportService import, CancellationToken ct) =>
            Results.Ok(await import.ImportAsync(request, ct)))
            .RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        adminRules.MapPost("/import/csv", async (IFormFile file,
            IProcurementRuleImportService import, CancellationToken ct) =>
        {
            if (file.Length == 0) return Results.BadRequest("CSV file is empty.");
            using var reader = new StreamReader(file.OpenReadStream());
            var csv = await reader.ReadToEndAsync(ct);
            return Results.Ok(await import.ImportCsvAsync(csv, ct));
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        adminRules.MapGet("/", async ([AsParameters] ProcurementRuleListRequest request,
            LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetProcurementRulesQuery(request), ct))
            .RequirePermission(ApplicationPermissions.ProcurementRuleView);

        adminRules.MapGet("/{id:guid}", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetProcurementRuleByIdQuery(id), ct) is { } dto ? Results.Ok(dto) : Results.NotFound())
            .RequirePermission(ApplicationPermissions.ProcurementRuleView);

        adminRules.MapPost("/{id:guid}/submit", async (Guid id, SubmitRuleForApprovalRequest? request,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new SubmitRuleForApprovalCommand(id, request?.Comment), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ProcurementRuleManage);

        adminRules.MapPost("/{id:guid}/approve", async (Guid id, ApproveRuleVersionRequest? request,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ApproveRuleVersionCommand(id, request?.Comment), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ProcurementRuleApprove);

        // In this domain, approving a pending version sets it active, so "activate" aliases approve.
        adminRules.MapPost("/{id:guid}/activate", async (Guid id,
            LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            await handler.Handle(new ApproveRuleVersionCommand(id, "Activated"), ct);
            return Results.NoContent();
        }).RequirePermission(ApplicationPermissions.ProcurementRuleApprove);

        return app;
    }
}
