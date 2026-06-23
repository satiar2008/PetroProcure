using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PetroProcure.Application.Security;

namespace PetroProcure.Api.Security;

public static class AuthorizationExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.RequireAuthorization(PermissionPolicyNames.For(permission));
        return builder;
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission) =>
        user.HasClaim(ApplicationPermissions.ClaimType, permission)
        || user.IsInRole(ApplicationRoles.SystemAdmin);
}
