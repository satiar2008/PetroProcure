using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class OrdersUiPolicy
{
    public const string ViewDashboard = "Orders.ViewDashboard";
    public const string ViewInventory = "Orders.ViewInventory";
    public const string ManageInventoryControl = "Orders.ManageInventoryControl";
    public const string CreateMaterialNeed = "Orders.CreateMaterialNeed";
    public const string ReviewMaterialNeed = "Orders.ReviewMaterialNeed";
    public const string ApproveMaterialNeed = "Orders.ApproveMaterialNeed";
    public const string ConvertNeedToIndent = "Orders.ConvertNeedToIndent";
    public const string ConvertShortageToIndent = "Orders.ConvertShortageToIndent";
    public const string ManageShortageAlerts = "Orders.ManageShortageAlerts";

    public static string StatusText(MaterialNeedStatus status) => status switch
    {
        MaterialNeedStatus.Draft => "پیش‌نویس",
        MaterialNeedStatus.Submitted => "ارسال‌شده",
        MaterialNeedStatus.UnderReview => "در حال بررسی",
        MaterialNeedStatus.Approved => "تأیید شده",
        MaterialNeedStatus.Rejected => "رد شده",
        MaterialNeedStatus.ConvertedToIndent => "تبدیل به تقاضا",
        MaterialNeedStatus.Cancelled => "لغو شده",
        _ => status.ToString()
    };

    public static string PriorityText(MaterialNeedPriority priority) => priority switch
    {
        MaterialNeedPriority.Low => "کم",
        MaterialNeedPriority.Normal => "عادی",
        MaterialNeedPriority.High => "زیاد",
        MaterialNeedPriority.Urgent => "فوری",
        _ => priority.ToString()
    };

    public static string AlertStatusText(ShortageAlertStatus status) => status switch
    {
        ShortageAlertStatus.Open => "باز",
        ShortageAlertStatus.InProgress => "در حال پیگیری",
        ShortageAlertStatus.ConvertedToIndent => "تبدیل به تقاضا",
        ShortageAlertStatus.Resolved => "رفع شده",
        ShortageAlertStatus.Cancelled => "لغو شده",
        _ => status.ToString()
    };
}
