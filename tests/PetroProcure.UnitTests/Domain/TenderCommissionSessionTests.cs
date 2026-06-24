using PetroProcure.Application.Commission;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.UnitTests.Domain;

public sealed class TenderCommissionSessionTests
{
    [Fact]
    public async Task Session_number_generation_uses_expected_format()
    {
        var service = new CommissionSessionNumberService(new FakeCommissionRepository("TCS-2026-000001"));

        var number = await service.GenerateNextSessionNumber(2026);

        Assert.Equal("TCS-2026-000001", number);
    }

    [Fact]
    public void Session_cannot_be_approved_without_members()
    {
        var session = CreateSession();
        session.AddMinute(new TenderCommissionMinute(Guid.NewGuid(), session.Id, null, "صورتجلسه", Guid.NewGuid()));

        var ex = Assert.Throws<InvalidOperationException>(() => session.Approve(Guid.NewGuid()));

        Assert.Contains("without at least one member", ex.Message);
    }

    [Fact]
    public void Session_cannot_be_approved_without_minute_or_decision()
    {
        var session = CreateSession();
        session.AddMember(Member(session.Id, TenderCommissionMemberRole.Member));

        var ex = Assert.Throws<InvalidOperationException>(() => session.Approve(Guid.NewGuid()));

        Assert.Contains("without at least one minute or decision", ex.Message);
    }

    [Fact]
    public void Only_one_chairperson_is_allowed()
    {
        var session = CreateSession();
        session.AddMember(Member(session.Id, TenderCommissionMemberRole.Chairperson));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.AddMember(Member(session.Id, TenderCommissionMemberRole.Chairperson)));

        Assert.Contains("Only one chairperson", ex.Message);
    }

    [Fact]
    public void Only_one_secretary_is_allowed()
    {
        var session = CreateSession();
        session.AddMember(Member(session.Id, TenderCommissionMemberRole.Secretary));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            session.AddMember(Member(session.Id, TenderCommissionMemberRole.Secretary)));

        Assert.Contains("Only one secretary", ex.Message);
    }

    [Fact]
    public void Approved_session_is_read_only()
    {
        var session = CreateSession();
        session.AddMember(Member(session.Id, TenderCommissionMemberRole.Member));
        session.AddMinute(new TenderCommissionMinute(Guid.NewGuid(), session.Id, null, "صورتجلسه", Guid.NewGuid()));
        session.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() =>
            session.AddAgendaItem(new TenderCommissionAgendaItem(Guid.NewGuid(), session.Id, 1, "دستور", null, null, null)));
    }

    [Fact]
    public void Decision_approve_updates_decision_status()
    {
        var session = CreateSession();
        var decision = new TenderCommissionDecision(Guid.NewGuid(), session.Id, TenderCommissionDecisionType.Other,
            session.TenderId, null, null, "تصمیم", null, Guid.NewGuid());
        session.AddDecision(decision);

        var approved = session.ApproveDecision(decision.Id, Guid.NewGuid());

        Assert.Equal(TenderCommissionDecisionStatus.Approved, approved.Status);
        Assert.NotNull(approved.ApprovedAt);
    }

    [Fact]
    public void Winner_decision_requires_selected_bid_and_supplier()
    {
        var session = CreateSession();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            new TenderCommissionDecision(Guid.NewGuid(), session.Id, TenderCommissionDecisionType.ApproveWinner,
                session.TenderId, null, null, "تأیید برنده", null, Guid.NewGuid()));

        Assert.Contains("Winner decisions require selected bid and supplier", ex.Message);
    }

    private static TenderCommissionSession CreateSession() =>
        new(Guid.NewGuid(), "TCS-2026-000001", Guid.NewGuid(), Guid.NewGuid(), "جلسه کمیسیون",
            DateTime.UtcNow, "اتاق جلسات", null, Guid.NewGuid());

    private static TenderCommissionMember Member(Guid sessionId, TenderCommissionMemberRole role) =>
        new(Guid.NewGuid(), sessionId, Guid.NewGuid(), $"عضو {role}", null, null, role);

    private sealed class FakeCommissionRepository(string number) : ICommissionSessionRepository
    {
        public Task<string> GenerateNextSessionNumberAsync(int year, CancellationToken cancellationToken) => Task.FromResult(number);
        public Task<CommissionTenderSnapshot?> GetTenderSnapshotAsync(Guid tenderId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<CommissionUserSnapshot?> GetUserSnapshotAsync(Guid userId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<CommissionTenderBidSnapshot?> GetTenderBidSnapshotAsync(Guid tenderId, Guid bidId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Tender?> FindTenderAsync(Guid tenderId, bool includeDetails, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TenderCommissionSession?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TenderCommissionSession?> FindByNumberAsync(string sessionNumber, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddAsync(TenderCommissionSession session, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<PagedResult<TenderCommissionSessionSummaryDto>> GetPagedAsync(CommissionSessionListRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TenderCommissionSessionDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<TenderCommissionSessionDetailDto?> GetDetailByNumberAsync(string sessionNumber, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> GetByTenderAsync(Guid tenderId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<TenderCommissionMemberDto>> GetMembersAsync(Guid sessionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<TenderCommissionAgendaItemDto>> GetAgendaAsync(Guid sessionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<TenderCommissionMinuteDto>> GetMinutesAsync(Guid sessionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<TenderCommissionDecisionDto>> GetDecisionsAsync(Guid sessionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task SaveChangesAsync(CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
