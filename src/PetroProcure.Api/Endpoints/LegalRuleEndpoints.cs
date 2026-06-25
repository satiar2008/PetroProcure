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
            [FromForm] string? description, LegalRuleCommandHandler handler, CancellationToken ct) =>
        {
            if (file.Length == 0)
                return Results.BadRequest("Uploaded legal document is empty.");

            await using var stream = file.OpenReadStream();
            var result = await handler.Handle(new UploadLegalDocumentCommand(title, file.FileName, stream, description), ct);
            return Results.Created($"/api/legal/documents/{result.Id}", result);
        }).DisableAntiforgery().RequirePermission(ApplicationPermissions.LegalDocumentManage);

        legal.MapGet("/documents/{id:guid}/articles", async (Guid id, LegalRuleQueryHandler handler, CancellationToken ct) =>
            await handler.Handle(new GetArticlesByDocumentQuery(id), ct))
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

        return app;
    }
}
