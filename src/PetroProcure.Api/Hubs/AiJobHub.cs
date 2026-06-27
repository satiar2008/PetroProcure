using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PetroProcure.Api.Security;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Security;

namespace PetroProcure.Api.Hubs;

/// <summary>
/// Real-time AI job notifications. Authenticated users automatically join their own user group
/// and may subscribe to specific job or entity groups they are already viewing. Only lightweight
/// status summaries are pushed; full results are still fetched from the permission-checked API.
/// </summary>
[Authorize]
public sealed class AiJobHub(IAiJobQueueService jobs) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrWhiteSpace(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        await base.OnConnectedAsync();
    }

    public async Task SubscribeToJob(string jobId)
    {
        EnsureCanViewAiJobs();
        if (!Guid.TryParse(jobId, out var parsedJobId))
            throw new HubException("Invalid AI job id.");

        var job = await jobs.GetJobAsync(parsedJobId, Context.ConnectionAborted)
            ?? throw new HubException("AI job was not found.");
        if (!CanViewJobOwner(job.CreatedByUserId))
            throw new HubException("AI job access denied.");

        await Groups.AddToGroupAsync(Context.ConnectionId, JobGroup(jobId));
    }

    public Task UnsubscribeFromJob(string jobId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, JobGroup(jobId));

    public async Task SubscribeToEntity(string entityType, string entityId)
    {
        EnsureCanViewAiJobs();
        if (!Guid.TryParse(entityId, out _))
            throw new HubException("Invalid entity id.");
        await Groups.AddToGroupAsync(Context.ConnectionId, EntityGroup(entityType, entityId));
    }

    public Task UnsubscribeFromEntity(string entityType, string entityId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, EntityGroup(entityType, entityId));

    public static string UserGroup(string userId) => $"user-{userId}";
    public static string JobGroup(string jobId) => $"job-{jobId}";
    public static string EntityGroup(string entityType, string entityId) =>
        $"entity-{entityType}-{entityId}".ToLowerInvariant();

    private void EnsureCanViewAiJobs()
    {
        if (Context.User is null ||
            !(Context.User.HasPermission(ApplicationPermissions.AiAgentUse)
              || Context.User.HasPermission(ApplicationPermissions.AiAgentEvaluatePurchaseRules)
              || Context.User.HasPermission(ApplicationPermissions.AiAgentAnalyzePurchaseFile)
              || Context.User.HasPermission(ApplicationPermissions.AiAnalyzePurchaseFile)
              || Context.User.HasPermission(ApplicationPermissions.AiViewEvaluations)))
        {
            throw new HubException("AI job access denied.");
        }
    }

    private bool CanViewJobOwner(Guid? ownerId)
    {
        if (ownerId is null || Context.User is null) return true;
        if (Context.User.HasPermission(ApplicationPermissions.AiViewEvaluations)) return true;
        return string.Equals(Context.UserIdentifier, ownerId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
