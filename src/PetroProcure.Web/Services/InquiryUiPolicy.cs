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
    public static string TypeText(InquiryType t) => t switch
    {
        InquiryType.PriceInquiry => "استعلام قیمت",
        InquiryType.TechnicalInquiry => "استعلام فنی",
        InquiryType.CommercialInquiry => "استعلام بازرگانی",
        InquiryType.TechnicalCommercialInquiry => "فنی/بازرگانی",
        _ => t.ToString()
    };
}
