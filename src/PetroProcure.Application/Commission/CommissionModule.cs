using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.TenderCommission;
using PetroProcure.Domain.Modules.Tenders;

namespace PetroProcure.Application.Commission;

public sealed record CreateCommissionSessionCommand(Guid TenderId, string Title, DateTime SessionDate, string? Location, string? Description);
public sealed record CreateCommissionSessionFromTenderCommand(Guid TenderId, string Title, DateTime SessionDate, string? Location, string? Description, AddCommissionMemberRequest[] Members, AddAgendaItemRequest[] AgendaItems);
public sealed record UpdateCommissionSessionCommand(Guid Id, string Title, DateTime SessionDate, string? Location, string? Description);
public sealed record ScheduleCommissionSessionCommand(Guid Id, DateTime SessionDate, string? Location);
public sealed record StartCommissionSessionCommand(Guid Id);
public sealed record CompleteCommissionSessionCommand(Guid Id);
public sealed record ApproveCommissionSessionCommand(Guid Id);
public sealed record CancelCommissionSessionCommand(Guid Id, string Reason);
public sealed record AddCommissionMemberCommand(Guid SessionId, Guid UserId, TenderCommissionMemberRole Role);
public sealed record UpdateCommissionMemberCommand(Guid SessionId, Guid MemberId, TenderCommissionMemberRole Role, TenderCommissionAttendanceStatus AttendanceStatus, TenderCommissionVoteStatus? VoteStatus, string? VoteNote);
public sealed record RemoveCommissionMemberCommand(Guid SessionId, Guid MemberId);
public sealed record AddAgendaItemCommand(Guid SessionId, int OrderNo, string Title, string? Description, Guid? RelatedTenderBidId, Guid? RelatedSupplierId);
public sealed record UpdateAgendaItemCommand(Guid SessionId, Guid AgendaItemId, int OrderNo, string Title, string? Description, TenderCommissionAgendaStatus Status, string? Notes);
public sealed record AddCommissionMinuteCommand(Guid SessionId, Guid? AgendaItemId, string Text);
public sealed record UpdateCommissionMinuteCommand(Guid SessionId, Guid MinuteId, string Text);
public sealed record AddCommissionDecisionCommand(Guid SessionId, TenderCommissionDecisionType DecisionType, Guid? SelectedTenderBidId, Guid? SelectedSupplierId, string DecisionText, string? Reason);
public sealed record ApproveCommissionDecisionCommand(Guid SessionId, Guid DecisionId);
public sealed record RejectCommissionDecisionCommand(Guid SessionId, Guid DecisionId);

public sealed record GetCommissionSessionsQuery(CommissionSessionListRequest Request);
public sealed record GetCommissionSessionByIdQuery(Guid Id);
public sealed record GetCommissionSessionByNumberQuery(string SessionNumber);
public sealed record GetCommissionSessionsByTenderQuery(Guid TenderId);
public sealed record GetCommissionSessionsByPurchaseFileQuery(Guid PurchaseFileId);
public sealed record GetCommissionSessionMembersQuery(Guid SessionId);
public sealed record GetCommissionSessionAgendaQuery(Guid SessionId);
public sealed record GetCommissionSessionMinutesQuery(Guid SessionId);
public sealed record GetCommissionSessionDecisionsQuery(Guid SessionId);

public sealed record CommissionTenderSnapshot(Guid Id, string TenderNumber, Guid PurchaseFileId, string? PurchaseFileNumber);
public sealed record CommissionUserSnapshot(Guid Id, string FullName, string? Position, Guid? DepartmentId);
public sealed record CommissionTenderBidSnapshot(Guid Id, Guid TenderId, Guid SupplierId);

