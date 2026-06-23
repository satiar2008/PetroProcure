using PetroProcure.Application.Workflow;
using PetroProcure.Application.Security;
using PetroProcure.Api.Security;
using PetroProcure.Contracts.V1.Workflow;
using PetroProcure.Api.Contracts;

namespace PetroProcure.Api.Endpoints;
public static class WorkflowEndpoints
{
    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inbox/my", async (WorkflowQueryHandler h, CancellationToken ct) =>
            (await h.Handle(new GetMyInboxTasksQuery(), ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapGet("/api/inbox/department/{departmentId:guid}", async (Guid departmentId, WorkflowQueryHandler h, CancellationToken ct) =>
            (await h.Handle(new GetDepartmentInboxTasksQuery(departmentId), ct)).Select(x => x.ToContract()))
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapGet("/api/inbox/tasks/{taskId:guid}", async (Guid taskId, WorkflowQueryHandler h, CancellationToken ct) =>
        {
            var result = await h.Handle(new GetInboxTaskByIdQuery(taskId), ct);
            return new InboxTaskDetailDto(result.Task.ToContract(), result.Timeline.Select(x => x.ToContract()).ToArray());
        }).RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapGet("/api/purchase-files/{id:guid}/workflow/timeline", async (Guid id, WorkflowQueryHandler h, CancellationToken ct) =>
            new WorkflowTimelineDto(id, (await h.Handle(new GetPurchaseFileWorkflowTimelineQuery(id), ct)).Select(x => x.ToContract()).ToArray()))
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapGet("/api/purchase-files/{id:guid}/workflow/state", async (Guid id, WorkflowQueryHandler h, CancellationToken ct) =>
        {
            var state = await h.Handle(new GetPurchaseFileWorkflowStateQuery(id), ct);
            return new PurchaseFileWorkflowStateDto(
                state.PurchaseFileId, state.WorkflowInstanceId, state.CurrentDepartmentId, state.Status,
                state.Steps.Select(x => x.ToContract()).ToArray(),
                state.OpenTasks.Select(x => x.ToContract()).ToArray(),
                state.CanStart, state.CanSend, state.CanReturn, state.CanCompleteTask);
        }).RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapGet("/api/purchase-files/{id:guid}/workflow/allowed-actions", async (
            Guid id, IWorkflowActionResolver resolver, ICurrentUserService currentUser, CancellationToken ct) =>
            (await resolver.GetAllowedActionsAsync(id, currentUser, ct)).Select(x => new WorkflowActionDto(
                x.Id, x.Code, x.Title, x.FromDepartmentType, x.ToDepartmentType, x.FromStatus,
                x.ToStatus, x.RequiredPermission, x.RequiresComment, x.IsReturnAction, x.IsFinalAction)))
            .RequirePermission(ApplicationPermissions.PurchaseFileView);
        app.MapPost("/api/purchase-files/{id:guid}/workflow/execute-action", async (
            Guid id, ExecuteWorkflowActionRequest request, IWorkflowActionResolver resolver,
            ICurrentUserService currentUser, CancellationToken ct) =>
        {
            var result = await resolver.ExecuteAsync(
                new ExecuteWorkflowActionCommand(id, request.ActionCode, request.Comment), currentUser, ct);
            return Results.Ok(new WorkflowActionExecutionResultDto(
                result.PurchaseFileId, result.WorkflowInstanceId, result.WorkflowStepId,
                result.CurrentDepartmentId, result.Status, result.WorkflowCompleted));
        }).RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        app.MapPost("/api/workflow/start", async (StartWorkflowRequest r, WorkflowCommandHandler h, CancellationToken ct) =>
            Results.Ok(new { Id = await h.Handle(new StartWorkflowCommand(r.EntityType, r.EntityId, r.CurrentDepartmentId), ct) }))
            .RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        app.MapPost("/api/workflow/send-to-department", async (SendToDepartmentRequest r, WorkflowCommandHandler h, CancellationToken ct) =>
            Results.Ok(new { StepId = await h.Handle(new SendToDepartmentCommand(
                r.WorkflowInstanceId, r.ToDepartmentId, r.ActionName, r.Comment, r.TaskTitle, r.TaskDescription, r.DueDate), ct) }))
            .RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        app.MapPost("/api/inbox/{taskId:guid}/assign", async (Guid taskId, AssignInboxTaskRequest r, WorkflowCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new AssignInboxTaskCommand(taskId, r.AssignedUserId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        app.MapPost("/api/inbox/{taskId:guid}/assign-to-self", async (Guid taskId, WorkflowCommandHandler h, CancellationToken ct) =>
        { await h.HandleAssignToSelf(taskId, ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        app.MapPost("/api/inbox/{taskId:guid}/complete", async (Guid taskId, WorkflowCommandHandler h, CancellationToken ct) =>
        { await h.Handle(new CompleteInboxTaskCommand(taskId), ct); return Results.NoContent(); })
            .RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        app.MapPost("/api/workflow/return", async (ReturnWorkflowRequest r, WorkflowCommandHandler h, CancellationToken ct) =>
            Results.Ok(new { StepId = await h.Handle(new ReturnToPreviousDepartmentCommand(
                r.WorkflowInstanceId, r.Comment, r.TaskTitle, r.TaskDescription), ct) }))
            .RequirePermission(ApplicationPermissions.PurchaseFileSendToDepartment);
        return app;
    }
}
