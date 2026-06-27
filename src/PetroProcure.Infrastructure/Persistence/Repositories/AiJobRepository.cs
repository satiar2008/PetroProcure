using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class AiJobRepository(PetroProcureDbContext db) : IAiJobRepository, IAiResultRepository
{
    public async Task<AiEvaluationJob> CreateJobAsync(AiEvaluationJob job, CancellationToken ct)
    {
        db.AiEvaluationJobs.Add(job);
        await db.SaveChangesAsync(ct);
        return job;
    }

    public Task<AiEvaluationJob?> GetJobAsync(Guid jobId, CancellationToken ct) =>
        db.AiEvaluationJobs.SingleOrDefaultAsync(x => x.Id == jobId, ct);

    public Task<AiEvaluationJob?> GetJobByCorrelationIdAsync(string correlationId, CancellationToken ct) =>
        db.AiEvaluationJobs.SingleOrDefaultAsync(x => x.CorrelationId == correlationId, ct);

    public Task<AiEvaluationJob?> GetJobByExternalJobIdAsync(string externalJobId, CancellationToken ct) =>
        db.AiEvaluationJobs
            .OrderByDescending(x => x.SubmittedToAiCoreAtUtc ?? x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.ExternalJobId == externalJobId, ct);

    public async Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct)
    {
        var job = await db.AiEvaluationJobs.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == jobId, ct);
        if (job is null) return null;
        var hasResult = await db.AiEvaluationResults.AsNoTracking().AnyAsync(x => x.JobId == jobId, ct);
        return ToStatusDto(job, hasResult);
    }

    public async Task<IReadOnlyList<AiJobStatusDto>> GetJobStatusesForEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct)
    {
        var jobs = await db.AiEvaluationJobs.AsNoTracking()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

        var jobIds = jobs.Select(x => x.Id).ToArray();
        var resultJobIds = await db.AiEvaluationResults.AsNoTracking()
            .Where(x => jobIds.Contains(x.JobId))
            .Select(x => x.JobId)
            .ToHashSetAsync(ct);

        return jobs.Select(x => ToStatusDto(x, resultJobIds.Contains(x.Id))).ToArray();
    }

    public async Task<IReadOnlyList<AiEvaluationJob>> ClaimNextJobsAsync(string workerId, int batchSize,
        DateTime utcNow, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(workerId)) throw new ArgumentException("Worker id is required.", nameof(workerId));
        if (batchSize <= 0) return [];

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var jobs = await db.AiEvaluationJobs
                .Where(x => x.Status == AiJobStatus.Queued &&
                    (x.NextRetryAtUtc == null || x.NextRetryAtUtc <= utcNow))
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.CreatedAtUtc)
                .Take(batchSize)
                .ToListAsync(ct);

            foreach (var job in jobs)
                job.Claim(workerId, utcNow);

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return jobs;
        });
    }

    public async Task MarkBuildingContextAsync(Guid jobId, string? contextJson, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkBuildingContext(contextJson);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkSendingToAiCoreAsync(Guid jobId, string? contextJson, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkSendingToAiCore(contextJson);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkSubmittedToAiCoreAsync(Guid jobId, string externalJobId, DateTime utcNow, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkSubmittedToAiCore(externalJobId, utcNow);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkRunningAsync(Guid jobId, int progressPercent, string? message, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkRunning(progressPercent, message);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkCompletedAsync(Guid jobId, string? resultJson, DateTime utcNow, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkCompleted(resultJson, utcNow);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid jobId, string errorMessage, DateTime utcNow, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkFailed(errorMessage, utcNow);
        await db.SaveChangesAsync(ct);
    }

    public async Task MarkCancelledAsync(Guid jobId, DateTime utcNow, string? reason, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.MarkCancelled(utcNow, reason);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReleaseForRetryAsync(Guid jobId, string errorMessage, DateTime nextRetryAtUtc, CancellationToken ct)
    {
        var job = await LoadJobAsync(jobId, ct);
        job.ReleaseForRetry(errorMessage, nextRetryAtUtc);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> RequeueStuckJobsAsync(DateTime lockedBeforeUtc, CancellationToken ct)
    {
        var statuses = new[]
        {
            AiJobStatus.Claimed,
            AiJobStatus.BuildingContext,
            AiJobStatus.SendingToAiCore
        };

        return await db.AiEvaluationJobs
            .Where(x => statuses.Contains(x.Status) &&
                x.LockedAtUtc != null &&
                x.LockedAtUtc <= lockedBeforeUtc)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, AiJobStatus.Queued)
                .SetProperty(x => x.LockedBy, (string?)null)
                .SetProperty(x => x.LockedAtUtc, (DateTime?)null)
                .SetProperty(x => x.NextRetryAtUtc, DateTime.UtcNow)
                .SetProperty(x => x.ErrorMessage, "Requeued after worker lock timeout."),
                ct);
    }

    public async Task<int> ExpireStaleJobsAsync(DateTime startedBeforeUtc, DateTime submittedBeforeUtc, CancellationToken ct)
    {
        var runningStatuses = new[] { AiJobStatus.Running, AiJobStatus.SubmittedToAiCore };
        return await db.AiEvaluationJobs
            .Where(x =>
                (x.Status == AiJobStatus.Running && x.StartedAtUtc != null && x.StartedAtUtc <= startedBeforeUtc) ||
                (x.Status == AiJobStatus.SubmittedToAiCore && x.SubmittedToAiCoreAtUtc != null && x.SubmittedToAiCoreAtUtc <= submittedBeforeUtc))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, AiJobStatus.Expired)
                .SetProperty(x => x.CompletedAtUtc, DateTime.UtcNow)
                .SetProperty(x => x.LockedBy, (string?)null)
                .SetProperty(x => x.LockedAtUtc, (DateTime?)null)
                .SetProperty(x => x.ErrorMessage, "AI job expired after timeout."),
                ct);
    }

    public async Task<AiJobMetricsDto> GetMetricsAsync(CancellationToken ct)
    {
        var rows = await db.AiEvaluationJobs.AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(ct);

        int Count(params AiJobStatus[] statuses) => rows
            .Where(x => statuses.Contains(x.Status))
            .Sum(x => x.Count);

        return new AiJobMetricsDto(
            Count(AiJobStatus.Queued, AiJobStatus.Claimed, AiJobStatus.BuildingContext, AiJobStatus.SendingToAiCore),
            Count(AiJobStatus.SubmittedToAiCore, AiJobStatus.Running),
            Count(AiJobStatus.Failed),
            Count(AiJobStatus.Completed),
            Count(AiJobStatus.Cancelled),
            Count(AiJobStatus.Expired));
    }

    public async Task<int> CleanupCompletedJobsAsync(DateTime completedBeforeUtc, CancellationToken ct)
    {
        // Keep AiEvaluationResult history for purchase files; only archive queue rows that have no result row.
        return await db.AiEvaluationJobs
            .Where(x => x.Status == AiJobStatus.Completed &&
                x.CompletedAtUtc != null &&
                x.CompletedAtUtc <= completedBeforeUtc &&
                !db.AiEvaluationResults.Any(r => r.JobId == x.Id))
            .ExecuteDeleteAsync(ct);
    }

    public async Task<AiEvaluationResult> SaveResultAsync(AiEvaluationResult result, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var existing = await db.AiEvaluationResults
                .Include(x => x.Findings)
                .Include(x => x.Recommendations)
                .SingleOrDefaultAsync(x => x.JobId == result.JobId, ct);
            if (existing is not null)
            {
                await tx.CommitAsync(ct);
                return existing;
            }

            var job = await LoadJobAsync(result.JobId, ct);
            db.AiEvaluationResults.Add(result);
            job.MarkCompleted(result.RawResultJson, DateTime.UtcNow);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return result;
        });
    }

    public async Task<AiJobResultDto?> GetResultAsync(Guid jobId, CancellationToken ct)
    {
        var result = await db.AiEvaluationResults.AsNoTracking()
            .Include(x => x.Findings)
            .Include(x => x.Recommendations)
            .SingleOrDefaultAsync(x => x.JobId == jobId, ct);
        return result is null ? null : ToResultDto(result);
    }

    private async Task<AiEvaluationJob> LoadJobAsync(Guid jobId, CancellationToken ct) =>
        await db.AiEvaluationJobs.SingleOrDefaultAsync(x => x.Id == jobId, ct)
        ?? throw new InvalidOperationException("AI job was not found.");

    private static AiJobStatusDto ToStatusDto(AiEvaluationJob job, bool hasResult) =>
        new(job.Id, job.EntityType, job.EntityId, job.AnalysisType, job.Status.ToString(),
            job.ProgressPercent, job.ErrorMessage ?? job.Status.ToString(), job.ExternalJobId,
            job.CreatedAtUtc, job.StartedAtUtc, job.CompletedAtUtc, job.ErrorMessage, hasResult);

    private static AiJobResultDto ToResultDto(AiEvaluationResult result) =>
        new(result.JobId, "Completed", result.Summary,
            result.Findings.Select(x => new AiFindingDto(x.Title, x.Description,
                ToContractSeverity(x.Severity), x.RelatedClauseCode, x.RelatedDocumentId)).ToArray(),
            result.Recommendations.Select(x => new AiRecommendationDto(x.Title, x.Description,
                x.Priority.ToString(), x.SuggestedAction)).ToArray(),
            result.RawResultJson,
            new AiUsageDto(result.InputTokens, result.OutputTokens, null, result.TotalTokens,
                result.DurationMs, result.Model, result.Provider),
            result.CreatedAtUtc);

    private static AiSeverity ToContractSeverity(AiFindingSeverity severity) => severity switch
    {
        AiFindingSeverity.Critical => AiSeverity.Critical,
        AiFindingSeverity.High => AiSeverity.High,
        AiFindingSeverity.Medium => AiSeverity.Medium,
        AiFindingSeverity.Low => AiSeverity.Low,
        _ => AiSeverity.Info
    };
}
