using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Tenders;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Tenders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class TenderRepository(PetroProcureDbContext db) : ITenderRepository
{
    public async Task<string> GenerateNextTenderNumberAsync(int year, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await db.TenderSequences.SingleOrDefaultAsync(x => x.Year == year, ct);
            int next;
            if (sequence is null) { next = 1; db.TenderSequences.Add(new TenderSequence(Guid.NewGuid(), year, next)); }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return $"TND-{year:0000}-{next:000000}";
        });
    }

    public Task<Tender?> FindAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<Tender> q = db.Tenders;
        if (includeDetails)
            q = q.Include(x => x.Items).Include(x => x.Participants).Include(x => x.Stages)
                .Include(x => x.Bids).ThenInclude(x => x.Items)
                .Include(x => x.Evaluations).Include(x => x.Decisions).Include(x => x.Documents);
        return q.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<Tender?> FindByNumberAsync(string tenderNumber, CancellationToken ct) =>
        db.Tenders.SingleOrDefaultAsync(x => x.TenderNumber == tenderNumber, ct);

    public Task<bool> PurchaseFileExistsAsync(Guid purchaseFileId, CancellationToken ct) =>
        db.PurchaseFiles.AnyAsync(x => x.Id == purchaseFileId, ct);

    public Task<TenderInquirySnapshot?> GetInquirySnapshotAsync(Guid inquiryId, CancellationToken ct) =>
        db.Inquiries.AsNoTracking().Where(x => x.Id == inquiryId)
            .Select(x => new TenderInquirySnapshot(x.Id, x.PurchaseFileId, x.InquiryNumber)).SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyList<TenderInquirySupplierSnapshot>> GetInquirySuppliersAsync(Guid inquiryId, Guid[] ids, CancellationToken ct)
    {
        var q = db.InquirySuppliers.AsNoTracking().Where(x => x.InquiryId == inquiryId);
        if (ids.Length > 0) q = q.Where(x => ids.Contains(x.Id));
        return await q.Select(x => new TenderInquirySupplierSnapshot(x.Id, x.SupplierId, x.ContactId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TenderPurchaseFileItemSnapshot>> GetPurchaseFileItemSnapshotsAsync(Guid purchaseFileId, Guid[] itemIds, CancellationToken ct)
    {
        var q = db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId);
        if (itemIds.Length > 0) q = q.Where(x => itemIds.Contains(x.Id));
        return await q.Select(x => new TenderPurchaseFileItemSnapshot(x.Id, x.PurchaseFileId, x.MescItemId, x.MescCode,
            x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
            x.ApprovedQuantity > 0 ? x.ApprovedQuantity : x.RequestedQuantity, x.TechnicalDescription)).ToListAsync(ct);
    }

    public Task<TenderPurchaseFileItemSnapshot?> GetPurchaseFileItemSnapshotAsync(Guid purchaseFileId, Guid itemId, CancellationToken ct) =>
        db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId && x.Id == itemId)
            .Select(x => new TenderPurchaseFileItemSnapshot(x.Id, x.PurchaseFileId, x.MescItemId, x.MescCode,
                x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId,
                x.ApprovedQuantity > 0 ? x.ApprovedQuantity : x.RequestedQuantity, x.TechnicalDescription)).SingleOrDefaultAsync(ct);

    public Task<TenderSupplierSnapshot?> GetSupplierSnapshotAsync(Guid supplierId, CancellationToken ct) =>
        db.Suppliers.AsNoTracking().Where(x => x.Id == supplierId)
            .Select(x => new TenderSupplierSnapshot(x.Id, x.SupplierCode, x.Name, x.IsActive, x.IsBlacklisted)).SingleOrDefaultAsync(ct);

    public Task<TenderSupplierContactSnapshot?> GetSupplierContactSnapshotAsync(Guid supplierId, Guid contactId, CancellationToken ct) =>
        db.SupplierContacts.AsNoTracking().Where(x => x.SupplierId == supplierId && x.Id == contactId)
            .Select(x => new TenderSupplierContactSnapshot(x.Id, x.FullName, x.Email)).SingleOrDefaultAsync(ct);

    public async Task AddAsync(Tender tender, CancellationToken ct) => await db.Tenders.AddAsync(tender, ct);

    public async Task<PagedResult<TenderSummaryDto>> GetPagedAsync(TenderListRequest r, CancellationToken ct)
    {
        var q = ApplyFilters(db.Tenders.AsNoTracking(), r);
        var total = await q.LongCountAsync(ct);
        q = (r.SortBy, r.SortDescending) switch
        {
            ("TenderNumber", false) => q.OrderBy(x => x.TenderNumber),
            ("TenderNumber", true) => q.OrderByDescending(x => x.TenderNumber),
            ("SubmissionDeadline", false) => q.OrderBy(x => x.SubmissionDeadline),
            ("SubmissionDeadline", true) => q.OrderByDescending(x => x.SubmissionDeadline),
            ("Status", false) => q.OrderBy(x => x.Status),
            ("Status", true) => q.OrderByDescending(x => x.Status),
            ("CreatedAt", false) => q.OrderBy(x => x.CreatedAt),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };
        var page = Math.Max(1, r.PageNumber); var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * size).Take(size).Select(x => new TenderSummaryDto(x.Id, x.TenderNumber,
            x.PurchaseFileId, db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
            x.Title, x.Status, x.TenderType, db.TenderItems.Count(i => i.TenderId == x.Id),
            db.TenderParticipants.Count(p => p.TenderId == x.Id), x.SubmissionDeadline, x.CreatedAt)).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<TenderDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (tender is null) return null;
        return new(ToDto(tender), await Items(id).ToListAsync(ct), await Participants(id).ToListAsync(ct),
            await Stages(id).ToListAsync(ct), await Bids(id).ToListAsync(ct), await Evaluations(id).ToListAsync(ct),
            await Decisions(id).ToListAsync(ct), await Documents(id).ToListAsync(ct));
    }

    public async Task<TenderDetailDto?> GetDetailByNumberAsync(string tenderNumber, CancellationToken ct)
    {
        var id = await db.Tenders.AsNoTracking().Where(x => x.TenderNumber == tenderNumber).Select(x => x.Id).SingleOrDefaultAsync(ct);
        return id == Guid.Empty ? null : await GetDetailAsync(id, ct);
    }

    public async Task<IReadOnlyList<TenderSummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        (await GetPagedAsync(new TenderListRequest(PageNumber: 1, PageSize: 100), ct)).Items.Where(x => x.PurchaseFileId == purchaseFileId).ToList();

    public async Task<IReadOnlyList<TenderItemsGroupedDto>> GetItemsGroupedAsync(Guid tenderId, CancellationToken ct) =>
        (await Items(tenderId).ToListAsync(ct)).GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
            .Select(x => new TenderItemsGroupedDto(x.Key.MescGeneralGroupCode, x.Key.GeneralDescription, x.ToList())).ToList();

    public async Task<IReadOnlyList<TenderParticipantDto>> GetParticipantsAsync(Guid tenderId, CancellationToken ct) => await Participants(tenderId).ToListAsync(ct);
    public async Task<IReadOnlyList<TenderBidDto>> GetBidsAsync(Guid tenderId, CancellationToken ct) => await Bids(tenderId).ToListAsync(ct);

    public async Task<TenderComparisonDto?> GetComparisonAsync(Guid tenderId, CancellationToken ct)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == tenderId, ct);
        if (tender is null) return null;
        var suppliers = await (from b in db.TenderBids.AsNoTracking()
                               join p in db.TenderParticipants.AsNoTracking() on b.TenderParticipantId equals p.Id
                               where b.TenderId == tenderId
                               select new TenderComparisonSupplierDto(p.SupplierId, p.SupplierCode, p.SupplierName, b.Id,
                                   b.Currency, b.TotalAmount, b.FinalAmount, b.DeliveryTerms, b.PaymentTerms,
                                   b.TechnicalScore, b.CommercialScore, b.FinalScore, b.Status == TenderBidStatus.Selected)).ToListAsync(ct);
        var bidItems = await db.TenderBidItems.AsNoTracking().Where(x => db.TenderBids.Any(b => b.Id == x.TenderBidId && b.TenderId == tenderId))
            .Select(BidItemDto()).ToListAsync(ct);
        var items = bidItems.GroupBy(x => new { x.TenderItemId, x.MescGeneralGroupCode, x.GeneralDescription, x.MescCode, x.SpecificDescription })
            .Select(x => new TenderComparisonItemDto(x.Key.TenderItemId, x.Key.MescGeneralGroupCode, x.Key.GeneralDescription,
                x.Key.MescCode, x.Key.SpecificDescription, x.ToList())).ToList();
        return new(tender.Id, tender.TenderNumber, suppliers, items);
    }

    public async Task<IReadOnlyList<TenderLookupDto>> GetLookupAsync(string? term, CancellationToken ct)
    {
        var q = db.Tenders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(term)) q = q.Where(x => x.TenderNumber.Contains(term) || x.Title.Contains(term));
        return await q.OrderByDescending(x => x.CreatedAt).Take(30).Select(x => new TenderLookupDto(x.Id, x.TenderNumber, x.Title, x.Status)).ToListAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private IQueryable<Tender> ApplyFilters(IQueryable<Tender> q, TenderListRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) q = q.Where(x => x.TenderNumber.Contains(r.SearchTerm) || x.Title.Contains(r.SearchTerm));
        if (!string.IsNullOrWhiteSpace(r.TenderNumber)) q = q.Where(x => x.TenderNumber.Contains(r.TenderNumber));
        if (!string.IsNullOrWhiteSpace(r.PurchaseFileNumber)) q = q.Where(x => db.PurchaseFiles.Any(p => p.Id == x.PurchaseFileId && p.FileNumber.Contains(r.PurchaseFileNumber)));
        if (r.Status.HasValue) q = q.Where(x => x.Status == r.Status);
        if (r.TenderType.HasValue) q = q.Where(x => x.TenderType == r.TenderType);
        if (r.SupplierId.HasValue) q = q.Where(x => db.TenderParticipants.Any(p => p.TenderId == x.Id && p.SupplierId == r.SupplierId));
        if (r.CreatedDateFrom.HasValue) q = q.Where(x => x.CreatedAt >= r.CreatedDateFrom);
        if (r.CreatedDateTo.HasValue) q = q.Where(x => x.CreatedAt <= r.CreatedDateTo);
        if (r.SubmissionDeadlineFrom.HasValue) q = q.Where(x => x.SubmissionDeadline >= r.SubmissionDeadlineFrom);
        if (r.SubmissionDeadlineTo.HasValue) q = q.Where(x => x.SubmissionDeadline <= r.SubmissionDeadlineTo);
        return q;
    }

    private TenderDto ToDto(Tender x) => new(x.Id, x.TenderNumber, x.PurchaseFileId,
        db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
        x.SourceInquiryId, x.SourceInquiryId.HasValue ? db.Inquiries.Where(i => i.Id == x.SourceInquiryId).Select(i => i.InquiryNumber).FirstOrDefault() : null,
        x.Title, x.Description, x.TenderType, x.Status, x.IssueDate, x.SubmissionDeadline, x.OpeningDate, x.CreatedAt,
        x.CreatedByUserId, x.PublishedAt, x.PublishedByUserId, x.CancelledAt, x.CancelledByUserId, x.CancellationReason, x.ClosedAt, x.ClosedByUserId);
    private IQueryable<TenderItemDto> Items(Guid id) => db.TenderItems.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderItemDto(x.Id, x.TenderId, x.PurchaseFileItemId, x.MescItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.UnitOfMeasureId, x.Quantity, x.TechnicalDescription));
    private IQueryable<TenderParticipantDto> Participants(Guid id) => db.TenderParticipants.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderParticipantDto(x.Id, x.TenderId, x.SupplierId, x.SupplierCode, x.SupplierName, x.ContactId, x.ContactName, x.ContactEmail, x.Status, x.InvitedAt, x.InvitedByUserId, x.SubmittedAt, x.DeclinedAt, x.DeclineReason));
    private IQueryable<TenderStageDto> Stages(Guid id) => db.TenderStages.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderStageDto(x.Id, x.TenderId, x.StageType, x.Status, x.StartedAt, x.StartedByUserId, x.CompletedAt, x.CompletedByUserId, x.Notes));
    private IQueryable<TenderBidDto> Bids(Guid id) => db.TenderBids.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderBidDto(x.Id, x.TenderId, x.TenderParticipantId, x.SupplierId, db.TenderParticipants.Where(p => p.Id == x.TenderParticipantId).Select(p => p.SupplierName).FirstOrDefault(), x.BidNumber, x.SubmittedAt, x.ReceivedAt, x.ReceivedByUserId, x.Status, x.TechnicalScore, x.CommercialScore, x.FinalScore, x.Currency, x.TotalAmount, x.FinalAmount, x.DeliveryTerms, x.PaymentTerms, x.ValidUntil, x.Notes, db.TenderBidItems.Where(i => i.TenderBidId == x.Id).Select(BidItemDto()).ToList()));
    private IQueryable<TenderEvaluationDto> Evaluations(Guid id) => db.TenderEvaluations.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderEvaluationDto(x.Id, x.TenderId, x.TenderBidId, x.EvaluationType, x.EvaluatorUserId, x.EvaluationDate, x.Score, x.Result, x.Notes));
    private IQueryable<TenderDecisionDto> Decisions(Guid id) => db.TenderDecisions.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderDecisionDto(x.Id, x.TenderId, x.DecisionType, x.DecisionDate, x.DecidedByUserId, x.SelectedTenderBidId, x.SelectedSupplierId, x.Reason, x.Notes));
    private IQueryable<TenderDocumentDto> Documents(Guid id) => db.TenderDocuments.AsNoTracking().Where(x => x.TenderId == id).Select(x => new TenderDocumentDto(x.Id, x.TenderId, x.FileDocumentId, x.DocumentType, x.OriginalFileName, x.Description, x.UploadedAt, x.UploadedByUserId));
    private static System.Linq.Expressions.Expression<Func<TenderBidItem, TenderBidItemDto>> BidItemDto() =>
        x => new TenderBidItemDto(x.Id, x.TenderBidId, x.TenderItemId, x.MescCode, x.MescGeneralGroupCode, x.GeneralDescription, x.SpecificDescription, x.Quantity, x.UnitPrice, x.TotalPrice, x.TechnicalComplianceStatus, x.TechnicalNote);
}
