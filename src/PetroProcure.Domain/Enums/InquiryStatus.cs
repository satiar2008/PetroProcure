namespace PetroProcure.Domain.Enums;

public enum InquiryStatus
{
    Draft = 0,
    ReadyToSend = 1,
    Sent = 2,
    PartiallyResponded = 3,
    FullyResponded = 4,
    UnderComparison = 5,
    SupplierSelected = 6,
    Closed = 7,
    Cancelled = 8
}
