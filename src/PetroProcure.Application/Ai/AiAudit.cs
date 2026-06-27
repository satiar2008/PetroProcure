using Microsoft.Extensions.Logging;

namespace PetroProcure.Application.Ai;

public interface IAiAuditService
{
    Task RecordAsync(string eventName, Guid? jobId, string? correlationId, string? message, CancellationToken ct);
}

public sealed class LoggingAiAuditService(ILogger<LoggingAiAuditService> logger) : IAiAuditService
{
    public Task RecordAsync(string eventName, Guid? jobId, string? correlationId, string? message, CancellationToken ct)
    {
        logger.LogInformation("AI audit event {EventName}: job {JobId}, correlation {CorrelationId}, message {Message}",
            eventName, jobId, correlationId, Redact(message));
        return Task.CompletedTask;
    }

    private static string? Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        var redacted = value;
        foreach (var marker in new[] { "ApiKey", "apiKey", "CallbackSecret", "callbackSecret", "Bearer " })
        {
            var index = redacted.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                redacted = redacted[..index] + marker + "=<redacted>";
        }
        return redacted;
    }
}

public static class AiAuditEvents
{
    public const string JobCreated = "JobCreated";
    public const string JobSubmittedToAiCore = "JobSubmittedToAiCore";
    public const string CallbackReceived = "CallbackReceived";
    public const string JobCompleted = "JobCompleted";
    public const string JobFailed = "JobFailed";
    public const string JobCancelled = "JobCancelled";
    public const string RetryScheduled = "RetryScheduled";
    public const string StuckJobRequeued = "StuckJobRequeued";
    public const string JobExpired = "JobExpired";
}
