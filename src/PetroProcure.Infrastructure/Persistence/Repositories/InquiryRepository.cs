using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Inquiries;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Inquiry;
using PetroProcure.Domain.Modules.Inquiries;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class InquiryRepository(PetroProcureDbContext db) : IInquiryRepository
{
    public async Task<string> GenerateNextInquiryNumberAsync(int year, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await db.InquirySequences.SingleOrDefaultAsync(x => x.Year == year, ct);
            int next;
            if (sequence is null) { next = 1; db.InquirySequences.Add(new InquirySequence(Guid.NewGuid(), year, next)); }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return $"INQ-{year:0000}-{next:000000}";
        });
    }

    public Task<bool> InquiryNumberExistsAsync(string inquiryNumber, CancellationToken ct) =>
        db.Inquiries.AnyAsync(x => x.InquiryNumber == inquiryNumber, ct);
    public Task<Inquiry?> FindAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<Inquiry> query = db.Inquiries;
        if (includeDetails) query = query.Include(x => x.Items).Include(x => x.Suppliers).Include(x => x.Quotes).ThenInclude(x => x.Items);
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }
    public Task<Inquiry?> FindByNumberAsync(string inquiryNumber, CancellationToken ct) =>
        db.Inquiries.SingleOrDefaultAsync(x => x.InquiryNumber == inquiryNumber, ct);
    public Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct) =>
        db.PurchaseFiles.AnyAsync(x => x.Id == purchaseFileId, ct);
    public Task<PurchaseFileItemInquirySnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileId, Guid itemId, CancellationToken ct) =>
        db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId && x.Id == itemId)
            .Select(x => new PurchaseFileItemInquirySnapshot(x.Id, x.PurchaseFileId, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.RequestedQuantity, x.TechnicalDescription))
            .SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<PurchaseFileItemInquirySnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, Guid[] itemIds, CancellationToken ct)
    {
        var query = db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId);
        if (itemIds.Length > 0) query = query.Where(x => itemIds.Contains(x.Id));
        return await query.Select(x => new PurchaseFileItemInquirySnapshot(x.Id, x.PurchaseFileId, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.RequestedQuantity, x.TechnicalDescription)).ToListAsync(ct);
    }
    public Task<SupplierInquirySnapshot?> GetSupplierSnapshotAsync(Guid supplierId, CancellationToken ct) =>
        db.Suppliers.AsNoTracking().Where(x => x.Id == supplierId)
            .Select(x => new SupplierInquirySnapshot(x.Id, x.SupplierCode, x.Name, x.IsActive, x.IsBlacklisted))
            .SingleOrDefaultAsync(ct);
    public Task<SupplierContactInquirySnapshot?> GetSupplierContactSnapshotAsync(Guid supplierId, Guid contactId, CancellationToken ct) =>
        db.SupplierContacts.AsNoTracking().Where(x => x.SupplierId == supplierId && x.Id == contactId)
            .Select(x => new SupplierContactInquirySnapshot(x.Id, x.FullName, x.Email)).SingleOrDefaultAsync(ct);
    public async Task AddAsync(Inquiry inquiry, CancellationToken ct) => await db.Inquiries.AddAsync(inquiry, ct);

    public async Task<PagedResult<InquirySummaryDto>> GetPagedAsync(InquiryListRequest r, CancellationToken ct)
    {
        var query = ApplyFilters(db.Inquiries.AsNoTracking(), r);
        var total = await query.LongCountAsync(ct);
        query = (r.SortBy, r.SortDescending) switch
        {
            ("InquiryNumber", false) => query.OrderBy(x => x.InquiryNumber),
            ("InquiryNumber", true) => query.OrderByDescending(x => x.InquiryNumber),
            ("DeadlineDate", false) => query.OrderBy(x => x.DeadlineDate),
            ("DeadlineDate", true) => query.OrderByDescending(x => x.DeadlineDate),
            ("Status", false) => query.OrderBy(x => x.Status),
            ("Status", true) => query.OrderByDescending(x => x.Status),
            ("CreatedAt", false) => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        var page = Math.Max(1, r.PageNumber); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await query.Skip((page - 1) * size).Take(size).Select(x => new InquirySummaryDto(x.Id, x.InquiryNumber, x.PurchaseFileId,
            db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
            x.Title, x.Status, x.InquiryType, db.InquiryItems.Count(i => i.InquiryId == x.Id),
            db.InquirySuppliers.Count(s => s.InquiryId == x.Id && s.Status != Domain.Enums.InquirySupplierStatus.Excluded),
            x.DeadlineDate, x.CreatedAt)).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<InquiryDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
    {
        var inquiry = await db.Inquiries.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (inquiry is null) return null;
        return new(ToDto(inquiry), await Items(id).ToListAsync(ct), await Suppliers(id).ToListAsync(ct), await Quotes(id).ToListAsync(ct), await Documents(id).ToListAsync(ct));
    }
    public async Task<InquiryDetailDto?> GetDetailByNumberAsync(string number, CancellationToken ct)
    {
        var id = await db.Inquiries.AsNoTracking().Where(x => x.InquiryNumber == number).Select(x => x.Id).SingleOrDefaultAsync(ct);
        return id == Guid.Empty ? null : await GetDetailAsync(id, ct);
    }
    public async Task<IReadOnlyList<InquirySummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        await db.Inquiries.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId).OrderByDescending(x => x.CreatedAt)
            .Select(x => new InquirySummaryDto(x.Id, x.InquiryNumber, x.PurchaseFileId,
                db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
                x.Title, x.Status, x.InquiryType, db.InquiryItems.Count(i => i.InquiryId == x.Id),
                db.InquirySuppliers.Count(s => s.InquiryId == x.Id && s.Status != Domain.Enums.InquirySupplierStatus.Excluded),
                x.DeadlineDate, x.CreatedAt)).ToListAsync(ct);
    public async Task<IReadOnlyList<InquiryItemsGroupedDto>> GetItemsGroupedAsync(Guid inquiryId, CancellationToken ct) =>
        (await Items(inquiryId).ToListAsync(ct)).GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
            .Select(g => new InquiryItemsGroupedDto(g.Key.MescGeneralGroupCode, g.Key.GeneralDescription, g.ToList())).ToList();
    public async Task<IReadOnlyList<InquirySupplierDto>> GetSuppliersAsync(Guid inquiryId, CancellationToken ct) => await Suppliers(inquiryId).ToListAsync(ct);
    public async Task<IReadOnlyList<SupplierQuoteDto>> GetQuotesAsync(Guid inquiryId, CancellationToken ct) => await Quotes(inquiryId).ToListAsync(ct);
    public async Task<InquiryComparisonDto?> GetComparisonAsync(Guid inquiryId, CancellationToken ct)
    {
        var inquiry = await db.Inquiries.AsNoTracking().SingleOrDefaultAsync(x => x.Id == inquiryId, ct);
        if (inquiry is null) return null;
        var suppliers = await (from q in db.SupplierQuotes.AsNoTracking()
                               join s in db.InquirySuppliers.AsNoTracking() on q.InquirySupplierId equals s.Id
                               where q.InquiryId == inquiryId
                               select new InquiryComparisonSupplierDto(s.SupplierId, s.SupplierCode, s.SupplierName, q.Id, q.QuoteDate, q.ValidUntil, q.Currency, q.TotalAmount, q.FinalAmount, q.DeliveryDate, q.PaymentTerms, q.DeliveryTerms, q.IsSelected, q.Status)).ToListAsync(ct);
        var quoteItems = await db.SupplierQuoteItems.AsNoTracking().Where(x => db.SupplierQuotes.Any(q => q.Id == x.SupplierQuoteId && q.InquiryId == inquiryId)).Select(QuoteItemDto()).ToListAsync(ct);
        var items = quoteItems.GroupBy(x => new { x.InquiryItemId, x.MescGeneralGroupCode, x.GeneralDescription, x.MescCode, x.SpecificDescription })
            .Select(g => new InquiryComparisonItemDto(g.Key.InquiryItemId, g.Key.MescGeneralGroupCode, g.Key.GeneralDescription, g.Key.MescCode, g.Key.SpecificDescription, g.ToList())).ToList();
        return new(inquiry.Id, inquiry.InquiryNumber, suppliers, items);
    }
    public async Task<IReadOnlyList<InquiryLookupDto>> GetLookupAsync(string? term, CancellationToken ct)
    {
        var query = db.Inquiries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(term)) query = query.Where(x => x.InquiryNumber.Contains(term) || x.Title.Contains(term));
        return await query.OrderByDescending(x => x.CreatedAt).Take(30).Select(x => new InquiryLookupDto(x.Id, x.InquiryNumber, x.Title, x.Status)).ToListAsync(ct);
    }
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private IQueryable<Inquiry> ApplyFilters(IQueryable<Inquiry> query, InquiryListRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.InquiryNumber.Contains(r.SearchTerm) || x.Title.Contains(r.SearchTerm));
        if (!string.IsNullOrWhiteSpace(r.InquiryNumber)) query = query.Where(x => x.InquiryNumber.Contains(r.InquiryNumber));
        if (!string.IsNullOrWhiteSpace(r.PurchaseFileNumber)) query = query.Where(x => db.PurchaseFiles.Any(p => p.Id == x.PurchaseFileId && p.FileNumber.Contains(r.PurchaseFileNumber)));
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        if (r.InquiryType.HasValue) query = query.Where(x => x.InquiryType == r.InquiryType);
        if (r.SupplierId.HasValue) query = query.Where(x => db.InquirySuppliers.Any(s => s.InquiryId == x.Id && s.SupplierId == r.SupplierId));
        if (r.CreatedDateFrom.HasValue) query = query.Where(x => x.CreatedAt >= r.CreatedDateFrom);
        if (r.CreatedDateTo.HasValue) query = query.Where(x => x.CreatedAt <= r.CreatedDateTo);
        if (r.DeadlineDateFrom.HasValue) query = query.Where(x => x.DeadlineDate >= r.DeadlineDateFrom);
        if (r.DeadlineDateTo.HasValue) query = query.Where(x => x.DeadlineDate <= r.DeadlineDateTo);
        return query;
    }
    private static InquiryDto ToDto(Inquiry x) => new(x.Id, x.InquiryNumber, x.PurchaseFileId, x.Title, x.Description, x.Status, x.InquiryType, x.IssueDate, x.DeadlineDate, x.CreatedAt, x.CreatedByUserId, x.SentAt, x.SentByUserId, x.ClosedAt, x.ClosedByUserId, x.CancelledAt, x.CancelledByUserId, x.CancellationReason);
    private IQueryable<InquiryItemDto> Items(Guid id) => db.InquiryItems.AsNoTracking().Where(x => x.InquiryId == id).Select(x => new InquiryItemDto(x.Id, x.InquiryId, x.PurchaseFileItemId, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.RequestedQuantity, x.TechnicalDescription));
    private IQueryable<InquirySupplierDto> Suppliers(Guid id) => db.InquirySuppliers.AsNoTracking().Where(x => x.InquiryId == id).Select(x => new InquirySupplierDto(x.Id, x.InquiryId, x.SupplierId, x.SupplierCode, x.SupplierName, x.ContactId, x.ContactName, x.ContactEmail, x.Status, x.InvitedAt, x.InvitedByUserId, x.RespondedAt, x.DeclinedAt, x.DeclineReason));
    private IQueryable<SupplierQuoteDto> Quotes(Guid id) => db.SupplierQuotes.AsNoTracking().Where(x => x.InquiryId == id).Select(x => new SupplierQuoteDto(x.Id, x.InquiryId, x.SupplierId, x.InquirySupplierId, x.QuoteNumber, x.QuoteDate, x.ValidUntil, x.Currency, x.DeliveryTerms, x.PaymentTerms, x.DeliveryDate, x.TotalAmount, x.TaxAmount, x.DiscountAmount, x.FinalAmount, x.TechnicalNote, x.CommercialNote, x.Status, x.ReceivedAt, x.ReceivedByUserId, x.IsSelected, x.SelectionReason, db.SupplierQuoteItems.Where(i => i.SupplierQuoteId == x.Id).Select(QuoteItemDto()).ToList()));
    private IQueryable<InquiryDocumentDto> Documents(Guid id) => db.InquiryDocuments.AsNoTracking().Where(x => x.InquiryId == id).Select(x => new InquiryDocumentDto(x.Id, x.InquiryId, x.FileDocumentId, x.DocumentType, x.OriginalFileName, x.Description, x.UploadedAt, x.UploadedByUserId));
    private static System.Linq.Expressions.Expression<Func<SupplierQuoteItem, SupplierQuoteItemDto>> QuoteItemDto() =>
        x => new SupplierQuoteItemDto(x.Id, x.SupplierQuoteId, x.InquiryItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.Quantity, x.UnitPrice, x.TotalPrice, x.DeliveryDate, x.TechnicalComplianceStatus, x.TechnicalNote);
}
