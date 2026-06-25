using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class PurchaseOrderReport : PersianReportBase
{
    public PurchaseOrderReport(PurchaseOrderReportData data) : base($"سفارش خرید {data.PurchaseOrderNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = AddPairs(detail,
        [
            ("شماره سفارش خرید", data.PurchaseOrderNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("شماره قرارداد", data.ContractNumber),
            ("تأمین‌کننده", data.SupplierName),
            ("کد تأمین‌کننده", data.SupplierCode),
            ("عنوان", data.Title),
            ("نوع سفارش", data.PurchaseOrderType),
            ("وضعیت", data.Status),
            ("تاریخ سفارش", data.OrderDate),
            ("موعد تحویل", data.ExpectedDeliveryDate),
            ("محل تحویل", data.DeliveryLocation),
            ("شرایط تحویل", data.DeliveryTerms),
            ("شرایط پرداخت", data.PaymentTerms),
            ("گارانتی", data.WarrantyTerms),
            ("مبلغ کل", data.TotalAmount),
            ("مالیات", data.TaxAmount),
            ("تخفیف", data.DiscountAmount),
            ("مبلغ نهایی", data.FinalAmount),
            ("ارز", data.Currency),
            ("یادداشت", data.Notes)
        ], 0);

        AddGroups(detail, data.Groups, top + 12);
        top = detail.HeightF + 20;

        detail.Controls.Add(Label("محل امضا و تأیید", 0, top, 747, 28, true, TextAlignment.MiddleCenter));
        top += 35;
        detail.Controls.Add(Label("واحد خرید", 500, top, 247, 70, true, TextAlignment.MiddleCenter));
        detail.Controls.Add(Label("تأمین‌کننده", 250, top, 247, 70, true, TextAlignment.MiddleCenter));
        detail.Controls.Add(Label("تأیید نهایی", 0, top, 247, 70, true, TextAlignment.MiddleCenter));
        detail.HeightF = top + 80;
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
