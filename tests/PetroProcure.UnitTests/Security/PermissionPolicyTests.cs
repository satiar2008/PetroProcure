using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Security;
using PetroProcure.Infrastructure.Identity;

namespace PetroProcure.UnitTests.Security;

public sealed class PermissionPolicyTests
{
    [Fact]
    public async Task PermissionPolicyProvider_CreatesPolicyForPermissionName()
    {
        var provider = new PermissionAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));

        var policy = await provider.GetPolicyAsync(PermissionPolicyNames.For(ApplicationPermissions.PurchaseFileView));

        Assert.NotNull(policy);
        Assert.Contains(policy!.Requirements, requirement =>
            requirement is PermissionRequirement permissionRequirement
            && permissionRequirement.Permission == ApplicationPermissions.PurchaseFileView);
    }
}
