using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Workflow;

public sealed record InboxTaskDto(
    Guid Id, Guid WorkflowInstanceId, Guid? PurchaseFileId, Guid? IndentId,
    Guid AssignedDepartmentId, Guid? AssignedUserId, string Title, string? Description,
    WorkflowStatus Status, DateOnly? DueDate, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record WorkflowStepDto(
    Guid Id, Guid FromDepartmentId, Guid ToDepartmentId, string ActionName,
    string? Comment, Guid CreatedByUserId, DateTime CreatedAt,
    Guid? CompletedByUserId, DateTime? CompletedAt);
public sealed record WorkflowTimelineDto(Guid EntityId, IReadOnlyList<WorkflowStepDto> Steps);
public sealed record InboxTaskDetailDto(InboxTaskDto Task, IReadOnlyList<WorkflowStepDto> Timeline);
public sealed record PurchaseFileWorkflowStateDto(
    Guid PurchaseFileId, Guid? WorkflowInstanceId, Guid CurrentDepartmentId,
    string Status, IReadOnlyList<WorkflowStepDto> Steps, IReadOnlyList<InboxTaskDto> OpenTasks,
    bool CanStart, bool CanSend, bool CanReturn, bool CanCompleteTask);
public sealed record StartWorkflowRequest(string EntityType, Guid EntityId, Guid CurrentDepartmentId);
public sealed record SendToDepartmentRequest(
    Guid WorkflowInstanceId, Guid ToDepartmentId, string ActionName,
    string? Comment, string TaskTitle, string? TaskDescription, DateOnly? DueDate);
public sealed record CompleteInboxTaskRequest;
public sealed record AssignInboxTaskRequest(Guid? AssignedUserId);
public sealed record ReturnWorkflowRequest(
    Guid WorkflowInstanceId, string? Comment, string TaskTitle, string? TaskDescription);
public sealed record WorkflowActionDto(
    Guid Id, string Code, string Title, DepartmentType FromDepartmentType,
    DepartmentType? ToDepartmentType, PurchaseFileStatus FromStatus,
    PurchaseFileStatus ToStatus, string RequiredPermission, bool RequiresComment,
    bool IsReturnAction, bool IsFinalAction);
public sealed record WorkflowActionDefinitionDto(
    Guid Id, string Code, string Title, DepartmentType FromDepartmentType,
    DepartmentType? ToDepartmentType, PurchaseFileStatus FromStatus,
    PurchaseFileStatus ToStatus, string RequiredPermission, bool RequiresComment,
    bool IsReturnAction, bool IsFinalAction, bool IsActive);
public sealed record UpdateWorkflowActionDefinitionRequest(
    string Title, string RequiredPermission, bool RequiresComment, bool IsActive);
public sealed record ExecuteWorkflowActionRequest(string ActionCode, string? Comment);
public sealed record WorkflowActionExecutionResultDto(
    Guid PurchaseFileId, Guid WorkflowInstanceId, Guid WorkflowStepId,
    Guid CurrentDepartmentId, PurchaseFileStatus Status, bool WorkflowCompleted);
