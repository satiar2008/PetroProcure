using MudBlazor;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class PurchaseOrderUiPolicy
{
    public const string View = "PurchaseOrder.View";
    public const string Create = "PurchaseOrder.Create";
    public const string Edit = "PurchaseOrder.Edit";
    public const string Submit = "PurchaseOrder.Submit";
    public const string Approve = "PurchaseOrder.Approve";
    public const string Reject = "PurchaseOrder.Reject";
    public const string Issue = "PurchaseOrder.Issue";
    public const string Cancel = "PurchaseOrder.Cancel";
    public const string ManageItems = "PurchaseOrder.ManageItems";
    public const string ManageDocuments = "PurchaseOrder.ManageDocuments";
    public const string ReportView = "PurchaseOrder.ReportView";
    public const string ReportExport = "PurchaseOrder.ReportExport";

    public static bool IsEditable(PurchaseOrderStatus status) =>
        status is PurchaseOrderStatus.Draft or PurchaseOrderStatus.UnderReview or PurchaseOrderStatus.WaitingForApproval or PurchaseOrderStatus.Approved;

    public static bool CanSubmit(PurchaseOrderStatus status, Func<string, bool> hasPermission) =>
        status == PurchaseOrderStatus.Draft && hasPermission(Submit);
    public static bool CanApprove(PurchaseOrderStatus status, Func<string, bool> hasPermission) =>
        status is PurchaseOrderStatus.UnderReview or PurchaseOrderStatus.WaitingForApproval && hasPermission(Approve);
    public static bool CanReject(PurchaseOrderStatus status, Func<string, bool> hasPermission) =>
        status is PurchaseOrderStatus.UnderReview or PurchaseOrderStatus.WaitingForApproval && hasPermission(Reject);
    public static bool CanIssue(PurchaseOrderStatus status, Func<string, bool> hasPermission) =>
        status == PurchaseOrderStatus.Approved && hasPermission(Issue);
    public static bool CanCancel(PurchaseOrderStatus status, Func<string, bool> hasPermission) =>
        status is not PurchaseOrderStatus.Completed and not PurchaseOrderStatus.Archived and not PurchaseOrderStatus.Cancelled
        && hasPermission(Cancel);

    public static string StatusText(PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.Draft => "پیش‌نویس",
        PurchaseOrderStatus.UnderReview => "در حال بررسی",
        PurchaseOrderStatus.WaitingForApproval => "در انتظار تأیید",
        PurchaseOrderStatus.Approved => "تأییدشده",
        PurchaseOrderStatus.Issued => "صادرشده",
        PurchaseOrderStatus.PartiallyReceived => "دریافت جزئی",
        PurchaseOrderStatus.FullyReceived => "دریافت کامل",
        PurchaseOrderStatus.Completed => "تکمیل‌شده",
        PurchaseOrderStatus.Cancelled => "لغوشده",
        PurchaseOrderStatus.Archived => "بایگانی‌شده",
        _ => status.ToString()
    };

    public static string TypeText(PurchaseOrderType type) => type switch
    {
        PurchaseOrderType.ContractBased => "مبتنی بر قرارداد",
        PurchaseOrderType.DirectPurchase => "خرید مستقیم",
        PurchaseOrderType.TenderBased => "مبتنی بر مناقصه",
        PurchaseOrderType.Service => "خدمات",
        PurchaseOrderType.Other => "سایر",
        _ => type.ToString()
    };

    public static Color StatusColor(PurchaseOrderStatus status) => status switch
    {
        PurchaseOrderStatus.UnderReview or PurchaseOrderStatus.WaitingForApproval => Color.Info,
        PurchaseOrderStatus.Approved => Color.Primary,
        PurchaseOrderStatus.Issued => Color.Success,
        PurchaseOrderStatus.PartiallyReceived => Color.Warning,
        PurchaseOrderStatus.FullyReceived or PurchaseOrderStatus.Completed => Color.Success,
        PurchaseOrderStatus.Cancelled => Color.Error,
        PurchaseOrderStatus.Archived => Color.Secondary,
        _ => Color.Default
    };
}
