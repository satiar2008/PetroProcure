namespace PetroProcure.Domain.Enums;

public enum TenderStatus
{
    Draft = 0,
    ReadyToPublish = 1,
    Published = 2,
    ReceivingBids = 3,
    UnderQualification = 4,
    UnderTechnicalEvaluation = 5,
    UnderCommercialEvaluation = 6,
    UnderFinalReview = 7,
    WinnerSelected = 8,
    Closed = 9,
    Cancelled = 10
}

public enum TenderType
{
    PublicTender = 0,
    LimitedTender = 1,
    TwoStageTender = 2,
    SingleStageTender = 3,
    NegotiatedTender = 4
}

public enum TenderParticipantStatus
{
    Draft = 0,
    Invited = 1,
    Submitted = 2,
    Declined = 3,
    Disqualified = 4,
    Qualified = 5,
    NoResponse = 6
}

public enum TenderStageType
{
    Preparation = 0,
    SupplierInvitation = 1,
    Qualification = 2,
    TechnicalEvaluation = 3,
    CommercialEvaluation = 4,
    FinalDecision = 5
}

public enum TenderStageStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum TenderBidStatus
{
    Draft = 0,
    Submitted = 1,
    Received = 2,
    UnderReview = 3,
    Accepted = 4,
    Rejected = 5,
    Selected = 6
}

public enum TenderEvaluationType
{
    Qualification = 0,
    Technical = 1,
    Commercial = 2,
    Final = 3
}

public enum TenderEvaluationResult
{
    Passed = 0,
    Failed = 1,
    Conditional = 2,
    NeedsReview = 3
}

public enum TenderDecisionType
{
    SelectWinner = 0,
    CancelTender = 1,
    Retender = 2,
    RejectAll = 3
}
