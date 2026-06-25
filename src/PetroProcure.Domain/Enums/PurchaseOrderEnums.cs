namespace PetroProcure.Domain.Enums;

public enum PurchaseOrderStatus
{
    Draft = 0,
    UnderReview = 1,
    WaitingForApproval = 2,
    Approved = 3,
    Issued = 4,
    PartiallyReceived = 5,
    FullyReceived = 6,
    Completed = 7,
    Cancelled = 8,
    Archived = 9
}

public enum PurchaseOrderType
{
    ContractBased = 0,
    DirectPurchase = 1,
    TenderBased = 2,
    Service = 3,
    Other = 4
}

public enum PurchaseOrderApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Skipped = 3,
    Cancelled = 4
}
