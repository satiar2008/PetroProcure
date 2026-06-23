using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public sealed class PurchaseFileItemsGroupedByMescReport : PersianReportBase
{
    public PurchaseFileItemsGroupedByMescReport(PurchaseFileReportData data) : base($"اقلام پرونده خرید {data.FileNumber}") =>
        AddGroups((DetailBand)Bands[BandKind.Detail], data.Groups, 0);
}