public interface ICommissionSessionRepository
{
    Task<string> GenerateNextSessionNumberAsync(int year, CancellationToken cancellationToken);
    Task<CommissionTenderSnapshot?> GetTenderSnapshotAsync(Guid tenderId, CancellationToken cancellationToken);
    Task<CommissionUserSnapshot?> GetUserSnapshotAsync(Guid userId, CancellationToken cancellationToken);
    Task<CommissionTenderBidSnapshot?> GetTenderBidSnapshotAsync(Guid tenderId, Guid bidId, CancellationToken cancellationToken);
    Task<Tender?> FindTenderAsync(Guid tenderId, bool includeDetails, CancellationToken cancellationToken);
    Task<TenderCommissionSession?> FindAsync(Guid id, bool includeDetails, CancellationToken cancellationToken);
    Task<TenderCommissionSession?> FindByNumberAsync(string sessionNumber, CancellationToken cancellationToken);
    Task AddAsync(TenderCommissionSession session, CancellationToken cancellationToken);
    Task<PagedResult<TenderCommissionSessionSummaryDto>> GetPagedAsync(CommissionSessionListRequest request, CancellationToken cancellationToken);
    Task<TenderCommissionSessionDetailDto?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<TenderCommissionSessionDetailDto?> GetDetailByNumberAsync(string sessionNumber, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> GetByTenderAsync(Guid tenderId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> GetByPurchaseFileAsync(Guid purchaseFileId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderCommissionMemberDto>> GetMembersAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderCommissionAgendaItemDto>> GetAgendaAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderCommissionMinuteDto>> GetMinutesAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenderCommissionDecisionDto>> GetDecisionsAsync(Guid sessionId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface ICommissionSessionNumberService
{
    Task<string> GenerateNextSessionNumber(int year, CancellationToken cancellationToken = default);
}

public sealed class CommissionSessionNumberService(ICommissionSessionRepository repository) : ICommissionSessionNumberService
{
    public Task<string> GenerateNextSessionNumber(int year, CancellationToken cancellationToken = default) =>
        repository.GenerateNextSessionNumberAsync(year, cancellationToken);
}

public interface ICommissionSessionEligibilityService;
public sealed class CommissionSessionEligibilityService : ICommissionSessionEligibilityService;

public sealed class CommissionCommandHandler(ICommissionSessionRepository repository, ICommissionSessionNumberService numbers,
    ICurrentUserService currentUser)
{
    public async Task<TenderCommissionSessionDetailDto> Handle(CreateCommissionSessionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var tender = await repository.GetTenderSnapshotAsync(command.TenderId, ct)
            ?? throw new CommissionValidationException("Tender was not found.");
        var number = await numbers.GenerateNextSessionNumber(DateTime.UtcNow.Year, ct);
        var session = new TenderCommissionSession(Guid.NewGuid(), number, tender.Id, tender.PurchaseFileId,
            command.Title, command.SessionDate, command.Location, command.Description, currentUser.UserId);
        await repository.AddAsync(session, ct);
        await repository.SaveChangesAsync(ct);
        return await Detail(session.Id, ct);
    }

    public async Task<TenderCommissionSessionDetailDto> Handle(CreateCommissionSessionFromTenderCommand command, CancellationToken ct = default)
    {
        var detail = await Handle(new CreateCommissionSessionCommand(command.TenderId, command.Title, command.SessionDate,
            command.Location, command.Description), ct);
        foreach (var member in command.Members)
            await Handle(new AddCommissionMemberCommand(detail.Session.Id, member.UserId, member.Role), ct);
        foreach (var item in command.AgendaItems)
            await Handle(new AddAgendaItemCommand(detail.Session.Id, item.OrderNo, item.Title, item.Description,
                item.RelatedTenderBidId, item.RelatedSupplierId), ct);
        await repository.SaveChangesAsync(ct);
        return await Detail(detail.Session.Id, ct);
    }

    public async Task<TenderCommissionSessionDetailDto> Handle(UpdateCommissionSessionCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.Id, true, ct);
        session.Update(command.Title, command.SessionDate, command.Location, command.Description);
        await repository.SaveChangesAsync(ct);
        return await Detail(session.Id, ct);
    }

    public async Task Handle(ScheduleCommissionSessionCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.Id, true, ct);
        session.Schedule(command.SessionDate, command.Location);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(StartCommissionSessionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.Id, true, ct);
        session.Start(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CompleteCommissionSessionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.Id, true, ct);
        session.Complete(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(ApproveCommissionSessionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.Id, true, ct);
        session.Approve(currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(CancelCommissionSessionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.Id, true, ct);
        session.Cancel(command.Reason, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<TenderCommissionMemberDto> Handle(AddCommissionMemberCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.SessionId, true, ct);
        var user = await repository.GetUserSnapshotAsync(command.UserId, ct)
            ?? throw new CommissionValidationException("User was not found.");
        var member = new TenderCommissionMember(Guid.NewGuid(), session.Id, user.Id, user.FullName,
            user.Position, user.DepartmentId, command.Role);
        session.AddMember(member);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetMembersAsync(session.Id, ct)).Single(x => x.Id == member.Id);
    }

    public async Task<TenderCommissionMemberDto> Handle(UpdateCommissionMemberCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.SessionId, true, ct);
        session.UpdateMember(command.MemberId, command.Role, command.AttendanceStatus, command.VoteStatus, command.VoteNote);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetMembersAsync(session.Id, ct)).Single(x => x.Id == command.MemberId);
    }

    public async Task Handle(RemoveCommissionMemberCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.SessionId, true, ct);
        session.RemoveMember(command.MemberId);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<TenderCommissionAgendaItemDto> Handle(AddAgendaItemCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.SessionId, true, ct);
        var item = new TenderCommissionAgendaItem(Guid.NewGuid(), session.Id, command.OrderNo, command.Title,
            command.Description, command.RelatedTenderBidId, command.RelatedSupplierId);
        session.AddAgendaItem(item);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetAgendaAsync(session.Id, ct)).Single(x => x.Id == item.Id);
    }

    public async Task<TenderCommissionAgendaItemDto> Handle(UpdateAgendaItemCommand command, CancellationToken ct = default)
    {
        var session = await Find(command.SessionId, true, ct);
        session.UpdateAgendaItem(command.AgendaItemId, command.Title, command.Description, command.OrderNo,
            command.Status, command.Notes);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetAgendaAsync(session.Id, ct)).Single(x => x.Id == command.AgendaItemId);
    }

