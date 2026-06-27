using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class AiJobQueueServiceTests
{
    [Fact]
    public async Task QueueServiceRecordsProductionAuditEvents()
    {
        var repository = new InMemoryAiJobRepository();
        var audit = new RecordingAiAuditService();
        var service = new AiJobQueueService(repository, new NullAiJobNotifier(), audit);

        var created = await service.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", Guid.NewGuid(), "Summary"), Guid.NewGuid(), CancellationToken.None);
        await service.MarkSubmittedToAiCoreAsync(created.JobId, "aicore-1", DateTime.UtcNow, CancellationToken.None);
        await service.ReleaseForRetryAsync(created.JobId, "temporary outage", DateTime.UtcNow.AddMinutes(1), CancellationToken.None);
        await service.MarkCancelledAsync(created.JobId, DateTime.UtcNow, "cancelled by test", CancellationToken.None);

        Assert.Contains(AiAuditEvents.JobCreated, audit.Events);
        Assert.Contains(AiAuditEvents.JobSubmittedToAiCore, audit.Events);
        Assert.Contains(AiAuditEvents.RetryScheduled, audit.Events);
        Assert.Contains(AiAuditEvents.JobCancelled, audit.Events);
    }

    private sealed class RecordingAiAuditService : IAiAuditService
    {
        public List<string> Events { get; } = [];

        public Task RecordAsync(string eventName, Guid? jobId, string? correlationId, string? message, CancellationToken ct)
        {
            Events.Add(eventName);
            return Task.CompletedTask;
        }
    }

    private sealed class InMemoryAiJobRepository : IAiJobRepository
    {
        private readonly Dictionary<Guid, AiEvaluationJob> jobs = [];

        public Task<AiEvaluationJob> CreateJobAsync(AiEvaluationJob job, CancellationToken ct)
        {
            jobs[job.Id] = job;
            return Task.FromResult(job);
        }

        public Task<AiEvaluationJob?> GetJobAsync(Guid jobId, CancellationToken ct) =>
            Task.FromResult(jobs.GetValueOrDefault(jobId));

        public Task<AiEvaluationJob?> GetJobByCorrelationIdAsync(string correlationId, CancellationToken ct) =>
            Task.FromResult(jobs.Values.SingleOrDefault(x => x.CorrelationId == correlationId));

        public Task<AiEvaluationJob?> GetJobByExternalJobIdAsync(string externalJobId, CancellationToken ct) =>
            Task.FromResult(jobs.Values.SingleOrDefault(x => x.ExternalJobId == externalJobId));

        public Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct) =>
            Task.FromResult<AiJobStatusDto?>(null);

        public Task<IReadOnlyList<AiJobStatusDto>> GetJobStatusesForEntityAsync(string entityType, Guid entityId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AiJobStatusDto>>([]);

        public Task<IReadOnlyList<AiEvaluationJob>> ClaimNextJobsAsync(string workerId, int batchSize, DateTime utcNow, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AiEvaluationJob>>([]);

        public Task MarkBuildingContextAsync(Guid jobId, string? contextJson, CancellationToken ct)
        {
            jobs[jobId].MarkBuildingContext(contextJson);
            return Task.CompletedTask;
        }

        public Task MarkSendingToAiCoreAsync(Guid jobId, string? contextJson, CancellationToken ct)
        {
            jobs[jobId].MarkSendingToAiCore(contextJson);
            return Task.CompletedTask;
        }

        public Task MarkSubmittedToAiCoreAsync(Guid jobId, string externalJobId, DateTime utcNow, CancellationToken ct)
        {
            jobs[jobId].MarkSubmittedToAiCore(externalJobId, utcNow);
            return Task.CompletedTask;
        }

        public Task MarkRunningAsync(Guid jobId, int progressPercent, string? message, CancellationToken ct)
        {
            jobs[jobId].MarkRunning(progressPercent, message);
            return Task.CompletedTask;
        }

        public Task MarkCompletedAsync(Guid jobId, string? resultJson, DateTime utcNow, CancellationToken ct)
        {
            jobs[jobId].MarkCompleted(resultJson, utcNow);
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(Guid jobId, string errorMessage, DateTime utcNow, CancellationToken ct)
        {
            jobs[jobId].MarkFailed(errorMessage, utcNow);
            return Task.CompletedTask;
        }

        public Task MarkCancelledAsync(Guid jobId, DateTime utcNow, string? reason, CancellationToken ct)
        {
            jobs[jobId].MarkCancelled(utcNow, reason);
            return Task.CompletedTask;
        }

        public Task ReleaseForRetryAsync(Guid jobId, string errorMessage, DateTime nextRetryAtUtc, CancellationToken ct)
        {
            jobs[jobId].ReleaseForRetry(errorMessage, nextRetryAtUtc);
            return Task.CompletedTask;
        }

        public Task<int> RequeueStuckJobsAsync(DateTime lockedBeforeUtc, CancellationToken ct) =>
            Task.FromResult(0);

        public Task<int> ExpireStaleJobsAsync(DateTime startedBeforeUtc, DateTime submittedBeforeUtc, CancellationToken ct) =>
            Task.FromResult(0);

        public Task<AiJobMetricsDto> GetMetricsAsync(CancellationToken ct) =>
            Task.FromResult(new AiJobMetricsDto(0, 0, 0, 0, 0, 0));

        public Task<int> CleanupCompletedJobsAsync(DateTime completedBeforeUtc, CancellationToken ct) =>
            Task.FromResult(0);
    }
}
