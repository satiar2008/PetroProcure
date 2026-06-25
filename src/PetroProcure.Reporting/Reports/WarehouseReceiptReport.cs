using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class WarehouseReceiptReport : PersianReportBase
{
    public WarehouseReceiptReport(WarehouseReceiptReportData data) : base($"رسید انبار {data.ReceiptNumber}")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var top = AddPairs(detail,
        [
            ("شماره رسید", data.ReceiptNumber),
            ("شماره سفارش خرید", data.PurchaseOrderNumber),
            ("شماره پرونده خرید", data.PurchaseFileNumber),
            ("تأمین‌کننده", data.SupplierName),
            ("انبار", data.WarehouseName),
            ("تاریخ رسید", data.ReceiptDate),
            ("شماره حواله/بارنامه", data.DeliveryNoteNumber),
            ("حمل‌کننده", data.CarrierName),
            ("شماره خودرو", data.VehicleNumber),
            ("وضعیت", data.Status)
        ], 0);

        var groups = data.Items.GroupBy(x => new { x.GeneralGroupCode, x.GeneralDescription }).OrderBy(x => x.Key.GeneralGroupCode);
        foreach (var group in groups)
        {
            detail.Controls.Add(Label($"{group.Key.GeneralGroupCode} - {group.Key.GeneralDescription}", 0, top + 10, 747, 28, true, TextAlignment.MiddleCenter));
            top += 42;
            foreach (var item in group.OrderBy(x => x.MescCode))
            {
                var text = $"{item.MescCode} | {item.SpecificDescription} | سفارش: {item.OrderedQuantity} | قبلاً دریافت: {item.PreviouslyReceivedQuantity} | رسید: {item.ReceivedQuantity} | مانده: {item.RemainingQuantityAfterReceipt} | کیفیت: {item.QualityStatus}";
                detail.Controls.Add(Label(text, 0, top, 747, 25));
                top += 25;
            }
        }

        top += 20;
        detail.Controls.Add(Label("تحویل‌دهنده", 500, top, 247, 65, true, TextAlignment.MiddleCenter));
        detail.Controls.Add(Label("تحویل‌گیرنده انبار", 250, top, 247, 65, true, TextAlignment.MiddleCenter));
        detail.Controls.Add(Label("تأییدکننده", 0, top, 247, 65, true, TextAlignment.MiddleCenter));
        detail.HeightF = top + 75;
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
