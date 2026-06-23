namespace PetroProcure.Application.Indents;

public sealed record GetIndentByIdQuery(Guid Id);
public sealed record GetIndentByNumberQuery(string IndentNumber);
public sealed record GetIndentsQuery;
public sealed record GetIndentItemsGroupedByMescGeneralGroupQuery(Guid IndentId);

public sealed class IndentQueryHandler(IIndentRepository repository, IIndentNumberService numberService)
{
    public async Task<IndentDto?> Handle(GetIndentByIdQuery query, CancellationToken cancellationToken = default) =>
        await repository.FindAsync(query.Id, true, cancellationToken) is { } indent
            ? IndentCommandHandler.ToDto(indent) : null;

    public async Task<IndentDto?> Handle(GetIndentByNumberQuery query, CancellationToken cancellationToken = default)
    {
        var number = numberService.ParseIndentNumber(query.IndentNumber).IndentNumber;
        return await repository.FindByNumberAsync(number, cancellationToken) is { } indent
            ? IndentCommandHandler.ToDto(indent) : null;
    }

    public Task<IReadOnlyList<IndentListDto>> Handle(GetIndentsQuery query, CancellationToken cancellationToken = default) =>
        repository.GetAllAsync(cancellationToken);

    public async Task<IReadOnlyList<IndentItemsGroupedDto>> Handle(
        GetIndentItemsGroupedByMescGeneralGroupQuery query, CancellationToken cancellationToken = default)
    {
        var indent = await repository.FindAsync(query.IndentId, true, cancellationToken)
            ?? throw new IndentNotFoundException("Indent was not found.");
        return indent.Items
            .GroupBy(item => new { item.MescGeneralGroupCode, item.GeneralDescription })
            .OrderBy(group => group.Key.MescGeneralGroupCode)
            .Select(group => new IndentItemsGroupedDto(
                group.Key.MescGeneralGroupCode, group.Key.GeneralDescription,
                group.OrderBy(item => item.MescCode).Select(IndentCommandHandler.ToDto).ToArray()))
            .ToArray();
    }
}
