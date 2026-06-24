using System.Data;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Commission;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class CommissionSessionRepository(PetroProcureDbContext db) : ICommissionSessionRepository
{
    public async Task<string> GenerateNextSessionNumberAsync(int year, CancellationToken ct)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            var sequence = await db.TenderCommissionSessionSequences.SingleOrDefaultAsync(x => x.Year == year, ct);
            int next;
            if (sequence is null) { next = 1; db.TenderCommissionSessionSequences.Add(new TenderCommissionSessionSequence(Guid.NewGuid(), year, next)); }
            else next = sequence.Next();
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return $"TCS-{year:0000}-{next:000000}";
        });
    }

    public Task<CommissionTenderSnapshot?> GetTenderSnapshotAsync(Guid tenderId, CancellationToken ct) =>
        db.Tenders.AsNoTracking().Where(x => x.Id == tenderId)
            .Select(x => new CommissionTenderSnapshot(x.Id, x.TenderNumber, x.PurchaseFileId,
                db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault()))
            .SingleOrDefaultAsync(ct);

    public async Task<CommissionUserSnapshot?> GetUserSnapshotAsync(Guid userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null) return null;
        var profile = user.UserProfileId.HasValue
            ? await db.ApplicationUserProfiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == user.UserProfileId, ct)
            : null;
        var departmentId = user.UserProfileId.HasValue
            ? await db.UserDepartments.AsNoTracking().Where(x => x.UserProfileId == user.UserProfileId && x.IsPrimary)
                .Select(x => (Guid?)x.DepartmentId).FirstOrDefaultAsync(ct)
            : null;
        return new CommissionUserSnapshot(user.Id, profile?.DisplayName ?? user.UserName ?? user.Email ?? user.Id.ToString(), null, departmentId);
    }

    public Task<CommissionTenderBidSnapshot?> GetTenderBidSnapshotAsync(Guid tenderId, Guid bidId, CancellationToken ct) =>
        db.TenderBids.AsNoTracking().Where(x => x.TenderId == tenderId && x.Id == bidId)
            .Select(x => new CommissionTenderBidSnapshot(x.Id, x.TenderId, x.SupplierId)).SingleOrDefaultAsync(ct);

    public Task<Tender?> FindTenderAsync(Guid tenderId, bool includeDetails, CancellationToken ct)
    {
        IQueryable<Tender> q = db.Tenders;
        if (includeDetails)
            q = q.Include(x => x.Items).Include(x => x.Participants).Include(x => x.Bids).ThenInclude(x => x.Items)
                .Include(x => x.Decisions);
        return q.SingleOrDefaultAsync(x => x.Id == tenderId, ct);
    }

    public Task<TenderCommissionSession?> FindAsync(Guid id, bool includeDetails, CancellationToken ct)
    {
        IQueryable<TenderCommissionSession> q = db.TenderCommissionSessions;
        if (includeDetails)
            q = q.Include(x => x.Members).Include(x => x.AgendaItems).Include(x => x.Minutes)
                .Include(x => x.Decisions).Include(x => x.Attachments);
        return q.SingleOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<TenderCommissionSession?> FindByNumberAsync(string sessionNumber, CancellationToken ct) =>
        db.TenderCommissionSessions.SingleOrDefaultAsync(x => x.SessionNumber == sessionNumber, ct);

    public async Task AddAsync(TenderCommissionSession session, CancellationToken ct) =>
        await db.TenderCommissionSessions.AddAsync(session, ct);

    public async Task<PagedResult<TenderCommissionSessionSummaryDto>> GetPagedAsync(CommissionSessionListRequest r, CancellationToken ct)
    {
        var q = ApplyFilters(db.TenderCommissionSessions.AsNoTracking(), r);
        var total = await q.LongCountAsync(ct);
        q = (r.SortBy, r.SortDescending) switch
        {
            ("SessionNumber", false) => q.OrderBy(x => x.SessionNumber),
            ("SessionNumber", true) => q.OrderByDescending(x => x.SessionNumber),
            ("CreatedAt", false) => q.OrderBy(x => x.CreatedAt),
            ("CreatedAt", true) => q.OrderByDescending(x => x.CreatedAt),
            ("Status", false) => q.OrderBy(x => x.Status),
            ("Status", true) => q.OrderByDescending(x => x.Status),
            ("SessionDate", false) => q.OrderBy(x => x.SessionDate),
            _ => q.OrderByDescending(x => x.SessionDate)
        };
        var page = Math.Max(1, r.PageNumber);
        var size = Math.Clamp(r.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * size).Take(size)
            .Select(x => new TenderCommissionSessionSummaryDto(x.Id, x.SessionNumber, x.TenderId,
                db.Tenders.Where(t => t.Id == x.TenderId).Select(t => t.TenderNumber).FirstOrDefault(),
                x.PurchaseFileId, db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
                x.Title, x.Status, x.SessionDate, db.TenderCommissionMembers.Count(m => m.SessionId == x.Id),
                db.TenderCommissionDecisions.Count(d => d.SessionId == x.Id), x.CreatedAt)).ToListAsync(ct);
        return new(items, page, size, total);
    }

    public async Task<TenderCommissionSessionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
    {
        var session = await db.TenderCommissionSessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (session is null) return null;
        return new(ToDto(session), await Members(id).ToListAsync(ct), await Agenda(id).ToListAsync(ct),
            await Minutes(id).ToListAsync(ct), await Decisions(id).ToListAsync(ct), await Attachments(id).ToListAsync(ct));
    }

    public async Task<TenderCommissionSessionDetailDto?> GetDetailByNumberAsync(string sessionNumber, CancellationToken ct)
    {
        var id = await db.TenderCommissionSessions.AsNoTracking()
            .Where(x => x.SessionNumber == sessionNumber).Select(x => x.Id).SingleOrDefaultAsync(ct);
        return id == Guid.Empty ? null : await GetDetailAsync(id, ct);
    }

    public async Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> GetByTenderAsync(Guid tenderId, CancellationToken ct) =>
        await SummaryQuery(db.TenderCommissionSessions.AsNoTracking().Where(x => x.TenderId == tenderId))
            .OrderByDescending(x => x.SessionDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
        await SummaryQuery(db.TenderCommissionSessions.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId))
            .OrderByDescending(x => x.SessionDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TenderCommissionMemberDto>> GetMembersAsync(Guid sessionId, CancellationToken ct) => await Members(sessionId).ToListAsync(ct);
    public async Task<IReadOnlyList<TenderCommissionAgendaItemDto>> GetAgendaAsync(Guid sessionId, CancellationToken ct) => await Agenda(sessionId).ToListAsync(ct);
    public async Task<IReadOnlyList<TenderCommissionMinuteDto>> GetMinutesAsync(Guid sessionId, CancellationToken ct) => await Minutes(sessionId).ToListAsync(ct);
    public async Task<IReadOnlyList<TenderCommissionDecisionDto>> GetDecisionsAsync(Guid sessionId, CancellationToken ct) => await Decisions(sessionId).ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private IQueryable<TenderCommissionSession> ApplyFilters(IQueryable<TenderCommissionSession> q, CommissionSessionListRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) q = q.Where(x => x.SessionNumber.Contains(r.SearchTerm) || x.Title.Contains(r.SearchTerm));
        if (!string.IsNullOrWhiteSpace(r.SessionNumber)) q = q.Where(x => x.SessionNumber.Contains(r.SessionNumber));
        if (!string.IsNullOrWhiteSpace(r.TenderNumber)) q = q.Where(x => db.Tenders.Any(t => t.Id == x.TenderId && t.TenderNumber.Contains(r.TenderNumber)));
        if (!string.IsNullOrWhiteSpace(r.PurchaseFileNumber)) q = q.Where(x => db.PurchaseFiles.Any(p => p.Id == x.PurchaseFileId && p.FileNumber.Contains(r.PurchaseFileNumber)));
        if (r.Status.HasValue) q = q.Where(x => x.Status == r.Status);
        if (r.SessionDateFrom.HasValue) q = q.Where(x => x.SessionDate >= r.SessionDateFrom);
        if (r.SessionDateTo.HasValue) q = q.Where(x => x.SessionDate <= r.SessionDateTo);
        if (r.MemberUserId.HasValue) q = q.Where(x => db.TenderCommissionMembers.Any(m => m.SessionId == x.Id && m.UserId == r.MemberUserId));
        return q;
    }

    private IQueryable<TenderCommissionSessionSummaryDto> SummaryQuery(IQueryable<TenderCommissionSession> q) =>
        q.Select(x => new TenderCommissionSessionSummaryDto(x.Id, x.SessionNumber, x.TenderId,
            db.Tenders.Where(t => t.Id == x.TenderId).Select(t => t.TenderNumber).FirstOrDefault(),
            x.PurchaseFileId, db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
            x.Title, x.Status, x.SessionDate, db.TenderCommissionMembers.Count(m => m.SessionId == x.Id),
            db.TenderCommissionDecisions.Count(d => d.SessionId == x.Id), x.CreatedAt));

    private TenderCommissionSessionDto ToDto(TenderCommissionSession x) => new(x.Id, x.SessionNumber, x.TenderId,
        db.Tenders.Where(t => t.Id == x.TenderId).Select(t => t.TenderNumber).FirstOrDefault(),
        x.PurchaseFileId, db.PurchaseFiles.Where(p => p.Id == x.PurchaseFileId).Select(p => p.FileNumber).FirstOrDefault(),
        x.Title, x.Description, x.SessionDate, x.Location, x.Status, x.CreatedAt, x.CreatedByUserId,
        x.StartedAt, x.StartedByUserId, x.CompletedAt, x.CompletedByUserId, x.ApprovedAt, x.ApprovedByUserId,
        x.CancelledAt, x.CancelledByUserId, x.CancellationReason);

    private IQueryable<TenderCommissionMemberDto> Members(Guid id) => db.TenderCommissionMembers.AsNoTracking()
        .Where(x => x.SessionId == id).Select(x => new TenderCommissionMemberDto(x.Id, x.SessionId, x.UserId,
            x.FullNameSnapshot, x.PositionSnapshot, x.DepartmentId, x.Role, x.AttendanceStatus, x.VoteStatus, x.VoteNote, x.SignedAt));

    private IQueryable<TenderCommissionAgendaItemDto> Agenda(Guid id) => db.TenderCommissionAgendaItems.AsNoTracking()
        .Where(x => x.SessionId == id).OrderBy(x => x.OrderNo).Select(x => new TenderCommissionAgendaItemDto(x.Id,
            x.SessionId, x.OrderNo, x.Title, x.Description, x.RelatedTenderBidId, x.RelatedSupplierId, x.Status, x.Notes));

    private IQueryable<TenderCommissionMinuteDto> Minutes(Guid id) => db.TenderCommissionMinutes.AsNoTracking()
        .Where(x => x.SessionId == id).OrderByDescending(x => x.CreatedAt).Select(x => new TenderCommissionMinuteDto(x.Id,
            x.SessionId, x.AgendaItemId, x.Text, x.CreatedAt, x.CreatedByUserId, x.UpdatedAt, x.UpdatedByUserId));

    private IQueryable<TenderCommissionDecisionDto> Decisions(Guid id) => db.TenderCommissionDecisions.AsNoTracking()
        .Where(x => x.SessionId == id).OrderByDescending(x => x.CreatedAt).Select(x => new TenderCommissionDecisionDto(x.Id,
            x.SessionId, x.DecisionType, x.TenderId, x.SelectedTenderBidId, x.SelectedSupplierId, x.DecisionText,
            x.Reason, x.Status, x.CreatedAt, x.CreatedByUserId, x.ApprovedAt, x.ApprovedByUserId));

    private IQueryable<TenderCommissionAttachmentDto> Attachments(Guid id) => db.TenderCommissionAttachments.AsNoTracking()
        .Where(x => x.SessionId == id).OrderByDescending(x => x.UploadedAt).Select(x => new TenderCommissionAttachmentDto(x.Id,
            x.SessionId, x.FileDocumentId, x.DocumentType, x.OriginalFileName, x.Description, x.UploadedAt, x.UploadedByUserId));
}