    public async Task<TenderCommissionMinuteDto> Handle(AddCommissionMinuteCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.SessionId, true, ct);
        var minute = new TenderCommissionMinute(Guid.NewGuid(), session.Id, command.AgendaItemId, command.Text, currentUser.UserId);
        session.AddMinute(minute);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetMinutesAsync(session.Id, ct)).Single(x => x.Id == minute.Id);
    }

    public async Task<TenderCommissionMinuteDto> Handle(UpdateCommissionMinuteCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.SessionId, true, ct);
        session.UpdateMinute(command.MinuteId, command.Text, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetMinutesAsync(session.Id, ct)).Single(x => x.Id == command.MinuteId);
    }

    public async Task<TenderCommissionDecisionDto> Handle(AddCommissionDecisionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.SessionId, true, ct);
        var supplierId = command.SelectedSupplierId;
        if (command.SelectedTenderBidId.HasValue && !supplierId.HasValue)
        {
            var bid = await repository.GetTenderBidSnapshotAsync(session.TenderId, command.SelectedTenderBidId.Value, ct)
                ?? throw new CommissionValidationException("Tender bid was not found.");
            supplierId = bid.SupplierId;
        }
        var decision = new TenderCommissionDecision(Guid.NewGuid(), session.Id, command.DecisionType, session.TenderId,
            command.SelectedTenderBidId, supplierId, command.DecisionText, command.Reason, currentUser.UserId);
        session.AddDecision(decision);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetDecisionsAsync(session.Id, ct)).Single(x => x.Id == decision.Id);
    }

    public async Task<TenderCommissionDecisionDto> Handle(ApproveCommissionDecisionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.SessionId, true, ct);
        var decision = session.ApproveDecision(command.DecisionId, currentUser.UserId);
        if (decision.DecisionType == TenderCommissionDecisionType.ApproveWinner && decision.SelectedTenderBidId.HasValue)
        {
            var tender = await repository.FindTenderAsync(decision.TenderId, true, ct)
                ?? throw new CommissionValidationException("Tender was not found.");
            if (tender.Status != TenderStatus.WinnerSelected)
                tender.SelectWinner(decision.SelectedTenderBidId.Value, currentUser.UserId, decision.Reason, decision.DecisionText);
        }
        await repository.SaveChangesAsync(ct);
        return (await repository.GetDecisionsAsync(session.Id, ct)).Single(x => x.Id == decision.Id);
    }

    public async Task<TenderCommissionDecisionDto> Handle(RejectCommissionDecisionCommand command, CancellationToken ct = default)
    {
        EnsureUser();
        var session = await Find(command.SessionId, true, ct);
        session.RejectDecision(command.DecisionId, currentUser.UserId);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetDecisionsAsync(session.Id, ct)).Single(x => x.Id == command.DecisionId);
    }

    private async Task<TenderCommissionSession> Find(Guid id, bool includeDetails, CancellationToken ct) =>
        await repository.FindAsync(id, includeDetails, ct) ?? throw new CommissionNotFoundException("Commission session was not found.");
    private async Task<TenderCommissionSessionDetailDto> Detail(Guid id, CancellationToken ct) =>
        await repository.GetDetailAsync(id, ct) ?? throw new CommissionNotFoundException("Commission session was not found.");
    private void EnsureUser() { if (!currentUser.IsAuthenticated || currentUser.UserId == Guid.Empty) throw new CurrentUserNotAuthenticatedException(); }
}

