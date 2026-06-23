using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Organization;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class WorkflowActionRepository(PetroProcureDbContext db) : IWorkflowActionRepository
{
    public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct) =>
        db.PurchaseFiles.Include(x => x.StatusHistory).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<WorkflowInstance?> FindWorkflowAsync(Guid purchaseFileId, CancellationToken ct) =>
        db.WorkflowInstances.Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.EntityType == "PurchaseFile" && x.EntityId == purchaseFileId, ct);
    public Task<Department?> FindDepartmentAsync(Guid id, CancellationToken ct) =>
        db.Departments.SingleOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    public Task<Department?> FindDepartmentByTypeAsync(DepartmentType type, CancellationToken ct) =>
        db.Departments.SingleOrDefaultAsync(x => x.Type == type && x.IsActive, ct);
    public async Task<IReadOnlyList<WorkflowActionDefinition>> GetDefinitionsAsync(
        DepartmentType type, PurchaseFileStatus status, CancellationToken ct) =>
        await db.WorkflowActionDefinitions.AsNoTracking()
            .Where(x => x.FromDepartmentType == type && x.FromStatus == status && x.IsActive)
            .OrderBy(x => x.Title).ToListAsync(ct);
    public Task<WorkflowActionDefinition?> FindDefinitionAsync(string code, CancellationToken ct) =>
        db.WorkflowActionDefinitions.SingleOrDefaultAsync(x => x.Code == code, ct);
    public async Task<IReadOnlyList<InboxTask>> GetOpenTasksAsync(Guid id, CancellationToken ct) =>
        await db.InboxTasks.Where(x => x.PurchaseFileId == id && x.Status != WorkflowStatus.Completed).ToListAsync(ct);
    public async Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct) =>
        await db.WorkflowInstances.AddAsync(workflow, ct);
    public async Task AddTaskAsync(InboxTask task, CancellationToken ct) =>
        await db.InboxTasks.AddAsync(task, ct);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
