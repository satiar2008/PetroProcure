using PetroProcure.Application.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.UnitTests.Application;
public sealed class WorkflowModuleTests
{
    [Fact]
    public async Task SendingPurchaseFileCreatesTaskChangesDepartmentAndTimeline()
    {
        var source = Guid.NewGuid(); var applicant = Guid.NewGuid(); var user = Guid.NewGuid();
        var file = new PurchaseFile(Guid.NewGuid(), "PF-2026-000001", "File", null, PurchaseFilePriority.Normal, null, source, source, null, user);
        var workflow = new WorkflowInstance(Guid.NewGuid(), "PurchaseFile", file.Id, source, user);
        var repo = new FakeRepo { Workflow = workflow, File = file };
        repo.Access.Add((user, source));
        var handler = new WorkflowCommandHandler(repo, new TestCurrentUser(user, [source]));

        await handler.Handle(new SendToDepartmentCommand(workflow.Id, applicant, "Technical review", "Please review", "Review technical data", null, null));

        Assert.Equal(applicant, file.CurrentDepartmentId);
        Assert.Equal(applicant, Assert.Single(repo.Tasks).AssignedDepartmentId);
        Assert.Equal("Technical review", Assert.Single(workflow.Steps).ActionName);
    }

    [Fact]
    public async Task UnauthorizedDepartmentUserCannotCompleteTask()
    {
        var task = new InboxTask(Guid.NewGuid(), Guid.NewGuid(), null, null, Guid.NewGuid(), null, "Task", null, null);
        var repo = new FakeRepo { CurrentTask = task };

        await Assert.ThrowsAsync<WorkflowAccessDeniedException>(() =>
            new WorkflowCommandHandler(repo, new TestCurrentUser(Guid.NewGuid()))
                .Handle(new CompleteInboxTaskCommand(task.Id)));
    }

    private sealed class FakeRepo : IWorkflowRepository
    {
        public WorkflowInstance? Workflow; public PurchaseFile? File; public InboxTask? CurrentTask;
        public List<InboxTask> Tasks { get; } = []; public HashSet<(Guid, Guid)> Access { get; } = [];
        public Task<WorkflowInstance?> FindWorkflowAsync(Guid id, bool includeSteps, CancellationToken ct) => Task.FromResult(Workflow?.Id == id ? Workflow : null);
        public Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid id, CancellationToken ct) => Task.FromResult(Workflow);
        public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct) => Task.FromResult(File?.Id == id ? File : null);
        public Task<InboxTask?> FindTaskAsync(Guid id, CancellationToken ct) => Task.FromResult(CurrentTask?.Id == id ? CurrentTask : null);
        public Task<bool> UserHasDepartmentAccessAsync(Guid userId, Guid departmentId, CancellationToken ct) => Task.FromResult(Access.Contains((userId, departmentId)));
        public Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct) { Workflow = workflow; return Task.CompletedTask; }
        public Task AddTaskAsync(InboxTask task, CancellationToken ct) { Tasks.Add(task); CurrentTask = task; return Task.CompletedTask; }
        public Task<IReadOnlyList<InboxTaskDto>> GetMyTasksAsync(Guid userId, CancellationToken ct) => System.Threading.Tasks.Task.FromResult<IReadOnlyList<InboxTaskDto>>([]);
        public Task<IReadOnlyList<InboxTaskDto>> GetDepartmentTasksAsync(Guid departmentId, CancellationToken ct) => System.Threading.Tasks.Task.FromResult<IReadOnlyList<InboxTaskDto>>([]);
        public Task<InboxTaskDto?> GetTaskDtoAsync(Guid taskId, CancellationToken ct) => Task.FromResult<InboxTaskDto?>(null);
        public Task<IReadOnlyList<WorkflowStepDto>> GetWorkflowTimelineAsync(Guid workflowInstanceId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<WorkflowStepDto>>([]);
        public Task<IReadOnlyList<WorkflowStepDto>> GetTimelineAsync(Guid purchaseFileId, CancellationToken ct) => System.Threading.Tasks.Task.FromResult<IReadOnlyList<WorkflowStepDto>>([]);
        public Task<IReadOnlyList<InboxTaskDto>> GetOpenPurchaseFileTasksAsync(Guid purchaseFileId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<InboxTaskDto>>([]);
        public Task SaveChangesAsync(CancellationToken ct) => System.Threading.Tasks.Task.CompletedTask;
    }
}
