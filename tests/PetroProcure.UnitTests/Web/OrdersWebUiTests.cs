using Microsoft.AspNetCore.Components;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class OrdersWebUiTests
{
    [Fact]
    public void Orders_dashboard_page_is_registered()
    {
        var type = typeof(OrdersUiPolicy).Assembly.GetType("PetroProcure.Web.Components.Pages.Orders.OrdersDashboard");
        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            route => route.Template == "/orders");
    }

    [Fact]
    public void Material_need_create_page_is_registered()
    {
        var type = typeof(OrdersUiPolicy).Assembly.GetType("PetroProcure.Web.Components.Pages.Orders.MaterialNeedCreate");
        Assert.NotNull(type);
    }

    [Fact]
    public void Shortage_alerts_page_is_registered()
    {
        var type = typeof(OrdersUiPolicy).Assembly.GetType("PetroProcure.Web.Components.Pages.Orders.ShortageAlertsPage");
        Assert.NotNull(type);
    }

    [Fact]
    public void Orders_permission_names_are_available()
    {
        Assert.Equal("Orders.ViewDashboard", OrdersUiPolicy.ViewDashboard);
        Assert.Equal("Orders.ConvertNeedToIndent", OrdersUiPolicy.ConvertNeedToIndent);
    }
}
