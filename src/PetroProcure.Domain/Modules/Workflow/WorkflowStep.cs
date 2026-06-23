using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Workflow;

public sealed class WorkflowStep : Entity<Guid>
{
    public WorkflowStep(Guid id, Guid workflowInstanceId, Guid fromDepartmentId, Guid toDepartmentId,
        string actionName, string? comment, Guid createdByUserId) : base(id)
    {
        WorkflowInstanceId = workflowInstanceId;
        FromDepartmentId = fromDepartmentId;
        ToDepartmentId = toDepartmentId;
        ActionName = Required(actionName);
        Comment = comment?.Trim();
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid WorkflowInstanceId { get; private set; }
    public Guid FromDepartmentId { get; private set; }
    public Guid ToDepartmentId { get; private set; }
    public string ActionName { get; private set; }
    public string? Comment { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public void Complete(Guid userId) { CompletedByUserId = userId; CompletedAt = DateTime.UtcNow; }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Action is required.") : value.Trim();
}
