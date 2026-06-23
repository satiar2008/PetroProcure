using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Workflow;

public sealed class WorkflowInstance : Entity<Guid>
{
    private readonly List<WorkflowStep> _steps = [];

    public WorkflowInstance(Guid id, string entityType, Guid entityId, Guid currentDepartmentId, Guid startedByUserId)
        : base(id)
    {
        EntityType = Required(entityType, nameof(entityType));
        EntityId = entityId;
        CurrentDepartmentId = currentDepartmentId;
        StartedByUserId = startedByUserId;
        StartedAt = DateTime.UtcNow;
        Status = WorkflowStatus.InProgress;
    }

    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid CurrentDepartmentId { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public Guid StartedByUserId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public IReadOnlyCollection<WorkflowStep> Steps => _steps.AsReadOnly();

    public WorkflowStep Send(Guid toDepartmentId, string actionName, string? comment, Guid userId)
    {
        if (Status == WorkflowStatus.Completed) throw new InvalidOperationException("Workflow is completed.");
        var step = new WorkflowStep(Guid.NewGuid(), Id, CurrentDepartmentId, toDepartmentId, actionName, comment, userId);
        _steps.Add(step);
        CurrentDepartmentId = toDepartmentId;
        return step;
    }

    public WorkflowStep Return(string actionName, string? comment, Guid userId)
    {
        var previous = _steps.OrderByDescending(step => step.CreatedAt)
            .FirstOrDefault(step => step.FromDepartmentId != CurrentDepartmentId)
            ?? throw new InvalidOperationException("No previous department exists.");
        return Send(previous.FromDepartmentId, actionName, comment, userId);
    }

    public void Complete(Guid userId)
    {
        if (Status == WorkflowStatus.Completed) return;
        foreach (var step in _steps.Where(x => !x.CompletedAt.HasValue))
            step.Complete(userId);
        Status = WorkflowStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
