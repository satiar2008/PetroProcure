using System.Text.Json;
using Microsoft.Extensions.Logging;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Ai;

/// <summary>
/// Outcome of processing an inbound AiCore callback. Maps to HTTP results at the API layer.
/// </summary>
public enum AiCoreCallbackOutcome
{
    /// <summary>State change applied (progress updated, result stored, job failed/cancelled).</summary>
    Processed,
    /// <summary>Callback ignored because the job is already in a terminal state (idempotent replay).</summary>
    Duplicate,
    /// <summary>correlationId missing from the payload.</summary>
    MissingCorrelationId,
    /// <summary>No job matches the supplied correlationId.</summary>
    UnknownCorrelationId,
    /// <summary>Status value is not one of the supported callback statuses.</summary>
    InvalidStatus
}

public interface IAiCoreCallbackService
{
    Task<AiCoreCallbackOutcome> HandleAsync(AiCoreCallbackRequest request, CancellationToken ct);
}

/// <summary>
/// Applies asynchronous AiCore callbacks to the PetroProcure AI job queue.
/// Resilient to missing optional fields; idempotent for terminal jobs; stores raw result JSON for audit.
/// </summary>
public sealed class AiCoreCallbackService(
    IAiJobRepository jobs,
    IAiResultRepository results,
    IAiJobNotifier notifier,
    IAiAuditService audit,
    ILogger<AiCoreCallbackService> logger) : IAiCoreCallbackService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AiCoreCallbackOutcome> HandleAsync(AiCoreCallbackRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CorrelationId) && string.IsNullOrWhiteSpace(request.ExternalJobId))
        {
            logger.LogWarning("AiCore callback rejected: both correlationId and externalJobId are missing.");
            return AiCoreCallbackOutcome.MissingCorrelationId;
        }

        await audit.RecordAsync(AiAuditEvents.CallbackReceived, null, request.CorrelationId,
            $"Status={request.Status}; ExternalJobId={request.ExternalJobId}", ct);

        if (!TryParseStatus(request.Status, out var status))
        {
            logger.LogWarning("AiCore callback rejected: unknown status '{Status}' for correlation {CorrelationId}.",
                request.Status, request.CorrelationId);
            return AiCoreCallbackOutcome.InvalidStatus;
        }

        var job = !string.IsNullOrWhiteSpace(request.CorrelationId)
            ? await jobs.GetJobByCorrelationIdAsync(request.CorrelationId.Trim(), ct)
            : null;
        if (job is null && !string.IsNullOrWhiteSpace(request.ExternalJobId))
            job = await jobs.GetJobByExternalJobIdAsync(request.ExternalJobId.Trim(), ct);

        if (job is null)
        {
            logger.LogWarning("AiCore callback rejected: unknown correlation {CorrelationId} or external job {ExternalJobId}.",
                request.CorrelationId, request.ExternalJobId);
            return AiCoreCallbackOutcome.UnknownCorrelationId;
        }

        // Idempotency: once a job is terminal, replays (including duplicate Completed) are accepted as no-ops.
        if (IsTerminal(job.Status))
        {
            logger.LogInformation(
                "AiCore callback for job {JobId} ignored as duplicate; job already {JobStatus} (incoming {IncomingStatus}).",
                job.Id, job.Status, status);
            return AiCoreCallbackOutcome.Duplicate;
        }

        var utcNow = DateTime.UtcNow;
        switch (status)
        {
            case CallbackStatus.Accepted:
            {
                var progress = Math.Max(request.ProgressPercent, 50);
                var message = request.Message ?? "AiCore accepted the analysis job.";
                await jobs.MarkRunningAsync(job.Id, progress, message, ct);
                break;
            }

            case CallbackStatus.Running:
                await jobs.MarkRunningAsync(job.Id, request.ProgressPercent, request.Message, ct);
                break;

            case CallbackStatus.Completed:
                await StoreResultAsync(job, request, ct);
                await audit.RecordAsync(AiAuditEvents.JobCompleted, job.Id, job.CorrelationId,
                    request.Message ?? request.Result?.Summary, ct);
                await PublishAsync(job, AiJobEvent.Completed, "Completed", 100,
                    request.Result?.Summary ?? request.Message, true, ct);
                break;

            case CallbackStatus.Failed:
            {
                var error = ResolveError(request);
                await jobs.MarkFailedAsync(job.Id, error, utcNow, ct);
                await audit.RecordAsync(AiAuditEvents.JobFailed, job.Id, job.CorrelationId, error, ct);
                break;
            }

            case CallbackStatus.Cancelled:
            {
                var reason = request.Message ?? request.Error?.Message ?? "Cancelled by AiCore.";
                await jobs.MarkCancelledAsync(job.Id, utcNow, reason, ct);
                await audit.RecordAsync(AiAuditEvents.JobCancelled, job.Id, job.CorrelationId, reason, ct);
                break;
            }
        }

        logger.LogInformation("AiCore callback applied: job {JobId} → {IncomingStatus} ({Progress}%).",
            job.Id, status, request.ProgressPercent);
        return AiCoreCallbackOutcome.Processed;
    }

    private async Task StoreResultAsync(AiEvaluationJob job, AiCoreCallbackRequest request, CancellationToken ct)
    {
        var callbackResult = request.Result;
        var rawJson = !string.IsNullOrWhiteSpace(callbackResult?.RawResultJson)
            ? callbackResult!.RawResultJson!
            : JsonSerializer.Serialize(request, JsonOptions);

        var summary = Coalesce(callbackResult?.Summary, request.Message, "تحلیل هوش مصنوعی تکمیل شد.");
        var usage = request.Usage;

        var result = new AiEvaluationResult(
            Guid.NewGuid(),
            job.Id,
            job.EntityType,
            job.EntityId,
            job.AnalysisType,
            summary,
            rawJson,
            usage?.Model,
            usage?.Provider ?? "AiCore",
            usage?.InputTokens,
            usage?.OutputTokens,
            usage?.TotalTokens,
            usage?.DurationMs);

        foreach (var finding in callbackResult?.Findings ?? [])
        {
            if (string.IsNullOrWhiteSpace(finding.Title) || string.IsNullOrWhiteSpace(finding.Description))
                continue;
            result.AddFinding(new AiFinding(Guid.NewGuid(), result.Id, finding.Title, finding.Description,
                ToFindingSeverity(finding.Severity), finding.RelatedClauseCode ?? finding.Code, finding.RelatedDocumentId));
        }

        foreach (var recommendation in callbackResult?.Recommendations ?? [])
        {
            if (string.IsNullOrWhiteSpace(recommendation.Title) || string.IsNullOrWhiteSpace(recommendation.Description))
                continue;
            result.AddRecommendation(new AiRecommendation(Guid.NewGuid(), result.Id, recommendation.Title,
                recommendation.Description, ToRecommendationPriority(recommendation.Priority),
                recommendation.SuggestedAction ?? recommendation.RelatedAction));
        }

        // SaveResultAsync is idempotent (no duplicate result) and marks the job Completed.
        await results.SaveResultAsync(result, ct);
    }

    private Task PublishAsync(AiEvaluationJob job, AiJobEvent eventType, string status,
        int progress, string? message, bool hasResult, CancellationToken ct) =>
        notifier.PublishAsync(eventType, new AiJobNotificationDto(
            job.Id, job.EntityType, job.EntityId, job.AnalysisType,
            status, Math.Clamp(progress, 0, 100), message, hasResult, DateTime.UtcNow),
            job.CreatedByUserId, ct);

    private static string ResolveError(AiCoreCallbackRequest request) =>
        Coalesce(request.Error?.Message, request.Message, "AiCore analysis failed.");

    private static bool TryParseStatus(string? value, out CallbackStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(value)) return false;
        switch (value.Trim().ToLowerInvariant())
        {
            case "accepted": status = CallbackStatus.Accepted; return true;
            case "running": status = CallbackStatus.Running; return true;
            case "completed": status = CallbackStatus.Completed; return true;
            case "failed": status = CallbackStatus.Failed; return true;
            case "cancelled":
            case "canceled": status = CallbackStatus.Cancelled; return true;
            default: return false;
        }
    }

    private static bool IsTerminal(AiJobStatus status) =>
        status is AiJobStatus.Completed or AiJobStatus.Failed or AiJobStatus.Cancelled or AiJobStatus.Expired;

    private static AiFindingSeverity ToFindingSeverity(AiSeverity severity) => severity switch
    {
        AiSeverity.Critical => AiFindingSeverity.Critical,
        AiSeverity.High => AiFindingSeverity.High,
        AiSeverity.Medium => AiFindingSeverity.Medium,
        AiSeverity.Low => AiFindingSeverity.Low,
        _ => AiFindingSeverity.Info
    };

    private static AiRecommendationPriority ToRecommendationPriority(string? priority) =>
        Enum.TryParse<AiRecommendationPriority>(priority, ignoreCase: true, out var parsed)
            ? parsed
            : AiRecommendationPriority.Medium;

    private static string Coalesce(params string?[] values)
    {
        foreach (var value in values)
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        return "—";
    }

    private enum CallbackStatus { Accepted, Running, Completed, Failed, Cancelled }
}
