using PetroProcure.Domain.Enums;

namespace PetroProcure.Application.Indents;

public sealed record IndentItemDto(
    Guid Id,
    Guid MescItemId,
    string MescCode,
    string MescGeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    Guid UnitOfMeasureId,
    decimal RequestedQuantity,
    string? TechnicalDescription,
    DateOnly? RequiredDate);

public sealed record IndentDto(
    Guid Id,
    string IndentNumber,
    int YearPart,
    int TypeDigit,
    int Sequence,
    IndentType IndentType,
    string Title,
    Guid RequestingDepartmentId,
    Guid? ApplicantDepartmentId,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    IndentStatus Status,
    string? Description,
    IReadOnlyList<IndentItemDto> Items);

public sealed record IndentListDto(
    Guid Id,
    string IndentNumber,
    IndentType IndentType,
    string Title,
    Guid RequestingDepartmentId,
    IndentStatus Status,
    DateTime CreatedAt,
    int ItemCount);

public sealed record IndentItemsGroupedDto(
    string MescGeneralGroupCode,
    string GeneralDescription,
    IReadOnlyList<IndentItemDto> Items);

public sealed record MescItemSnapshot(
    Guid Id,
    string Code,
    string GeneralGroupCode,
    string GeneralDescription,
    string SpecificDescription,
    bool IsActive);
