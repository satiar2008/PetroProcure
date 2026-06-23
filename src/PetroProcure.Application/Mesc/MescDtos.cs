namespace PetroProcure.Application.Mesc;

public sealed record MescGeneralGroupDto(
    Guid Id,
    string Code,
    string GeneralDescription,
    bool IsActive);

public sealed record MescItemDto(
    Guid Id,
    string Code,
    string GeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    string UnitOfMeasure,
    bool IsActive);

public sealed record MescItemGroupedDto(
    string GeneralGroupCode,
    string GeneralDescription,
    IReadOnlyList<MescItemDto> Items);
