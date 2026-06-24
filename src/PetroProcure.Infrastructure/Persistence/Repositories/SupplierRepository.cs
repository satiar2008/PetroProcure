using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Suppliers;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Suppliers;
using PetroProcure.Domain.Modules.Suppliers;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class SupplierRepository(PetroProcureDbContext dbContext) : ISupplierRepository
{
    public Task<bool> SupplierCodeExistsAsync(string supplierCode, Guid? excludingId, CancellationToken cancellationToken) =>
        dbContext.Suppliers.AnyAsync(x => x.SupplierCode == supplierCode && (!excludingId.HasValue || x.Id != excludingId), cancellationToken);

    public Task<Supplier?> FindSupplierAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Suppliers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Supplier?> FindSupplierByCodeAsync(string supplierCode, CancellationToken cancellationToken) =>
        dbContext.Suppliers.SingleOrDefaultAsync(x => x.SupplierCode == supplierCode, cancellationToken);

    public Task<SupplierContact?> FindContactAsync(Guid supplierId, Guid contactId, CancellationToken cancellationToken) =>
        dbContext.SupplierContacts.SingleOrDefaultAsync(x => x.SupplierId == supplierId && x.Id == contactId, cancellationToken);

    public Task<SupplierCategory?> FindCategoryAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SupplierCategories.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken) =>
        await dbContext.Suppliers.AddAsync(supplier, cancellationToken);

    public async Task<PagedResult<SupplierSummaryDto>> GetSuppliersAsync(SupplierListRequest request, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(dbContext.Suppliers.AsNoTracking(), request);
        var total = await query.LongCountAsync(cancellationToken);
        var suppliers = await ApplySort(query, request.SortBy, request.SortDescending)
            .Skip((Math.Max(1, request.PageNumber) - 1) * Math.Max(1, request.PageSize))
            .Take(Math.Clamp(request.PageSize, 1, 200))
            .ToListAsync(cancellationToken);
        var ids = suppliers.Select(x => x.Id).ToArray();
        var categories = await CategoryLookup(ids, cancellationToken);
        var contacts = await PrimaryContacts(ids, cancellationToken);
        return new PagedResult<SupplierSummaryDto>(
            suppliers.Select(x => ToSummary(x, categories, contacts)).ToArray(),
            request.PageNumber, request.PageSize, total);
    }

    public async Task<SupplierDetailDto?> GetSupplierDetailAsync(Guid id, CancellationToken cancellationToken) =>
        await GetDetailAsync(dbContext.Suppliers.AsNoTracking().Where(x => x.Id == id), cancellationToken);

    public async Task<SupplierDetailDto?> GetSupplierDetailByCodeAsync(string supplierCode, CancellationToken cancellationToken) =>
        await GetDetailAsync(dbContext.Suppliers.AsNoTracking().Where(x => x.SupplierCode == supplierCode), cancellationToken);

    public async Task<IReadOnlyList<SupplierLookupDto>> GetLookupAsync(string? term, bool includeInactive, bool includeBlacklisted, CancellationToken cancellationToken)
    {
        var query = dbContext.Suppliers.AsNoTracking()
            .Where(x => (includeInactive || x.IsActive) && (includeBlacklisted || !x.IsBlacklisted));
        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.Trim();
            query = query.Where(x => x.SupplierCode.Contains(normalized) || x.Name.Contains(normalized));
        }
        return await query.OrderBy(x => x.Name).Take(30)
            .Select(x => new SupplierLookupDto(x.Id, x.SupplierCode, x.Name, x.SupplierType, x.Status, x.IsActive, x.IsBlacklisted))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierCategoryDto>> GetCategoriesAsync(bool includeInactive, CancellationToken cancellationToken) =>
        await dbContext.SupplierCategories.AsNoTracking()
            .Where(x => includeInactive || x.IsActive)
            .OrderBy(x => x.Title)
            .Select(x => new SupplierCategoryDto(x.Id, x.Code, x.Title, x.Description, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupplierContactDto>> GetContactsAsync(Guid supplierId, CancellationToken cancellationToken) =>
        await dbContext.SupplierContacts.AsNoTracking()
            .Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.FullName)
            .Select(ToContactDto())
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SupplierEvaluationDto>> GetEvaluationsAsync(Guid supplierId, CancellationToken cancellationToken) =>
        await dbContext.SupplierEvaluations.AsNoTracking()
            .Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.EvaluationDate)
            .Select(x => new SupplierEvaluationDto(x.Id, x.SupplierId, x.EvaluationDate, x.Score, x.Result, x.EvaluatedByUserId, x.Description))
            .ToListAsync(cancellationToken);

    public async Task AddContactAsync(SupplierContact contact, CancellationToken cancellationToken)
    {
        if (contact.IsPrimary && contact.IsActive)
        {
            var existing = await dbContext.SupplierContacts
                .Where(x => x.SupplierId == contact.SupplierId && x.IsPrimary && x.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var item in existing) item.SetPrimary(false);
        }
        await dbContext.SupplierContacts.AddAsync(contact, cancellationToken);
    }

    public async Task AddCategoryAssignmentAsync(SupplierCategoryAssignment assignment, CancellationToken cancellationToken) =>
        await dbContext.SupplierCategoryAssignments.AddAsync(assignment, cancellationToken);

    public async Task RemoveCategoryAssignmentAsync(Guid supplierId, Guid categoryId, CancellationToken cancellationToken)
    {
        var assignment = await dbContext.SupplierCategoryAssignments
            .SingleOrDefaultAsync(x => x.SupplierId == supplierId && x.SupplierCategoryId == categoryId, cancellationToken);
        if (assignment is not null) dbContext.SupplierCategoryAssignments.Remove(assignment);
    }

    public Task<bool> CategoryAssignmentExistsAsync(Guid supplierId, Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.SupplierCategoryAssignments.AnyAsync(x => x.SupplierId == supplierId && x.SupplierCategoryId == categoryId, cancellationToken);

    public async Task AddEvaluationAsync(SupplierEvaluation evaluation, CancellationToken cancellationToken) =>
        await dbContext.SupplierEvaluations.AddAsync(evaluation, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<Supplier> ApplyFilters(IQueryable<Supplier> query, SupplierListRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => x.SupplierCode.Contains(term) || x.Name.Contains(term));
        }
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.SupplierType.HasValue) query = query.Where(x => x.SupplierType == request.SupplierType);
        if (request.IsActive.HasValue) query = query.Where(x => x.IsActive == request.IsActive);
        if (request.IsBlacklisted.HasValue) query = query.Where(x => x.IsBlacklisted == request.IsBlacklisted);
        if (!string.IsNullOrWhiteSpace(request.City)) query = query.Where(x => x.City == request.City);
        if (request.CategoryId.HasValue)
            query = query.Where(x => dbContext.SupplierCategoryAssignments.Any(a => a.SupplierId == x.Id && a.SupplierCategoryId == request.CategoryId));
        if (request.HasPrimaryContact.HasValue)
            query = query.Where(x => dbContext.SupplierContacts.Any(c => c.SupplierId == x.Id && c.IsActive && c.IsPrimary) == request.HasPrimaryContact);
        return query;
    }

    private static IQueryable<Supplier> ApplySort(IQueryable<Supplier> query, string? sortBy, bool desc) =>
        (sortBy ?? "Name").ToLowerInvariant() switch
        {
            "suppliercode" => desc ? query.OrderByDescending(x => x.SupplierCode) : query.OrderBy(x => x.SupplierCode),
            "status" => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "createdat" => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            _ => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name)
        };

    private async Task<SupplierDetailDto?> GetDetailAsync(IQueryable<Supplier> query, CancellationToken cancellationToken)
    {
        var supplier = await query.SingleOrDefaultAsync(cancellationToken);
        if (supplier is null) return null;
        var contacts = await GetContactsAsync(supplier.Id, cancellationToken);
        var categories = await (from assignment in dbContext.SupplierCategoryAssignments.AsNoTracking()
                                join category in dbContext.SupplierCategories.AsNoTracking() on assignment.SupplierCategoryId equals category.Id
                                where assignment.SupplierId == supplier.Id
                                orderby category.Title
                                select new SupplierCategoryDto(category.Id, category.Code, category.Title, category.Description, category.IsActive))
            .ToListAsync(cancellationToken);
        var evaluations = await GetEvaluationsAsync(supplier.Id, cancellationToken);
        var documents = await dbContext.SupplierDocuments.AsNoTracking()
            .Where(d => d.SupplierId == supplier.Id)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new SupplierDocumentDto(d.Id, d.SupplierId, d.DocumentType, d.FileDocumentId, d.OriginalFileName, d.Description, d.UploadedAt, d.UploadedByUserId))
            .ToListAsync(cancellationToken);
        return new SupplierDetailDto(ToDto(supplier), contacts, categories, evaluations, documents);
    }

    private async Task<Dictionary<Guid, List<SupplierCategoryDto>>> CategoryLookup(Guid[] supplierIds, CancellationToken cancellationToken) =>
        (await (from assignment in dbContext.SupplierCategoryAssignments.AsNoTracking()
                join category in dbContext.SupplierCategories.AsNoTracking() on assignment.SupplierCategoryId equals category.Id
                where supplierIds.Contains(assignment.SupplierId)
                select new { assignment.SupplierId, Category = new SupplierCategoryDto(category.Id, category.Code, category.Title, category.Description, category.IsActive) })
            .ToListAsync(cancellationToken))
        .GroupBy(x => x.SupplierId)
        .ToDictionary(x => x.Key, x => x.Select(c => c.Category).ToList());

    private async Task<Dictionary<Guid, SupplierContactDto>> PrimaryContacts(Guid[] supplierIds, CancellationToken cancellationToken) =>
        (await dbContext.SupplierContacts.AsNoTracking()
            .Where(x => supplierIds.Contains(x.SupplierId) && x.IsPrimary && x.IsActive)
            .Select(ToContactDto())
            .ToListAsync(cancellationToken))
        .GroupBy(x => x.SupplierId)
        .ToDictionary(x => x.Key, x => x.First());

    private static SupplierSummaryDto ToSummary(
        Supplier supplier,
        Dictionary<Guid, List<SupplierCategoryDto>> categories,
        Dictionary<Guid, SupplierContactDto> contacts) =>
        new(supplier.Id, supplier.SupplierCode, supplier.Name, supplier.Status, supplier.SupplierType,
            supplier.IsActive, supplier.IsBlacklisted, supplier.City,
            categories.GetValueOrDefault(supplier.Id) ?? [],
            contacts.GetValueOrDefault(supplier.Id),
            supplier.CreatedAt);

    private static SupplierDto ToDto(Supplier x) =>
        new(x.Id, x.SupplierCode, x.Name, x.NationalId, x.EconomicCode, x.RegistrationNumber, x.Phone, x.Email,
            x.Website, x.Address, x.City, x.Country, x.PostalCode, x.Status, x.SupplierType, x.IsActive,
            x.IsBlacklisted, x.BlacklistReason, x.Description, x.CreatedAt, x.CreatedByUserId, x.UpdatedAt, x.UpdatedByUserId);

    private static System.Linq.Expressions.Expression<Func<SupplierContact, SupplierContactDto>> ToContactDto() =>
        x => new SupplierContactDto(x.Id, x.SupplierId, x.FullName, x.Position, x.Phone, x.Mobile, x.Email, x.IsPrimary, x.IsActive, x.Description);
}
