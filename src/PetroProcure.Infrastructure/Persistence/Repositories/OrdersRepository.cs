using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Orders;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Orders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Indents;
using PetroProcure.Domain.Modules.Orders;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class OrdersRepository(PetroProcureDbContext db) : IOrdersRepository
{
    public async Task<string> GenerateNextNeedNumberAsync(int year, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await db.MaterialNeedSequences.SingleOrDefaultAsync(x => x.Year == year, ct);
            int next;
            if (sequence is null)
            {
                next = 1;
                db.MaterialNeedSequences.Add(new MaterialNeedSequence(Guid.NewGuid(), year, next));
            }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return $"MN-{year:0000}-{next:000000}";
        });
    }

    public Task<MescOrderSnapshot?> GetMescSnapshotAsync(Guid mescItemId, CancellationToken ct) =>
        db.MescItems.AsNoTracking().Where(x => x.Id == mescItemId)
            .Select(x => new MescOrderSnapshot(x.Id, x.Code, x.GeneralGroupCode, x.GeneralGroup!.Description,
                x.Description, x.UnitOfMeasure, x.UnitOfMeasureId, x.IsActive)).SingleOrDefaultAsync(ct);

    public async Task<Guid> ResolveUnitOfMeasureIdAsync(string unitOfMeasure, CancellationToken ct)
    {
        var normalized = unitOfMeasure.Trim();
        var id = await db.UnitOfMeasures.AsNoTracking()
            .Where(x => x.IsActive && (x.Code == normalized || x.Name == normalized))
            .Select(x => x.Id).FirstOrDefaultAsync(ct);
        if (id != Guid.Empty) return id;
        return await db.UnitOfMeasures.AsNoTracking().Where(x => x.IsActive).Select(x => x.Id).FirstAsync(ct);
    }

    public Task<InventoryControlItem?> FindInventoryControlItemAsync(Guid id, CancellationToken ct) =>
        db.InventoryControlItems.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<MaterialNeed?> FindMaterialNeedAsync(Guid id, CancellationToken ct) =>
        db.MaterialNeeds.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<ShortageAlert?> FindShortageAlertAsync(Guid id, CancellationToken ct) =>
        db.ShortageAlerts.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddMaterialNeedAsync(MaterialNeed need, CancellationToken ct) => await db.MaterialNeeds.AddAsync(need, ct);
    public async Task AddShortageAlertAsync(ShortageAlert alert, CancellationToken ct) => await db.ShortageAlerts.AddAsync(alert, ct);
    public async Task AddIndentAsync(Indent indent, CancellationToken ct) => await db.Indents.AddAsync(indent, ct);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<PagedResult<InventoryControlItemDto>> GetInventoryControlItemsAsync(InventoryControlListRequest r, CancellationToken ct)
    {
        var query = db.InventoryControlItems.AsNoTracking();
        if (!r.IncludeInactive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(r.SearchTerm))
            query = query.Where(x => x.MescCode.Contains(r.SearchTerm) || x.GeneralDescription.Contains(r.SearchTerm) || x.SpecificDescription.Contains(r.SearchTerm));
        var total = await query.LongCountAsync(ct);
        var page = Math.Max(1, r.PageNumber); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await query.OrderBy(x => x.MescCode).Skip((page - 1) * size).Take(size).Select(x => new InventoryControlItemDto(
            x.Id, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription,
            x.UnitOfMeasureId, x.MinimumStockLevel, x.ReorderPoint, x.MaximumStockLevel, x.SafetyStock,
            x.IsStockControlled, x.IsActive, x.Notes,
            db.StockBalances.Where(b => b.MescItemId == x.MescItemId).Sum(b => (decimal?)b.AvailableQuantity) ?? 0,
            db.StockBalances.Where(b => b.MescItemId == x.MescItemId).Sum(b => (decimal?)b.OnOrderQuantity) ?? 0,
            x.CreatedAt, x.UpdatedAt)).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<PagedResult<StockBalanceDto>> GetStockBalancesAsync(StockBalanceListRequest r, CancellationToken ct)
    {
        var query = db.StockBalances.AsNoTracking();
        if (r.WarehouseId.HasValue) query = query.Where(x => x.WarehouseId == r.WarehouseId);
        if (!string.IsNullOrWhiteSpace(r.SearchTerm))
            query = query.Where(x => db.MescItems.Any(m => m.Id == x.MescItemId && (m.Code.Contains(r.SearchTerm) || m.Description.Contains(r.SearchTerm))));
        var total = await query.LongCountAsync(ct);
        var page = Math.Max(1, r.PageNumber); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await query.OrderByDescending(x => x.LastUpdatedAt).Skip((page - 1) * size).Take(size)
            .Select(x => new StockBalanceDto(x.Id, x.MescItemId, x.WarehouseId,
                db.MescItems.Where(m => m.Id == x.MescItemId).Select(m => m.Code).FirstOrDefault() ?? "",
                db.MescItems.Where(m => m.Id == x.MescItemId).Select(m => m.Description).FirstOrDefault() ?? "",
                x.AvailableQuantity, x.ReservedQuantity, x.OnOrderQuantity, x.LastUpdatedAt)).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<PagedResult<MaterialNeedDto>> GetMaterialNeedsAsync(MaterialNeedListRequest r, CancellationToken ct)
    {
        var query = ApplyNeedFilters(db.MaterialNeeds.AsNoTracking(), r);
        var total = await query.LongCountAsync(ct);
        var page = Math.Max(1, r.PageNumber); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * size).Take(size).Select(NeedDto()).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public Task<MaterialNeedDto?> GetMaterialNeedDtoAsync(Guid id, CancellationToken ct) =>
        db.MaterialNeeds.AsNoTracking().Where(x => x.Id == id).Select(NeedDto()).SingleOrDefaultAsync(ct);

    public async Task<PagedResult<ShortageAlertDto>> GetShortageAlertsAsync(ShortageAlertListRequest r, CancellationToken ct)
    {
        var query = db.ShortageAlerts.AsNoTracking();
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        if (!string.IsNullOrWhiteSpace(r.MescCode)) query = query.Where(x => x.MescCode.Contains(r.MescCode));
        var total = await query.LongCountAsync(ct);
        var page = Math.Max(1, r.PageNumber); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await query.OrderByDescending(x => x.DetectedAt).Skip((page - 1) * size).Take(size).Select(AlertDto()).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<IReadOnlyList<MaterialNeedsGroupedByMescDto>> GetMaterialNeedsGroupedByMescAsync(MaterialNeedListRequest r, CancellationToken ct) =>
        (await ApplyNeedFilters(db.MaterialNeeds.AsNoTracking(), r).Select(NeedDto()).ToListAsync(ct))
        .GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
        .Select(g => new MaterialNeedsGroupedByMescDto(g.Key.MescGeneralGroupCode, g.Key.GeneralDescription, g.ToList())).ToList();

    public async Task<OrdersDashboardDto> GetDashboardAsync(CancellationToken ct)
    {
        var inventory = await GetInventoryControlItemsAsync(new InventoryControlListRequest(null, false, 1, 5), ct);
        var critical = inventory.Items.Where(x => x.IsStockControlled && x.CurrentStock <= x.ReorderPoint).ToList();
        var recent = await db.MaterialNeeds.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(5).Select(NeedDto()).ToListAsync(ct);
        return new OrdersDashboardDto(
            await db.MaterialNeeds.LongCountAsync(x => x.Status == MaterialNeedStatus.Submitted || x.Status == MaterialNeedStatus.UnderReview, ct),
            await db.MaterialNeeds.LongCountAsync(x => x.Status == MaterialNeedStatus.Approved, ct),
            await db.ShortageAlerts.LongCountAsync(x => x.Status == ShortageAlertStatus.Open || x.Status == ShortageAlertStatus.InProgress, ct),
            await db.Indents.LongCountAsync(x => x.Status == IndentStatus.Submitted, ct),
            await db.Indents.LongCountAsync(x => x.Status == IndentStatus.SentToPurchaseDepartment, ct),
            critical, recent);
    }

    public async Task<IReadOnlyList<ShortageAlert>> DetectShortagesAsync(bool includeExistingOpen, CancellationToken ct)
    {
        var candidates = await db.InventoryControlItems.AsNoTracking().Where(x => x.IsActive && x.IsStockControlled)
            .Select(x => new
            {
                Item = x,
                Current = db.StockBalances.Where(b => b.MescItemId == x.MescItemId).Sum(b => (decimal?)b.AvailableQuantity) ?? 0
            }).Where(x => x.Current < x.Item.ReorderPoint).ToListAsync(ct);
        var alerts = new List<ShortageAlert>();
        foreach (var candidate in candidates)
        {
            var exists = await db.ShortageAlerts.AnyAsync(x => x.MescItemId == candidate.Item.MescItemId &&
                (x.Status == ShortageAlertStatus.Open || x.Status == ShortageAlertStatus.InProgress), ct);
            if (exists && !includeExistingOpen) continue;
            var alert = new ShortageAlert(Guid.NewGuid(), candidate.Item.MescItemId, candidate.Item.MescCode,
                candidate.Item.MescGeneralGroupCode, candidate.Item.GeneralDescription, candidate.Item.SpecificDescription,
                candidate.Item.UnitOfMeasureId, candidate.Current, candidate.Item.ReorderPoint);
            await db.ShortageAlerts.AddAsync(alert, ct);
            alerts.Add(alert);
        }
        return alerts;
    }

    private IQueryable<MaterialNeed> ApplyNeedFilters(IQueryable<MaterialNeed> query, MaterialNeedListRequest r)
    {
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        if (r.Priority.HasValue) query = query.Where(x => x.Priority == r.Priority);
        if (!string.IsNullOrWhiteSpace(r.MescCode)) query = query.Where(x => x.MescCode.Contains(r.MescCode));
        if (r.ApplicantDepartmentId.HasValue) query = query.Where(x => x.ApplicantDepartmentId == r.ApplicantDepartmentId);
        if (r.CreatedDateFrom.HasValue) query = query.Where(x => x.CreatedAt >= r.CreatedDateFrom);
        if (r.CreatedDateTo.HasValue) query = query.Where(x => x.CreatedAt <= r.CreatedDateTo);
        return query;
    }

    private static System.Linq.Expressions.Expression<Func<MaterialNeed, MaterialNeedDto>> NeedDto() => x => new MaterialNeedDto(
        x.Id, x.NeedNumber, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription,
        x.SpecificDescription, x.UnitOfMeasureId, x.RequestedQuantity, x.NeededByDate, x.SourceDepartmentId,
        x.ApplicantDepartmentId, x.RequestedByUserId, x.Status, x.Priority, x.Description, x.CreatedAt,
        x.SubmittedAt, x.ReviewedAt, x.ReviewedByUserId, x.RelatedIndentId, x.RejectionReason);

    private static System.Linq.Expressions.Expression<Func<ShortageAlert, ShortageAlertDto>> AlertDto() => x => new ShortageAlertDto(
        x.Id, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription,
        x.UnitOfMeasureId, x.CurrentStock, x.ReorderPoint, x.ShortageQuantity, x.Status, x.DetectedAt,
        x.ResolvedAt, x.RelatedIndentId, x.ResolutionNote);
}
