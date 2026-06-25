using Microsoft.AspNetCore.Components;
using PetroProcure.Domain.Enums;
using PetroProcure.Web.Services;

namespace PetroProcure.UnitTests.Web;

public sealed class ContractWebUiTests
{
    [Theory]
    [InlineData("PetroProcure.Web.Components.Pages.Contracts.ContractList", "/purchase/contracts")]
    [InlineData("PetroProcure.Web.Components.Pages.Contracts.ContractCreate", "/purchase/contracts/create")]
    [InlineData("PetroProcure.Web.Components.Pages.Contracts.ContractDetail", "/purchase/contracts/{Id:guid}")]
    [InlineData("PetroProcure.Web.Components.Pages.Contracts.ContractTemplates", "/purchase/contracts/templates")]
    [InlineData("PetroProcure.Web.Components.Pages.Contracts.ContractFromPurchaseFile", "/purchase/contracts/from-purchase-file/{PurchaseFileId:guid}")]
    [InlineData("PetroProcure.Web.Components.Pages.Contracts.ContractFromTender", "/purchase/contracts/from-tender/{TenderId:guid}")]
    public void Contract_pages_are_registered(string typeName, string route)
    {
        var type = typeof(ContractUiPolicy).Assembly.GetType(typeName);

        Assert.NotNull(type);
        Assert.Contains(type!.GetCustomAttributes(typeof(RouteAttribute), true).Cast<RouteAttribute>(),
            attribute => attribute.Template == route);
    }

    [Fact]
    public void Contract_permission_names_are_available_for_buttons()
    {
        Assert.Equal("Contract.View", ContractUiPolicy.View);
        Assert.Equal("Contract.Create", ContractUiPolicy.Create);
        Assert.Equal("Contract.ManageTemplates", ContractUiPolicy.ManageTemplates);
    }

    [Fact]
    public void Contract_status_text_is_persian()
    {
        Assert.Equal("پیش‌نویس", ContractUiPolicy.StatusText(ContractStatus.Draft));
        Assert.Equal("امضاشده", ContractUiPolicy.StatusText(ContractStatus.Signed));
    }
}
