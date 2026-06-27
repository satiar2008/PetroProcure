using System.Text.Json;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Ai;

public interface IAiJobRepository
{
    Task<AiEvaluationJob> CreateJobAsync(AiEvaluationJob job, CancellationToken ct);
    Task<AiEvaluationJob?> GetJobAsync(Guid jobId, CancellationToken ct);
    Task<AiEvaluationJob?> GetJobByCorrelationIdAsync(string correlationId, CancellationToken ct);
    Task<AiEvaluationJob?> GetJobByExternalJobIdAsync(string externalJobId, CancellationToken ct);
    Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct);
    Task<IReadOnlyList<AiJobStatusDto>> GetJobStatusesForEntityAsync(string entityType, Guid entityId, CancellationToken ct);
    Task<IReadOnlyList<AiEvaluationJob>> ClaimNextJobsAsync(string workerId, int batchSize, DateTime utcNow, CancellationToken ct);
    Task MarkBuildingContextAsync(Guid jobId, string? contextJson, CancellationToken ct);
    Task MarkSendingToAiCoreAsync(Guid jobId, string? contextJson, CancellationToken ct);
    Task MarkSubmittedToAiCoreAsync(Guid jobId, string externalJobId, DateTime utcNow, CancellationToken ct);
    Task MarkRunningAsync(Guid jobId, int progressPercent, string? message, CancellationToken ct);
    Task MarkCompletedAsync(Guid jobId, string? resultJson, DateTime utcNow, CancellationToken ct);
    Task MarkFailedAsync(Guid jobId, string errorMessage, DateTime utcNow, CancellationToken ct);
    Task MarkCancelledAsync(Guid jobId, DateTime utcNow, string? reason, CancellationToken ct);
    Task ReleaseForRetryAsync(Guid jobId, string errorMessage, DateTime nextRetryAtUtc, CancellationToken ct);
    Task<int> RequeueStuckJobsAsync(DateTime lockedBeforeUtc, CancellationToken ct);
    Task<int> ExpireStaleJobsAsync(DateTime startedBeforeUtc, DateTime submittedBeforeUtc, CancellationToken ct);
    Task<AiJobMetricsDto> GetMetricsAsync(CancellationToken ct);
    Task<int> CleanupCompletedJobsAsync(DateTime completedBeforeUtc, CancellationToken ct);
}

public interface IAiResultRepository
{
    Task<AiEvaluationResult> SaveResultAsync(AiEvaluationResult result, CancellationToken ct);
    Task<AiJobResultDto?> GetResultAsync(Guid jobId, CancellationToken ct);
}

public interface IAiJobQueueService
{
    Task<CreateAiJobResponse> CreateJobAsync(CreateAiJobRequest request, Guid? createdByUserId, CancellationToken ct);
    Task<AiEvaluationJob?> GetJobAsync(Guid jobId, CancellationToken ct);
    Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct);
    Task<IReadOnlyList<AiJobStatusDto>> GetJobStatusesForEntityAsync(string entityType, Guid entityId, CancellationToken ct);
    Task<IReadOnlyList<AiEvaluationJob>> ClaimNextJobsAsync(string workerId, int batchSize, DateTime utcNow, CancellationToken ct);
    Task MarkBuildingContextAsync(Guid jobId, string? contextJson, CancellationToken ct);
    Task MarkSendingToAiCoreAsync(Guid jobId, string? contextJson, CancellationToken ct);
    Task MarkSubmittedToAiCoreAsync(Guid jobId, string externalJobId, DateTime utcNow, CancellationToken ct);
    Task MarkRunningAsync(Guid jobId, int progressPercent, string? message, CancellationToken ct);
    Task MarkCompletedAsync(Guid jobId, string? resultJson, DateTime utcNow, CancellationToken ct);
    Task MarkFailedAsync(Guid jobId, string errorMessage, DateTime utcNow, CancellationToken ct);
    Task MarkCancelledAsync(Guid jobId, DateTime utcNow, string? reason, CancellationToken ct);
    Task ReleaseForRetryAsync(Guid jobId, string errorMessage, DateTime nextRetryAtUtc, CancellationToken ct);
    Task<int> RequeueStuckJobsAsync(DateTime lockedBeforeUtc, CancellationToken ct);
    Task<int> ExpireStaleJobsAsync(DateTime startedBeforeUtc, DateTime submittedBeforeUtc, CancellationToken ct);
    Task<AiJobMetricsDto> GetMetricsAsync(CancellationToken ct);
    Task<int> CleanupCompletedJobsAsync(DateTime completedBeforeUtc, CancellationToken ct);
}

