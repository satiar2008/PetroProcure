using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class TenderUiPolicy
{
    public const string View = "Tender.View";
    public const string Create = "Tender.Create";
    public const string Edit = "Tender.Edit";
    public const string Publish = "Tender.Publish";
    public const string Cancel = "Tender.Cancel";
    public const string ReceiveBid = "Tender.ReceiveBid";
    public const string Evaluate = "Tender.Evaluate";
    public const string CompareBids = "Tender.CompareBids";
    public const string SelectWinner = "Tender.SelectWinner";
    public const string Close = "Tender.Close";

    public static bool CanPublish(TenderStatus status, int itemCount, int participantCount, Func<string, bool> has) =>
        has(Publish) && status is TenderStatus.Draft or TenderStatus.ReadyToPublish && itemCount > 0 && participantCount > 0;
    public static bool CanCancel(TenderStatus status, Func<string, bool> has) =>
        has(Cancel) && status is not TenderStatus.Closed and not TenderStatus.Cancelled;
    public static bool CanClose(TenderStatus status, Func<string, bool> has) =>
        has(Close) && status is TenderStatus.WinnerSelected or TenderStatus.UnderFinalReview;

    public static string StatusText(TenderStatus status) => status switch
    {
        TenderStatus.Draft => "پیش‌نویس",
        TenderStatus.ReadyToPublish => "آماده انتشار",
        TenderStatus.Published => "منتشر شده",
        TenderStatus.ReceivingBids => "دریافت پیشنهادها",
        TenderStatus.UnderQualification => "ارزیابی صلاحیت",
        TenderStatus.UnderTechnicalEvaluation => "ارزیابی فنی",
        TenderStatus.UnderCommercialEvaluation => "ارزیابی مالی",
        TenderStatus.UnderFinalReview => "بررسی نهایی",
        TenderStatus.WinnerSelected => "برنده انتخاب شده",
        TenderStatus.Closed => "بسته شده",
        TenderStatus.Cancelled => "لغو شده",
        _ => status.ToString()
    };

    public static string TypeText(TenderType type) => type switch
    {
        TenderType.PublicTender => "مناقصه عمومی",
        TenderType.LimitedTender => "مناقصه محدود",
        TenderType.TwoStageTender => "مناقصه دو مرحله‌ای",
        TenderType.SingleStageTender => "مناقصه یک مرحله‌ای",
        TenderType.NegotiatedTender => "مذاکره‌ای",
        _ => type.ToString()
    };
}
