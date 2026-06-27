using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.Domain;

public sealed class AiJobDomainTests
{
    [Fact]
    public void NewAiEvaluationJobStartsQueued()
    {
        var job = CreateJob();

        Assert.Equal(AiJobStatus.Queued, job.Status);
        Assert.Equal(0, job.ProgressPercent);
        Assert.Equal(3, job.MaxRetryCount);
        Assert.Null(job.ExternalJobId);
    }

    [Fact]
    public void FailedJobSchedulesRetryUntilMaxRetryCount()
    {
        var job = CreateJob(maxRetryCount: 2);
        job.Claim("worker-1", DateTime.UtcNow);

        job.ReleaseForRetry("temporary failure", DateTime.UtcNow.AddMinutes(1));

        Assert.Equal(AiJobStatus.Queued, job.Status);
        Assert.Equal(1, job.RetryCount);
        Assert.NotNull(job.NextRetryAtUtc);
        Assert.Null(job.LockedBy);

        job.Claim("worker-2", DateTime.UtcNow);
        job.ReleaseForRetry("temporary failure again", DateTime.UtcNow.AddMinutes(2));

        Assert.Equal(AiJobStatus.Failed, job.Status);
        Assert.Equal(2, job.RetryCount);
        Assert.Null(job.NextRetryAtUtc);
        Assert.NotNull(job.CompletedAtUtc);
    }

    private static AiEvaluationJob CreateJob(int maxRetryCount = 3) =>
        new(Guid.NewGuid(), "PetroProcure", "PurchaseFile", Guid.NewGuid(), "Summary",
            0, Guid.NewGuid().ToString("N"), "{}", maxRetryCount, Guid.NewGuid());
}
