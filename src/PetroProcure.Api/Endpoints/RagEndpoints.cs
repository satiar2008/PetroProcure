using PetroProcure.Api.Security;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;

namespace PetroProcure.Api.Endpoints;

public static class RagEndpoints
{
    public static IEndpointRouteBuilder MapRagEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/rag/retrieve", async (
            RagRetrieveRequest request,
            IRagRetriever retriever,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
            Results.Ok(await retriever.RetrieveAsync(request, ToRagUserContext(currentUser), ct)))
            .WithTags("RAG")
            .RequireAnyPermission(
                ApplicationPermissions.AiAgentUse,
                ApplicationPermissions.LegalDocumentView,
                ApplicationPermissions.ProcurementRuleView,
                ApplicationPermissions.PurchaseFileView);

        app.MapPost("/api/purchase-files/{purchaseFileId:guid}/rag/retrieve", async (
            Guid purchaseFileId,
            RagRetrieveRequest request,
            IRagRetriever retriever,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
            Results.Ok(await retriever.RetrieveAsync(
                request with { Scope = RagRetrievalScope.PurchaseFile, PurchaseFileId = purchaseFileId },
                ToRagUserContext(currentUser),
                ct)))
            .WithTags("RAG")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.PurchaseFileView);

        app.MapPost("/api/legal/rag/retrieve", async (
            RagRetrieveRequest request,
            IRagRetriever retriever,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
            Results.Ok(await retriever.RetrieveAsync(
                request with { Scope = RagRetrievalScope.LegalCorpus, PurchaseFileId = null },
                ToRagUserContext(currentUser),
                ct)))
            .WithTags("RAG")
            .RequireAnyPermission(
                ApplicationPermissions.AiAgentUse,
                ApplicationPermissions.LegalDocumentView,
                ApplicationPermissions.ProcurementRuleView);

        var admin = app.MapGroup("/api/admin/rag")
            .WithTags("RAG Admin")
            .RequirePermission(ApplicationPermissions.AiAdmin);

        admin.MapPost("/evaluate-quality", async (
            RagQualityEvaluationRequest request,
            IRagQualityEvaluator evaluator,
            ICurrentUserService currentUser,
            CancellationToken ct) =>
            Results.Ok(await evaluator.EvaluateAsync(request, ToRagUserContext(currentUser), ct)));

        admin.MapPost("/reindex", async (
            RagReindexRequest? request,
            IRagMaintenanceService maintenance,
            CancellationToken ct) =>
            Results.Ok(await maintenance.ReindexAsync(request?.Force ?? false, ct)));

        admin.MapGet("/embedding-model-status", async (
            IRagMaintenanceService maintenance,
            CancellationToken ct) =>
            Results.Ok(await maintenance.GetEmbeddingModelStatusAsync(ct)));

        return app;
    }

    private static RagUserContext ToRagUserContext(ICurrentUserService currentUser) =>
        new(
            currentUser.UserId,
            currentUser.IsSystemAdmin,
            currentUser.Permissions,
            currentUser.DepartmentIds);

    private sealed record RagReindexRequest(bool Force = false);
}
