using PetroProcure.Application.Security;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.Application.Workflow;

public sealed record AllowedWorkflowAction(
    Guid Id, string Code, string Title, DepartmentType FromDepartmentType,
    DepartmentType? ToDepartmentType, PurchaseFileStatus FromStatus,
    PurchaseFileStatus ToStatus, string RequiredPermission, bool RequiresComment,
    bool IsReturnAction, bool IsFinalAction);
public sealed record ExecuteWorkflowActionCommand(Guid PurchaseFileId, string ActionCode, string? Comment);
public sealed record WorkflowActionExecutionResult(
    Guid PurchaseFileId, Guid WorkflowInstanceId, Guid WorkflowStepId,
    Guid CurrentDepartmentId, PurchaseFileStatus Status, bool WorkflowCompleted);

public interface IWorkflowActionResolver
{
    Task<IReadOnlyList<AllowedWorkflowAction>> GetAllowedActionsAsync(
        Guid purchaseFileId, ICurrentUserService currentUser, CancellationToken ct = default);
    Task<WorkflowActionExecutionResult> ExecuteAsync(
        ExecuteWorkflowActionCommand command, ICurrentUserService currentUser, CancellationToken ct = default);
}

public interface IWorkflowActionRepository
{
    Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct);
    Task<WorkflowInstance?> FindWorkflowAsync(Guid purchaseFileId, CancellationToken ct);
    Task<Department?> FindDepartmentAsync(Guid id, CancellationToken ct);
    Task<Department?> FindDepartmentByTypeAsync(DepartmentType type, CancellationToken ct);
    Task<IReadOnlyList<WorkflowActionDefinition>> GetDefinitionsAsync(
        DepartmentType departmentType, PurchaseFileStatus status, CancellationToken ct);
    Task<WorkflowActionDefinition?> FindDefinitionAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<InboxTask>> GetOpenTasksAsync(Guid purchaseFileId, CancellationToken ct);
    Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct);
    Task AddTaskAsync(InboxTask task, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class WorkflowActionResolver(IWorkflowActionRepository repository) : IWorkflowActionResolver
{
    public async Task<IReadOnlyList<AllowedWorkflowAction>> GetAllowedActionsAsync(
        Guid purchaseFileId, ICurrentUserService currentUser, CancellationToken ct = default)
    {
        var file = await repository.FindPurchaseFileAsync(purchaseFileId, ct)
            ?? throw new WorkflowNotFoundException("Purchase file was not found.");
        var department = await repository.FindDepartmentAsync(file.CurrentDepartmentId, ct)
            ?? throw new WorkflowValidationException("Current department was not found.");
        if (!HasDepartmentAccess(currentUser, file.CurrentDepartmentId)) return [];
        var definitions = await repository.GetDefinitionsAsync(department.Type, file.Status, ct);
        return definitions
            .Where(action => currentUser.IsSystemAdmin
                || currentUser.Permissions.Contains(action.RequiredPermission, StringComparer.OrdinalIgnoreCase))
            .Select(ToAllowed).ToArray();
    }

    public async Task<WorkflowActionExecutionResult> ExecuteAsync(
        ExecuteWorkflowActionCommand command, ICurrentUserService currentUser, CancellationToken ct = default)
    {
        var file = await repository.FindPurchaseFileAsync(command.PurchaseFileId, ct)
            ?? throw new WorkflowNotFoundException("Purchase file was not found.");
        var currentDepartment = await repository.FindDepartmentAsync(file.CurrentDepartmentId, ct)
            ?? throw new WorkflowValidationException("Current department was not found.");
        var action = await repository.FindDefinitionAsync(command.ActionCode, ct)
            ?? throw new WorkflowValidationException("Workflow action was not found.");
        if (!action.IsActive || action.FromDepartmentType != currentDepartment.Type || action.FromStatus != file.Status)
            throw new WorkflowAccessDeniedException("Workflow action is not allowed for the current state.");
        if (!HasDepartmentAccess(currentUser, file.CurrentDepartmentId))
            throw new WorkflowAccessDeniedException("User does not belong to the current department.");
        if (!currentUser.IsSystemAdmin
            && !currentUser.Permissions.Contains(action.RequiredPermission, StringComparer.OrdinalIgnoreCase))
            throw new WorkflowAccessDeniedException("User does not have the required permission.");
        if (action.RequiresComment && string.IsNullOrWhiteSpace(command.Comment))
            throw new WorkflowValidationException("Comment is required for this action.");

        var workflow = await repository.FindWorkflowAsync(file.Id, ct);
        if (workflow is null)
        {
            workflow = new WorkflowInstance(Guid.NewGuid(), "PurchaseFile", file.Id, file.CurrentDepartmentId, currentUser.UserId);
            await repository.AddWorkflowAsync(workflow, ct);
        }
        var targetDepartment = await ResolveTarget(action, workflow, currentDepartment, ct);
        foreach (var openStep in workflow.Steps.Where(x => !x.CompletedAt.HasValue))
            openStep.Complete(currentUser.UserId);
        foreach (var task in await repository.GetOpenTasksAsync(file.Id, ct))
            task.Complete();

        var step = workflow.Send(targetDepartment.Id, action.Title, command.Comment, currentUser.UserId);
        file.ApplyWorkflowTransition(targetDepartment.Id, action.ToStatus, currentUser.UserId, command.Comment, currentUser.IsSystemAdmin);

        if (action.IsFinalAction)
        {
            step.Complete(currentUser.UserId);
            workflow.Complete(currentUser.UserId);
        }
        else
        {
            await repository.AddTaskAsync(new InboxTask(
                Guid.NewGuid(), workflow.Id, file.Id, null, targetDepartment.Id, null,
                action.Title, command.Comment, null), ct);
        }
        await repository.SaveChangesAsync(ct);
        return new(file.Id, workflow.Id, step.Id, file.CurrentDepartmentId, file.Status, action.IsFinalAction);
    }

    private async Task<Department> ResolveTarget(
        WorkflowActionDefinition action, WorkflowInstance workflow, Department current, CancellationToken ct)
    {
        if (action.IsFinalAction) return current;
        if (action.IsReturnAction)
        {
            var previousId = workflow.Steps.OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault(x => x.FromDepartmentId != current.Id)?.FromDepartmentId
                ?? throw new WorkflowValidationException("No previous department exists.");
            return await repository.FindDepartmentAsync(previousId, ct)
                ?? throw new WorkflowValidationException("Previous department was not found.");
        }
        return action.ToDepartmentType.HasValue
            ? await repository.FindDepartmentByTypeAsync(action.ToDepartmentType.Value, ct)
                ?? throw new WorkflowValidationException("Target department was not found.")
            : current;
    }

    private static bool HasDepartmentAccess(ICurrentUserService user, Guid departmentId) =>
        user.IsSystemAdmin || user.DepartmentIds.Contains(departmentId);
    private static AllowedWorkflowAction ToAllowed(WorkflowActionDefinition x) =>
        new(x.Id, x.Code, x.Title, x.FromDepartmentType, x.ToDepartmentType, x.FromStatus,
            x.ToStatus, x.RequiredPermission, x.RequiresComment, x.IsReturnAction, x.IsFinalAction);
}
