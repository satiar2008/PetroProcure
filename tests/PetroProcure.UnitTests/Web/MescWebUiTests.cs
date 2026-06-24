using Microsoft.AspNetCore.Components;
using PetroProcure.Contracts.V1.Mesc;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class MescWebUiTests
{
    [Fact]
    public void Groups_page_is_registered()
    {
        var type = typeof(MescUiPolicy).Assembly.GetType(
            "PetroProcure.Web.Components.Pages.Mesc.MescGroups");

        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            route => route.Template == "/orders/mesc/groups");
    }

    [Fact]
    public void Items_page_is_registered()
    {
        var type = typeof(MescUiPolicy).Assembly.GetType(
            "PetroProcure.Web.Components.Pages.Mesc.MescItems");

        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            route => route.Template == "/orders/mesc/items");
    }

    [Fact]
    public void Unauthorized_create_action_is_hidden()
    {
        Assert.False(MescUiPolicy.CanCreate(_ => false));
    }

    [Fact]
    public void Item_selector_display_contains_general_description()
    {
        var item = Item("12345601", "123456", "لوله و اتصالات");

        Assert.Contains("لوله و اتصالات", MescUiPolicy.GetItemDisplayText(item));
    }

    [Fact]
    public void Grouped_items_are_grouped_by_general_group_code()
    {
        var groups = MescUiPolicy.GroupItems([
            Item("22222201", "222222", "شیرآلات"),
            Item("11111102", "111111", "لوله"),
            Item("11111101", "111111", "لوله")
        ]);

        Assert.Equal(2, groups.Count);
        Assert.Equal("111111", groups[0].GeneralGroupCode);
        Assert.Equal(2, groups[0].Items.Count);
    }

    private static MescItemDto Item(string code, string groupCode, string generalDescription) =>
        new(Guid.NewGuid(), code, groupCode, generalDescription, "شرح اختصاصی", "عدد", Guid.NewGuid(), true);
}
