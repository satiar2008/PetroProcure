using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class IndentReport : PersianReportBase
{
    public IndentReport(IndentReportData data) : base("گزارش درخواست خرید")
    {
        var detail=(DetailBand)Bands[BandKind.Detail];
        var fields=new[]{("شماره Indent",data.IndentNumber),("نوع درخواست",data.IndentType),("واحد درخواست‌کننده",data.RequestingDepartment)};
        var top=0f;
        foreach(var (caption,value) in fields){detail.Controls.Add(Label(caption,547,top,200,27,true));detail.Controls.Add(Label(value,0,top,547,27));top+=27;}
        AddGroups(detail,data.Groups,top+15);
    }
}
