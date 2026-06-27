using PetroProcure.Domain.Enums;
using MudBlazor;

namespace PetroProcure.Web.Services;

public static class PurchaseFileUiPolicy
{
    public static bool CanEdit(PurchaseFileStatus status, Func<string, bool> hasPermission) =>
        status is not PurchaseFileStatus.Completed and not PurchaseFileStatus.Archived
        && hasPermission("PurchaseFile.Edit");

    public static string StatusText(PurchaseFileStatus status) => status switch
    {
        PurchaseFileStatus.Draft => "پیش‌نویس",
        PurchaseFileStatus.WaitingForIndentReview => "بررسی درخواست",
        PurchaseFileStatus.WaitingForPurchaseDepartment => "در انتظار خرید",
        PurchaseFileStatus.InPurchaseDepartment => "در واحد خرید",
        PurchaseFileStatus.WaitingForTechnicalReview => "بررسی فنی",
        PurchaseFileStatus.WaitingForTenderCommission => "در انتظار کمیسیون",
        PurchaseFileStatus.InTender => "در مناقصه",
        PurchaseFileStatus.WaitingForContract => "در انتظار قرارداد",
        PurchaseFileStatus.WaitingForPurchaseOrder => "در انتظار سفارش",
        PurchaseFileStatus.WaitingForWarehouseReceipt => "در انتظار انبار",
        PurchaseFileStatus.Completed => "تکمیل‌شده",
        PurchaseFileStatus.Cancelled => "لغوشده",
        PurchaseFileStatus.Archived => "آرشیوشده",
        _ => status.ToString()
    };

    public static Color StatusColor(PurchaseFileStatus status) => status switch
    {
        PurchaseFileStatus.Completed => Color.Success,
        PurchaseFileStatus.Archived => Color.Dark,
        PurchaseFileStatus.Cancelled => Color.Error,
        PurchaseFileStatus.Draft => Color.Default,
        PurchaseFileStatus.WaitingForTechnicalReview or PurchaseFileStatus.WaitingForTenderCommission => Color.Warning,
        _ => Color.Primary
    };
}
