using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Workflow;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class WorkflowRepository(PetroProcureDbContext db) : IWorkflowRepository
{
    public Task<WorkflowInstance?> FindWorkflowAsync(Guid id, bool steps, CancellationToken ct)
    { IQueryable<WorkflowInstance> q = db.WorkflowInstances; if (steps) q = q.Include(x => x.Steps); return q.SingleOrDefaultAsync(x => x.Id == id, ct); }
    public Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid id, CancellationToken ct) =>
        db.WorkflowInstances.Include(x => x.Steps).SingleOrDefaultAsync(x => x.EntityType == "PurchaseFile" && x.EntityId == id, ct);
    public Task<PurchaseFile?> FindPurchaseFileAsync(Guid id, CancellationToken ct) => db.PurchaseFiles.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<InboxTask?> FindTaskAsync(Guid id, CancellationToken ct) => db.InboxTasks.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<bool> UserHasDepartmentAccessAsync(Guid userId, Guid departmentId, CancellationToken ct)
    {
        var profileId = await db.Users.Where(x => x.Id == userId).Select(x => x.UserProfileId).SingleOrDefaultAsync(ct);
        return profileId.HasValue && await db.UserDepartments.AnyAsync(x => x.UserProfileId == profileId && x.DepartmentId == departmentId, ct);
    }
    public async Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct) => await db.WorkflowInstances.AddAsync(workflow, ct);
    public async Task AddTaskAsync(InboxTask task, CancellationToken ct) => await db.InboxTasks.AddAsync(task, ct);
    public async Task<IReadOnlyList<InboxTaskDto>> GetMyTasksAsync(Guid userId, CancellationToken ct)
    {
        var profile = await db.Users.Where(x => x.Id == userId).Select(x => x.UserProfileId).SingleOrDefaultAsync(ct);
        var deps = profile.HasValue ? await db.UserDepartments.Where(x => x.UserProfileId == profile).Select(x => x.DepartmentId).ToListAsync(ct) : [];
        return await Project(db.InboxTasks.AsNoTracking()
            .Where(x => (x.AssignedUserId == userId || (!x.AssignedUserId.HasValue && deps.Contains(x.AssignedDepartmentId)))
                && x.Status != WorkflowStatus.Completed)).ToListAsync(ct);
    }
    public async Task<IReadOnlyList<InboxTaskDto>> GetDepartmentTasksAsync(Guid departmentId, CancellationToken ct) =>
        await Project(db.InboxTasks.AsNoTracking()
            .Where(x => x.AssignedDepartmentId == departmentId && x.Status != WorkflowStatus.Completed)).ToListAsync(ct);
    public Task<InboxTaskDto?> GetTaskDtoAsync(Guid taskId, CancellationToken ct) =>
        Project(db.InboxTasks.AsNoTracking().Where(x => x.Id == taskId)).SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<WorkflowStepDto>> GetWorkflowTimelineAsync(Guid workflowInstanceId, CancellationToken ct) =>
        await db.WorkflowSteps.AsNoTracking().Where(x => x.WorkflowInstanceId == workflowInstanceId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new WorkflowStepDto(x.Id, x.FromDepartmentId, x.ToDepartmentId, x.ActionName,
                x.Comment, x.CreatedByUserId, x.CreatedAt, x.CompletedByUserId, x.CompletedAt))
            .ToListAsync(ct);
    public async Task<IReadOnlyList<WorkflowStepDto>> GetTimelineAsync(Guid id, CancellationToken ct) =>
        await db.WorkflowSteps.AsNoTracking().Where(x => db.WorkflowInstances.Any(w => w.Id == x.WorkflowInstanceId && w.EntityType == "PurchaseFile" && w.EntityId == id))
            .OrderBy(x => x.CreatedAt).Select(x => new WorkflowStepDto(x.Id, x.FromDepartmentId, x.ToDepartmentId, x.ActionName, x.Comment, x.CreatedByUserId, x.CreatedAt, x.CompletedByUserId, x.CompletedAt)).ToListAsync(ct);
    public async Task<IReadOnlyList<InboxTaskDto>> GetOpenPurchaseFileTasksAsync(Guid id, CancellationToken ct) =>
        await Project(db.InboxTasks.AsNoTracking()
            .Where(x => x.PurchaseFileId == id && x.Status != WorkflowStatus.Completed)).ToListAsync(ct);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    private static IQueryable<InboxTaskDto> Project(IQueryable<InboxTask> query) =>
        query.Select(x => new InboxTaskDto(x.Id, x.WorkflowInstanceId, x.PurchaseFileId, x.IndentId,
            x.AssignedDepartmentId, x.AssignedUserId, x.Title, x.Description, x.Status,
            x.DueDate, x.CreatedAt, x.CompletedAt));
}
