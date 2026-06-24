using Microsoft.AspNetCore.Components;
using PetroProcure.Contracts.V1.Suppliers;
using PetroProcure.Domain.Enums;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class SupplierWebUiTests
{
    [Fact]
    public void Supplier_list_page_is_registered()
    {
        var type = typeof(SupplierUiPolicy).Assembly.GetType(
            "PetroProcure.Web.Components.Pages.Suppliers.SupplierList");

        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            route => route.Template == "/purchase/suppliers");
    }

    [Fact]
    public void Supplier_selector_hides_blacklisted_supplier_by_default()
    {
        var supplier = new SupplierLookupDto(Guid.NewGuid(), "SUP-1", "Supplier",
            SupplierType.Distributor, SupplierStatus.Blacklisted, false, true);

        Assert.False(SupplierUiPolicy.IsSelectable(supplier));
    }

    [Fact]
    public void Permission_names_are_available_for_supplier_buttons()
    {
        Assert.Equal("Supplier.Create", SupplierUiPolicy.Create);
        Assert.Equal("Supplier.Edit", SupplierUiPolicy.Edit);
    }
}
