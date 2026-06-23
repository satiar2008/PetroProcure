using Microsoft.AspNetCore.Components;
using PetroProcure.Contracts.V1.Identity;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class AdminWebUiTests
{
    [Fact]
    public void Admin_menu_is_visible_only_with_admin_permission()
    {
        var items = new[]
        {
            new NavigationAccess("مدیریت سامانه", "/admin", "icon", null, AdminUiPolicy.ManageUsers)
        };

        Assert.NotEmpty(NavigationAccessPolicy.VisibleShared(items, permission => permission == AdminUiPolicy.ManageUsers));
        Assert.Empty(NavigationAccessPolicy.VisibleShared(items, _ => false));
    }

    [Fact]
    public void Users_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Admin.AdminUsers", "/admin/users");

    [Fact]
    public void Roles_page_is_registered() =>
        AssertRoute("PetroProcure.Web.Components.Pages.Admin.AdminRoles", "/admin/roles");

    [Fact]
    public void Permissions_are_grouped_by_module()
    {
        var groups = AdminUiPolicy.GroupPermissions([
            new PermissionDto(Guid.NewGuid(), "PurchaseFile.View", "view", true),
            new PermissionDto(Guid.NewGuid(), "PurchaseFile.Edit", "edit", true),
            new PermissionDto(Guid.NewGuid(), "Indent.View", "view", true)
        ]);

        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups["PurchaseFile"].Count);
    }

    [Fact]
    public void Unauthorized_admin_page_should_be_rejected_by_policy()
    {
        Assert.False(AdminUiPolicy.CanOpenAdmin(_ => false));
        Assert.True(AdminUiPolicy.CanOpenAdmin(permission => permission == AdminUiPolicy.ManageSettings));
    }

    private static void AssertRoute(string typeName, string route)
    {
        var type = typeof(AdminUiPolicy).Assembly.GetType(typeName);
        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            attribute => attribute.Template == route);
    }
}