public sealed class CommissionQueryHandler(ICommissionSessionRepository repository)
{
    public Task<PagedResult<TenderCommissionSessionSummaryDto>> Handle(GetCommissionSessionsQuery query, CancellationToken ct = default) => repository.GetPagedAsync(query.Request, ct);
    public Task<TenderCommissionSessionDetailDto?> Handle(GetCommissionSessionByIdQuery query, CancellationToken ct = default) => repository.GetDetailAsync(query.Id, ct);
    public Task<TenderCommissionSessionDetailDto?> Handle(GetCommissionSessionByNumberQuery query, CancellationToken ct = default) => repository.GetDetailByNumberAsync(query.SessionNumber, ct);
    public Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> Handle(GetCommissionSessionsByTenderQuery query, CancellationToken ct = default) => repository.GetByTenderAsync(query.TenderId, ct);
    public Task<IReadOnlyList<TenderCommissionSessionSummaryDto>> Handle(GetCommissionSessionsByPurchaseFileQuery query, CancellationToken ct = default) => repository.GetByPurchaseFileAsync(query.PurchaseFileId, ct);
    public Task<IReadOnlyList<TenderCommissionMemberDto>> Handle(GetCommissionSessionMembersQuery query, CancellationToken ct = default) => repository.GetMembersAsync(query.SessionId, ct);
    public Task<IReadOnlyList<TenderCommissionAgendaItemDto>> Handle(GetCommissionSessionAgendaQuery query, CancellationToken ct = default) => repository.GetAgendaAsync(query.SessionId, ct);
    public Task<IReadOnlyList<TenderCommissionMinuteDto>> Handle(GetCommissionSessionMinutesQuery query, CancellationToken ct = default) => repository.GetMinutesAsync(query.SessionId, ct);
    public Task<IReadOnlyList<TenderCommissionDecisionDto>> Handle(GetCommissionSessionDecisionsQuery query, CancellationToken ct = default) => repository.GetDecisionsAsync(query.SessionId, ct);
}

public sealed class CommissionValidationException(string message) : Exception(message);
public sealed class CommissionNotFoundException(string message) : Exception(message);
