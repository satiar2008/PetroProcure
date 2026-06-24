using Microsoft.AspNetCore.Components;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class InquiryWebUiTests
{
    [Fact]
    public void Inquiry_list_page_is_registered()
    {
        var type = typeof(InquiryUiPolicy).Assembly.GetType("PetroProcure.Web.Components.Pages.Inquiries.InquiryList");
        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            route => route.Template == "/purchase/inquiries");
    }

    [Fact]
    public void Inquiry_from_purchase_file_page_is_registered()
    {
        var type = typeof(InquiryUiPolicy).Assembly.GetType("PetroProcure.Web.Components.Pages.Inquiries.InquiryFromPurchaseFile");
        Assert.NotNull(type);
    }

    [Fact]
    public void Permission_names_are_available()
    {
        Assert.Equal("Inquiry.Send", InquiryUiPolicy.Send);
        Assert.Equal("Inquiry.CompareQuotes", InquiryUiPolicy.CompareQuotes);
    }
}
