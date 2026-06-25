namespace PetroProcure.Domain.Enums;

public enum WarehouseReceiptStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Cancelled = 3
}

public enum WarehouseReceiptQualityStatus
{
    NotInspected = 0,
    Accepted = 1,
    PartiallyAccepted = 2,
    Rejected = 3,
    NeedsInspection = 4
}

public enum InventoryTransactionType
{
    Receipt = 0,
    Issue = 1,
    Adjustment = 2,
    Reservation = 3,
    ReleaseReservation = 4
}

public enum InventoryTransactionReferenceType
{
    WarehouseReceipt = 0,
    ManualAdjustment = 1,
    PurchaseOrder = 2,
    MaterialNeed = 3,
    Other = 4
}
