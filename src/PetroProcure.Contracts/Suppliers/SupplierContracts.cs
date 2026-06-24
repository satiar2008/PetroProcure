using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Suppliers;

public sealed record SupplierSummaryDto(
    Guid Id, string SupplierCode, string Name, SupplierStatus Status, SupplierType SupplierType,
    bool IsActive, bool IsBlacklisted, string? City, IReadOnlyList<SupplierCategoryDto> Categories,
    SupplierContactDto? PrimaryContact, DateTime CreatedAt);

public sealed record SupplierDto(
    Guid Id, string SupplierCode, string Name, string? NationalId, string? EconomicCode,
    string? RegistrationNumber, string? Phone, string? Email, string? Website, string? Address,
    string? City, string? Country, string? PostalCode, SupplierStatus Status, SupplierType SupplierType,
    bool IsActive, bool IsBlacklisted, string? BlacklistReason, string? Description,
    DateTime CreatedAt, Guid CreatedByUserId, DateTime? UpdatedAt, Guid? UpdatedByUserId);

public sealed record SupplierDetailDto(
    SupplierDto Supplier, IReadOnlyList<SupplierContactDto> Contacts, IReadOnlyList<SupplierCategoryDto> Categories,
    IReadOnlyList<SupplierEvaluationDto> Evaluations, IReadOnlyList<SupplierDocumentDto> Documents);

public sealed record SupplierContactDto(
    Guid Id, Guid SupplierId, string FullName, string? Position, string? Phone, string? Mobile,
    string? Email, bool IsPrimary, bool IsActive, string? Description);

public sealed record SupplierCategoryDto(Guid Id, string Code, string Title, string? Description, bool IsActive);

public sealed record SupplierEvaluationDto(
    Guid Id, Guid SupplierId, DateTime EvaluationDate, decimal? Score,
    SupplierEvaluationResult Result, Guid EvaluatedByUserId, string? Description);

public sealed record SupplierDocumentDto(
    Guid Id, Guid SupplierId, string DocumentType, Guid? FileDocumentId, string OriginalFileName,
    string? Description, DateTime UploadedAt, Guid UploadedByUserId);

public sealed record SupplierLookupDto(
    Guid Id, string SupplierCode, string Name, SupplierType SupplierType, SupplierStatus Status,
    bool IsActive, bool IsBlacklisted);

public sealed record SupplierListRequest(
    string? SearchTerm = null,
    SupplierStatus? Status = null,
    SupplierType? SupplierType = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    bool? IsBlacklisted = null,
    string? City = null,
    bool? HasPrimaryContact = null,
    string SortBy = "Name",
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record CreateSupplierRequest(
    string SupplierCode, string Name, string? NationalId, string? EconomicCode, string? RegistrationNumber,
    string? Phone, string? Email, string? Website, string? Address, string? City, string? Country,
    string? PostalCode, SupplierType SupplierType, string? Description, Guid[] CategoryIds,
    AddSupplierContactRequest? PrimaryContact);

public sealed record UpdateSupplierRequest(
    string SupplierCode, string Name, string? NationalId, string? EconomicCode, string? RegistrationNumber,
    string? Phone, string? Email, string? Website, string? Address, string? City, string? Country,
    string? PostalCode, SupplierType SupplierType, string? Description);

public sealed record ChangeSupplierStatusRequest(string? Reason);
public sealed record AddSupplierContactRequest(
    string FullName, string? Position, string? Phone, string? Mobile, string? Email, bool IsPrimary, string? Description);
public sealed record UpdateSupplierContactRequest(
    string FullName, string? Position, string? Phone, string? Mobile, string? Email, bool IsPrimary, string? Description);
public sealed record AssignSupplierCategoryRequest(Guid CategoryId);
public sealed record AddSupplierEvaluationRequest(DateTime EvaluationDate, decimal? Score, SupplierEvaluationResult Result, string? Description);

public sealed record SupplierListResult(PagedResult<SupplierSummaryDto> Suppliers);
