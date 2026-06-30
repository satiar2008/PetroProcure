using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class InquiryUiPolicy
{
    public const string View = "Inquiry.View";
    public const string Create = "Inquiry.Create";
    public const string Edit = "Inquiry.Edit";
    public const string Send = "Inquiry.Send";
    public const string Cancel = "Inquiry.Cancel";
    public const string ManageSuppliers = "Inquiry.ManageSuppliers";
    public const string ReceiveQuote = "Inquiry.ReceiveQuote";
    public const string CompareQuotes = "Inquiry.CompareQuotes";
    public const string SelectSupplier = "Inquiry.SelectSupplier";

    public static bool CanSend(InquiryStatus status, Func<string, bool> hasPermission) =>
        hasPermission(Send) && status is InquiryStatus.Draft or InquiryStatus.ReadyToSend;

    public static bool CanManageSuppliers(InquiryStatus status, Func<string, bool> hasPermission) =>
        hasPermission(ManageSuppliers) && status is InquiryStatus.Draft or InquiryStatus.ReadyToSend;

    public static bool CanReceiveQuote(InquiryStatus status, Func<string, bool> hasPermission) =>
        hasPermission(ReceiveQuote)
        && status is InquiryStatus.Sent or InquiryStatus.PartiallyResponded or InquiryStatus.FullyResponded;

    public static bool CanSelectSupplier(InquiryStatus status, Func<string, bool> hasPermission) =>
        hasPermission(SelectSupplier)
        && status is InquiryStatus.FullyResponded or InquiryStatus.UnderComparison;

    public static string StatusText(InquiryStatus s) => s switch
    {
        InquiryStatus.Draft => "پیش‌نویس",
        InquiryStatus.ReadyToSend => "آماده ارسال",
        InquiryStatus.Sent => "ارسال‌شده",
        InquiryStatus.PartiallyResponded => "پاسخ ناقص",
        InquiryStatus.FullyResponded => "پاسخ کامل",
        InquiryStatus.UnderComparison => "در حال مقایسه",
        InquiryStatus.SupplierSelected => "تأمین‌کننده انتخاب شد",
        InquiryStatus.Closed => "بسته‌شده",
        InquiryStatus.Cancelled => "لغوشده",
        _ => s.ToString()
    };
    public static MudBlazor.Color StatusColor(InquiryStatus s) => s switch
    {
        InquiryStatus.Draft => MudBlazor.Color.Default,
        InquiryStatus.ReadyToSend => MudBlazor.Color.Info,
        InquiryStatus.Sent => MudBlazor.Color.Info,
        InquiryStatus.PartiallyResponded => MudBlazor.Color.Warning,
        InquiryStatus.FullyResponded => MudBlazor.Color.Primary,
        InquiryStatus.UnderComparison => MudBlazor.Color.Primary,
        InquiryStatus.SupplierSelected => MudBlazor.Color.Success,
        InquiryStatus.Closed => MudBlazor.Color.Success,
        InquiryStatus.Cancelled => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default
    };

    public static string TypeText(InquiryType t) => t switch
    {
        InquiryType.PriceInquiry => "استعلام قیمت",
        InquiryType.TechnicalInquiry => "استعلام فنی",
        InquiryType.CommercialInquiry => "استعلام بازرگانی",
        InquiryType.TechnicalCommercialInquiry => "فنی/بازرگانی",
        _ => t.ToString()
    };
}
