using System.Drawing;
using System.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace PetroProcure.Reporting.Reports;

public abstract class PersianReportBase : XtraReport
{
    protected static readonly Font HeaderFont = new("Tahoma", 15, FontStyle.Bold);
    protected static readonly Font SectionFont = new("Tahoma", 10, FontStyle.Bold);
    protected static readonly Font BodyFont = new("Tahoma", 8.5f);

    protected PersianReportBase(string title)
    {
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = RightToLeftLayout.Yes;
        Margins = new Margins(40, 40, 45, 45);
        PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
        var header = new ReportHeaderBand { HeightF = 65 };
        header.Controls.Add(new XRLabel { Text = title, BoundsF = new RectangleF(0, 5, 747, 35), Font = HeaderFont, TextAlignment = TextAlignment.MiddleCenter });
        header.Controls.Add(new XRLabel { Text = "سامانه یکپارچه تدارکات پتروپروکیور", BoundsF = new RectangleF(0, 40, 747, 20), Font = BodyFont, ForeColor = Color.DimGray, TextAlignment = TextAlignment.MiddleCenter });
        Bands.Add(header);
        Bands.Add(new DetailBand());
        var footer = new PageFooterBand { HeightF = 25 };
        footer.Controls.Add(new XRPageInfo { PageInfo = PageInfo.NumberOfTotal, TextFormatString = "صفحه {0} از {1}", BoundsF = new RectangleF(0, 0, 747, 20), Font = BodyFont, TextAlignment = TextAlignment.MiddleCenter });
        Bands.Add(footer);
    }

    protected static XRLabel Label(string text, float x, float y, float width, float height, bool bold = false, TextAlignment alignment = TextAlignment.MiddleRight) =>
        new() { Text = text, BoundsF = new RectangleF(x, y, width, height), Font = bold ? SectionFont : BodyFont, TextAlignment = alignment, Borders = BorderSide.All, Padding = new PaddingInfo(4, 4, 2, 2) };

    protected static void AddGroups(DetailBand detail, IReadOnlyList<ReportItemGroupData> groups, float top)
    {
        foreach (var group in groups)
        {
            detail.Controls.Add(Label($"گروه MESC: {group.GeneralGroupCode} — {group.GeneralDescription}", 0, top, 747, 26, true));
            top += 26;
            var headers = new[] { ("کد MESC",125f),("کد عمومی",85f),("شرح عمومی",175f),("شرح اختصاصی",210f),("واحد",70f),("مقدار",82f) };
            var x = 0f;
            foreach (var (text,width) in headers) { detail.Controls.Add(Label(text,x,top,width,24,true,TextAlignment.MiddleCenter)); x += width; }
            top += 24;
            foreach (var item in group.Items)
            {
                x = 0;
                foreach (var (text,width) in new[] {(item.MescCode,125f),(item.GeneralGroupCode,85f),(item.GeneralDescription,175f),(item.SpecificDescription,210f),(item.Unit,70f),(item.Quantity.ToString("0.####"),82f)})
                { detail.Controls.Add(Label(text,x,top,width,25,false,TextAlignment.MiddleCenter)); x += width; }
                top += 25;
            }
            top += 12;
        }
        detail.HeightF = top;
    }
}
