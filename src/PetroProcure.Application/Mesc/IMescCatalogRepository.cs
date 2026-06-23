using PetroProcure.Domain.Modules.Items;

namespace PetroProcure.Application.Mesc;

public interface IMescCatalogRepository
{
    Task<bool> GeneralGroupCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);
    Task<bool> ItemCodeExistsAsync(string code, Guid? excludingId, CancellationToken cancellationToken);
    Task<MescGeneralGroup?> FindGeneralGroupAsync(Guid id, CancellationToken cancellationToken);
    Task<MescGeneralGroup?> FindGeneralGroupByCodeAsync(string code, CancellationToken cancellationToken);
    Task<MescItem?> FindItemAsync(Guid id, CancellationToken cancellationToken);
    Task AddGeneralGroupAsync(MescGeneralGroup group, CancellationToken cancellationToken);
    Task AddItemAsync(MescItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<MescGeneralGroupDto>> GetGeneralGroupsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<IReadOnlyList<MescItemDto>> GetItemsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<MescItemDto?> GetItemByCodeAsync(string code, bool includeInactive, CancellationToken cancellationToken);
    Task<IReadOnlyList<MescItemDto>> SearchItemsAsync(string term, bool includeInactive, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
