using PetroProcure.Application.Security;
using PetroProcure.Application.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.UnitTests.Application;

public sealed class WorkflowActionMatrixTests
{
    [Fact]
    public async Task AllowedActionAppearsForCorrectDepartmentAndPermission()
    {
        var setup = Setup();
        var actions = await setup.Resolver.GetAllowedActionsAsync(setup.File.Id, setup.User);
        Assert.Contains(actions, x => x.Code == "PURCHASE_TO_APPLICANT");
    }

    [Fact]
    public async Task ActionDoesNotAppearForUnauthorizedDepartment()
    {
        var setup = Setup(userDepartments: []);
        Assert.Empty(await setup.Resolver.GetAllowedActionsAsync(setup.File.Id, setup.User));
    }

    [Fact]
    public async Task ExecuteActionChangesStatusDepartmentAndCreatesStep()
    {
        var setup = Setup();
        var result = await setup.Resolver.ExecuteAsync(
            new(setup.File.Id, "PURCHASE_TO_APPLICANT", "Technical review"), setup.User);
        Assert.Equal(PurchaseFileStatus.WaitingForTechnicalReview, setup.File.Status);
        Assert.Equal(setup.Applicant.Id, setup.File.CurrentDepartmentId);
        Assert.Single(setup.Repository.Workflow!.Steps);
        Assert.Equal(setup.Applicant.Id, result.CurrentDepartmentId);
    }

    [Fact]
    public async Task FinalActionCompletesWorkflow()
    {
        var setup = Setup(finalAction: true);
        var result = await setup.Resolver.ExecuteAsync(
            new(setup.File.Id, "PURCHASE_COMPLETE", "Completed"), setup.User);
        Assert.True(result.WorkflowCompleted);
        Assert.Equal(WorkflowStatus.Completed, setup.Repository.Workflow!.Status);
        Assert.Equal(PurchaseFileStatus.Completed, setup.File.Status);
    }

    [Fact]
    public async Task ReturnActionReturnsToPreviousDepartment()
    {
        var setup = Setup(returnAction: true);
        var workflow = new WorkflowInstance(Guid.NewGuid(), "PurchaseFile", setup.File.Id, setup.Applicant.Id, setup.User.UserId);
        workflow.Send(setup.Purchase.Id, "Applicant result", null, setup.User.UserId);
        setup.Repository.Workflow = workflow;
        var result = await setup.Resolver.ExecuteAsync(
            new(setup.File.Id, "RETURN_PREVIOUS", "Return"), setup.User);
        Assert.Equal(setup.Applicant.Id, result.CurrentDepartmentId);
        Assert.Equal(setup.Applicant.Id, setup.File.CurrentDepartmentId);
    }

    private static TestSetup Setup(
        IReadOnlyCollection<Guid>? userDepartments = null, bool finalAction = false, bool returnAction = false)
    {
        var purchase = new Department(Guid.NewGuid(), "Purchase", DepartmentType.PurchaseDepartment);
        var applicant = new Department(Guid.NewGuid(), "Applicant", DepartmentType.Applicant);
        var file = new PurchaseFile(Guid.NewGuid(), "PF-2026-000999", "Test", null,
            PurchaseFilePriority.Normal, null, purchase.Id, purchase.Id, null, Guid.NewGuid());
        file.ChangeStatus(PurchaseFileStatus.InPurchaseDepartment, Guid.NewGuid());
        var action = finalAction
            ? new WorkflowActionDefinition(Guid.NewGuid(), "PURCHASE_COMPLETE", "Complete",
                DepartmentType.PurchaseDepartment, null, PurchaseFileStatus.InPurchaseDepartment,
                PurchaseFileStatus.Completed, ApplicationPermissions.PurchaseFileSendToDepartment, true, false, true)
            : returnAction
                ? new WorkflowActionDefinition(Guid.NewGuid(), "RETURN_PREVIOUS", "Return",
                    DepartmentType.PurchaseDepartment, null, PurchaseFileStatus.InPurchaseDepartment,
                    PurchaseFileStatus.WaitingForTechnicalReview, ApplicationPermissions.PurchaseFileSendToDepartment, true, true, false)
                : new WorkflowActionDefinition(Guid.NewGuid(), "PURCHASE_TO_APPLICANT", "Technical review",
                    DepartmentType.PurchaseDepartment, DepartmentType.Applicant, PurchaseFileStatus.InPurchaseDepartment,
                    PurchaseFileStatus.WaitingForTechnicalReview, ApplicationPermissions.PurchaseFileSendToDepartment, true, false, false);
        var repo = new FakeRepository(file, purchase, applicant, action);
        var user = new TestUser(userDepartments ?? [purchase.Id]);
        return new(new WorkflowActionResolver(repo), repo, file, purchase, applicant, user);
    }

    private sealed record TestSetup(
        WorkflowActionResolver Resolver, FakeRepository Repository, PurchaseFile File,
        Department Purchase, Department Applicant, TestUser User);

    private sealed class TestUser(IReadOnlyCollection<Guid> departments) : ICurrentUserService
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public string? UserName => "user"; public string? Email => null;
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [ApplicationPermissions.PurchaseFileSendToDepartment];
        public IReadOnlyCollection<Guid> DepartmentIds => departments;
        public bool IsAuthenticated => true; public bool IsSystemAdmin => false;
    }

    private sealed class FakeRepository(
        PurchaseFile file, Department purchase, Department applicant,
        WorkflowActionDefinition action) : IWorkflowActionRepository
    {
        public WorkflowInstance? Workflow { get; set; }
        public List<InboxTask> Tasks { get; } = [];
        public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct) => Task.FromResult<PurchaseFile?>(file);
        public Task<WorkflowInstance?> FindWorkflowAsync(Guid id, CancellationToken ct) => Task.FromResult(Workflow);
        public Task<Department?> FindDepartmentAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<Department?>(id == purchase.Id ? purchase : id == applicant.Id ? applicant : null);
        public Task<Department?> FindDepartmentByTypeAsync(DepartmentType type, CancellationToken ct) =>
            Task.FromResult<Department?>(type == DepartmentType.PurchaseDepartment ? purchase : type == DepartmentType.Applicant ? applicant : null);
        public Task<IReadOnlyList<WorkflowActionDefinition>> GetDefinitionsAsync(DepartmentType type, PurchaseFileStatus status, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<WorkflowActionDefinition>>(
                action.FromDepartmentType == type && action.FromStatus == status ? [action] : []);
        public Task<WorkflowActionDefinition?> FindDefinitionAsync(string code, CancellationToken ct) =>
            Task.FromResult<WorkflowActionDefinition?>(action.Code == code ? action : null);
        public Task<IReadOnlyList<InboxTask>> GetOpenTasksAsync(Guid id, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<InboxTask>>(Tasks);
        public Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct) { Workflow = workflow; return Task.CompletedTask; }
        public Task AddTaskAsync(InboxTask task, CancellationToken ct) { Tasks.Add(task); return Task.CompletedTask; }
        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
