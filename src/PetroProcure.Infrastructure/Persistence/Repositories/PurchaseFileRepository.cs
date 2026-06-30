using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Domain.Modules.Workflow;
using PetroProcure.Contracts.V1.Common;
using PurchaseFileListRequest = PetroProcure.Contracts.V1.PurchaseFiles.PurchaseFileListRequest;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class PurchaseFileRepository(PetroProcureDbContext dbContext) : IPurchaseFileRepository
{
    public async Task<string> GenerateNextFileNumberAsync(int year, CancellationToken ct)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await dbContext.PurchaseFileSequences.SingleOrDefaultAsync(value => value.Year == year, ct);
            int next;
            if (sequence is null)
            {
                next = 1;
                dbContext.PurchaseFileSequences.Add(new PurchaseFileSequence(Guid.NewGuid(), year, next));
            }
            else next = sequence.Next();
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return $"PF-{year:0000}-{next:000000}";
        });
    }

    public Task<bool> FileNumberExistsAsync(string fileNumber, CancellationToken ct) =>
        dbContext.PurchaseFiles.AnyAsync(file => file.FileNumber == fileNumber, ct);
    public Task<bool> SourceIndentAlreadyUsedAsync(Guid indentId, CancellationToken ct) =>
        dbContext.PurchaseFiles.AnyAsync(file => file.SourceIndentId == indentId, ct);
    public Task<PurchaseFile?> FindAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<PurchaseFile> query = dbContext.PurchaseFiles;
        if (includeDetails)
            query = query.Include(file => file.Items).Include(file => file.StatusHistory).Include(file => file.Notes);
        return query.SingleOrDefaultAsync(file => file.Id == id, ct);
    }
    public Task<PurchaseFile?> FindByNumberAsync(string fileNumber, CancellationToken ct) =>
        dbContext.PurchaseFiles.Include(file => file.Items).Include(file => file.StatusHistory).Include(file => file.Notes)
            .SingleOrDefaultAsync(file => file.FileNumber == fileNumber, ct);
    public Task<PurchaseFile?> FindBySourceIndentAsync(Guid indentId, bool includeDetails, CancellationToken ct)
    {
        IQueryable<PurchaseFile> query = dbContext.PurchaseFiles;
        if (includeDetails)
            query = query.Include(file => file.Items).Include(file => file.StatusHistory).Include(file => file.Notes);
        return query.SingleOrDefaultAsync(file => file.SourceIndentId == indentId, ct);
    }
    public Task<Indent?> FindApprovedIndentAsync(Guid id, CancellationToken ct) =>
        dbContext.Indents.Include(indent => indent.Items)
            .SingleOrDefaultAsync(indent => indent.Id == id
                && (indent.Status == IndentStatus.Approved
                    || indent.Status == IndentStatus.SentToPurchaseDepartment), ct);
    public Task<string?> GetDepartmentNameAsync(Guid id, CancellationToken ct) =>
        dbContext.Departments.AsNoTracking().Where(x => x.Id == id).Select(x => x.Name).SingleOrDefaultAsync(ct);
    public Task<Guid?> GetDepartmentIdByTypeAsync(DepartmentType type, CancellationToken ct) =>
        dbContext.Departments.AsNoTracking().Where(x => x.Type == type && x.IsActive)
            .Select(x => (Guid?)x.Id).FirstOrDefaultAsync(ct);
    public Task<WorkflowInstance?> FindPurchaseFileWorkflowAsync(Guid purchaseFileId, CancellationToken ct) =>
        dbContext.WorkflowInstances.Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.EntityType == "PurchaseFile" && x.EntityId == purchaseFileId, ct);
    public async Task AddWorkflowAsync(WorkflowInstance workflow, CancellationToken ct) =>
        await dbContext.WorkflowInstances.AddAsync(workflow, ct);
    public async Task AddInboxTaskAsync(InboxTask task, CancellationToken ct) =>
        await dbContext.InboxTasks.AddAsync(task, ct);
    public Task<InboxTask?> FindInboxTaskAsync(Guid id, CancellationToken ct) =>
        dbContext.InboxTasks.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> HasActiveTechnicalReviewAsync(Guid purchaseFileId, Guid departmentId, CancellationToken ct) =>
        dbContext.PurchaseFileTechnicalReviews.AnyAsync(x => x.PurchaseFileId == purchaseFileId
            && x.DepartmentId == departmentId
            && x.Status != PurchaseFileTechnicalReviewStatus.Approved
            && x.Status != PurchaseFileTechnicalReviewStatus.Rejected
            && x.Status != PurchaseFileTechnicalReviewStatus.Cancelled, ct);
    public async Task AddTechnicalReviewAsync(PurchaseFileTechnicalReview review, CancellationToken ct) =>
        await dbContext.PurchaseFileTechnicalReviews.AddAsync(review, ct);
    public Task<PurchaseFileTechnicalReview?> FindTechnicalReviewAsync(Guid id, bool includeFile, CancellationToken ct) =>
        dbContext.PurchaseFileTechnicalReviews.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> GetTechnicalReviewsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        await ProjectTechnicalReviews(dbContext.PurchaseFileTechnicalReviews.AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId)).ToListAsync(ct);
    public async Task<IReadOnlyList<PurchaseFileTechnicalReviewDto>> GetTechnicalReviewsByDepartmentsAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken ct)
    {
        var query = dbContext.PurchaseFileTechnicalReviews.AsNoTracking();
        if (departmentIds.Count > 0) query = query.Where(x => departmentIds.Contains(x.DepartmentId));
        return await ProjectTechnicalReviews(query.OrderByDescending(x => x.RequestedAt)).ToListAsync(ct);
    }
    public Task<PurchaseFileTechnicalReviewDto?> GetTechnicalReviewDtoAsync(Guid id, CancellationToken ct) =>
        ProjectTechnicalReviews(dbContext.PurchaseFileTechnicalReviews.AsNoTracking().Where(x => x.Id == id))
            .SingleOrDefaultAsync(ct);
    public async Task<ApplicantDashboardDto> GetApplicantDashboardAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken ct)
    {
        var query = dbContext.PurchaseFileTechnicalReviews.AsNoTracking();
        if (departmentIds.Count > 0) query = query.Where(x => departmentIds.Contains(x.DepartmentId));
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var completed = query.Where(x => x.CompletedAt.HasValue);
        var completedDurations = await completed
            .Where(x => x.CompletedAt >= monthStart)
            .Select(x => new { x.RequestedAt, x.CompletedAt })
            .ToListAsync(ct);
        var recent = await ProjectTechnicalReviews(query.OrderByDescending(x => x.RequestedAt).Take(5)).ToListAsync(ct);
        return new(
            await query.CountAsync(x => x.Status == PurchaseFileTechnicalReviewStatus.Requested, ct),
            await query.CountAsync(x => x.Status == PurchaseFileTechnicalReviewStatus.InReview, ct),
            await query.CountAsync(x => x.Status == PurchaseFileTechnicalReviewStatus.ClarificationRequested, ct),
            completedDurations.Count,
            completedDurations.Count == 0 ? null : completedDurations.Average(x => (x.CompletedAt!.Value - x.RequestedAt).TotalHours),
            recent);
    }
    public async Task<DepartmentDashboardDto> GetDepartmentDashboardAsync(string departmentKey, IReadOnlyCollection<Guid> departmentIds, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var inbox = dbContext.InboxTasks.AsNoTracking()
            .Where(x => x.Status != WorkflowStatus.Completed);
        var files = dbContext.PurchaseFiles.AsNoTracking().AsQueryable();
        if (departmentIds.Count > 0)
        {
            inbox = inbox.Where(x => departmentIds.Contains(x.AssignedDepartmentId));
            files = files.Where(x => departmentIds.Contains(x.CurrentDepartmentId));
        }
        var statusCounts = await files.GroupBy(x => x.Status.ToString()).Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        var recent = await files.OrderByDescending(x => x.CreatedAt).Take(5)
            .Select(file => new PurchaseFileListDto(file.Id, file.FileNumber, file.Title, file.Status, file.Priority,
                file.CurrentDepartmentId, file.ResponsibleUserId, file.CreatedAt, file.Items.Count, null,
                dbContext.Inquiries.Count(inquiry => inquiry.PurchaseFileId == file.Id)))
            .ToListAsync(ct);
        return new(
            departmentKey,
            DepartmentTitle(departmentKey),
            await inbox.CountAsync(ct),
            await inbox.CountAsync(ct),
            await inbox.CountAsync(x => x.DueDate.HasValue && x.DueDate < today, ct),
            statusCounts,
            recent);
    }
    public Task<PurchaseFileMescSnapshot?> GetMescSnapshotAsync(Guid id, CancellationToken ct) =>
        dbContext.MescItems.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new PurchaseFileMescSnapshot(
                item.Id, item.Code, item.GeneralGroupCode, item.GeneralGroup!.Description, item.Description, item.IsActive))
            .SingleOrDefaultAsync(ct);
    public Task<bool> UnitOfMeasureExistsAsync(Guid id, CancellationToken ct) =>
        dbContext.UnitOfMeasures.AnyAsync(unit => unit.Id == id && unit.IsActive, ct);
    public async Task AddAsync(PurchaseFile purchaseFile, CancellationToken ct) =>
        await dbContext.PurchaseFiles.AddAsync(purchaseFile, ct);
    public async Task<IReadOnlyList<PurchaseFileListDto>> GetAllAsync(CancellationToken ct) =>
        await dbContext.PurchaseFiles.AsNoTracking().OrderByDescending(file => file.CreatedAt)
            .Select(file => new PurchaseFileListDto(
                file.Id, file.FileNumber, file.Title, file.Status, file.Priority,
                file.CurrentDepartmentId, file.ResponsibleUserId, file.CreatedAt, file.Items.Count, null,
                dbContext.Inquiries.Count(inquiry => inquiry.PurchaseFileId == file.Id)))
            .ToListAsync(ct);
    public async Task<PagedResult<PurchaseFileListDto>> GetPagedAsync(PurchaseFileListRequest request, CancellationToken ct)
    {
        var query = dbContext.PurchaseFiles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.FileNumber))
            query = query.Where(x => x.FileNumber.Contains(request.FileNumber));
        if (!string.IsNullOrWhiteSpace(request.Title))
            query = query.Where(x => x.Title.Contains(request.Title));
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.CurrentDepartmentId.HasValue) query = query.Where(x => x.CurrentDepartmentId == request.CurrentDepartmentId);
        if (request.CreatedDateFrom.HasValue) query = query.Where(x => x.CreatedAt >= request.CreatedDateFrom);
        if (request.CreatedDateTo.HasValue) query = query.Where(x => x.CreatedAt < request.CreatedDateTo.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(request.SourceIndentNumber))
            query = query.Where(x => x.SourceIndentId.HasValue &&
                dbContext.Indents.Any(i => i.Id == x.SourceIndentId && i.IndentNumber.Contains(request.SourceIndentNumber)));

        query = (request.SortBy, request.SortDescending) switch
        {
            ("FileNumber", false) => query.OrderBy(x => x.FileNumber),
            ("FileNumber", true) => query.OrderByDescending(x => x.FileNumber),
            ("Status", false) => query.OrderBy(x => x.Status),
            ("Status", true) => query.OrderByDescending(x => x.Status),
            ("CreatedAt", false) => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        var total = await query.LongCountAsync(ct);
        var page = Math.Max(1, request.PageNumber);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var items = await query.Skip((page - 1) * size).Take(size)
            .Select(file => new PurchaseFileListDto(
                file.Id, file.FileNumber, file.Title, file.Status, file.Priority,
                file.CurrentDepartmentId, file.ResponsibleUserId, file.CreatedAt, file.Items.Count,
                file.SourceIndentId.HasValue
                    ? dbContext.Indents.Where(i => i.Id == file.SourceIndentId).Select(i => i.IndentNumber).FirstOrDefault()
                    : null,
                dbContext.Inquiries.Count(inquiry => inquiry.PurchaseFileId == file.Id)))
            .ToListAsync(ct);
        return new(items, page, size, total);
    }
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try { await dbContext.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        { throw new PurchaseFileConflictException("Duplicate purchase file number or source indent."); }
    }

    private IQueryable<PurchaseFileTechnicalReviewDto> ProjectTechnicalReviews(IQueryable<PurchaseFileTechnicalReview> query) =>
        query.Select(review => new PurchaseFileTechnicalReviewDto(
            review.Id,
            review.PurchaseFileId,
            dbContext.PurchaseFiles.Where(file => file.Id == review.PurchaseFileId).Select(file => file.FileNumber).FirstOrDefault() ?? "—",
            dbContext.PurchaseFiles.Where(file => file.Id == review.PurchaseFileId).Select(file => file.Title).FirstOrDefault() ?? "—",
            review.DepartmentId,
            dbContext.Departments.Where(department => department.Id == review.DepartmentId).Select(department => department.Name).FirstOrDefault() ?? "—",
            review.RequestedByUserId,
            review.ReviewedByUserId,
            review.Status,
            review.Decision,
            review.RequestComment,
            review.Comments,
            review.RecommendationNotes,
            review.RequestedAt,
            review.StartedAt,
            review.CompletedAt,
            dbContext.PurchaseFileItems.Where(item => item.PurchaseFileId == review.PurchaseFileId)
                .OrderBy(item => item.MescCode)
                .Select(item => new PurchaseFileItemDto(item.Id, item.MescItemId, item.MescCode, item.MescGeneralGroupCode,
                    item.GeneralDescription, item.SpecificDescription, item.UnitOfMeasureId, item.RequestedQuantity,
                    item.ApprovedQuantity, item.TechnicalDescription, item.SourceIndentItemId))
                .ToArray()));

    private static string DepartmentTitle(string key) => key.ToLowerInvariant() switch
    {
        "purchase" => "داشبورد واحد خرید",
        "orders" => "سفارشات و کنترل موجودی",
        "warehouse" => "داشبورد انبار",
        "applicant" => "داشبورد متقاضی",
        "tender-commission" => "کمیسیون مناقصه",
        "contracts" => "قراردادها و تدارکات",
        _ => "داشبورد واحد"
    };
}
