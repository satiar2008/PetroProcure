namespace PetroProcure.Application.PurchaseFiles;

using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.PurchaseFiles;

public sealed record GetPurchaseFilesQuery(PurchaseFileListRequest Request);
public sealed record GetPurchaseFileByIdQuery(Guid Id);
public sealed record GetPurchaseFileByNumberQuery(string FileNumber);
public sealed record GetPurchaseFileItemsGroupedByMescGeneralGroupQuery(Guid Id);
public sealed record GetPurchaseFileTimelineQuery(Guid Id);

public sealed class PurchaseFileQueryHandler(IPurchaseFileRepository repository)
{
    public Task<PagedResult<PurchaseFileListDto>> Handle(GetPurchaseFilesQuery query, CancellationToken ct = default) =>
        repository.GetPagedAsync(query.Request, ct);
    public async Task<PurchaseFileDto?> Handle(GetPurchaseFileByIdQuery query, CancellationToken ct = default) =>
        await repository.FindAsync(query.Id, true, ct) is { } file ? PurchaseFileCommandHandler.ToDto(file) : null;
    public async Task<PurchaseFileDto?> Handle(GetPurchaseFileByNumberQuery query, CancellationToken ct = default) =>
        await repository.FindByNumberAsync(query.FileNumber.Trim(), ct) is { } file ? PurchaseFileCommandHandler.ToDto(file) : null;
    public async Task<IReadOnlyList<PurchaseFileItemsGroupedDto>> Handle(GetPurchaseFileItemsGroupedByMescGeneralGroupQuery query, CancellationToken ct = default)
    {
        var file = await repository.FindAsync(query.Id, true, ct) ?? throw new PurchaseFileNotFoundException("Purchase file was not found.");
        return file.Items.GroupBy(item => new { item.MescGeneralGroupCode, item.GeneralDescription })
            .OrderBy(group => group.Key.MescGeneralGroupCode)
            .Select(group => new PurchaseFileItemsGroupedDto(
                group.Key.MescGeneralGroupCode, group.Key.GeneralDescription,
                group.OrderBy(item => item.MescCode).Select(PurchaseFileCommandHandler.ToDto).ToArray()))
            .ToArray();
    }
    public async Task<PurchaseFileTimelineDto> Handle(GetPurchaseFileTimelineQuery query, CancellationToken ct = default)
    {
        var file = await repository.FindAsync(query.Id, true, ct) ?? throw new PurchaseFileNotFoundException("Purchase file was not found.");
        return new(
            file.StatusHistory.OrderBy(item => item.ChangedAt).Select(PurchaseFileCommandHandler.ToDto).ToArray(),
            file.Notes.OrderBy(item => item.CreatedAt).Select(PurchaseFileCommandHandler.ToDto).ToArray());
    }
}
