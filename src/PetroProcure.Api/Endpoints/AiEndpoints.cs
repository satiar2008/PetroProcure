using System.Text.Json;
using PetroProcure.AI;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Api.Contracts;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            var validationError = ValidateAiCoreSettings(request);
            if (validationError is not null)
                return Results.BadRequest(new { error = validationError });

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
            await UpsertSetting(db, "AI:AiCore:Mode", request.Mode, "AiCore integration mode", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:SubmitJobPath", request.SubmitJobPath, "AiCore async job submit path", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:CallbackPublicUrl", request.CallbackPublicUrl, "Public PetroProcure callback URL", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:RequestTimeoutSeconds", request.RequestTimeoutSeconds.ToString(), "AiCore short request timeout seconds", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:WorkerBatchSize", request.WorkerBatchSize.ToString(), "AI worker batch size", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:MaxRetryCount", request.MaxRetryCount.ToString(), "AI retry limit", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:RetryDelaySeconds", request.RetryDelaySeconds.ToString(), "AI retry delay seconds", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:CallbackTimestampToleranceSeconds", request.CallbackTimestampToleranceSeconds.ToString(), "AiCore callback timestamp tolerance", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:StuckJobTimeoutMinutes", request.StuckJobTimeoutMinutes.ToString(), "AI stuck lock timeout minutes", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:RunningJobTimeoutMinutes", request.RunningJobTimeoutMinutes.ToString(), "AI running/submitted timeout minutes", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:CompletedJobRetentionDays", request.CompletedJobRetentionDays.ToString(), "AI completed queue retention days", currentUser.UserId, ct);
            await UpsertSetting(db, "AI:AiCore:CallbackAllowedIpAddresses",
                request.CallbackAllowedIpAddresses is null ? null : string.Join(',', request.CallbackAllowedIpAddresses),
                "Optional AiCore callback IP allowlist", currentUser.UserId, ct);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderManage, ApplicationPermissions.AdminManageSettings);

        providers.MapPost("/aicore/test", async (IAiCoreClient client, CancellationToken ct) =>
            Results.Ok(await client.GetHealthAsync(ct)))
            .RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderTest, ApplicationPermissions.AdminManageSettings);

        providers.MapGet("/aicore/health", async (IAiCoreClient client, CancellationToken ct) =>
            Results.Ok(await client.GetHealthAsync(ct)))
            .RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderTest, ApplicationPermissions.AdminManageSettings);

        providers.MapGet("/aicore/dashboard", async (IAiCoreJobClient client, IAiJobQueueService queue, CancellationToken ct) =>
        {
            var health = await client.GetHealthAsync(ct);
            var metrics = await queue.GetMetricsAsync(ct);
            return Results.Ok(new { health, metrics });
        }).RequireAnyPermission(ApplicationPermissions.AiAdmin, ApplicationPermissions.AiProviderManage, ApplicationPermissions.AdminManageSettings);

        var jobs = app.MapGroup("/api/ai/jobs").WithTags("AI Jobs");
        jobs.MapPost("", async (CreateAiJobRequest request, IAiJobQueueService queue,
            ICurrentUserService currentUser, CancellationToken ct) =>
        {
            var response = await queue.CreateJobAsync(request, currentUser.UserId, ct);
            return Results.Accepted($"/api/ai/jobs/{response.JobId}", response);
        }).RequireAnyPermission(ApplicationPermissions.AiAgentUse,
            ApplicationPermissions.AiAgentEvaluatePurchaseRules, ApplicationPermissions.AiAgentAnalyzePurchaseFile,
            ApplicationPermissions.AiAnalyzePurchaseFile);

        jobs.MapGet("/{jobId:guid}", async (Guid jobId, IAiJobQueueService queue, CancellationToken ct) =>
            await queue.GetJobStatusAsync(jobId, ct) is { } status ? Results.Ok(status) : Results.NotFound())
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse,
                ApplicationPermissions.AiAgentEvaluatePurchaseRules, ApplicationPermissions.AiAgentAnalyzePurchaseFile,
                ApplicationPermissions.AiAnalyzePurchaseFile);

        jobs.MapGet("/{jobId:guid}/result", async (Guid jobId, IAiResultRepository results, CancellationToken ct) =>
            await results.GetResultAsync(jobId, ct) is { } result ? Results.Ok(result) : Results.NotFound())
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse,
                ApplicationPermissions.AiAgentEvaluatePurchaseRules, ApplicationPermissions.AiAgentAnalyzePurchaseFile,
                ApplicationPermissions.AiAnalyzePurchaseFile);

        jobs.MapPost("/{jobId:guid}/cancel", async (Guid jobId, IAiJobQueueService queue, CancellationToken ct) =>
        {
            if (await queue.GetJobAsync(jobId, ct) is null)
                return Results.NotFound();

            await queue.MarkCancelledAsync(jobId, DateTime.UtcNow, "Cancelled by user.", ct);
            return Results.NoContent();
        }).RequireAnyPermission(ApplicationPermissions.AiAgentUse,
            ApplicationPermissions.AiAgentEvaluatePurchaseRules, ApplicationPermissions.AiAgentAnalyzePurchaseFile,
            ApplicationPermissions.AiAnalyzePurchaseFile);

        app.MapGet("/api/ai/entities/{entityType}/{entityId:guid}/jobs",
            async (string entityType, Guid entityId, IAiJobQueueService queue, CancellationToken ct) =>
                Results.Ok(await queue.GetJobStatusesForEntityAsync(entityType, entityId, ct)))
            .WithTags("AI Jobs")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse,
                ApplicationPermissions.AiAgentEvaluatePurchaseRules, ApplicationPermissions.AiAgentAnalyzePurchaseFile,
                ApplicationPermissions.AiAnalyzePurchaseFile);

        // AiCore async result callback. Authenticated by AiCore-specific credentials (API key + optional
        // HMAC signature/timestamp) — never by user JWT. Must remain anonymous to the user auth pipeline.
        app.MapPost("/api/ai/aicore/callback", HandleAiCoreCallbackAsync)
            .WithTags("AI Jobs")
            .AllowAnonymous();

        app.MapPost("/api/ai/callbacks/aicore", HandleAiCoreCallbackAsync)
            .WithTags("AI Jobs")
            .AllowAnonymous();

        app.MapPost("/api/ai/aicore/callback/debug", HandleAiCoreCallbackDebugAsync)
            .WithTags("AI Jobs")
            .AllowAnonymous();

        var purchaseFileJobs = app.MapGroup("/api/ai/purchase-files/{purchaseFileId:guid}/jobs").WithTags("AI Jobs");
        purchaseFileJobs.MapPost("/summarize", async (Guid purchaseFileId, IAiJobQueueService queue,
            ICurrentUserService currentUser, CancellationToken ct) =>
            await AcceptedJob(queue, currentUser, new CreateAiJobRequest(
                "PurchaseFile", purchaseFileId, "Summary"), ct))
            .RequirePermission(ApplicationPermissions.AiAgentUse);

        purchaseFileJobs.MapPost("/check-missing-documents", async (Guid purchaseFileId, IAiJobQueueService queue,
            ICurrentUserService currentUser, CancellationToken ct) =>
            await AcceptedJob(queue, currentUser, new CreateAiJobRequest(
                "PurchaseFile", purchaseFileId, "MissingDocuments"), ct))
            .RequirePermission(ApplicationPermissions.AiAgentUse);

        purchaseFileJobs.MapPost("/evaluate-rules", async (Guid purchaseFileId, IAiJobQueueService queue,
            ICurrentUserService currentUser, CancellationToken ct) =>
            await AcceptedJob(queue, currentUser, new CreateAiJobRequest(
                "PurchaseFile", purchaseFileId, "LegalCompliance"), ct))
            .RequirePermission(ApplicationPermissions.AiAgentEvaluatePurchaseRules);

        purchaseFileJobs.MapPost("/analyze", async (Guid purchaseFileId, AnalyzePurchaseFileRequest request,
            IAiJobQueueService queue, ICurrentUserService currentUser, CancellationToken ct) =>
            await AcceptedJob(queue, currentUser, new CreateAiJobRequest(
                "PurchaseFile", purchaseFileId, string.IsNullOrWhiteSpace(request.AnalysisType) ? "Summary" : request.AnalysisType,
                request.UserQuestion), ct))
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAgentAnalyzePurchaseFile,
                ApplicationPermissions.AiAnalyzePurchaseFile);

        var group = app.MapGroup("/api/ai/purchase-files/{purchaseFileId:guid}").WithTags("AI Assistant");
        group.MapPost("/summarize", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.SummarizeAsync(purchaseFileId, ct)).ToContract())
            .WithMetadata(new ObsoleteAttribute("Development-only synchronous endpoint. Use POST /api/ai/purchase-files/{purchaseFileId}/jobs/summarize."))
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        group.MapPost("/check-missing-documents", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.CheckMissingDocumentsAsync(purchaseFileId, ct)).ToContract())
            .WithMetadata(new ObsoleteAttribute("Development-only synchronous endpoint. Use POST /api/ai/purchase-files/{purchaseFileId}/jobs/check-missing-documents."))
            .RequirePermission(ApplicationPermissions.AiAgentUse);
        group.MapPost("/evaluate-rules", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.EvaluateRulesAsync(purchaseFileId, ct)).ToContract())
            .WithMetadata(new ObsoleteAttribute("Development-only synchronous endpoint. Use POST /api/ai/purchase-files/{purchaseFileId}/jobs/evaluate-rules."))
            .RequirePermission(ApplicationPermissions.AiAgentEvaluatePurchaseRules);
        group.MapGet("/evaluations", async (Guid purchaseFileId, IAiAgentService ai, CancellationToken ct) =>
            (await ai.GetEvaluationsAsync(purchaseFileId, ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.AiAgentUse);

        group.MapPost("/analyze", async (Guid purchaseFileId, AnalyzePurchaseFileRequest request,
            IAiAnalysisService analysis, CancellationToken ct) =>
            Results.Ok(await analysis.AnalyzePurchaseFileAsync(purchaseFileId, request.AnalysisType, request.UserQuestion, ct)))
            .WithMetadata(new ObsoleteAttribute("Development-only synchronous endpoint. Use POST /api/ai/purchase-files/{purchaseFileId}/jobs/analyze."))
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzePurchaseFile);

        group.MapPost("/ask", async (Guid purchaseFileId, GroundedAiQuestionRequest request,
            IGroundedAiAnalysisService grounded, CancellationToken ct) =>
            Results.Ok(await grounded.AskAboutFileAsync(purchaseFileId, request.Question ?? string.Empty, request.TopK, ct)))
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.AiAnalyzePurchaseFile,
                ApplicationPermissions.PurchaseFileView);

        app.MapPost("/api/ai/legal/find-relevant-regulations", async (GroundedAiQuestionRequest request,
            IGroundedAiAnalysisService grounded, CancellationToken ct) =>
            Results.Ok(await grounded.FindRelevantRegulationsAsync(request.Question ?? string.Empty, request.TopK, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.LegalDocumentView,
                ApplicationPermissions.ProcurementRuleView);

        app.MapPost("/api/ai/rule-findings/{findingId:guid}/explain", async (Guid findingId,
            GroundedAiQuestionRequest request, IGroundedAiAnalysisService grounded, CancellationToken ct) =>
            Results.Ok(await grounded.ExplainRuleFindingAsync(findingId, request.Question, request.TopK, ct)))
            .WithTags("AI Assistant")
            .RequireAnyPermission(ApplicationPermissions.AiAgentUse, ApplicationPermissions.ProcurementRuleView,
                ApplicationPermissions.ProcurementRuleEvaluate);

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

    private static readonly JsonSerializerOptions CallbackJsonOptions = new(JsonSerializerDefaults.Web);

    private static async Task<IResult> HandleAiCoreCallbackAsync(
        HttpRequest http,
        IAiCoreCallbackAuthenticator authenticator,
        IAiCoreCallbackService callback,
        IAiJobRepository jobs,
        IOptions<AiCoreIntegrationOptions> options,
        ILoggerFactory loggerFactory,
        IHostEnvironment environment,
        CancellationToken ct)
    {
        var logger = loggerFactory.CreateLogger("PetroProcure.Api.AiCoreCallback");
        logger.LogInformation("AiCore callback received from {RemoteIp} on {Path} with Content-Type {ContentType}.",
            http.HttpContext.Connection.RemoteIpAddress,
            http.Path.Value,
            http.ContentType ?? "<none>");

        if (!IsAllowedCallbackIp(http, options.Value.CallbackAllowedIpAddresses))
        {
            logger.LogWarning("AiCore callback rejected from remote IP {RemoteIp}.", http.HttpContext.Connection.RemoteIpAddress);
            return Results.Unauthorized();
        }

        string rawBody;
        using (var reader = new StreamReader(http.Body, leaveOpen: false))
            rawBody = await reader.ReadToEndAsync(ct);

        if (environment.IsDevelopment())
            logger.LogInformation("AiCore callback raw body: {RawBody}", rawBody);

        var auth = authenticator.Validate(
            http.Headers["X-AI-API-KEY"].FirstOrDefault(),
            http.Headers.Authorization.FirstOrDefault(),
            http.Headers["X-AiCore-Signature"].FirstOrDefault(),
            http.Headers["X-AiCore-Timestamp"].FirstOrDefault(),
            rawBody);
        if (!auth.IsAuthorized)
        {
            // FailureReason never contains the secret.
            logger.LogWarning("AiCore callback authentication failed: {Reason}", auth.FailureReason);
            return Results.Unauthorized();
        }

        var parsed = ParseCallbackRequest(rawBody, http.Headers["X-AiCore-JobId"].FirstOrDefault());
        if (!parsed.IsValid)
        {
            logger.LogWarning("AiCore callback payload validation failed: {ValidationError}", parsed.ValidationError);
            if (environment.IsDevelopment())
                logger.LogWarning("Invalid AiCore callback raw body: {RawBody}", rawBody);
            return Results.BadRequest(new { error = "Invalid callback payload.", details = parsed.ValidationError });
        }

        var request = parsed.Request!;
        var matchedJob = !string.IsNullOrWhiteSpace(request.CorrelationId)
            ? await jobs.GetJobByCorrelationIdAsync(request.CorrelationId.Trim(), ct)
            : null;
        if (matchedJob is null && !string.IsNullOrWhiteSpace(request.ExternalJobId))
            matchedJob = await jobs.GetJobByExternalJobIdAsync(request.ExternalJobId.Trim(), ct);

        logger.LogInformation("AiCore callback parsed: ExternalJobId={ExternalJobId}; Status={Status}; MatchedJobId={MatchedJobId}.",
            request.ExternalJobId,
            request.Status,
            matchedJob?.Id);

        var outcome = await callback.HandleAsync(request, ct);
        return outcome switch
        {
            AiCoreCallbackOutcome.Processed or AiCoreCallbackOutcome.Duplicate =>
                Results.Ok(new { status = "ok", correlationId = request.CorrelationId, outcome = outcome.ToString() }),
            AiCoreCallbackOutcome.MissingCorrelationId =>
                Results.BadRequest(new { error = "correlationId is required." }),
            AiCoreCallbackOutcome.InvalidStatus =>
                Results.BadRequest(new { error = $"Unsupported callback status '{request.Status}'." }),
            AiCoreCallbackOutcome.UnknownCorrelationId =>
                Results.NotFound(new { error = "Unknown correlationId." }),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private static async Task<IResult> HandleAiCoreCallbackDebugAsync(
        HttpRequest http,
        ILoggerFactory loggerFactory,
        IHostEnvironment environment,
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
            return Results.NotFound();

        var logger = loggerFactory.CreateLogger("PetroProcure.Api.AiCoreCallback.Debug");
        using var reader = new StreamReader(http.Body, leaveOpen: false);
        var rawBody = await reader.ReadToEndAsync(ct);
        logger.LogInformation("AiCore debug callback received from {RemoteIp} on {Path} with Content-Type {ContentType}. Raw body: {RawBody}",
            http.HttpContext.Connection.RemoteIpAddress,
            http.Path.Value,
            http.ContentType ?? "<none>",
            rawBody);
        return Results.Ok(new { status = "ok", path = http.Path.Value });
    }

    private static ParsedCallbackRequest ParseCallbackRequest(string rawBody, string? externalJobIdHeader)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
            return ParsedCallbackRequest.Invalid("Request body is empty.");

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(rawBody);
        }
        catch (JsonException ex)
        {
            return ParsedCallbackRequest.Invalid($"Request body is not valid JSON: {ex.Message}");
        }

        using (document)
        {
            AiCoreCallbackRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<AiCoreCallbackRequest>(rawBody, CallbackJsonOptions);
            }
            catch (JsonException ex)
            {
                return ParsedCallbackRequest.Invalid($"Callback JSON does not match the expected schema: {ex.Message}");
            }

            if (request is null)
                return ParsedCallbackRequest.Invalid("Callback JSON could not be deserialized.");

            var root = document.RootElement;
            var correlationId = Coalesce(
                request.CorrelationId,
                ReadString(root, "correlationId"),
                ReadString(root, "correlationID"));
            var externalJobId = Coalesce(
                request.ExternalJobId,
                ReadString(root, "externalJobId"),
                ReadString(root, "externalJobID"),
                ReadString(root, "jobId"),
                ReadString(root, "id"),
                externalJobIdHeader);
            var status = Coalesce(
                request.Status,
                ReadString(root, "status"),
                ReadString(root, "state"));
            var message = Coalesce(
                request.Message,
                ReadString(root, "message"),
                ReadString(root, "errorMessage"));

            var error = request.Error;
            if (error is null && IsFailedStatus(status))
            {
                var errorElement = TryGetProperty(root, "error");
                var errorCode = errorElement?.ValueKind == JsonValueKind.Object
                    ? ReadString(errorElement.Value, "code")
                    : null;
                var errorMessage = errorElement?.ValueKind == JsonValueKind.String
                    ? errorElement.Value.GetString()
                    : ReadString(errorElement ?? root, "message");
                errorMessage = Coalesce(errorMessage, ReadString(root, "errorMessage"), message, "AiCore analysis failed.");
                error = new AiCoreCallbackErrorDto(errorCode ?? "AICORE_FAILED", errorMessage!, false);
            }

            if (string.IsNullOrWhiteSpace(status))
                return ParsedCallbackRequest.Invalid("status is required.");
            if (string.IsNullOrWhiteSpace(correlationId) && string.IsNullOrWhiteSpace(externalJobId))
                return ParsedCallbackRequest.Invalid("correlationId or externalJobId is required.");

            return ParsedCallbackRequest.Valid(request with
            {
                CorrelationId = correlationId ?? string.Empty,
                ExternalJobId = externalJobId ?? string.Empty,
                Status = status!,
                Message = message,
                Error = error
            });
        }
    }

    private static bool IsFailedStatus(string? status) =>
        string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase);

    private static string? Coalesce(params string?[] values)
    {
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        return null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        var property = TryGetProperty(element, propertyName);
        return property?.ValueKind switch
        {
            JsonValueKind.String => property.Value.GetString(),
            JsonValueKind.Number => property.Value.GetRawText(),
            _ => null
        };
    }

    private static JsonElement? TryGetProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var property in element.EnumerateObject())
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return property.Value;

        return null;
    }

    private sealed record ParsedCallbackRequest(AiCoreCallbackRequest? Request, bool IsValid, string? ValidationError)
    {
        public static ParsedCallbackRequest Valid(AiCoreCallbackRequest request) => new(request, true, null);
        public static ParsedCallbackRequest Invalid(string validationError) => new(null, false, validationError);
    }

    private static bool IsAllowedCallbackIp(HttpRequest http, IReadOnlyCollection<string>? allowlist)
    {
        if (allowlist is null || allowlist.Count == 0) return true;
        var remote = http.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrWhiteSpace(remote)) return false;
        return allowlist.Any(x => string.Equals(x?.Trim(), remote, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<IResult> AcceptedJob(
        IAiJobQueueService queue,
        ICurrentUserService currentUser,
        CreateAiJobRequest request,
        CancellationToken ct)
    {
        var response = await queue.CreateJobAsync(request, currentUser.UserId, ct);
        return Results.Accepted($"/api/ai/jobs/{response.JobId}", response);
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

    private static string? ValidateAiCoreSettings(ConfigureAiCoreProviderRequest request)
    {
        if (!Enum.TryParse<AiCoreIntegrationMode>(request.Mode, true, out _))
            return "Invalid AiCore integration mode.";
        if (request.RequestTimeoutSeconds <= 0 || request.TimeoutSeconds <= 0)
            return "Timeout values must be positive.";
        if (request.WorkerBatchSize <= 0)
            return "Worker batch size must be positive.";
        if (request.MaxRetryCount <= 0)
            return "Max retry count must be positive.";
        if (request.RetryDelaySeconds <= 0)
            return "Retry delay must be positive.";
        if (request.CallbackTimestampToleranceSeconds <= 0)
            return "Callback timestamp tolerance must be positive.";
        if (request.StuckJobTimeoutMinutes <= 0 || request.RunningJobTimeoutMinutes <= 0)
            return "AI job timeout values must be positive.";
        if (request.CompletedJobRetentionDays < 0)
            return "Completed job retention cannot be negative.";
        if (!string.IsNullOrWhiteSpace(request.CallbackPublicUrl) &&
            !Uri.TryCreate(request.CallbackPublicUrl, UriKind.Absolute, out _))
            return "CallbackPublicUrl must be an absolute URL.";
        return null;
    }
}
