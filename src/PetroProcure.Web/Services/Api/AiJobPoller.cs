using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.Web.Services.Api;

/// <summary>
/// Polls an AI job's status until it reaches a terminal state, then fetches the result when completed.
/// The loop is driven entirely by a caller-supplied CancellationToken so that disposing the UI
/// component stops polling without cancelling the server-side job.
/// </summary>
public sealed class AiJobPoller(IPetroProcureAiApiClient client)
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(3);

    public async Task PollAsync(
        Guid jobId,
        Func<AiJobStatusDto, Task> onStatus,
        Func<AiJobResultDto?, Task> onCompleted,
        CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            AiJobStatusDto? status;
            try
            {
                status = await client.GetJobStatusAsync(jobId, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (status is null)
                continue;

            await onStatus(status);

            if (!IsTerminal(status.Status))
                continue;

            if (IsCompleted(status.Status))
            {
                AiJobResultDto? result = null;
                try
                {
                    result = await client.GetJobResultAsync(jobId, ct);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                await onCompleted(result);
            }

            return;
        }
    }

    public static bool IsTerminal(string? status) =>
        IsCompleted(status)
        || Matches(status, "Failed")
        || Matches(status, "Cancelled")
        || Matches(status, "Expired");

    public static bool IsCompleted(string? status) => Matches(status, "Completed");

    private static bool Matches(string? status, string value) =>
        status is not null && status.Equals(value, StringComparison.OrdinalIgnoreCase);
}
