using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Ai;

public sealed class AiEvaluationJob : Entity<Guid>
{
    public AiEvaluationJob(
        Guid id,
        string sourceSystem,
        string entityType,
        Guid entityId,
        string analysisType,
        int priority,
        string correlationId,
        string requestJson,
        int maxRetryCount,
        Guid? createdByUserId = null)
        : base(id)
    {
        SourceSystem = Required(sourceSystem, nameof(sourceSystem));
        EntityType = Required(entityType, nameof(entityType));
        EntityId = entityId;
        AnalysisType = Required(analysisType, nameof(analysisType));
        Status = AiJobStatus.Queued;
        Priority = priority;
        CorrelationId = Required(correlationId, nameof(correlationId));
        RequestJson = Required(requestJson, nameof(requestJson));
        MaxRetryCount = Math.Max(0, maxRetryCount);
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private AiEvaluationJob(Guid id) : base(id)
    {
        SourceSystem = string.Empty;
        EntityType = string.Empty;
        AnalysisType = string.Empty;
        CorrelationId = string.Empty;
        RequestJson = "{}";
    }

    public string SourceSystem { get; private set; }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string AnalysisType { get; private set; }
    public AiJobStatus Status { get; private set; }
    public int Priority { get; private set; }
    public string CorrelationId { get; private set; }
    public string? ExternalJobId { get; private set; }
    public string RequestJson { get; private set; }
    public string? ContextJson { get; private set; }
    public string? ResultJson { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int ProgressPercent { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetryCount { get; private set; }
    public DateTime? NextRetryAtUtc { get; private set; }
    public string? LockedBy { get; private set; }
    public DateTime? LockedAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? SubmittedToAiCoreAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }

    public bool CanRetry => RetryCount < MaxRetryCount;

    public void Claim(string workerId, DateTime utcNow)
    {
        if (Status != AiJobStatus.Queued)
            throw new InvalidOperationException("Only queued AI jobs can be claimed.");
        Status = AiJobStatus.Claimed;
        LockedBy = Required(workerId, nameof(workerId));
        LockedAtUtc = utcNow;
        StartedAtUtc ??= utcNow;
        ErrorMessage = null;
        ProgressPercent = Math.Max(ProgressPercent, 5);
    }

    public void MarkBuildingContext(string? contextJson = null)
    {
        EnsureNotTerminal();
        Status = AiJobStatus.BuildingContext;
        if (!string.IsNullOrWhiteSpace(contextJson))
            ContextJson = contextJson;
        ProgressPercent = Math.Max(ProgressPercent, 15);
    }

    public void MarkSendingToAiCore(string? contextJson = null)
    {
        EnsureNotTerminal();
        Status = AiJobStatus.SendingToAiCore;
        if (!string.IsNullOrWhiteSpace(contextJson))
            ContextJson = contextJson;
        ProgressPercent = Math.Max(ProgressPercent, 35);
    }

    public void MarkSubmittedToAiCore(string externalJobId, DateTime utcNow)
    {
        EnsureNotTerminal();
        ExternalJobId = Required(externalJobId, nameof(externalJobId));
        Status = AiJobStatus.SubmittedToAiCore;
        SubmittedToAiCoreAtUtc = utcNow;
        ProgressPercent = Math.Max(ProgressPercent, 50);
    }

    public void MarkRunning(int progressPercent, string? message = null)
    {
        EnsureNotTerminal();
        Status = AiJobStatus.Running;
        ProgressPercent = Math.Clamp(progressPercent, 0, 99);
        ErrorMessage = message;
    }

    public void MarkCompleted(string? resultJson, DateTime utcNow)
    {
        EnsureNotTerminal(allowCompleted: true);
        Status = AiJobStatus.Completed;
        ResultJson = resultJson;
        ProgressPercent = 100;
        CompletedAtUtc = utcNow;
        ErrorMessage = null;
        NextRetryAtUtc = null;
        LockedBy = null;
        LockedAtUtc = null;
    }

    public void MarkFailed(string errorMessage, DateTime utcNow)
    {
        EnsureNotTerminal();
        Status = AiJobStatus.Failed;
        ErrorMessage = Required(errorMessage, nameof(errorMessage));
        CompletedAtUtc = utcNow;
        LockedBy = null;
        LockedAtUtc = null;
    }

    public void MarkCancelled(DateTime utcNow, string? reason = null)
    {
        EnsureNotTerminal();
        Status = AiJobStatus.Cancelled;
        CancelledAtUtc = utcNow;
        CompletedAtUtc = utcNow;
        ErrorMessage = reason;
        LockedBy = null;
        LockedAtUtc = null;
    }

    public void ReleaseForRetry(string errorMessage, DateTime nextRetryAtUtc)
    {
        EnsureNotTerminal();
        RetryCount++;
        ErrorMessage = Required(errorMessage, nameof(errorMessage));
        LockedBy = null;
        LockedAtUtc = null;
        if (RetryCount >= MaxRetryCount)
        {
            Status = AiJobStatus.Failed;
            CompletedAtUtc = DateTime.UtcNow;
            NextRetryAtUtc = null;
            return;
        }
        Status = AiJobStatus.Queued;
        NextRetryAtUtc = nextRetryAtUtc;
    }

    private void EnsureNotTerminal(bool allowCompleted = false)
    {
        if (allowCompleted && Status == AiJobStatus.Completed)
            return;
        if (Status is AiJobStatus.Completed or AiJobStatus.Failed or AiJobStatus.Cancelled or AiJobStatus.Expired)
            throw new InvalidOperationException("Terminal AI jobs cannot be changed.");
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
