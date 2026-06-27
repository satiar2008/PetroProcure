using System.Text.Json;
using Microsoft.Extensions.Options;
using PetroProcure.AI;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Rag;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;
using DomainAiEvaluationJob = PetroProcure.Domain.Modules.Ai.AiEvaluationJob;
using DomainAiEvaluationResult = PetroProcure.Domain.Modules.Ai.AiEvaluationResult;
using DomainAiFinding = PetroProcure.Domain.Modules.Ai.AiFinding;
using DomainAiRecommendation = PetroProcure.Domain.Modules.Ai.AiRecommendation;
using LegacyAiContextBuilder = PetroProcure.AI.IAiContextBuilder;
using PurchaseFileAiContextBuilder = PetroProcure.Application.Ai.IPurchaseFileAiContextBuilder;

namespace PetroProcure.Worker;

public sealed class AiJobProcessor(
    IAiJobQueueService queue,
    IAiResultRepository results,
    PurchaseFileAiContextBuilder purchaseFileContextBuilder,
    LegacyAiContextBuilder legacyContextBuilder,
    IAiCoreJobClient aiCoreJobClient,
    ILocalOllamaAnalysisClient localOllamaClient,
    IAiCoreClient syncAiCoreClient,
    IRagIngestionService ragIngestion,
    IOptions<AiCoreIntegrationOptions> aiCoreOptions,
    ILogger<AiJobProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string AdvisorySystemPrompt =
        "You are an advisory refinery procurement analysis assistant. Provide summaries, findings, warnings, and recommendations only. Never make final business decisions.";

    public async Task<int> ProcessBatchAsync(string workerId, int batchSize, CancellationToken ct)
    {
        var jobs = await queue.ClaimNextJobsAsync(workerId, batchSize, DateTime.UtcNow, ct);
        foreach (var job in jobs)
            await ProcessClaimedJobAsync(job.Id, ct);

        return jobs.Count;
    }

    public async Task ProcessClaimedJobAsync(Guid jobId, CancellationToken ct)
    {
        try
        {
            var job = await queue.GetJobAsync(jobId, ct) ?? throw new InvalidOperationException("AI job was not found.");
            var request = DeserializeRequest(job.RequestJson);
            if (IsEmbeddingIngestion(request))
            {
                await RunEmbeddingIngestionAsync(job, request, ct);
                return;
            }

            await queue.MarkBuildingContextAsync(job.Id, null, ct);
            var context = await BuildContextAsync(request, ct);
            var contextJson = JsonSerializer.Serialize(context, JsonOptions);
            await queue.MarkSendingToAiCoreAsync(job.Id, contextJson, ct);

            if (aiCoreOptions.Value.Mode == AiCoreIntegrationMode.LocalOllamaWorker)
            {
                await RunLocalOllamaWorkerAsync(job, request, context, ct);
                return;
            }

            if (aiCoreOptions.Value.Mode == AiCoreIntegrationMode.SyncAiCoreDirect)
            {
                await RunSyncFallbackAsync(job, request, context, ct);
                return;
            }

            if (aiCoreOptions.Value.Mode != AiCoreIntegrationMode.AsyncAiCoreJob)
                throw new InvalidOperationException($"AI integration mode {aiCoreOptions.Value.Mode} is not supported by the worker dispatcher.");

            var submit = BuildSubmitRequest(job, request, context);
            var submitResponse = await SubmitWithRetryAsync(submit, ct);
            await queue.MarkSubmittedToAiCoreAsync(job.Id, submitResponse.ExternalJobId, submitResponse.AcceptedAtUtc, ct);

            if (submitResponse.Status.Equals("Running", StringComparison.OrdinalIgnoreCase))
                await queue.MarkRunningAsync(job.Id, 55, submitResponse.Message, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "AI job {JobId} processing failed.", jobId);
            var job = await queue.GetJobAsync(jobId, ct);
            if (job is not null && IsEmbeddingIngestion(job.AnalysisType))
                await queue.MarkFailedAsync(jobId, ex.Message, DateTime.UtcNow, ct);
            else
                await ReleaseOrFailAsync(jobId, ex.Message, ct);
        }
    }

    private async Task RunEmbeddingIngestionAsync(DomainAiEvaluationJob job, CreateAiJobRequest request, CancellationToken ct)
    {
        var payload = RagIngestionService.ParsePayload(request);
        logger.LogInformation("Processing embedding ingestion job {JobId} for {SourceType}/{SourceId}.",
            job.Id, payload.SourceType, payload.SourceId);
        await queue.MarkRunningAsync(job.Id, 20, "RAG ingestion started.", ct);
        var result = await ragIngestion.IngestAsync(payload, ct);
        await queue.MarkCompletedAsync(job.Id, RagIngestionService.SerializeResult(result), DateTime.UtcNow, ct);
        logger.LogInformation("Embedding ingestion job {JobId} completed: {ChunkCount} chunks, {EmbeddingCount} embeddings, {Skipped} skipped.",
            job.Id, result.ChunkCount, result.EmbeddingCount, result.SkippedEmbeddingCount);
    }

    public async Task RunMaintenanceAsync(CancellationToken ct)
    {
        var options = aiCoreOptions.Value;
        var now = DateTime.UtcNow;
        await queue.RequeueStuckJobsAsync(
            now.AddMinutes(-Math.Max(1, options.StuckJobTimeoutMinutes)),
            ct);
        await queue.ExpireStaleJobsAsync(
            now.AddMinutes(-Math.Max(1, options.RunningJobTimeoutMinutes)),
            now.AddMinutes(-Math.Max(1, options.RunningJobTimeoutMinutes)),
            ct);
        if (options.CompletedJobRetentionDays > 0)
        {
            await queue.CleanupCompletedJobsAsync(
                now.AddDays(-options.CompletedJobRetentionDays),
                ct);
        }
    }

    private async Task<object> BuildContextAsync(CreateAiJobRequest request, CancellationToken ct)
    {
        if (request.EntityType.Equals("PurchaseFile", StringComparison.OrdinalIgnoreCase))
        {
            var analysisType = Enum.TryParse<AiAnalysisType>(request.AnalysisType, true, out var parsed)
                ? parsed
                : AiAnalysisType.Summary;
            return await purchaseFileContextBuilder.BuildAsync(request.EntityId, analysisType, ct);
        }

        return request.EntityType switch
        {
            "Tender" => await legacyContextBuilder.BuildTenderContextAsync(request.EntityId, ct),
            "Contract" => await legacyContextBuilder.BuildContractContextAsync(request.EntityId, ct),
            "PurchaseOrder" => await legacyContextBuilder.BuildPurchaseOrderContextAsync(request.EntityId, ct),
            "WarehouseReceipt" => await legacyContextBuilder.BuildWarehouseReceiptContextAsync(request.EntityId, ct),
            _ => throw new InvalidOperationException($"AI context builder is not available for entity type {request.EntityType}.")
        };
    }

    private AiCoreSubmitJobRequest BuildSubmitRequest(DomainAiEvaluationJob job, CreateAiJobRequest request, object context)
    {
        var metadata = new Dictionary<string, string>(request.Metadata ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase)
        {
            ["petroProcureJobId"] = job.Id.ToString(),
            ["advisoryOnly"] = "true"
        };

        return new AiCoreSubmitJobRequest(
            job.CorrelationId,
            job.SourceSystem,
            request.EntityType,
            request.EntityId,
            request.AnalysisType,
            BuildCallbackUrl(),
            request.RequestedModel ?? aiCoreOptions.Value.DefaultModel,
            [
                new AiCoreMessageDto("system", AdvisorySystemPrompt),
                new AiCoreMessageDto("user", string.IsNullOrWhiteSpace(request.UserQuestion)
                    ? request.AnalysisType
                    : request.UserQuestion)
            ],
            context,
            metadata);
    }

    private async Task<AiCoreSubmitJobResponse> SubmitWithRetryAsync(AiCoreSubmitJobRequest submit, CancellationToken ct)
    {
        var maxAttempts = Math.Max(1, aiCoreOptions.Value.MaxRetryCount);
        Exception? last = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await aiCoreJobClient.SubmitJobAsync(submit, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
            {
                last = ex;
                if (attempt == maxAttempts)
                    break;
                var delaySeconds = Math.Max(1, aiCoreOptions.Value.RetryDelaySeconds) * Math.Pow(2, attempt - 1);
                logger.LogWarning(ex, "AI job submit attempt {Attempt}/{MaxAttempts} failed for correlation {CorrelationId}. Retrying.",
                    attempt, maxAttempts, submit.CorrelationId);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
            }
        }

        throw new AiCoreClientException($"AiCore job submission failed after retry limit: {last!.Message}", last);
    }

    private async Task RunSyncFallbackAsync(
        DomainAiEvaluationJob job,
        CreateAiJobRequest request,
        object context,
        CancellationToken ct)
    {
        var response = await syncAiCoreClient.SendAnalysisAsync(new AiCoreAnalysisRequest(
            job.CorrelationId,
            job.SourceSystem,
            request.EntityType,
            request.EntityId,
            request.AnalysisType,
            request.RequestedModel ?? aiCoreOptions.Value.DefaultModel,
            [new AiCoreMessage("system", AdvisorySystemPrompt),
                new AiCoreMessage("user", string.IsNullOrWhiteSpace(request.UserQuestion) ? request.AnalysisType : request.UserQuestion)],
            context,
            request.Metadata), ct);

        await StoreAnalysisResponseAsync(job, request, response, "AiCore", ct);
    }

    private async Task RunLocalOllamaWorkerAsync(
        DomainAiEvaluationJob job,
        CreateAiJobRequest request,
        object context,
        CancellationToken ct)
    {
        await queue.MarkRunningAsync(job.Id, 55, "Local Ollama is analyzing the job.", ct);
        var response = await localOllamaClient.AnalyzeAsync(new AiCoreAnalysisRequest(
            job.CorrelationId,
            job.SourceSystem,
            request.EntityType,
            request.EntityId,
            request.AnalysisType,
            request.RequestedModel ?? aiCoreOptions.Value.DefaultModel,
            [new AiCoreMessage("system", AdvisorySystemPrompt),
                new AiCoreMessage("user", string.IsNullOrWhiteSpace(request.UserQuestion) ? request.AnalysisType : request.UserQuestion)],
            context,
            request.Metadata), ct);

        await StoreAnalysisResponseAsync(job, request, response, "Ollama", ct);
    }

    private async Task StoreAnalysisResponseAsync(
        DomainAiEvaluationJob job,
        CreateAiJobRequest request,
        AiCoreAnalysisResponse response,
        string defaultProvider,
        CancellationToken ct)
    {
        var rawJson = JsonSerializer.Serialize(response, JsonOptions);
        var result = new DomainAiEvaluationResult(
            Guid.NewGuid(),
            job.Id,
            request.EntityType,
            request.EntityId,
            request.AnalysisType,
            response.Summary,
            rawJson,
            response.Usage?.Model,
            response.Usage?.Provider ?? defaultProvider,
            response.Usage?.InputTokens ?? 0,
            response.Usage?.OutputTokens ?? 0,
            response.Usage?.TotalTokens ?? 0,
            response.Usage?.DurationMs ?? 0);

        foreach (var finding in response.Findings)
            result.AddFinding(new DomainAiFinding(Guid.NewGuid(), result.Id, finding.Title, finding.Description,
                ToSeverity(finding.Severity), finding.Code));

        foreach (var recommendation in response.Recommendations)
            result.AddRecommendation(new DomainAiRecommendation(Guid.NewGuid(), result.Id, recommendation.Title,
                recommendation.Description, ToPriority(recommendation.Severity), recommendation.RelatedAction));

        await results.SaveResultAsync(result, ct);
    }

    private async Task ReleaseOrFailAsync(Guid jobId, string errorMessage, CancellationToken ct)
    {
        var job = await queue.GetJobAsync(jobId, ct);
        if (job is null || job.Status is AiJobStatus.Completed or AiJobStatus.Failed or AiJobStatus.Cancelled or AiJobStatus.Expired)
            return;

        if (job.CanRetry)
        {
            var delaySeconds = Math.Max(1, aiCoreOptions.Value.RetryDelaySeconds) * Math.Pow(2, job.RetryCount);
            await queue.ReleaseForRetryAsync(jobId, errorMessage, DateTime.UtcNow.AddSeconds(delaySeconds), ct);
            return;
        }

        await queue.MarkFailedAsync(jobId, errorMessage, DateTime.UtcNow, ct);
    }

    private string BuildCallbackUrl()
    {
        var configuredUrl = aiCoreOptions.Value.CallbackPublicUrl;
        if (string.IsNullOrWhiteSpace(configuredUrl))
            return string.Empty;

        if (!Uri.TryCreate(configuredUrl, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("AiCore CallbackPublicUrl must be an absolute http/https URL.");

        if (uri.Scheme is not ("http" or "https"))
            throw new InvalidOperationException("AiCore CallbackPublicUrl must use http or https.");

        var path = uri.AbsolutePath.TrimEnd('/');
        return string.IsNullOrWhiteSpace(path) || path == "/"
            ? $"{configuredUrl.TrimEnd('/')}/api/ai/aicore/callback"
            : configuredUrl.TrimEnd('/');
    }

    private static CreateAiJobRequest DeserializeRequest(string requestJson) =>
        JsonSerializer.Deserialize<CreateAiJobRequest>(requestJson, JsonOptions)
        ?? throw new InvalidOperationException("AI job request payload is invalid.");

    private static bool IsEmbeddingIngestion(CreateAiJobRequest request) => IsEmbeddingIngestion(request.AnalysisType);
    private static bool IsEmbeddingIngestion(string analysisType) =>
        analysisType.Equals(AiAnalysisType.EmbeddingIngestion.ToString(), StringComparison.OrdinalIgnoreCase);

    private static AiFindingSeverity ToSeverity(string severity) =>
        Enum.TryParse<AiFindingSeverity>(severity, true, out var parsed) ? parsed : AiFindingSeverity.Info;

    private static AiRecommendationPriority ToPriority(string priority) =>
        Enum.TryParse<AiRecommendationPriority>(priority, true, out var parsed) ? parsed : AiRecommendationPriority.Medium;
}
