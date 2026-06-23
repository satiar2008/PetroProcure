using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class PurchaseFileSummaryReport : PersianReportBase
{
    public PurchaseFileSummaryReport(PurchaseFileReportData data) : base("خلاصه پرونده خرید")
    {
        var detail = (DetailBand)Bands[BandKind.Detail];
        var fields = new[] { ("شماره پرونده",data.FileNumber),("عنوان",data.Title),("وضعیت",data.Status),("واحد جاری",data.CurrentDepartment),("تاریخ ایجاد",data.CreatedAt.ToString("yyyy/MM/dd")),("شماره درخواست مرتبط",data.IndentNumber ?? "—") };
        var top = 0f;
        foreach (var (caption,value) in fields) { detail.Controls.Add(Label(caption,547,top,200,27,true)); detail.Controls.Add(Label(value,0,top,547,27)); top += 27; }
        AddGroups(detail,data.Groups,top+15);
    }
}
