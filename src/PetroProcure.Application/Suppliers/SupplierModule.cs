using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Suppliers;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Suppliers;

namespace PetroProcure.Application.Suppliers;

public sealed record CreateSupplierCommand(
    string SupplierCode, string Name, string? NationalId, string? EconomicCode, string? RegistrationNumber,
    string? Phone, string? Email, string? Website, string? Address, string? City, string? Country,
    string? PostalCode, SupplierType SupplierType, string? Description, Guid[] CategoryIds,
    AddSupplierContactRequest? PrimaryContact);

public sealed record UpdateSupplierCommand(
    Guid Id, string SupplierCode, string Name, string? NationalId, string? EconomicCode, string? RegistrationNumber,
    string? Phone, string? Email, string? Website, string? Address, string? City, string? Country,
    string? PostalCode, SupplierType SupplierType, string? Description);

public sealed record ActivateSupplierCommand(Guid Id);
public sealed record DeactivateSupplierCommand(Guid Id);
public sealed record BlacklistSupplierCommand(Guid Id, string Reason);
public sealed record RemoveSupplierFromBlacklistCommand(Guid Id);
public sealed record AddSupplierContactCommand(Guid SupplierId, string FullName, string? Position, string? Phone, string? Mobile, string? Email, bool IsPrimary, string? Description);
public sealed record UpdateSupplierContactCommand(Guid SupplierId, Guid ContactId, string FullName, string? Position, string? Phone, string? Mobile, string? Email, bool IsPrimary, string? Description);
public sealed record DeactivateSupplierContactCommand(Guid SupplierId, Guid ContactId);
public sealed record AssignSupplierCategoryCommand(Guid SupplierId, Guid CategoryId);
public sealed record RemoveSupplierCategoryCommand(Guid SupplierId, Guid CategoryId);
public sealed record AddSupplierEvaluationCommand(Guid SupplierId, DateTime EvaluationDate, decimal? Score, SupplierEvaluationResult Result, string? Description);

public sealed record GetSuppliersQuery(SupplierListRequest Request);
public sealed record GetSupplierByIdQuery(Guid Id);
public sealed record GetSupplierByCodeQuery(string SupplierCode);
public sealed record SearchSuppliersQuery(string Term, bool IncludeInactive = false, bool IncludeBlacklisted = false);
public sealed record GetSupplierLookupQuery(string? Term = null, bool IncludeInactive = false, bool IncludeBlacklisted = false);
public sealed record GetSupplierCategoriesQuery(bool IncludeInactive = false);
public sealed record GetSupplierContactsQuery(Guid SupplierId);
public sealed record GetSupplierEvaluationsQuery(Guid SupplierId);

