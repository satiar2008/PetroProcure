using MudBlazor;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class CommissionUiPolicy
{
    public const string View = "Commission.View";
    public const string Create = "Commission.Create";
    public const string Edit = "Commission.Edit";
    public const string Schedule = "Commission.Schedule";
    public const string Start = "Commission.Start";
    public const string Complete = "Commission.Complete";
    public const string Approve = "Commission.Approve";
    public const string Cancel = "Commission.Cancel";
    public const string ManageMembers = "Commission.ManageMembers";
    public const string ManageAgenda = "Commission.ManageAgenda";
    public const string ManageMinutes = "Commission.ManageMinutes";
    public const string ManageDecisions = "Commission.ManageDecisions";
    public const string ManageDocuments = "Commission.ManageDocuments";
    public const string ReportView = "Commission.ReportView";
    public const string ReportExport = "Commission.ReportExport";

    public static bool CanCreate(Func<string, bool> has) => has(Create);
    public static bool CanEdit(TenderCommissionSessionStatus status, Func<string, bool> has) =>
        has(Edit) && status is not TenderCommissionSessionStatus.Approved and not TenderCommissionSessionStatus.Cancelled;
    public static bool CanStart(TenderCommissionSessionStatus status, Func<string, bool> has) =>
        has(Start) && status is TenderCommissionSessionStatus.Draft or TenderCommissionSessionStatus.Scheduled;
    public static bool CanComplete(TenderCommissionSessionStatus status, Func<string, bool> has) =>
        has(Complete) && status == TenderCommissionSessionStatus.InProgress;
    public static bool CanApprove(TenderCommissionSessionStatus status, Func<string, bool> has) =>
        has(Approve) && status == TenderCommissionSessionStatus.Completed;
    public static bool CanCancel(TenderCommissionSessionStatus status, Func<string, bool> has) =>
        has(Cancel) && status is not TenderCommissionSessionStatus.Approved and not TenderCommissionSessionStatus.Cancelled;

    public static string StatusText(TenderCommissionSessionStatus status) => status switch
    {
        TenderCommissionSessionStatus.Draft => "پیش‌نویس",
        TenderCommissionSessionStatus.Scheduled => "زمان‌بندی‌شده",
        TenderCommissionSessionStatus.InProgress => "در حال برگزاری",
        TenderCommissionSessionStatus.Completed => "تکمیل‌شده",
        TenderCommissionSessionStatus.Approved => "تأییدشده",
        TenderCommissionSessionStatus.Cancelled => "لغوشده",
        _ => status.ToString()
    };

    public static Color StatusColor(TenderCommissionSessionStatus status) => status switch
    {
        TenderCommissionSessionStatus.Draft => Color.Default,
        TenderCommissionSessionStatus.Scheduled => Color.Info,
        TenderCommissionSessionStatus.InProgress => Color.Warning,
        TenderCommissionSessionStatus.Completed => Color.Primary,
        TenderCommissionSessionStatus.Approved => Color.Success,
        TenderCommissionSessionStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    public static string RoleText(TenderCommissionMemberRole role) => role switch
    {
        TenderCommissionMemberRole.Chairperson => "رئیس کمیسیون",
        TenderCommissionMemberRole.Secretary => "دبیر",
        TenderCommissionMemberRole.Member => "عضو",
        TenderCommissionMemberRole.Observer => "ناظر",
        TenderCommissionMemberRole.TechnicalExpert => "کارشناس فنی",
        TenderCommissionMemberRole.FinancialExpert => "کارشناس مالی",
        _ => role.ToString()
    };

    public static string AttendanceText(TenderCommissionAttendanceStatus status) => status switch
    {
        TenderCommissionAttendanceStatus.Invited => "دعوت‌شده",
        TenderCommissionAttendanceStatus.Present => "حاضر",
        TenderCommissionAttendanceStatus.Absent => "غایب",
        TenderCommissionAttendanceStatus.Excused => "غیبت موجه",
        _ => status.ToString()
    };

    public static string AgendaStatusText(TenderCommissionAgendaStatus status) => status switch
    {
        TenderCommissionAgendaStatus.Pending => "در انتظار طرح",
        TenderCommissionAgendaStatus.Discussed => "بررسی‌شده",
        TenderCommissionAgendaStatus.Deferred => "موکول‌شده",
        TenderCommissionAgendaStatus.Closed => "بسته‌شده",
        _ => status.ToString()
    };

    public static string DecisionTypeText(TenderCommissionDecisionType type) => type switch
    {
        TenderCommissionDecisionType.RecommendWinner => "پیشنهاد برنده",
        TenderCommissionDecisionType.ApproveWinner => "تأیید برنده",
        TenderCommissionDecisionType.RejectAll => "رد همه پیشنهادها",
        TenderCommissionDecisionType.Retender => "تجدید مناقصه",
        TenderCommissionDecisionType.RequestTechnicalReview => "درخواست بررسی فنی",
        TenderCommissionDecisionType.RequestCommercialReview => "درخواست بررسی مالی",
        TenderCommissionDecisionType.CancelTender => "لغو مناقصه",
        TenderCommissionDecisionType.Other => "سایر",
        _ => type.ToString()
    };

    public static string DecisionStatusText(TenderCommissionDecisionStatus status) => status switch
    {
        TenderCommissionDecisionStatus.Draft => "پیش‌نویس",
        TenderCommissionDecisionStatus.Approved => "تأییدشده",
        TenderCommissionDecisionStatus.Rejected => "ردشده",
        TenderCommissionDecisionStatus.Cancelled => "لغوشده",
        _ => status.ToString()
    };

    public static IReadOnlyList<CommissionDecisionGroup> GroupDecisions(IReadOnlyList<TenderCommissionDecisionDto> decisions) =>
        decisions
            .GroupBy(x => x.DecisionType)
            .OrderBy(x => x.Key)
            .Select(x => new CommissionDecisionGroup(x.Key, DecisionTypeText(x.Key), x.ToList()))
            .ToList();
}

public sealed record CommissionDecisionGroup(TenderCommissionDecisionType DecisionType, string Title,
    IReadOnlyList<TenderCommissionDecisionDto> Decisions);
