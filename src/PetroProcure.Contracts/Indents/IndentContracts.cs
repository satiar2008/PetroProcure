using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Indents;

public sealed record IndentItemDto(
    Guid Id, Guid MescItemId, string MescCode, string MescGeneralGroupCode,
    string GeneralDescription, string SpecificDescription, Guid UnitOfMeasureId,
    decimal RequestedQuantity, string? TechnicalDescription, DateOnly? RequiredDate);
public sealed record IndentDto(
    Guid Id, string IndentNumber, int YearPart, int TypeDigit, int Sequence,
    IndentType IndentType, string Title, Guid RequestingDepartmentId,
    Guid? ApplicantDepartmentId, Guid CreatedByUserId, DateTime CreatedAt,
    IndentStatus Status, string? Description, IReadOnlyList<IndentItemDto> Items);
public sealed record IndentSummaryDto(
    Guid Id, string IndentNumber, IndentType IndentType, string Title,
    Guid RequestingDepartmentId, IndentStatus Status, DateTime CreatedAt, int ItemCount);
public sealed record CreateIndentRequest(
    int YearPart, int TypeDigit, string Title, Guid RequestingDepartmentId,
    Guid? ApplicantDepartmentId, string? Description);
public sealed record AddIndentItemRequest(
    Guid MescItemId, Guid UnitOfMeasureId, decimal RequestedQuantity,
    string? TechnicalDescription, DateOnly? RequiredDate);
public sealed record IndentListRequest(
    string? Search = null, IndentStatus? Status = null, Guid? DepartmentId = null,
    int PageNumber = 1, int PageSize = 25);
public sealed record IndentGroupedItemsDto(
    string MescGeneralGroupCode, string GeneralDescription, IReadOnlyList<IndentItemDto> Items);
