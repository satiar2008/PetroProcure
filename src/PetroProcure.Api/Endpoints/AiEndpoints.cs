using PetroProcure.AI;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Api.Contracts;

namespace PetroProcure.Api.Endpoints;
public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group=app.MapGroup("/api/ai/purchase-files/{purchaseFileId:guid}").WithTags("AI Assistant");
        group.MapPost("/summarize",async (Guid purchaseFileId,IAiAgentService ai,CancellationToken ct)=>
            (await ai.SummarizeAsync(purchaseFileId,ct)).ToContract())
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        group.MapPost("/check-missing-documents",async (Guid purchaseFileId,IAiAgentService ai,CancellationToken ct)=>
            (await ai.CheckMissingDocumentsAsync(purchaseFileId,ct)).ToContract())
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        group.MapPost("/evaluate-rules",async (Guid purchaseFileId,IAiAgentService ai,CancellationToken ct)=>
            (await ai.EvaluateRulesAsync(purchaseFileId,ct)).ToContract())
            .RequirePermission(ApplicationPermissions.AiAgentEvaluatePurchaseRules);
        group.MapGet("/evaluations",async (Guid purchaseFileId,IAiAgentService ai,CancellationToken ct)=>
            (await ai.GetEvaluationsAsync(purchaseFileId,ct)).Select(x=>x.ToContract()))
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        return app;
    }
}
