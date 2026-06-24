namespace PetroProcure.Domain.Enums;

public enum TenderCommissionSessionStatus
{
    Draft = 0,
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Approved = 4,
    Cancelled = 5
}

public enum TenderCommissionMemberRole
{
    Chairperson = 0,
    Secretary = 1,
    Member = 2,
    Observer = 3,
    TechnicalExpert = 4,
    FinancialExpert = 5
}

public enum TenderCommissionAttendanceStatus
{
    Invited = 0,
    Present = 1,
    Absent = 2,
    Excused = 3
}

public enum TenderCommissionVoteStatus
{
    NotVoted = 0,
    Approve = 1,
    Reject = 2,
    Abstain = 3,
    NeedsMoreReview = 4
}

public enum TenderCommissionAgendaStatus
{
    Pending = 0,
    Discussed = 1,
    Deferred = 2,
    Closed = 3
}

public enum TenderCommissionDecisionType
{
    RecommendWinner = 0,
    ApproveWinner = 1,
    RejectAll = 2,
    Retender = 3,
    RequestTechnicalReview = 4,
    RequestCommercialReview = 5,
    CancelTender = 6,
    Other = 7
}

public enum TenderCommissionDecisionStatus
{
    Draft = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}
