namespace PetroProcure.Contracts.V1.Ai;

/// <summary>
/// SignalR event (method) names for AI job updates. Shared by the API hub and the Web client.
/// </summary>
public static class AiJobHubEvents
{
    public const string Created = "AiJobCreated";
    public const string StatusChanged = "AiJobStatusChanged";
    public const string Completed = "AiJobCompleted";
    public const string Failed = "AiJobFailed";
    public const string Cancelled = "AiJobCancelled";

    public const string HubPath = "/hubs/ai-jobs";
    public const string SubscribeToJob = "SubscribeToJob";
    public const string UnsubscribeFromJob = "UnsubscribeFromJob";
    public const string SubscribeToEntity = "SubscribeToEntity";
    public const string UnsubscribeFromEntity = "UnsubscribeFromEntity";
}

/// <summary>
/// Lightweight status summary pushed over SignalR. Deliberately carries NO raw result JSON,
/// findings, or recommendations — clients fetch the full result from the permission-checked API.
/// </summary>
public sealed record AiJobNotificationDto(
    Guid JobId,
    string EntityType,
    Guid EntityId,
    string AnalysisType,
    string Status,
    int ProgressPercent,
    string? Message,
    bool HasResult,
    DateTime TimestampUtc);
