using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class TenderSummaryReport : PersianReportBase
{
    public TenderSummaryReport(TenderReportData data) : base($"خلاصه مناقصه {data.TenderNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = ReportLayout.AddPairs(detail,
        [
            ("شماره مناقصه", data.TenderNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("عنوان پرونده خرید", data.PurchaseFileTitle),
            ("استعلام مبنا", data.SourceInquiryNumber ?? "—"),
            ("عنوان مناقصه", data.Title),
            ("نوع", data.TenderType),
            ("وضعیت", data.Status),
            ("تاریخ صدور", data.IssueDate),
            ("مهلت ارسال", data.SubmissionDeadline),
            ("تاریخ گشایش", data.OpeningDate),
            ("ایجادکننده", data.CreatedBy),
            ("منتشرکننده", data.PublishedBy)
        ], 0);
        top = ReportLayout.AddTable(detail, "شرکت‌کنندگان", ["کد", "نام تأمین‌کننده", "وضعیت", "دعوت"], data.Participants.Select(x =>
            new[] { x.SupplierCode, x.SupplierName, x.Status, x.InvitedAt }), top + 12);
        AddGroups(detail, data.Groups, top + 12);
    }
}

public sealed class TenderComparisonReport : PersianReportBase
{
    public TenderComparisonReport(TenderComparisonReportData data) : base($"گزارش مقایسه مناقصه {data.TenderNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = ReportLayout.AddPairs(detail,
        [
            ("شماره مناقصه", data.TenderNumber),
            ("پرونده خرید", $"{data.PurchaseFileNumber} — {data.PurchaseFileTitle}"),
            ("برنده منتخب", data.SelectedWinner)
        ], 0);
        top = ReportLayout.AddTable(detail, "خلاصه پیشنهادها",
            ["تأمین‌کننده", "پیشنهاد", "فنی", "مالی", "نهایی", "مبلغ کل", "مبلغ نهایی", "تحویل", "پرداخت"],
            data.Suppliers.Select(x => new[] { x.SupplierName, x.BidNumber, x.TechnicalScore, x.CommercialScore,
                x.FinalScore, x.TotalAmount, x.FinalAmount, x.DeliveryTerms, x.PaymentTerms }), top + 12);
        foreach (var group in data.Groups)
        {
            detail.Controls.Add(Label($"گروه MESC: {group.GeneralGroupCode} — {group.GeneralDescription}", 0, top + 12, 747, 26, true));
            top += 42;
            foreach (var item in group.Items)
            {
                detail.Controls.Add(Label($"{item.MescCode} — {item.SpecificDescription} — مقدار: {item.Quantity:0.###}", 0, top, 747, 24, true));
                top += 24;
                top = ReportLayout.AddTable(detail, "مقایسه اقلام پیشنهادها",
                    ["تأمین‌کننده", "قیمت واحد", "قیمت کل", "تطابق فنی", "یادداشت"],
                    item.SupplierItems.Select(x => new[] { x.SupplierName, x.UnitPrice, x.TotalPrice, x.TechnicalComplianceStatus, x.TechnicalNote }), top);
            }
        }
        detail.HeightF = top + 20;
    }
}

public sealed class TenderWinnerDecisionReport : PersianReportBase
{
    public TenderWinnerDecisionReport(TenderWinnerDecisionReportData data) : base($"گزارش تصمیم برنده {data.TenderNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        detail.HeightF = ReportLayout.AddPairs(detail,
        [
            ("شماره مناقصه", data.TenderNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("تأمین‌کننده منتخب", data.SelectedSupplier),
            ("پیشنهاد منتخب", data.SelectedBid),
            ("تاریخ تصمیم", data.DecisionDate),
            ("علت تصمیم", data.DecisionReason),
            ("ارجاع تصمیم کمیسیون", data.CommissionDecisionReference),
            ("مبلغ نهایی", data.FinalAmount),
            ("یادداشت", data.Notes)
        ], 0) + 20;
    }
}

