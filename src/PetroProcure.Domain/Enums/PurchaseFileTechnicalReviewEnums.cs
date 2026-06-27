namespace PetroProcure.Domain.Enums;

public enum PurchaseFileTechnicalReviewStatus
{
    Requested = 0,
    InReview = 1,
    ClarificationRequested = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

public enum PurchaseFileTechnicalReviewDecision
{
    TechnicallyAccepted = 0,
    TechnicallyRejected = 1,
    NeedsClarification = 2,
    ConditionalAcceptance = 3
}
