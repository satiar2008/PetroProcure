using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Application.Security;

namespace PetroProcure.Application.Workflow;

public sealed record InboxTaskDto(Guid Id, Guid WorkflowInstanceId, Guid? PurchaseFileId, Guid? IndentId,
    Guid AssignedDepartmentId, Guid? AssignedUserId, string Title, string? Description,
    WorkflowStatus Status, DateOnly? DueDate, DateTime CreatedAt, DateTime? CompletedAt);
public sealed record WorkflowStepDto(Guid Id, Guid FromDepartmentId, Guid ToDepartmentId, string ActionName,
    string? Comment, Guid CreatedByUserId, DateTime CreatedAt, Guid? CompletedByUserId, DateTime? CompletedAt);

public sealed record StartWorkflowCommand(string EntityType, Guid EntityId, Guid CurrentDepartmentId);
public sealed record SendToDepartmentCommand(Guid WorkflowInstanceId, Guid ToDepartmentId, string ActionName,
    string? Comment, string TaskTitle, string? TaskDescription, DateOnly? DueDate);
public sealed record AssignInboxTaskCommand(Guid TaskId, Guid? AssignedUserId);
public sealed record CompleteInboxTaskCommand(Guid TaskId);
public sealed record ReturnToPreviousDepartmentCommand(Guid WorkflowInstanceId, string? Comment,
    string TaskTitle, string? TaskDescription);
public sealed record GetMyInboxTasksQuery;
public sealed record GetDepartmentInboxTasksQuery(Guid DepartmentId);
public sealed record GetInboxTaskByIdQuery(Guid TaskId);
public sealed record GetPurchaseFileWorkflowTimelineQuery(Guid PurchaseFileId);
public sealed record GetPurchaseFileWorkflowStateQuery(Guid PurchaseFileId);
public sealed record PurchaseFileWorkflowState(
    Guid PurchaseFileId, Guid? WorkflowInstanceId, Guid CurrentDepartmentId, string Status,
    IReadOnlyList<WorkflowStepDto> Steps, IReadOnlyList<InboxTaskDto> OpenTasks,
    bool CanStart, bool CanSend, bool CanReturn, bool CanCompleteTask);

