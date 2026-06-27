using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PetroProcure.AI;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Rag;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Infrastructure.Persistence.Seeding;
using PetroProcure.Worker;
using LegacyAiContextBuilder = PetroProcure.AI.IAiContextBuilder;
using PurchaseFileAiContextBuilder = PetroProcure.Application.Ai.IPurchaseFileAiContextBuilder;

namespace PetroProcure.IntegrationTests;

[Collection("sqlserver")]
public sealed class AiJobWorkerIntegrationTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task WorkerClaimsBuildsContextSubmitsAndSavesExternalJobId()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var client = new FakeAiCoreJobClient("aicore-job-1");
        var processor = CreateProcessor(scope.ServiceProvider, client);

        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);

        var processed = await processor.ProcessBatchAsync("worker-test", 1, CancellationToken.None);

        var status = await queue.GetJobStatusAsync(created.JobId, CancellationToken.None);
        var job = await queue.GetJobAsync(created.JobId, CancellationToken.None);
        Assert.Equal(1, processed);
        Assert.Equal("SubmittedToAiCore", status!.Status);
        Assert.Equal("aicore-job-1", status.ExternalJobId);
        Assert.Equal("aicore-job-1", job!.ExternalJobId);
        Assert.Contains("PF-2026-000001", job.ContextJson);
        Assert.NotNull(client.LastRequest);
        Assert.Equal(created.JobId.ToString(), client.LastRequest!.Metadata!["petroProcureJobId"]);
        Assert.Equal("PurchaseFile", client.LastRequest.EntityType);
        Assert.Equal("https://petroprocure.test/api/ai/aicore/callback", client.LastRequest.CallbackUrl);
    }

    [Fact]
    public async Task WorkerSchedulesRetryWhenAiCoreSubmissionFails()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var client = new FakeAiCoreJobClient("unused") { ShouldFail = true };
        var processor = CreateProcessor(scope.ServiceProvider, client);

        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);

        await processor.ProcessBatchAsync("worker-test", 1, CancellationToken.None);

        var job = await queue.GetJobAsync(created.JobId, CancellationToken.None);
        Assert.Equal(AiJobStatus.Queued, job!.Status);
        Assert.Equal(1, job.RetryCount);
        Assert.NotNull(job.NextRetryAtUtc);
        Assert.Contains("fake AiCore failure", job.ErrorMessage);
    }

    [Fact]
    public async Task WorkerDefaultsMissingRequestAnalysisTypeToSummary()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        var client = new FakeAiCoreJobClient("aicore-job-default-analysis");
        var processor = CreateProcessor(scope.ServiceProvider, client);
        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);
        await db.AiEvaluationJobs
            .Where(x => x.Id == created.JobId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RequestJson,
                $$"""{"entityType":"PurchaseFile","entityId":"{{SeedDataIds.SamplePurchaseFileId}}"}"""));

        var processed = await processor.ProcessBatchAsync("worker-test", 1, CancellationToken.None);

        Assert.Equal(1, processed);
        Assert.NotNull(client.LastRequest);
        Assert.Equal("Summary", client.LastRequest!.AnalysisType);
    }

    [Fact]
    public async Task WorkerSubmitRetryStopsAtConfiguredLimit()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var client = new FakeAiCoreJobClient("unused") { ShouldFail = true };
        var processor = CreateProcessor(scope.ServiceProvider, client, maxRetryCount: 2);

        await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);

        await processor.ProcessBatchAsync("worker-test", 1, CancellationToken.None);

        Assert.Equal(2, client.SubmitCount);
    }

    [Fact]
    public async Task WorkerMaintenanceRecoversStuckAndExpiredJobs()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var client = new FakeAiCoreJobClient("unused");
        var processor = CreateProcessor(scope.ServiceProvider, client);
        var stuck = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);
        var stale = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "RiskReview"), Guid.NewGuid(), CancellationToken.None);
        var old = DateTime.UtcNow.AddMinutes(-30);

        await queue.ClaimNextJobsAsync("crashed-worker", 1, old, CancellationToken.None);
        await queue.ClaimNextJobsAsync("slow-worker", 1, old, CancellationToken.None);
        await queue.MarkRunningAsync(stale.JobId, 70, "running too long", CancellationToken.None);

        await processor.RunMaintenanceAsync(CancellationToken.None);

        var stuckJob = await GetJobFromFreshScopeAsync(stuck.JobId);
        var staleJob = await GetJobFromFreshScopeAsync(stale.JobId);
        Assert.Equal(AiJobStatus.Queued, stuckJob!.Status);
        Assert.Equal(AiJobStatus.Expired, staleJob!.Status);
    }

    [Fact]
    public async Task WorkerDoesNotProcessCancelledJobs()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var client = new FakeAiCoreJobClient("unused");
        var processor = CreateProcessor(scope.ServiceProvider, client);

        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);
        await queue.MarkCancelledAsync(created.JobId, DateTime.UtcNow, "test cancel", CancellationToken.None);

        var processed = await processor.ProcessBatchAsync("worker-test", 1, CancellationToken.None);

        var job = await queue.GetJobAsync(created.JobId, CancellationToken.None);
        Assert.Equal(0, processed);
        Assert.Equal(0, client.SubmitCount);
        Assert.Equal(AiJobStatus.Cancelled, job!.Status);
    }

    private static AiJobProcessor CreateProcessor(IServiceProvider services, FakeAiCoreJobClient client, int maxRetryCount = 3)
    {
        return new AiJobProcessor(
            services.GetRequiredService<IAiJobQueueService>(),
            services.GetRequiredService<IAiResultRepository>(),
            services.GetRequiredService<PurchaseFileAiContextBuilder>(),
            services.GetRequiredService<LegacyAiContextBuilder>(),
            client,
            new FakeLocalOllamaAnalysisClient(),
            new FakeSyncAiCoreClient(),
            new NullRagIngestionService(),
            Options.Create(new AiCoreIntegrationOptions
            {
                Mode = AiCoreIntegrationMode.AsyncAiCoreJob,
                BaseUrl = "https://aicore.test",
                CallbackPublicUrl = "https://petroprocure.test",
                RetryDelaySeconds = 1,
                MaxRetryCount = maxRetryCount,
                StuckJobTimeoutMinutes = 15,
                RunningJobTimeoutMinutes = 15
            }),
            NullLogger<AiJobProcessor>.Instance);
    }

    [Fact]
    public async Task LocalOllamaWorkerCompletesJobWithoutAiCoreAndStoresResult()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        await CancelOpenAiJobsAsync(scope.ServiceProvider);
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var results = scope.ServiceProvider.GetRequiredService<IAiResultRepository>();
        var aiCoreClient = new FakeAiCoreJobClient("unused");
        var ollamaClient = new FakeLocalOllamaAnalysisClient();
        var processor = CreateLocalOllamaProcessor(scope.ServiceProvider, aiCoreClient, ollamaClient);

        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", SeedDataIds.SamplePurchaseFileId, "Summary"), Guid.NewGuid(), CancellationToken.None);

        var processed = await processor.ProcessBatchAsync("worker-local-ollama", 1, CancellationToken.None);

        var status = await queue.GetJobStatusAsync(created.JobId, CancellationToken.None);
        var result = await results.GetResultAsync(created.JobId, CancellationToken.None);
        Assert.Equal(1, processed);
        Assert.Equal(0, aiCoreClient.SubmitCount);
        Assert.Equal(1, ollamaClient.AnalyzeCount);
        Assert.Equal("Completed", status!.Status);
        Assert.True(status.HasResult);
        Assert.NotNull(result);
        Assert.Equal("local ollama summary", result!.Summary);
        Assert.Equal("Ollama", result.Usage?.Provider);
        Assert.Single(result.Findings);
        Assert.Single(result.Recommendations);
    }

    private static AiJobProcessor CreateLocalOllamaProcessor(
        IServiceProvider services,
        FakeAiCoreJobClient aiCoreClient,
        FakeLocalOllamaAnalysisClient ollamaClient)
    {
        return new AiJobProcessor(
            services.GetRequiredService<IAiJobQueueService>(),
            services.GetRequiredService<IAiResultRepository>(),
            services.GetRequiredService<PurchaseFileAiContextBuilder>(),
            services.GetRequiredService<LegacyAiContextBuilder>(),
            aiCoreClient,
            ollamaClient,
            new FakeSyncAiCoreClient(),
            new NullRagIngestionService(),
            Options.Create(new AiCoreIntegrationOptions
            {
                Mode = AiCoreIntegrationMode.LocalOllamaWorker,
                RetryDelaySeconds = 1
            }),
            NullLogger<AiJobProcessor>.Instance);
    }

    private sealed class NullRagIngestionService : IRagIngestionService
    {
        public Task<RagIngestionResult> IngestAsync(EmbeddingIngestionPayload payload, CancellationToken ct = default) =>
            Task.FromResult(new RagIngestionResult(0, 0, 0));
    }

    private static async Task CancelOpenAiJobsAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<PetroProcureDbContext>();
        await db.AiEvaluationJobs
            .Where(x => x.Status != AiJobStatus.Completed &&
                x.Status != AiJobStatus.Failed &&
                x.Status != AiJobStatus.Cancelled &&
                x.Status != AiJobStatus.Expired)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, AiJobStatus.Cancelled)
                .SetProperty(x => x.CompletedAtUtc, DateTime.UtcNow)
                .SetProperty(x => x.CancelledAtUtc, DateTime.UtcNow)
                .SetProperty(x => x.LockedBy, (string?)null)
                .SetProperty(x => x.LockedAtUtc, (DateTime?)null));
    }

    private async Task<PetroProcure.Domain.Modules.Ai.AiEvaluationJob?> GetJobFromFreshScopeAsync(Guid jobId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        return await queue.GetJobAsync(jobId, CancellationToken.None);
    }

    private sealed class FakeAiCoreJobClient(string externalJobId) : IAiCoreJobClient
    {
        public bool ShouldFail { get; init; }
        public int SubmitCount { get; private set; }
        public AiCoreSubmitJobRequest? LastRequest { get; private set; }

        public Task<AiCoreSubmitJobResponse> SubmitJobAsync(AiCoreSubmitJobRequest request, CancellationToken ct = default)
        {
            SubmitCount++;
            LastRequest = request;
            if (ShouldFail)
                throw new AiCoreClientException("fake AiCore failure");

            return Task.FromResult(new AiCoreSubmitJobResponse(
                externalJobId,
                "SubmittedToAiCore",
                "accepted",
                DateTime.UtcNow));
        }

        public Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiProviderHealthDto("AiCore", true, "Healthy", DateTime.UtcNow));
    }

    private sealed class FakeSyncAiCoreClient : IAiCoreClient
    {
        public Task<AiCoreAnalysisResponse> SendAnalysisAsync(AiCoreAnalysisRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiCoreAnalysisResponse("sync summary", "Info", [], []));

        public Task<AiCoreTextResponse> SendTextAsync(AiCoreTextRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiCoreTextResponse("AiCore", "sync text"));

        public Task<AiChatResponse> SendChatAsync(AiChatRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiChatResponse("sync chat", "AiCore", "AiCore"));

        public Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiProviderHealthDto("AiCore", true, "Healthy", DateTime.UtcNow));
    }

    private sealed class FakeLocalOllamaAnalysisClient : ILocalOllamaAnalysisClient
    {
        public int AnalyzeCount { get; private set; }

        public Task<AiCoreAnalysisResponse> AnalyzeAsync(AiCoreAnalysisRequest request, CancellationToken ct = default)
        {
            AnalyzeCount++;
            return Task.FromResult(new AiCoreAnalysisResponse(
                "local ollama summary",
                "Info",
                [new AiCoreFinding("Local finding", "Generated by local Ollama.", "Info", "LOCAL")],
                [new AiCoreRecommendation("Local recommendation", "Review advisory output.", "Medium", "Review")],
                new AiUsageDto(10, 20, null, 30, 500, "gemma3", "Ollama")));
        }
    }
}
