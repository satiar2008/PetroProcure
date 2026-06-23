using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Mesc;
using PetroProcure.Domain.Modules.Items;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class MescCatalogRepository(PetroProcureDbContext dbContext) : IMescCatalogRepository
{
    public Task<bool> GeneralGroupCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.MescGeneralGroups.AnyAsync(group => group.Code == code && (!excludingId.HasValue || group.Id != excludingId), cancellationToken);

    public Task<bool> ItemCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.MescItems.AnyAsync(item => item.Code == code && (!excludingId.HasValue || item.Id != excludingId), cancellationToken);

    public Task<MescGeneralGroup?> FindGeneralGroupAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.MescGeneralGroups.SingleOrDefaultAsync(group => group.Id == id, cancellationToken);

    public Task<MescGeneralGroup?> FindGeneralGroupByCodeAsync(string code, CancellationToken cancellationToken) =>
        dbContext.MescGeneralGroups.SingleOrDefaultAsync(group => group.Code == code, cancellationToken);

    public Task<MescItem?> FindItemAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.MescItems.Include(item => item.GeneralGroup).SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task AddGeneralGroupAsync(MescGeneralGroup group, CancellationToken cancellationToken) =>
        await dbContext.MescGeneralGroups.AddAsync(group, cancellationToken);

    public async Task AddItemAsync(MescItem item, CancellationToken cancellationToken) =>
        await dbContext.MescItems.AddAsync(item, cancellationToken);

    public async Task<IReadOnlyList<MescGeneralGroupDto>> GetGeneralGroupsAsync(bool includeInactive, CancellationToken cancellationToken) =>
        await dbContext.MescGeneralGroups.AsNoTracking()
            .Where(group => includeInactive || group.IsActive)
            .OrderBy(group => group.Code)
            .Select(group => new MescGeneralGroupDto(group.Id, group.Code, group.Description, group.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MescItemDto>> GetItemsAsync(bool includeInactive, CancellationToken cancellationToken) =>
        await ItemDtos(includeInactive).ToListAsync(cancellationToken);

    public async Task<MescItemDto?> GetItemByCodeAsync(string code, bool includeInactive, CancellationToken cancellationToken) =>
        (await ItemDtos(includeInactive).ToListAsync(cancellationToken)).SingleOrDefault(item => item.Code == code);

    public async Task<IReadOnlyList<MescItemDto>> SearchItemsAsync(string term, bool includeInactive, CancellationToken cancellationToken)
    {
        var items = await ItemDtos(includeInactive).ToListAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(term))
        {
            items = items.Where(item =>
                item.Code.Contains(term) ||
                item.GeneralGroupCode.Contains(term) ||
                item.GeneralDescription.Contains(term) ||
                item.SpecificDescription.Contains(term)).ToList();
        }

        return items;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<MescItemDto> ItemDtos(bool includeInactive) =>
        from item in dbContext.MescItems.AsNoTracking()
        join generalGroup in dbContext.MescGeneralGroups.AsNoTracking()
            on item.GeneralGroupCode equals generalGroup.Code
        where includeInactive || item.IsActive
        orderby item.GeneralGroupCode, item.Code
        select new MescItemDto(
            item.Id,
            item.Code,
            item.GeneralGroupCode,
            generalGroup.Description,
            item.Description,
            item.UnitOfMeasure,
            item.IsActive);
}