public interface IWorkflowRepository
{
    Task<WorkflowInstance?> FindWorkflowAsync(Guid id, bool includeSteps, CancellationToken ct);
    Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid purchaseFileId, CancellationToken ct);
    Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct);
    Task<InboxTask?> FindTaskAsync(Guid id, CancellationToken ct);
    Task<bool> UserHasDepartmentAccessAsync(Guid userId, Guid departmentId, CancellationToken ct);
    Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct);
    Task AddTaskAsync(InboxTask task, CancellationToken ct);
    Task<IReadOnlyList<InboxTaskDto>> GetMyTasksAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<InboxTaskDto>> GetDepartmentTasksAsync(Guid departmentId, CancellationToken ct);
    Task<InboxTaskDto?> GetTaskDtoAsync(Guid taskId, CancellationToken ct);
    Task<IReadOnlyList<WorkflowStepDto>> GetWorkflowTimelineAsync(Guid workflowInstanceId, CancellationToken ct);
    Task<IReadOnlyList<WorkflowStepDto>> GetTimelineAsync(Guid purchaseFileId, CancellationToken ct);
    Task<IReadOnlyList<InboxTaskDto>> GetOpenPurchaseFileTasksAsync(Guid purchaseFileId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class WorkflowCommandHandler(IWorkflowRepository repository, ICurrentUserService currentUser)
{
    public async Task<Guid> Handle(StartWorkflowCommand command, CancellationToken ct = default)
    {
        if (!await HasDepartmentAccess(command.CurrentDepartmentId, ct))
            throw new WorkflowAccessDeniedException("User does not have access to the starting department.");
        if (command.EntityType.Equals("PurchaseFile", StringComparison.OrdinalIgnoreCase) &&
            await repository.FindPurchaseFileWorkflowAsync(command.EntityId, ct) is not null)
            throw new WorkflowValidationException("A workflow already exists for this purchase file.");
        var workflow = new WorkflowInstance(Guid.NewGuid(), command.EntityType, command.EntityId,
            command.CurrentDepartmentId, currentUser.UserId);
        await repository.AddWorkflowAsync(workflow, ct);
        await repository.SaveChangesAsync(ct);
        return workflow.Id;
    }

    public async Task<Guid> Handle(SendToDepartmentCommand command, CancellationToken ct = default)
    {
        var workflow = await RequiredWorkflow(command.WorkflowInstanceId, true, ct);
        await RequireAccess(workflow.CurrentDepartmentId, ct);
        var step = Execute(() => workflow.Send(command.ToDepartmentId, command.ActionName, command.Comment, currentUser.UserId));
        var purchaseFileId = PurchaseFileId(workflow);
        if (purchaseFileId.HasValue)
        {
            var file = await repository.FindPurchaseFileAsync(purchaseFileId.Value, ct)
                ?? throw new WorkflowValidationException("Purchase file was not found.");
            Execute(() => file.RouteToDepartment(command.ToDepartmentId));
        }
        var task = new InboxTask(Guid.NewGuid(), workflow.Id, purchaseFileId, IndentId(workflow),
            command.ToDepartmentId, null, command.TaskTitle, command.TaskDescription, command.DueDate);
        await repository.AddTaskAsync(task, ct);
        await repository.SaveChangesAsync(ct);
        return step.Id;
    }

    public async Task Handle(AssignInboxTaskCommand command, CancellationToken ct = default)
    {
        var task = await RequiredTask(command.TaskId, ct);
        await RequireAccess(task.AssignedDepartmentId, ct);
        if (command.AssignedUserId.HasValue)
        {
            if (!await repository.UserHasDepartmentAccessAsync(command.AssignedUserId.Value, task.AssignedDepartmentId, ct))
                throw new WorkflowAccessDeniedException("Assigned user does not have access to the task department.");
        }
        task.Assign(command.AssignedUserId);
        await repository.SaveChangesAsync(ct);
    }

    public Task HandleAssignToSelf(Guid taskId, CancellationToken ct = default) =>
        Handle(new AssignInboxTaskCommand(taskId, currentUser.UserId), ct);

    public async Task Handle(CompleteInboxTaskCommand command, CancellationToken ct = default)
    {
        var task = await RequiredTask(command.TaskId, ct);
        await RequireAccess(task.AssignedDepartmentId, ct);
        if (task.AssignedUserId.HasValue && task.AssignedUserId != currentUser.UserId)
            throw new WorkflowAccessDeniedException("Task is assigned to another user.");
        task.Complete();
        await repository.SaveChangesAsync(ct);
    }

    public async Task<Guid> Handle(ReturnToPreviousDepartmentCommand command, CancellationToken ct = default)
    {
        var workflow = await RequiredWorkflow(command.WorkflowInstanceId, true, ct);
        await RequireAccess(workflow.CurrentDepartmentId, ct);
        var step = Execute(() => workflow.Return("Return", command.Comment, currentUser.UserId));
        var fileId = PurchaseFileId(workflow);
        if (fileId.HasValue)
        {
            var file = await repository.FindPurchaseFileAsync(fileId.Value, ct)
                ?? throw new WorkflowValidationException("Purchase file was not found.");
            file.RouteToDepartment(workflow.CurrentDepartmentId);
        }
        await repository.AddTaskAsync(new InboxTask(Guid.NewGuid(), workflow.Id, fileId, IndentId(workflow),
            workflow.CurrentDepartmentId, null, command.TaskTitle, command.TaskDescription, null), ct);
        await repository.SaveChangesAsync(ct);
        return step.Id;
    }

    private async Task<WorkflowInstance> RequiredWorkflow(Guid id, bool steps, CancellationToken ct) =>
        await repository.FindWorkflowAsync(id, steps, ct) ?? throw new WorkflowNotFoundException("Workflow was not found.");
    private async Task<InboxTask> RequiredTask(Guid id, CancellationToken ct) =>
        await repository.FindTaskAsync(id, ct) ?? throw new WorkflowNotFoundException("Inbox task was not found.");
    private async Task RequireAccess(Guid department, CancellationToken ct)
    {
        if (!await HasDepartmentAccess(department, ct))
            throw new WorkflowAccessDeniedException("User does not have access to the assigned department.");
    }
    private async Task<bool> HasDepartmentAccess(Guid department, CancellationToken ct) =>
        currentUser.IsSystemAdmin
        || currentUser.DepartmentIds.Contains(department)
        || await repository.UserHasDepartmentAccessAsync(currentUser.UserId, department, ct);
    private static Guid? PurchaseFileId(WorkflowInstance w) => w.EntityType.Equals("PurchaseFile", StringComparison.OrdinalIgnoreCase) ? w.EntityId : null;
    private static Guid? IndentId(WorkflowInstance w) => w.EntityType.Equals("Indent", StringComparison.OrdinalIgnoreCase) ? w.EntityId : null;
    private static T Execute<T>(Func<T> action) { try { return action(); } catch (InvalidOperationException e) { throw new WorkflowValidationException(e.Message); } }
    private static void Execute(Action action) { try { action(); } catch (InvalidOperationException e) { throw new WorkflowValidationException(e.Message); } }
}

public sealed class WorkflowQueryHandler(IWorkflowRepository repository, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<InboxTaskDto>> Handle(GetMyInboxTasksQuery q, CancellationToken ct = default) =>
        await repository.GetMyTasksAsync(currentUser.UserId, ct);
    public async Task<IReadOnlyList<InboxTaskDto>> Handle(GetDepartmentInboxTasksQuery q, CancellationToken ct = default)
    {
        if (!currentUser.IsSystemAdmin
            && !currentUser.DepartmentIds.Contains(q.DepartmentId)
            && !await repository.UserHasDepartmentAccessAsync(currentUser.UserId, q.DepartmentId, ct))
            throw new WorkflowAccessDeniedException("User does not have access to this department.");
        return await repository.GetDepartmentTasksAsync(q.DepartmentId, ct);
    }
    public async Task<(InboxTaskDto Task, IReadOnlyList<WorkflowStepDto> Timeline)> Handle(
        GetInboxTaskByIdQuery q, CancellationToken ct = default)
    {
        var task = await repository.GetTaskDtoAsync(q.TaskId, ct)
            ?? throw new WorkflowNotFoundException("Inbox task was not found.");
        if (!currentUser.IsSystemAdmin
            && !currentUser.DepartmentIds.Contains(task.AssignedDepartmentId)
            && task.AssignedUserId != currentUser.UserId
            && !await repository.UserHasDepartmentAccessAsync(currentUser.UserId, task.AssignedDepartmentId, ct))
            throw new WorkflowAccessDeniedException("User does not have access to this task.");
        return (task, await repository.GetWorkflowTimelineAsync(task.WorkflowInstanceId, ct));
    }
    public Task<IReadOnlyList<WorkflowStepDto>> Handle(GetPurchaseFileWorkflowTimelineQuery q, CancellationToken ct = default) =>
        repository.GetTimelineAsync(q.PurchaseFileId, ct);
    public async Task<PurchaseFileWorkflowState> Handle(GetPurchaseFileWorkflowStateQuery q, CancellationToken ct = default)
    {
        var file = await repository.FindPurchaseFileAsync(q.PurchaseFileId, ct)
            ?? throw new WorkflowNotFoundException("Purchase file was not found.");
        var workflow = await repository.FindPurchaseFileWorkflowAsync(q.PurchaseFileId, ct);
        var steps = await repository.GetTimelineAsync(q.PurchaseFileId, ct);
        var tasks = await repository.GetOpenPurchaseFileTasksAsync(q.PurchaseFileId, ct);
        var hasAccess = currentUser.IsSystemAdmin || currentUser.DepartmentIds.Contains(file.CurrentDepartmentId)
            || await repository.UserHasDepartmentAccessAsync(currentUser.UserId, file.CurrentDepartmentId, ct);
        var canSend = hasAccess && (currentUser.IsSystemAdmin || currentUser.Permissions.Contains(ApplicationPermissions.PurchaseFileSendToDepartment));
        var editable = file.Status is not PurchaseFileStatus.Completed and not PurchaseFileStatus.Archived;
        return new(q.PurchaseFileId, workflow?.Id, file.CurrentDepartmentId, file.Status.ToString(),
            steps, tasks, editable && workflow is null && canSend, editable && workflow is not null && canSend,
            editable && workflow is not null && steps.Count > 0 && canSend,
            tasks.Any(x => x.AssignedDepartmentId == file.CurrentDepartmentId) && canSend);
    }
}

public sealed class WorkflowValidationException(string message) : Exception(message);
public sealed class WorkflowNotFoundException(string message) : Exception(message);
public sealed class WorkflowAccessDeniedException(string message) : Exception(message);
