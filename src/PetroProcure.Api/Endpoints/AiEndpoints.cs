using PetroProcure.AI;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Api.Contracts;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace PetroProcure.Api.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var providers = app.MapGroup("/api/ai/providers").WithTags("AI Providers");
        providers.MapGet("/", async (IAiCoreSettingsProvider settingsProvider, CancellationToken ct) =>
        {
            var settings = await settingsProvider.GetAsync(ct);
            return Results.Ok(new[] { new AiProviderDto("AiCore", "AiCore", settings.IsEnabled, settings.BaseUrl, settings.DefaultModel) });
        }).RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderManage, ApplicationPermissions.AdminManageSettings);

        providers.MapGet("/aicore/settings", async (IAiCoreSettingsProvider settingsProvider, CancellationToken ct) =>
            Results.Ok((await settingsProvider.GetAsync(ct)).ToDto()))
            .RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderManage, ApplicationPermissions.AdminManageSettings);

        providers.MapPut("/aicore/settings", async (ConfigureAiCoreProviderRequest request,
            PetroProcureDbContext db, ICurrentUserService currentUser, CancellationToken ct) =>
        {
            await UpsertSetting(db, "AI:AiCore:BaseUrl", request.BaseUrl, "AiCore provider base URL", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:DefaultModel", request.DefaultModel, "AiCore default model", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:TimeoutSeconds", request.TimeoutSeconds.ToString(), "AiCore request timeout seconds", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:MaxInputTokens", request.MaxInputTokens?.ToString(), "AiCore maximum input tokens", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:MaxOutputTokens", request.MaxOutputTokens?.ToString(), "AiCore maximum output tokens", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:IsEnabled", request.IsEnabled.ToString(), "AiCore provider enabled flag", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:UseStreaming", request.UseStreaming.ToString(), "AiCore streaming flag", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:Tenant", request.Tenant, "AiCore tenant identifier", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:ClientId", request.ClientId, "AiCore client identifier", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:ApiKeySecretName", request.ApiKeySecretName, "AiCore API key environment variable name", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:AnalysisPath", request.AnalysisPath, "AiCore analysis endpoint path", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:HealthPath", request.HealthPath, "AiCore health endpoint path", currentUser.UserId, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderManage, ApplicationPermissions.AdminManageSettings);

        providers.MapPost("/aicore/test", async (IAiCoreClient client, CancellationToken ct) =>
            Results.Ok(await client.GetHealthAsync(ct)))
            .RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderTest, ApplicationPermissions.AdminManageSettings);

        providers.MapGet("/aicore/health", async (IAiCoreClient client, CancellationToken ct) =>
            Results.Ok(await client.GetHealthAsync(ct)))
            .RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderTest, ApplicationPermissions.AdminManageSettings);

        var group = app.MapGroup("/api/ai/purchase-files/{purchaseFileId:guid}").WithTags("AI Assistant");
        group.MapPost("/summarize", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.SummarizeAsync(purchaseFileId, ct)).ToContract())
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        group.MapPost("/check-missing-documents", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.CheckMissingDocumentsAsync(purchaseFileId, ct)).ToContract())
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        group.MapPost("/evaluate-rules", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.EvaluateRulesAsync(purchaseFileId, ct)).ToContract())
            .RequirePermission(ApplicationPermissions.AiAgentEvaluatePurchaseRules);
        group.MapGet("/evaluations", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.GetEvaluationsAsync(purchaseFileId, ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.AiAgentUse);

        group.MapPost("/analyze", async (Guid purchaseFileId, AnalyzePurchaseFileRequest request,
            IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzePurchaseFileAsync(purchaseFileId, request.AnalysisType, request.UserQuestion, ct)))
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzePurchaseFile);

        app.MapPost("/api/ai/tenders/{tenderId:guid}/analyze", async (Guid tenderId, AnalyzeTenderRequest request,
            IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzeTenderAsync(tenderId, request.AnalysisType, request.UserQuestion, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzeTender);

        app.MapPost("/api/ai/contracts/{contractId:guid}/analyze", async (Guid contractId, AnalyzeContractRequest request,
            IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzeContractAsync(contractId, request.AnalysisType, request.UserQuestion, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzeContract);

        app.MapPost("/api/ai/purchase-orders/{purchaseOrderId:guid}/analyze", async (Guid purchaseOrderId,
            AnalyzePurchaseOrderRequest request, IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzePurchaseOrderAsync(purchaseOrderId, request.AnalysisType, request.UserQuestion, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzePurchaseOrder);

        app.MapPost("/api/ai/warehouse-receipts/{receiptId:guid}/analyze", async (Guid receiptId,
            AnalyzeWarehouseReceiptRequest request, IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzeWarehouseReceiptAsync(receiptId, request.AnalysisType, request.UserQuestion, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzeWarehouseReceipt);

        app.MapPost("/api/ai/legal-compliance/analyze", async (AnalyzeLegalComplianceRequest request,
            IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzeLegalComplianceAsync(request.EntityType, request.EntityId,
                request.AppliesTo, request.UserQuestion, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentEvaluatePurchaseRules, ApplicationPermissions.ProcurementRuleEvaluate);

        app.MapGet("/api/ai/evaluations", async (string? entityType, Guid? entityId,
            IAiAnalysisRepository repository, CancellationToken ct) =>
            Results.Ok(await repository.GetAsync(entityType, entityId, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiViewEvaluations);

        app.MapGet("/api/ai/evaluations/{id:guid}", async (Guid id,
            IAiAnalysisRepository repository, CancellationToken ct) =>
            await repository.GetByIdAsync(id, ct) is { } result ? Results.Ok(result) : Results.NotFound())
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiViewEvaluations);
        return app;
    }

    private static async Task UpsertSetting(PetroProcureDbContext db, string key, string? value,
        string description, Guid userId, CancellationToken ct)
    {
        var setting = await db.SystemSettings.SingleOrDefaultAsync(x => x.Key == key, ct);
        if (setting is null)
        {
            db.SystemSettings.Add(new SystemSetting
            {
                Key = key,
                Value = value,
                Description = description,
                IsSecret = false,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUserId = userId
            });
            return;
        }

        setting.Value = value;
        setting.Description = description;
        setting.IsSecret = false;
        setting.UpdatedAt = DateTime.UtcNow;
        setting.UpdatedByUserId = userId;
    }
}
