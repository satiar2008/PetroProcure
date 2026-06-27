using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class LegalComplianceReport : PersianReportBase
{
    public LegalComplianceReport(LegalComplianceReportData data) : base("گزارش انطباق حقوقی پرونده خرید")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = LegalReportLayout.AddPairs(detail,
        [
            ("شماره پرونده", data.FileNumber),
            ("عنوان پرونده", data.Title),
            ("وضعیت پرونده", data.Status),
            ("تاریخ ایجاد", data.CreatedAt.ToString("yyyy/MM/dd")),
            ("آخرین ارزیابی", data.EvaluatedAt?.ToString("yyyy/MM/dd HH:mm") ?? "—"),
            ("خلاصه ارزیابی", data.EvaluationSummary)
        ], 0);

        top = LegalReportLayout.AddPairs(detail,
        [
            ("قبول", data.Summary.PassCount.ToString()),
            ("رد", data.Summary.FailCount.ToString()),
            ("هشدار", data.Summary.WarningCount.ToString()),
            ("غیرقابل اعمال", data.Summary.NotApplicableCount.ToString()),
            ("نیازمند بازبینی انسانی", data.Summary.NeedHumanReviewCount.ToString())
        ], top + 12);

        top = LegalReportLayout.AddFindingTable(detail, "قواعد مسدودکننده ناموفق",
            data.FailedBlockingFindings, top + 14);
        top = LegalReportLayout.AddFindingTable(detail, "هشدارها",
            data.Warnings, top + 14);
        top = LegalReportLayout.AddFindingTable(detail, "موارد نیازمند بازبینی انسانی",
            data.NeedHumanReviewItems, top + 14);
        top = LegalReportLayout.AddAuditTable(detail, data.Overrides, top + 14);

        detail.HeightF = top + 20;
    }
}

file static class LegalReportLayout
{
    public static float AddPairs(DetailBand detail, IReadOnlyList<(string Label, string Value)> rows, float top)
    {
        foreach (var (label, value) in rows)
        {
            detail.Controls.Add(PersianReportBase.Label(label, 547, top, 200, 27, true));
            detail.Controls.Add(PersianReportBase.Label(value, 0, top, 547, 27));
            top += 27;
        }
        return top;
    }

    public static float AddFindingTable(DetailBand detail, string title,
        IReadOnlyList<LegalComplianceFindingReportData> findings, float top)
    {
        detail.Controls.Add(PersianReportBase.Label(title, 0, top, 747, 26, true));
        top += 26;
        if (findings.Count == 0)
        {
            detail.Controls.Add(PersianReportBase.Label("موردی ثبت نشده است.", 0, top, 747, 24));
            return top + 24;
        }

        foreach (var finding in findings)
        {
            var ai = finding.IsAiGenerated ? " / تولیدشده با هوش مصنوعی" : "";
            detail.Controls.Add(PersianReportBase.Label(
                $"{finding.RuleTitle} | نتیجه: {finding.Result} | شدت: {finding.Severity}{ai}",
                0, top, 747, 25, true));
            top += 25;
            detail.Controls.Add(PersianReportBase.Label($"مرجع: {finding.LegalReference}", 0, top, 747, 24));
            top += 24;
            detail.Controls.Add(PersianReportBase.Label($"توضیح: {finding.Explanation}", 0, top, 747, 36));
            top += 36;
            detail.Controls.Add(PersianReportBase.Label($"پیشنهاد اصلاحی: {finding.Recommendation}", 0, top, 747, 30));
            top += 30;
            detail.Controls.Add(PersianReportBase.Label($"ارجاعات: {finding.Citations}", 0, top, 747, 24));
            top += 30;
        }

        return top;
    }

    public static float AddAuditTable(DetailBand detail,
        IReadOnlyList<LegalComplianceAuditReportData> audits, float top)
    {
        detail.Controls.Add(PersianReportBase.Label("مجوزهای عبور و یادداشت‌های ممیزی", 0, top, 747, 26, true));
        top += 26;
        if (audits.Count == 0)
        {
            detail.Controls.Add(PersianReportBase.Label("موردی ثبت نشده است.", 0, top, 747, 24));
            return top + 24;
        }

        foreach (var audit in audits)
        {
            detail.Controls.Add(PersianReportBase.Label(
                $"{audit.CreatedAt} | {audit.Action} | {audit.RuleTitle} | {audit.PreviousResult} -> {audit.NewResult}",
                0, top, 747, 25, true));
            top += 25;
            detail.Controls.Add(PersianReportBase.Label($"کاربر: {audit.User} | دلیل: {audit.Reason}", 0, top, 747, 32));
            top += 36;
        }

        return top;
    }
}
