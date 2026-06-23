namespace PetroProcure.Application.Mesc;

public sealed record GetMescGeneralGroupsQuery(bool IncludeInactive = false);
public sealed record GetMescItemsQuery(bool IncludeInactive = false);
public sealed record GetMescItemByCodeQuery(string Code, bool IncludeInactive = false);
public sealed record GetMescItemsGroupedByGeneralCodeQuery(bool IncludeInactive = false);
public sealed record SearchMescItemsQuery(string Term, bool IncludeInactive = false);

public sealed class MescQueryHandler(IMescCatalogRepository repository)
{
    public Task<IReadOnlyList<MescGeneralGroupDto>> Handle(GetMescGeneralGroupsQuery query, CancellationToken cancellationToken = default) =>
        repository.GetGeneralGroupsAsync(query.IncludeInactive, cancellationToken);

    public Task<IReadOnlyList<MescItemDto>> Handle(GetMescItemsQuery query, CancellationToken cancellationToken = default) =>
        repository.GetItemsAsync(query.IncludeInactive, cancellationToken);

    public Task<MescItemDto?> Handle(GetMescItemByCodeQuery query, CancellationToken cancellationToken = default) =>
        repository.GetItemByCodeAsync(query.Code, query.IncludeInactive, cancellationToken);

    public async Task<IReadOnlyList<MescItemGroupedDto>> Handle(GetMescItemsGroupedByGeneralCodeQuery query, CancellationToken cancellationToken = default)
    {
        var items = await repository.GetItemsAsync(query.IncludeInactive, cancellationToken);
        return items
            .GroupBy(item => new { item.GeneralGroupCode, item.GeneralDescription })
            .OrderBy(group => group.Key.GeneralGroupCode)
            .Select(group => new MescItemGroupedDto(
                group.Key.GeneralGroupCode,
                group.Key.GeneralDescription,
                group.OrderBy(item => item.Code).ToArray()))
            .ToArray();
    }

    public Task<IReadOnlyList<MescItemDto>> Handle(SearchMescItemsQuery query, CancellationToken cancellationToken = default) =>
        repository.SearchItemsAsync(query.Term?.Trim() ?? string.Empty, query.IncludeInactive, cancellationToken);
}
