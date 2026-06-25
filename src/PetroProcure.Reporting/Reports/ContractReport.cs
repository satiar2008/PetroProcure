using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class ContractReport : PersianReportBase
{
    public ContractReport(ContractReportData data) : base($"قرارداد خرید {data.ContractNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = AddPairs(detail,
        [
            ("شماره قرارداد", data.ContractNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("تأمین‌کننده", data.SupplierName),
            ("شماره مناقصه", data.TenderNumber),
            ("ارجاع تصمیم کمیسیون", data.CommissionDecisionReference),
            ("عنوان", data.Title),
            ("موضوع", data.Subject),
            ("نوع قرارداد", data.ContractType),
            ("وضعیت", data.Status),
            ("مبلغ کل", data.TotalAmount),
            ("مالیات", data.TaxAmount),
            ("مبلغ نهایی", data.FinalAmount),
            ("ارز", data.Currency),
            ("تاریخ شروع", data.StartDate),
            ("تاریخ پایان", data.EndDate),
            ("مهلت تحویل", data.DeliveryDeadline),
            ("شرایط پرداخت", data.PaymentTerms),
            ("شرایط تحویل", data.DeliveryTerms),
            ("گارانتی", data.WarrantyTerms),
            ("جرائم", data.PenaltyTerms)
        ], 0);

        AddGroups(detail, data.Groups, top + 12);
        top = detail.HeightF + 10;

        detail.Controls.Add(Label("بندهای قرارداد", 0, top, 747, 26, true, TextAlignment.MiddleCenter));
        top += 26;
        foreach (var clause in data.Clauses.OrderBy(x => x.OrderNo))
        {
            detail.Controls.Add(Label($"{clause.OrderNo}. {clause.Title} ({clause.ClauseType}){(clause.IsRequired ? " - الزامی" : "")}",
                0, top, 747, 25, true));
            top += 25;
            detail.Controls.Add(Label(clause.Body, 0, top, 747, 45));
            top += 45;
        }

        top += 20;
        detail.Controls.Add(Label("امضای نماینده کارفرما", 500, top, 247, 60, true, TextAlignment.MiddleCenter));
        detail.Controls.Add(Label("امضای تأمین‌کننده", 250, top, 247, 60, true, TextAlignment.MiddleCenter));
        detail.Controls.Add(Label("تأیید نهایی", 0, top, 247, 60, true, TextAlignment.MiddleCenter));
        detail.HeightF = top + 70;
    }

    private static float AddPairs(DetailBand detail, IReadOnlyList<(string Caption, string Value)> pairs, float top)
    {
        foreach (var (caption, value) in pairs)
        {
            detail.Controls.Add(Label(caption, 547, top, 200, 27, true));
            detail.Controls.Add(Label(value, 0, top, 547, 27));
            top += 27;
        }
        detail.HeightF = top;
        return top;
    }
}
