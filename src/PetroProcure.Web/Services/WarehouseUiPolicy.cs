using PetroProcure.Domain.Enums;
using MudBlazor;

namespace PetroProcure.Web.Services;

public static class WarehouseUiPolicy
{
    public const string View = "WarehouseReceipt.View";
    public const string Create = "WarehouseReceipt.Create";
    public const string Edit = "WarehouseReceipt.Edit";
    public const string Submit = "WarehouseReceipt.Submit";
    public const string Approve = "WarehouseReceipt.Approve";
    public const string Cancel = "WarehouseReceipt.Cancel";
    public const string ManageDocuments = "WarehouseReceipt.ManageDocuments";
    public const string ReportView = "WarehouseReceipt.ReportView";
    public const string ReportExport = "WarehouseReceipt.ReportExport";
    public const string ViewStock = "Inventory.ViewStockBalance";
    public const string ViewTransactions = "Inventory.ViewTransactions";
    public const string ManageWarehouses = "Warehouse.ManageWarehouses";

    public static string ReceiptStatusText(WarehouseReceiptStatus status) => status switch
    {
        WarehouseReceiptStatus.Draft => "پیش‌نویس",
        WarehouseReceiptStatus.Submitted => "ارسال‌شده",
        WarehouseReceiptStatus.Approved => "تأییدشده",
        WarehouseReceiptStatus.Cancelled => "لغوشده",
        _ => status.ToString()
    };

    public static Color ReceiptStatusColor(WarehouseReceiptStatus status) => status switch
    {
        WarehouseReceiptStatus.Draft => Color.Default,
        WarehouseReceiptStatus.Submitted => Color.Info,
        WarehouseReceiptStatus.Approved => Color.Success,
        WarehouseReceiptStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    public static string QualityText(WarehouseReceiptQualityStatus status) => status switch
    {
        WarehouseReceiptQualityStatus.NotInspected => "بازرسی نشده",
        WarehouseReceiptQualityStatus.Accepted => "پذیرفته‌شده",
        WarehouseReceiptQualityStatus.PartiallyAccepted => "پذیرش جزئی",
        WarehouseReceiptQualityStatus.Rejected => "ردشده",
        WarehouseReceiptQualityStatus.NeedsInspection => "نیازمند بازرسی",
        _ => status.ToString()
    };

    public static string TransactionTypeText(InventoryTransactionType type) => type switch
    {
        InventoryTransactionType.Receipt => "رسید",
        InventoryTransactionType.Issue => "خروج",
        InventoryTransactionType.Adjustment => "اصلاحی",
        InventoryTransactionType.Reservation => "رزرو",
        InventoryTransactionType.ReleaseReservation => "آزادسازی رزرو",
        _ => type.ToString()
    };

    public static bool IsEditable(WarehouseReceiptStatus status) => status == WarehouseReceiptStatus.Draft;
    public static bool CanSubmit(WarehouseReceiptStatus status, Func<string, bool> has) => status == WarehouseReceiptStatus.Draft && has(Submit);
    public static bool CanApprove(WarehouseReceiptStatus status, Func<string, bool> has) => status == WarehouseReceiptStatus.Submitted && has(Approve);
    public static bool CanCancel(WarehouseReceiptStatus status, Func<string, bool> has) =>
        (status is WarehouseReceiptStatus.Draft or WarehouseReceiptStatus.Submitted) && has(Cancel);
}
