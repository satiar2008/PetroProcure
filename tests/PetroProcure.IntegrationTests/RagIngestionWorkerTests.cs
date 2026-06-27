using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PetroProcure.AI;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Rag;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Worker;
using DomainAiEvaluationJob = PetroProcure.Domain.Modules.Ai.AiEvaluationJob;

namespace PetroProcure.IntegrationTests;

public sealed class RagIngestionWorkerTests
{
    [Fact]
    public async Task FailedExtractionMarksJobFailedWithoutCrashingWorker()
    {
        var queue = new FakeQueue();
        var request = new CreateAiJobRequest(
            AiDocumentSourceType.PurchaseFileDocument.ToString(),
            Guid.NewGuid(),
            AiAnalysisType.EmbeddingIngestion.ToString(),
            Metadata: new Dictionary<string, string>
            {
                ["sourceType"] = AiDocumentSourceType.PurchaseFileDocument.ToString(),
                ["sourceId"] = Guid.NewGuid().ToString(),
                ["forceRebuild"] = "false"
            });
        var job = new DomainAiEvaluationJob(Guid.NewGuid(), "PetroProcure", request.EntityType, request.EntityId,
            request.AnalysisType, 0, Guid.NewGuid().ToString("N"),
            JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web)), 0);
        job.Claim("worker", DateTime.UtcNow);
        queue.Job = job;

        var processor = new AiJobProcessor(queue, default!, default!, default!, default!, default!, default!,
            new FailingRagIngestionService(),
            Options.Create(new AiCoreIntegrationOptions()),
            NullLogger<AiJobProcessor>.Instance);

        await processor.ProcessClaimedJobAsync(job.Id, CancellationToken.None);

        Assert.Equal(AiJobStatus.Failed, queue.Job.Status);
        Assert.Contains("extraction failed", queue.Job.ErrorMessage);
    }

    private sealed class FailingRagIngestionService : IRagIngestionService
    {
        public Task<RagIngestionResult> IngestAsync(EmbeddingIngestionPayload payload, CancellationToken ct = default) =>
            throw new InvalidOperationException("extraction failed");
    }

    private sealed class FakeQueue : IAiJobQueueService
    {
        public DomainAiEvaluationJob Job { get; set; } = default!;

        public Task<DomainAiEvaluationJob?> GetJobAsync(Guid jobId, CancellationToken ct) =>
            Task.FromResult<DomainAiEvaluationJob?>(Job.Id == jobId ? Job : null);

        public Task MarkRunningAsync(Guid jobId, int progressPercent, string? message, CancellationToken ct)
        {
            Job.MarkRunning(progressPercent, message);
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(Guid jobId, string errorMessage, DateTime utcNow, CancellationToken ct)
        {
            Job.MarkFailed(errorMessage, utcNow);
            return Task.CompletedTask;
        }

        public Task<CreateAiJobResponse> CreateJobAsync(CreateAiJobRequest request, Guid? createdByUserId, CancellationToken ct) => throw new NotSupportedException();
        public Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<AiJobStatusDto>> GetJobStatusesForEntityAsync(string entityType, Guid entityId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<DomainAiEvaluationJob>> ClaimNextJobsAsync(string workerId, int batchSize, DateTime utcNow, CancellationToken ct) => throw new NotSupportedException();
        public Task MarkBuildingContextAsync(Guid jobId, string? contextJson, CancellationToken ct) => throw new NotSupportedException();
        public Task MarkSendingToAiCoreAsync(Guid jobId, string? contextJson, CancellationToken ct) => throw new NotSupportedException();
        public Task MarkSubmittedToAiCoreAsync(Guid jobId, string externalJobId, DateTime utcNow, CancellationToken ct) => throw new NotSupportedException();
        public Task MarkCompletedAsync(Guid jobId, string? resultJson, DateTime utcNow, CancellationToken ct) => throw new NotSupportedException();
        public Task MarkCancelledAsync(Guid jobId, DateTime utcNow, string? reason, CancellationToken ct) => throw new NotSupportedException();
        public Task ReleaseForRetryAsync(Guid jobId, string errorMessage, DateTime nextRetryAtUtc, CancellationToken ct) => throw new NotSupportedException();
        public Task<int> RequeueStuckJobsAsync(DateTime lockedBeforeUtc, CancellationToken ct) => throw new NotSupportedException();
        public Task<int> ExpireStaleJobsAsync(DateTime startedBeforeUtc, DateTime submittedBeforeUtc, CancellationToken ct) => throw new NotSupportedException();
        public Task<AiJobMetricsDto> GetMetricsAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task<int> CleanupCompletedJobsAsync(DateTime completedBeforeUtc, CancellationToken ct) => throw new NotSupportedException();
    }
}
