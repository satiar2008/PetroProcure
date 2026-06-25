namespace PetroProcure.Domain.Enums;

public enum ContractStatus
{
    Draft = 0,
    UnderReview = 1,
    WaitingForApproval = 2,
    Approved = 3,
    Signed = 4,
    Active = 5,
    Completed = 6,
    Cancelled = 7,
    Archived = 8
}

public enum ContractType
{
    DirectPurchase = 0,
    TenderBased = 1,
    Service = 2,
    Framework = 3,
    Other = 4
}

public enum ContractClauseType
{
    General = 0,
    Technical = 1,
    Commercial = 2,
    Payment = 3,
    Delivery = 4,
    Warranty = 5,
    Penalty = 6,
    Legal = 7,
    AttachmentReference = 8,
    Other = 9
}

public enum ContractApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3,
    Cancelled = 4
}
