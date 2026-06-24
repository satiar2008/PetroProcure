using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Commission;

public sealed record TenderCommissionSessionDto(Guid Id, string SessionNumber, Guid TenderId, string? TenderNumber,
    Guid PurchaseFileId, string? PurchaseFileNumber, string Title, string? Description, DateTime SessionDate,
    string? Location, TenderCommissionSessionStatus Status, DateTime CreatedAt, Guid CreatedByUserId,
    DateTime? StartedAt, Guid? StartedByUserId, DateTime? CompletedAt, Guid? CompletedByUserId,
    DateTime? ApprovedAt, Guid? ApprovedByUserId, DateTime? CancelledAt, Guid? CancelledByUserId,
    string? CancellationReason);

public sealed record TenderCommissionSessionSummaryDto(Guid Id, string SessionNumber, Guid TenderId,
    string? TenderNumber, Guid PurchaseFileId, string? PurchaseFileNumber, string Title,
    TenderCommissionSessionStatus Status, DateTime SessionDate, int MemberCount, int DecisionCount, DateTime CreatedAt);

public sealed record TenderCommissionSessionDetailDto(TenderCommissionSessionDto Session,
    IReadOnlyList<TenderCommissionMemberDto> Members,
    IReadOnlyList<TenderCommissionAgendaItemDto> AgendaItems,
    IReadOnlyList<TenderCommissionMinuteDto> Minutes,
    IReadOnlyList<TenderCommissionDecisionDto> Decisions,
    IReadOnlyList<TenderCommissionAttachmentDto> Attachments);

public sealed record TenderCommissionMemberDto(Guid Id, Guid SessionId, Guid UserId, string FullNameSnapshot,
    string? PositionSnapshot, Guid? DepartmentId, TenderCommissionMemberRole Role,
    TenderCommissionAttendanceStatus AttendanceStatus, TenderCommissionVoteStatus? VoteStatus,
    string? VoteNote, DateTime? SignedAt);

public sealed record TenderCommissionAgendaItemDto(Guid Id, Guid SessionId, int OrderNo, string Title,
    string? Description, Guid? RelatedTenderBidId, Guid? RelatedSupplierId,
    TenderCommissionAgendaStatus Status, string? Notes);

public sealed record TenderCommissionMinuteDto(Guid Id, Guid SessionId, Guid? AgendaItemId, string Text,
    DateTime CreatedAt, Guid CreatedByUserId, DateTime? UpdatedAt, Guid? UpdatedByUserId);

public sealed record TenderCommissionDecisionDto(Guid Id, Guid SessionId, TenderCommissionDecisionType DecisionType,
    Guid TenderId, Guid? SelectedTenderBidId, Guid? SelectedSupplierId, string DecisionText, string? Reason,
    TenderCommissionDecisionStatus Status, DateTime CreatedAt, Guid CreatedByUserId,
    DateTime? ApprovedAt, Guid? ApprovedByUserId);

public sealed record TenderCommissionAttachmentDto(Guid Id, Guid SessionId, Guid? FileDocumentId,
    string DocumentType, string? OriginalFileName, string? Description, DateTime UploadedAt, Guid UploadedByUserId);
public sealed record CommissionAttachmentDto(Guid Id, Guid SessionId, Guid? FileDocumentId,
    string DocumentType, string? OriginalFileName, string? Description, DateTime UploadedAt, Guid UploadedByUserId);

public sealed record CommissionSessionLookupDto(Guid Id, string SessionNumber, string Title,
    TenderCommissionSessionStatus Status);

public sealed record CommissionSessionListRequest(string? SearchTerm = null, string? SessionNumber = null,
    string? TenderNumber = null, string? PurchaseFileNumber = null, TenderCommissionSessionStatus? Status = null,
    DateTime? SessionDateFrom = null, DateTime? SessionDateTo = null, Guid? MemberUserId = null,
    string? SortBy = "SessionDate", bool SortDescending = true, int PageNumber = 1, int PageSize = 20);

public sealed record CreateCommissionSessionRequest(Guid TenderId, string Title, DateTime SessionDate,
    string? Location, string? Description);
public sealed record CreateCommissionSessionFromTenderRequest(string Title, DateTime SessionDate,
    string? Location, string? Description, AddCommissionMemberRequest[] Members, AddAgendaItemRequest[] AgendaItems);
public sealed record UpdateCommissionSessionRequest(string Title, DateTime SessionDate, string? Location, string? Description);
public sealed record ScheduleCommissionSessionRequest(DateTime SessionDate, string? Location);
public sealed record StartCommissionSessionRequest;
public sealed record CompleteCommissionSessionRequest;
public sealed record ApproveCommissionSessionRequest;
public sealed record CancelCommissionSessionRequest(string Reason);

public sealed record AddCommissionMemberRequest(Guid UserId, TenderCommissionMemberRole Role);
public sealed record UpdateCommissionMemberRequest(TenderCommissionMemberRole Role,
    TenderCommissionAttendanceStatus AttendanceStatus, TenderCommissionVoteStatus? VoteStatus, string? VoteNote);
public sealed record RemoveCommissionMemberRequest(Guid MemberId);

public sealed record AddAgendaItemRequest(int OrderNo, string Title, string? Description,
    Guid? RelatedTenderBidId, Guid? RelatedSupplierId);
public sealed record UpdateAgendaItemRequest(int OrderNo, string Title, string? Description,
    TenderCommissionAgendaStatus Status, string? Notes);

public sealed record AddCommissionMinuteRequest(Guid? AgendaItemId, string Text);
public sealed record UpdateCommissionMinuteRequest(string Text);

public sealed record AddCommissionDecisionRequest(TenderCommissionDecisionType DecisionType,
    Guid? SelectedTenderBidId, Guid? SelectedSupplierId, string DecisionText, string? Reason);
public sealed record ApproveCommissionDecisionRequest;
public sealed record RejectCommissionDecisionRequest;
