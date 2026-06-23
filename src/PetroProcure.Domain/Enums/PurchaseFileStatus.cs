namespace PetroProcure.Domain.Enums;

public enum PurchaseFileStatus
{
    Draft = 1,
    WaitingForIndentReview = 2,
    WaitingForPurchaseDepartment = 3,
    InPurchaseDepartment = 4,
    WaitingForTechnicalReview = 5,
    WaitingForTenderCommission = 6,
    InTender = 7,
    WaitingForContract = 8,
    WaitingForPurchaseOrder = 9,
    WaitingForWarehouseReceipt = 10,
    Completed = 11,
    Cancelled = 12,
    Archived = 13
}
