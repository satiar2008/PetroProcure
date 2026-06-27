using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.IntegrationTests;

[Collection("sqlserver")]
public sealed class AiJobQueueIntegrationTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task CreateJobStoresQueuedJob()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();

        var response = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", Guid.NewGuid(), "Summary", Priority: 5), Guid.NewGuid(), CancellationToken.None);

        var status = await queue.GetJobStatusAsync(response.JobId, CancellationToken.None);
        Assert.NotNull(status);
        Assert.Equal("Queued", status.Status);
        Assert.Equal(5, (await queue.GetJobAsync(response.JobId, CancellationToken.None))!.Priority);
        Assert.False(status.HasResult);
    }

    [Fact]
    public async Task ClaimNextJobsClaimsJobOnlyOnceUnderConcurrentAttempts()
    {
        await CancelOpenAiJobsAsync();
        Guid jobId;
        await using (var setupScope = fixture.Services.CreateAsyncScope())
        {
            var queue = setupScope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
            var response = await queue.CreateJobAsync(new CreateAiJobRequest(
                "PurchaseFile", Guid.NewGuid(), "Summary"), Guid.NewGuid(), CancellationToken.None);
            jobId = response.JobId;
        }

        var now = DateTime.UtcNow;
        var first = ClaimAsync("worker-a", now);
        var second = ClaimAsync("worker-b", now);
        var claimed = (await Task.WhenAll(first, second)).SelectMany(x => x).ToArray();

        Assert.Single(claimed, x => x.Id == jobId);
        Assert.Equal(claimed.Length, claimed.Select(x => x.Id).Distinct().Count());
        Assert.All(claimed, job => Assert.Equal(AiJobStatus.Claimed, job.Status));

        async Task<IReadOnlyList<AiEvaluationJob>> ClaimAsync(string workerId, DateTime utcNow)
        {
            await using var scope = fixture.Services.CreateAsyncScope();
            var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
            return await queue.ClaimNextJobsAsync(workerId, 1, utcNow, CancellationToken.None);
        }
    }

    [Fact]
    public async Task MarkCompletedStoresResult()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var results = scope.ServiceProvider.GetRequiredService<IAiResultRepository>();
        var response = await queue.CreateJobAsync(new CreateAiJobRequest(
            "Tender", Guid.NewGuid(), "LegalCompliance"), Guid.NewGuid(), CancellationToken.None);

        var result = new AiEvaluationResult(Guid.NewGuid(), response.JobId, "Tender", Guid.NewGuid(),
            "LegalCompliance", "Advisory summary", "{\"summary\":\"Advisory summary\"}",
            "gemma3", "AiCore", 10, 20, 30, 1500);
        result.AddFinding(new AiFinding(Guid.NewGuid(), result.Id, "Finding", "Finding details", AiFindingSeverity.High, "CLAUSE-1"));
        result.AddRecommendation(new AiRecommendation(Guid.NewGuid(), result.Id, "Recommendation",
            "Recommendation details", AiRecommendationPriority.High, "Review"));

        await results.SaveResultAsync(result, CancellationToken.None);

        var status = await queue.GetJobStatusAsync(response.JobId, CancellationToken.None);
        var stored = await results.GetResultAsync(response.JobId, CancellationToken.None);
        Assert.Equal("Completed", status!.Status);
        Assert.True(status.HasResult);
        Assert.NotNull(stored);
        Assert.Equal("Advisory summary", stored.Summary);
        Assert.Single(stored.Findings);
        Assert.Single(stored.Recommendations);
        Assert.Equal(30, stored.Usage?.TotalTokens);
    }

    [Fact]
    public async Task DuplicateCallbackForSameCorrelationIsIdempotent()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var results = scope.ServiceProvider.GetRequiredService<IAiResultRepository>();
        var response = await queue.CreateJobAsync(new CreateAiJobRequest(
            "Contract", Guid.NewGuid(), "Summary"), Guid.NewGuid(), CancellationToken.None);

        var first = new AiEvaluationResult(Guid.NewGuid(), response.JobId, "Contract", Guid.NewGuid(),
            "Summary", "First result", "{\"summary\":\"First result\"}", "gemma3", "AiCore", 1, 2, 3, 4);
        var second = new AiEvaluationResult(Guid.NewGuid(), response.JobId, "Contract", Guid.NewGuid(),
            "Summary", "Second result", "{\"summary\":\"Second result\"}", "gemma3", "AiCore", 5, 6, 11, 12);

        var storedFirst = await results.SaveResultAsync(first, CancellationToken.None);
        var storedSecond = await results.SaveResultAsync(second, CancellationToken.None);
        var dto = await results.GetResultAsync(response.JobId, CancellationToken.None);

        Assert.Equal(storedFirst.Id, storedSecond.Id);
        Assert.Equal("First result", dto!.Summary);
        Assert.Equal(3, dto.Usage?.TotalTokens);
    }

    [Fact]
    public async Task RequeueStuckJobsReleasesCrashedWorkerLocks()
    {
        await CancelOpenAiJobsAsync();
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", Guid.NewGuid(), "Summary"), Guid.NewGuid(), CancellationToken.None);
        var lockedAt = DateTime.UtcNow.AddMinutes(-30);

        var claimed = await queue.ClaimNextJobsAsync("crashed-worker", 1, lockedAt, CancellationToken.None);
        var requeued = await queue.RequeueStuckJobsAsync(DateTime.UtcNow.AddMinutes(-15), CancellationToken.None);

        var job = await GetJobFromFreshScopeAsync(created.JobId);
        Assert.Contains(claimed, x => x.Id == created.JobId);
        Assert.True(requeued >= 1);
        Assert.Equal(AiJobStatus.Queued, job!.Status);
        Assert.Null(job.LockedBy);
        Assert.Null(job.LockedAtUtc);
        Assert.NotNull(job.NextRetryAtUtc);
    }

    [Fact]
    public async Task ExpireStaleJobsMarksRunningJobExpired()
    {
        await CancelOpenAiJobsAsync();
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", Guid.NewGuid(), "RiskReview"), Guid.NewGuid(), CancellationToken.None);
        var startedAt = DateTime.UtcNow.AddHours(-3);

        await queue.ClaimNextJobsAsync("slow-worker", 1, startedAt, CancellationToken.None);
        await queue.MarkRunningAsync(created.JobId, 60, "still running", CancellationToken.None);
        var expired = await queue.ExpireStaleJobsAsync(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-2), CancellationToken.None);

        var job = await GetJobFromFreshScopeAsync(created.JobId);
        Assert.True(expired >= 1);
        Assert.Equal(AiJobStatus.Expired, job!.Status);
        Assert.NotNull(job.CompletedAtUtc);
        Assert.Equal("AI job expired after timeout.", job.ErrorMessage);
    }

    [Fact]
    public async Task MetricsCountsAiJobsByProductionBuckets()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var created = await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile", Guid.NewGuid(), "Summary"), Guid.NewGuid(), CancellationToken.None);

        var metrics = await queue.GetMetricsAsync(CancellationToken.None);

        Assert.True(metrics.Queued >= 1);
        Assert.True(metrics.Queued + metrics.Running + metrics.Failed + metrics.Completed + metrics.Cancelled + metrics.Expired >= 1);
        Assert.NotEqual(Guid.Empty, created.JobId);
    }

    private async Task CancelOpenAiJobsAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
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

    private async Task<AiEvaluationJob?> GetJobFromFreshScopeAsync(Guid jobId)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        return await queue.GetJobAsync(jobId, CancellationToken.None);
    }
}
