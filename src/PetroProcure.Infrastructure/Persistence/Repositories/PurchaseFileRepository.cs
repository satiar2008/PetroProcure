using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.PurchaseFiles;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseFiles;

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
    public Task<Indent?> FindApprovedIndentAsync(Guid id, CancellationToken ct) =>
        dbContext.Indents.Include(indent => indent.Items)
            .SingleOrDefaultAsync(indent => indent.Id == id
                && (indent.Status == IndentStatus.Approved
                    || indent.Status == IndentStatus.SentToPurchaseDepartment), ct);
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
                file.CurrentDepartmentId, file.ResponsibleUserId, file.CreatedAt, file.Items.Count, null))
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
                    : null))
            .ToListAsync(ct);
        return new(items, page, size, total);
    }
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try { await dbContext.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) == true)
        { throw new PurchaseFileConflictException("Duplicate purchase file number or source indent."); }
    }
}
