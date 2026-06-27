using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.Application.Ai;

public enum AiJobEvent
{
    Created,
    StatusChanged,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Publishes AI job status summaries to interested clients (e.g. via SignalR). The Application
/// layer depends only on this abstraction; the API supplies the concrete transport. A no-op
/// implementation is used wherever no real-time transport is available (Worker, tests).
/// </summary>
public interface IAiJobNotifier
{
    Task PublishAsync(AiJobEvent eventType, AiJobNotificationDto notification, Guid? createdByUserId, CancellationToken ct);
}

public sealed class NullAiJobNotifier : IAiJobNotifier
{
    public Task PublishAsync(AiJobEvent eventType, AiJobNotificationDto notification, Guid? createdByUserId, CancellationToken ct) =>
        Task.CompletedTask;
}