public interface ISupplierRepository
{
    Task<bool> SupplierCodeExistsAsync(string supplierCode, Guid? excludingId, CancellationToken cancellationToken);
    Task<Supplier?> FindSupplierAsync(Guid id, CancellationToken cancellationToken);
    Task<Supplier?> FindSupplierByCodeAsync(string supplierCode, CancellationToken cancellationToken);
    Task<SupplierContact?> FindContactAsync(Guid supplierId, Guid contactId, CancellationToken cancellationToken);
    Task<SupplierCategory?> FindCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken);
    Task<PagedResult<SupplierSummaryDto>> GetSuppliersAsync(SupplierListRequest request, CancellationToken cancellationToken);
    Task<SupplierDetailDto?> GetSupplierDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<SupplierDetailDto?> GetSupplierDetailByCodeAsync(string supplierCode, CancellationToken cancellationToken);
    Task<IReadOnlyList<SupplierLookupDto>> GetLookupAsync(string? term, bool includeInactive, bool includeBlacklisted, CancellationToken cancellationToken);
    Task<IReadOnlyList<SupplierCategoryDto>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<IReadOnlyList<SupplierContactDto>> GetContactsAsync(Guid supplierId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SupplierEvaluationDto>> GetEvaluationsAsync(Guid supplierId, CancellationToken cancellationToken);
    Task AddContactAsync(SupplierContact contact, CancellationToken cancellationToken);
    Task AddCategoryAssignmentAsync(SupplierCategoryAssignment assignment, CancellationToken cancellationToken);
    Task RemoveCategoryAssignmentAsync(Guid supplierId, Guid categoryId, CancellationToken cancellationToken);
    Task<bool> CategoryAssignmentExistsAsync(Guid supplierId, Guid categoryId, CancellationToken cancellationToken);
    Task AddEvaluationAsync(SupplierEvaluation evaluation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed class SupplierCommandHandler(ISupplierRepository repository, ICurrentUserService currentUser)
{
    public async Task<SupplierDetailDto> Handle(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var code = NormalizeRequired(command.SupplierCode, nameof(command.SupplierCode));
        if (await repository.SupplierCodeExistsAsync(code, null, cancellationToken))
            throw new SupplierConflictException("Supplier code already exists.");

        var supplier = new Supplier(Guid.NewGuid(), code, command.Name, command.SupplierType, currentUser.UserId);
        supplier.Update(code, command.Name, command.NationalId, command.EconomicCode, command.RegistrationNumber,
            command.Phone, command.Email, command.Website, command.Address, command.City, command.Country,
            command.PostalCode, command.SupplierType, command.Description, currentUser.UserId);

        await repository.AddSupplierAsync(supplier, cancellationToken);
        foreach (var categoryId in command.CategoryIds.Distinct())
        {
            if (await repository.FindCategoryAsync(categoryId, cancellationToken) is null)
                throw new SupplierValidationException("Supplier category was not found.");
            await repository.AddCategoryAssignmentAsync(new SupplierCategoryAssignment(Guid.NewGuid(), supplier.Id, categoryId), cancellationToken);
        }

        if (command.PrimaryContact is not null)
        {
            await repository.AddContactAsync(new SupplierContact(
                Guid.NewGuid(), supplier.Id, command.PrimaryContact.FullName, command.PrimaryContact.Position,
                command.PrimaryContact.Phone, command.PrimaryContact.Mobile, command.PrimaryContact.Email, true,
                command.PrimaryContact.Description), cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetSupplierDetailAsync(supplier.Id, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier was not found.");
    }

    public async Task<SupplierDetailDto> Handle(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var supplier = await repository.FindSupplierAsync(command.Id, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier was not found.");
        var code = NormalizeRequired(command.SupplierCode, nameof(command.SupplierCode));
        if (await repository.SupplierCodeExistsAsync(code, command.Id, cancellationToken))
            throw new SupplierConflictException("Supplier code already exists.");
        supplier.Update(code, command.Name, command.NationalId, command.EconomicCode, command.RegistrationNumber,
            command.Phone, command.Email, command.Website, command.Address, command.City, command.Country,
            command.PostalCode, command.SupplierType, command.Description, currentUser.UserId);
        await repository.SaveChangesAsync(cancellationToken);
        return await repository.GetSupplierDetailAsync(supplier.Id, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier was not found.");
    }

    public Task Handle(ActivateSupplierCommand command, CancellationToken cancellationToken = default) =>
        Change(command.Id, supplier => supplier.Activate(currentUser.UserId), cancellationToken);

    public Task Handle(DeactivateSupplierCommand command, CancellationToken cancellationToken = default) =>
        Change(command.Id, supplier => supplier.Deactivate(currentUser.UserId), cancellationToken);

    public Task Handle(BlacklistSupplierCommand command, CancellationToken cancellationToken = default) =>
        Change(command.Id, supplier => supplier.Blacklist(command.Reason, currentUser.UserId), cancellationToken);

    public Task Handle(RemoveSupplierFromBlacklistCommand command, CancellationToken cancellationToken = default) =>
        Change(command.Id, supplier => supplier.RemoveFromBlacklist(currentUser.UserId), cancellationToken);

    public async Task<SupplierContactDto> Handle(AddSupplierContactCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        _ = await repository.FindSupplierAsync(command.SupplierId, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier was not found.");
        var contact = new SupplierContact(Guid.NewGuid(), command.SupplierId, command.FullName, command.Position,
            command.Phone, command.Mobile, command.Email, command.IsPrimary, command.Description);
        await repository.AddContactAsync(contact, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return (await repository.GetContactsAsync(command.SupplierId, cancellationToken)).Single(x => x.Id == contact.Id);
    }

    public async Task<SupplierContactDto> Handle(UpdateSupplierContactCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var contact = await repository.FindContactAsync(command.SupplierId, command.ContactId, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier contact was not found.");
        contact.Update(command.FullName, command.Position, command.Phone, command.Mobile, command.Email, command.IsPrimary, command.Description);
        await repository.SaveChangesAsync(cancellationToken);
        return (await repository.GetContactsAsync(command.SupplierId, cancellationToken)).Single(x => x.Id == contact.Id);
    }

    public async Task Handle(DeactivateSupplierContactCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var contact = await repository.FindContactAsync(command.SupplierId, command.ContactId, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier contact was not found.");
        contact.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(AssignSupplierCategoryCommand command, CancellationToken cancellationToken = default)
    {
        if (await repository.FindCategoryAsync(command.CategoryId, cancellationToken) is null)
            throw new SupplierNotFoundException("Supplier category was not found.");
        if (!await repository.CategoryAssignmentExistsAsync(command.SupplierId, command.CategoryId, cancellationToken))
            await repository.AddCategoryAssignmentAsync(new SupplierCategoryAssignment(Guid.NewGuid(), command.SupplierId, command.CategoryId), cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(RemoveSupplierCategoryCommand command, CancellationToken cancellationToken = default)
    {
        await repository.RemoveCategoryAssignmentAsync(command.SupplierId, command.CategoryId, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<SupplierEvaluationDto> Handle(AddSupplierEvaluationCommand command, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        _ = await repository.FindSupplierAsync(command.SupplierId, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier was not found.");
        var evaluation = new SupplierEvaluation(Guid.NewGuid(), command.SupplierId, command.EvaluationDate,
            command.Score, command.Result, currentUser.UserId, command.Description);
        await repository.AddEvaluationAsync(evaluation, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return (await repository.GetEvaluationsAsync(command.SupplierId, cancellationToken)).Single(x => x.Id == evaluation.Id);
    }

    private async Task Change(Guid id, Action<Supplier> action, CancellationToken cancellationToken)
    {
        EnsureAuthenticated();
        var supplier = await repository.FindSupplierAsync(id, cancellationToken)
            ?? throw new SupplierNotFoundException("Supplier was not found.");
        action(supplier);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private void EnsureAuthenticated()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("Authenticated user is required.");
    }

    private static string NormalizeRequired(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new SupplierValidationException($"{name} is required.") : value.Trim();
}

public sealed class SupplierQueryHandler(ISupplierRepository repository)
{
    public Task<PagedResult<SupplierSummaryDto>> Handle(GetSuppliersQuery query, CancellationToken cancellationToken = default) =>
        repository.GetSuppliersAsync(query.Request, cancellationToken);

    public Task<SupplierDetailDto?> Handle(GetSupplierByIdQuery query, CancellationToken cancellationToken = default) =>
        repository.GetSupplierDetailAsync(query.Id, cancellationToken);

    public Task<SupplierDetailDto?> Handle(GetSupplierByCodeQuery query, CancellationToken cancellationToken = default) =>
        repository.GetSupplierDetailByCodeAsync(query.SupplierCode, cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> Handle(GetSupplierLookupQuery query, CancellationToken cancellationToken = default) =>
        repository.GetLookupAsync(query.Term, query.IncludeInactive, query.IncludeBlacklisted, cancellationToken);

    public Task<IReadOnlyList<SupplierLookupDto>> Handle(SearchSuppliersQuery query, CancellationToken cancellationToken = default) =>
        repository.GetLookupAsync(query.Term, query.IncludeInactive, query.IncludeBlacklisted, cancellationToken);

    public Task<IReadOnlyList<SupplierCategoryDto>> Handle(GetSupplierCategoriesQuery query, CancellationToken cancellationToken = default) =>
        repository.GetCategoriesAsync(query.IncludeInactive, cancellationToken);

    public Task<IReadOnlyList<SupplierContactDto>> Handle(GetSupplierContactsQuery query, CancellationToken cancellationToken = default) =>
        repository.GetContactsAsync(query.SupplierId, cancellationToken);

    public Task<IReadOnlyList<SupplierEvaluationDto>> Handle(GetSupplierEvaluationsQuery query, CancellationToken cancellationToken = default) =>
        repository.GetEvaluationsAsync(query.SupplierId, cancellationToken);
}

public sealed class SupplierValidationException(string message) : Exception(message);
public sealed class SupplierNotFoundException(string message) : Exception(message);
public sealed class SupplierConflictException(string message) : Exception(message);
