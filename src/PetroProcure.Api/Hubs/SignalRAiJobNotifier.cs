using Microsoft.AspNetCore.SignalR;
using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.Api.Hubs;

/// <summary>
/// Publishes AI job notifications to the SignalR hub. Sends to the job owner's user group, the
/// per-job group, and the per-entity group so any client subscribed to the relevant scope is
/// updated. Payload is a status summary only — never raw result JSON.
/// </summary>
public sealed class SignalRAiJobNotifier(IHubContext<AiJobHub> hub) : IAiJobNotifier
{
    public async Task PublishAsync(AiJobEvent eventType, AiJobNotificationDto notification,
        Guid? createdByUserId, CancellationToken ct)
    {
        var method = EventName(eventType);

        if (createdByUserId is { } userId)
            await hub.Clients.Group(AiJobHub.UserGroup(userId.ToString()))
                .SendAsync(method, notification, ct);

        await hub.Clients.Group(AiJobHub.JobGroup(notification.JobId.ToString()))
            .SendAsync(method, notification, ct);

        await hub.Clients.Group(AiJobHub.EntityGroup(notification.EntityType, notification.EntityId.ToString()))
            .SendAsync(method, notification, ct);
    }

    private static string EventName(AiJobEvent eventType) => eventType switch
    {
        AiJobEvent.Created => AiJobHubEvents.Created,
        AiJobEvent.StatusChanged => AiJobHubEvents.StatusChanged,
        AiJobEvent.Completed => AiJobHubEvents.Completed,
        AiJobEvent.Failed => AiJobHubEvents.Failed,
        AiJobEvent.Cancelled => AiJobHubEvents.Cancelled,
        _ => AiJobHubEvents.StatusChanged
    };
}