public sealed class CommissionSessionMinutesReport : PersianReportBase
{
    public CommissionSessionMinutesReport(CommissionSessionReportData data) : base($"صورتجلسه کمیسیون {data.SessionNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = ReportLayout.AddPairs(detail,
        [
            ("شماره جلسه", data.SessionNumber),
            ("شماره مناقصه", data.TenderNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("عنوان جلسه", data.Title),
            ("تاریخ جلسه", data.SessionDate),
            ("محل", data.Location),
            ("وضعیت", data.Status),
            ("رئیس کمیسیون", data.Chairperson),
            ("دبیر", data.Secretary)
        ], 0);
        top = ReportLayout.AddTable(detail, "اعضا", ["نام", "نقش", "حضور", "رأی"], data.Members.Select(x =>
            new[] { x.FullName, x.Role, x.AttendanceStatus, x.VoteStatus }), top + 12);
        top = ReportLayout.AddTable(detail, "دستور جلسه", ["ردیف", "عنوان", "شرح", "وضعیت", "یادداشت"], data.AgendaItems.Select(x =>
            new[] { x.OrderNo.ToString(), x.Title, x.Description, x.Status, x.Notes }), top + 12);
        top = ReportLayout.AddTable(detail, "صورتجلسه", ["دستور", "متن", "تاریخ"], data.Minutes.Select(x =>
            new[] { x.AgendaTitle, x.Text, x.CreatedAt }), top + 12);
        top = ReportLayout.AddTable(detail, "تصمیمات", ["نوع", "وضعیت", "تأمین‌کننده", "پیشنهاد", "متن", "علت"], data.Decisions.Select(x =>
            new[] { x.DecisionType, x.Status, x.SelectedSupplier, x.SelectedBid, x.DecisionText, x.Reason }), top + 12);
        detail.HeightF = top + 20;
    }
}

public sealed class CommissionDecisionReport : PersianReportBase
{
    public CommissionDecisionReport(CommissionDecisionReportData data) : base($"گزارش تصمیم کمیسیون {data.SessionNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        detail.HeightF = ReportLayout.AddPairs(detail,
        [
            ("شماره جلسه", data.SessionNumber),
            ("شماره مناقصه", data.TenderNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("نوع تصمیم", data.DecisionType),
            ("وضعیت", data.Status),
            ("تأمین‌کننده منتخب", data.SelectedSupplier),
            ("پیشنهاد منتخب", data.SelectedBid),
            ("متن تصمیم", data.DecisionText),
            ("علت", data.Reason),
            ("ایجادکننده", data.CreatedBy),
            ("تأییدکننده", data.ApprovedBy),
            ("تاریخ تأیید", data.ApprovedAt)
        ], 0) + 20;
    }
}

file static class ReportLayout
{
    public static float AddPairs(DetailBand detail, IReadOnlyList<(string Caption, string Value)> pairs, float top)
    {
        foreach (var (caption, value) in pairs)
        {
            detail.Controls.Add(PersianReportBase.Label(caption, 547, top, 200, 27, true));
            detail.Controls.Add(PersianReportBase.Label(value, 0, top, 547, 27));
            top += 27;
        }
        return top;
    }

    public static float AddTable(DetailBand detail, string title, IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string>> rows, float top)
    {
        detail.Controls.Add(PersianReportBase.Label(title, 0, top, 747, 25, true, TextAlignment.MiddleCenter));
        top += 25;
        var width = 747f / headers.Count;
        var x = 0f;
        foreach (var header in headers)
        {
            detail.Controls.Add(PersianReportBase.Label(header, x, top, width, 24, true, TextAlignment.MiddleCenter));
            x += width;
        }
        top += 24;
        foreach (var row in rows)
        {
            x = 0f;
            foreach (var cell in row)
            {
                detail.Controls.Add(PersianReportBase.Label(cell, x, top, width, 25, false, TextAlignment.MiddleCenter));
                x += width;
            }
            top += 25;
        }
        return top + 8;
    }
}
