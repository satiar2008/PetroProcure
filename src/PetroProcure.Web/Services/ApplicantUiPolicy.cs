using PetroProcure.Domain.Enums;
using MudBlazor;

namespace PetroProcure.Web.Services;

public static class ApplicantUiPolicy
{
    public const string ViewDashboard = "Applicant.ViewDashboard";
    public const string ViewTechnicalReviews = "Applicant.ViewTechnicalReviews";
    public const string SubmitTechnicalReview = "Applicant.SubmitTechnicalReview";
    public const string RequestClarification = "Applicant.RequestClarification";

    public static string StatusText(PurchaseFileTechnicalReviewStatus status) => status switch
    {
        PurchaseFileTechnicalReviewStatus.Requested => "درخواست‌شده",
        PurchaseFileTechnicalReviewStatus.InReview => "در حال بررسی",
        PurchaseFileTechnicalReviewStatus.ClarificationRequested => "درخواست شفاف‌سازی",
        PurchaseFileTechnicalReviewStatus.Approved => "تأیید فنی",
        PurchaseFileTechnicalReviewStatus.Rejected => "رد فنی",
        PurchaseFileTechnicalReviewStatus.Cancelled => "لغو شده",
        _ => status.ToString()
    };

    public static Color StatusColor(PurchaseFileTechnicalReviewStatus status) => status switch
    {
        PurchaseFileTechnicalReviewStatus.Approved => Color.Success,
        PurchaseFileTechnicalReviewStatus.Rejected or PurchaseFileTechnicalReviewStatus.Cancelled => Color.Error,
        PurchaseFileTechnicalReviewStatus.ClarificationRequested => Color.Warning,
        PurchaseFileTechnicalReviewStatus.InReview => Color.Info,
        _ => Color.Primary
    };

    public static string DecisionText(PurchaseFileTechnicalReviewDecision? decision) => decision switch
    {
        PurchaseFileTechnicalReviewDecision.TechnicallyAccepted => "تأیید فنی",
        PurchaseFileTechnicalReviewDecision.TechnicallyRejected => "رد فنی",
        PurchaseFileTechnicalReviewDecision.NeedsClarification => "نیازمند شفاف‌سازی",
        PurchaseFileTechnicalReviewDecision.ConditionalAcceptance => "تأیید مشروط",
        null => "ثبت نشده",
        _ => decision.ToString() ?? "ثبت نشده"
    };
}
