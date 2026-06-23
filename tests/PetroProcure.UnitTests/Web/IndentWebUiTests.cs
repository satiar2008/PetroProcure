using Microsoft.AspNetCore.Components;
using PetroProcure.Contracts.V1.Indents;
using PetroProcure.Domain.Enums;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class IndentWebUiTests
{
    [Fact]
    public void Indent_list_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Indents.IndentList", "/orders/indents");

    [Fact]
    public void Create_form_requires_title_department_and_item()
    {
        Assert.False(IndentUiPolicy.IsCreateFormValid("", Guid.NewGuid(), 1));
        Assert.False(IndentUiPolicy.IsCreateFormValid("عنوان", Guid.Empty, 1));
        Assert.False(IndentUiPolicy.IsCreateFormValid("عنوان", Guid.NewGuid(), 0));
        Assert.True(IndentUiPolicy.IsCreateFormValid("عنوان", Guid.NewGuid(), 1));
    }

    [Fact]
    public void Type_digit_help_explains_all_ranges()
    {
        Assert.Contains("۰، ۱، ۲", IndentUiPolicy.TypeDigitHelpText);
        Assert.Contains("۳، ۴", IndentUiPolicy.TypeDigitHelpText);
        Assert.Contains("۵ تا ۹", IndentUiPolicy.TypeDigitHelpText);
    }

    [Fact]
    public void Mesc_item_can_be_grouped_for_indent()
    {
        var groups = IndentUiPolicy.GroupItems([Item("12345601"), Item("12345602")]);
        Assert.Single(groups);
        Assert.Equal(2, groups[0].Items.Count);
    }

    [Fact]
    public void Unauthorized_approve_action_is_hidden() =>
        Assert.False(IndentUiPolicy.CanApproveOrReject(IndentStatus.Submitted, _ => false));

    [Theory]
    [InlineData(IndentStatus.Approved, true, true)]
    [InlineData(IndentStatus.Draft, true, false)]
    [InlineData(IndentStatus.Approved, false, false)]
    public void Send_to_purchase_requires_status_and_permission(IndentStatus status, bool permission, bool expected) =>
        Assert.Equal(expected, IndentUiPolicy.CanSendToPurchase(status, _ => permission));

    [Fact]
    public void Indent_number_is_formatted_for_display() =>
        Assert.Equal("26-3-0001", IndentUiPolicy.FormatNumber("2630001"));

    private static void AssertRoute(string typeName, string route)
    {
        var type = typeof(IndentUiPolicy).Assembly.GetType(typeName);
        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            attribute => attribute.Template == route);
    }

    private static IndentItemDto Item(string code) => new(
        Guid.NewGuid(), Guid.NewGuid(), code, "123456", "گروه عمومی",
        "شرح اختصاصی", Guid.NewGuid(), 1, null, null);
}