public sealed class AiJobQueueService(
    IAiJobRepository repository,
    IAiJobNotifier notifier,
    IAiAuditService audit) : IAiJobQueueService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<CreateAiJobResponse> CreateJobAsync(CreateAiJobRequest request, Guid? createdByUserId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var job = new AiEvaluationJob(
            Guid.NewGuid(),
            "PetroProcure",
            request.EntityType,
            request.EntityId,
            request.AnalysisType,
            request.Priority,
            Guid.NewGuid().ToString("N"),
            JsonSerializer.Serialize(request, JsonOptions),
            3,
            createdByUserId);

        var created = await repository.CreateJobAsync(job, ct);
        await audit.RecordAsync(AiAuditEvents.JobCreated, created.Id, created.CorrelationId,
            $"{created.EntityType}/{created.EntityId}:{created.AnalysisType}", ct);
        await notifier.PublishAsync(AiJobEvent.Created, new AiJobNotificationDto(
            created.Id, created.EntityType, created.EntityId, created.AnalysisType,
            created.Status.ToString(), created.ProgressPercent, "AI analysis job queued.",
            false, DateTime.UtcNow), created.CreatedByUserId, ct);
        return new CreateAiJobResponse(created.Id, created.Status.ToString(),
            "AI analysis job queued.", created.CreatedAtUtc);
    }

    public Task<AiEvaluationJob?> GetJobAsync(Guid jobId, CancellationToken ct) =>
        repository.GetJobAsync(jobId, ct);

    public Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct) =>
        repository.GetJobStatusAsync(jobId, ct);

    public Task<IReadOnlyList<AiJobStatusDto>> GetJobStatusesForEntityAsync(string entityType, Guid entityId, CancellationToken ct) =>
        repository.GetJobStatusesForEntityAsync(entityType, entityId, ct);

    public Task<IReadOnlyList<AiEvaluationJob>> ClaimNextJobsAsync(string workerId, int batchSize, DateTime utcNow, CancellationToken ct) =>
        repository.ClaimNextJobsAsync(workerId, batchSize, utcNow, ct);

    public Task MarkBuildingContextAsync(Guid jobId, string? contextJson, CancellationToken ct) =>
        repository.MarkBuildingContextAsync(jobId, contextJson, ct);

    public Task MarkSendingToAiCoreAsync(Guid jobId, string? contextJson, CancellationToken ct) =>
        repository.MarkSendingToAiCoreAsync(jobId, contextJson, ct);

    public async Task MarkSubmittedToAiCoreAsync(Guid jobId, string externalJobId, DateTime utcNow, CancellationToken ct)
    {
        await repository.MarkSubmittedToAiCoreAsync(jobId, externalJobId, utcNow, ct);
        await audit.RecordAsync(AiAuditEvents.JobSubmittedToAiCore, jobId, null,
            $"ExternalJobId={externalJobId}", ct);
        await PublishStatusAsync(jobId, AiJobEvent.StatusChanged, "SubmittedToAiCore", null, ct);
    }

    public async Task MarkRunningAsync(Guid jobId, int progressPercent, string? message, CancellationToken ct)
    {
        await repository.MarkRunningAsync(jobId, progressPercent, message, ct);
        await PublishStatusAsync(jobId, AiJobEvent.StatusChanged, "Running", message, ct);
    }

    public Task MarkCompletedAsync(Guid jobId, string? resultJson, DateTime utcNow, CancellationToken ct) =>
        repository.MarkCompletedAsync(jobId, resultJson, utcNow, ct);

    public async Task MarkFailedAsync(Guid jobId, string errorMessage, DateTime utcNow, CancellationToken ct)
    {
        await repository.MarkFailedAsync(jobId, errorMessage, utcNow, ct);
        await audit.RecordAsync(AiAuditEvents.JobFailed, jobId, null, errorMessage, ct);
        await PublishStatusAsync(jobId, AiJobEvent.Failed, "Failed", errorMessage, ct);
    }

    public async Task MarkCancelledAsync(Guid jobId, DateTime utcNow, string? reason, CancellationToken ct)
    {
        await repository.MarkCancelledAsync(jobId, utcNow, reason, ct);
        await audit.RecordAsync(AiAuditEvents.JobCancelled, jobId, null, reason, ct);
        await PublishStatusAsync(jobId, AiJobEvent.Cancelled, "Cancelled", reason, ct);
    }

    public async Task ReleaseForRetryAsync(Guid jobId, string errorMessage, DateTime nextRetryAtUtc, CancellationToken ct)
    {
        await repository.ReleaseForRetryAsync(jobId, errorMessage, nextRetryAtUtc, ct);
        await audit.RecordAsync(AiAuditEvents.RetryScheduled, jobId, null,
            $"{errorMessage}; nextRetryAtUtc={nextRetryAtUtc:O}", ct);
    }

    public async Task<int> RequeueStuckJobsAsync(DateTime lockedBeforeUtc, CancellationToken ct)
    {
        var count = await repository.RequeueStuckJobsAsync(lockedBeforeUtc, ct);
        if (count > 0)
            await audit.RecordAsync(AiAuditEvents.StuckJobRequeued, null, null,
                $"Requeued {count} stuck AI jobs locked before {lockedBeforeUtc:O}", ct);
        return count;
    }

    public async Task<int> ExpireStaleJobsAsync(DateTime startedBeforeUtc, DateTime submittedBeforeUtc, CancellationToken ct)
    {
        var count = await repository.ExpireStaleJobsAsync(startedBeforeUtc, submittedBeforeUtc, ct);
        if (count > 0)
            await audit.RecordAsync(AiAuditEvents.JobExpired, null, null,
                $"Expired {count} stale AI jobs", ct);
        return count;
    }

    public Task<AiJobMetricsDto> GetMetricsAsync(CancellationToken ct) =>
        repository.GetMetricsAsync(ct);

    public Task<int> CleanupCompletedJobsAsync(DateTime completedBeforeUtc, CancellationToken ct) =>
        repository.CleanupCompletedJobsAsync(completedBeforeUtc, ct);

    private async Task PublishStatusAsync(Guid jobId, AiJobEvent eventType, string fallbackStatus, string? message, CancellationToken ct)
    {
        var job = await repository.GetJobAsync(jobId, ct);
        if (job is null) return;

        await notifier.PublishAsync(eventType, new AiJobNotificationDto(
            job.Id, job.EntityType, job.EntityId, job.AnalysisType,
            string.IsNullOrWhiteSpace(job.Status.ToString()) ? fallbackStatus : job.Status.ToString(),
            job.ProgressPercent, message ?? job.ErrorMessage ?? job.Status.ToString(),
            job.Status == AiJobStatus.Completed, DateTime.UtcNow),
            job.CreatedByUserId, ct);
    }
}

public sealed record AiJobMetricsDto(
    int Queued,
    int Running,
    int Failed,
    int Completed,
    int Cancelled,
    int Expired);
