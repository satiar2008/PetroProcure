using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class ContractUiPolicy
{
    public const string View = "Contract.View";
    public const string Create = "Contract.Create";
    public const string Edit = "Contract.Edit";
    public const string Submit = "Contract.Submit";
    public const string Approve = "Contract.Approve";
    public const string Reject = "Contract.Reject";
    public const string Sign = "Contract.Sign";
    public const string Cancel = "Contract.Cancel";
    public const string ManageClauses = "Contract.ManageClauses";
    public const string ManageTemplates = "Contract.ManageTemplates";
    public const string ManageDocuments = "Contract.ManageDocuments";
    public const string ReportView = "Contract.ReportView";
    public const string ReportExport = "Contract.ReportExport";

    public static bool IsEditable(ContractStatus status) =>
        status is ContractStatus.Draft or ContractStatus.UnderReview or ContractStatus.WaitingForApproval;
    public static bool CanSubmit(ContractStatus status, Func<string, bool> has) =>
        status == ContractStatus.Draft && has(Submit);
    public static bool CanApprove(ContractStatus status, Func<string, bool> has) =>
        status is ContractStatus.UnderReview or ContractStatus.WaitingForApproval && has(Approve);
    public static bool CanReject(ContractStatus status, Func<string, bool> has) =>
        status is ContractStatus.UnderReview or ContractStatus.WaitingForApproval && has(Reject);
    public static bool CanSign(ContractStatus status, Func<string, bool> has) =>
        status == ContractStatus.Approved && has(Sign);
    public static string StatusText(ContractStatus status) => status switch
    {
        ContractStatus.Draft => "پیش‌نویس",
        ContractStatus.UnderReview => "در حال بررسی",
        ContractStatus.WaitingForApproval => "در انتظار تأیید",
        ContractStatus.Approved => "تأییدشده",
        ContractStatus.Signed => "امضاشده",
        ContractStatus.Active => "فعال",
        ContractStatus.Completed => "تکمیل‌شده",
        ContractStatus.Cancelled => "لغوشده",
        ContractStatus.Archived => "بایگانی‌شده",
        _ => status.ToString()
    };
    public static string TypeText(ContractType type) => type switch
    {
        ContractType.DirectPurchase => "خرید مستقیم",
        ContractType.TenderBased => "مبتنی بر مناقصه",
        ContractType.Service => "خدمات",
        ContractType.Framework => "چارچوب",
        ContractType.Other => "سایر",
        _ => type.ToString()
    };
    public static string ClauseTypeText(ContractClauseType type) => type switch
    {
        ContractClauseType.General => "عمومی",
        ContractClauseType.Technical => "فنی",
        ContractClauseType.Commercial => "بازرگانی",
        ContractClauseType.Payment => "پرداخت",
        ContractClauseType.Delivery => "تحویل",
        ContractClauseType.Warranty => "گارانتی",
        ContractClauseType.Penalty => "جریمه",
        ContractClauseType.Legal => "حقوقی",
        ContractClauseType.AttachmentReference => "ارجاع پیوست",
        ContractClauseType.Other => "سایر",
        _ => type.ToString()
    };
}
