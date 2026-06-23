using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Workflow;

public sealed class InboxTask : Entity<Guid>
{
    public InboxTask(Guid id, Guid workflowInstanceId, Guid? purchaseFileId, Guid? indentId,
        Guid assignedDepartmentId, Guid? assignedUserId, string title, string? description, DateOnly? dueDate) : base(id)
    {
        WorkflowInstanceId = workflowInstanceId;
        PurchaseFileId = purchaseFileId;
        IndentId = indentId;
        AssignedDepartmentId = assignedDepartmentId;
        AssignedUserId = assignedUserId;
        Title = Required(title);
        Description = description?.Trim();
        DueDate = dueDate;
        Status = WorkflowStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid WorkflowInstanceId { get; private set; }
    public Guid? PurchaseFileId { get; private set; }
    public Guid? IndentId { get; private set; }
    public Guid AssignedDepartmentId { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public void Assign(Guid? userId) { AssignedUserId = userId; Status = WorkflowStatus.InProgress; }
    public void Complete() { Status = WorkflowStatus.Completed; CompletedAt = DateTime.UtcNow; }
    private static string Required(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Title is required.") : value.Trim();
}
