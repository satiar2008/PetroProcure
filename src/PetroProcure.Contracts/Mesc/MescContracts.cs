namespace PetroProcure.Contracts.V1.Mesc;

public sealed record MescGeneralGroupDto(Guid Id, string Code, string GeneralDescription, bool IsActive);
public sealed record MescItemDto(
    Guid Id, string Code, string GeneralGroupCode, string GeneralDescription,
    string SpecificDescription, string UnitOfMeasure, bool IsActive);
public sealed record MescItemGroupedDto(
    string GeneralGroupCode, string GeneralDescription, IReadOnlyList<MescItemDto> Items);
public sealed record CreateMescGeneralGroupRequest(string Code, string GeneralDescription);
public sealed record UpdateMescGeneralGroupRequest(string Code, string GeneralDescription);
public sealed record CreateMescItemRequest(
    string Code, string SpecificDescription, string UnitOfMeasure, string? GeneralDescription);
public sealed record UpdateMescItemRequest(
    string Code, string SpecificDescription, string UnitOfMeasure, string? GeneralDescription);
public sealed record MescItemSearchRequest(string? Term, bool IncludeInactive = false);
