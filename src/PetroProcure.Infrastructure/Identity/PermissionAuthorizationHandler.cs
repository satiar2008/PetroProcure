using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Security;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.Infrastructure.Identity;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly PetroProcureDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionAuthorizationHandler(PetroProcureDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim(ApplicationPermissions.ClaimType, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return;
        }

        var roleNames = await _userManager.GetRolesAsync(user);
        if (roleNames.Count == 0)
        {
            return;
        }

        var normalizedRoleNames = roleNames
            .Select(role => role.ToUpperInvariant())
            .ToArray();

        var hasPermission = await (
            from role in _dbContext.Roles
            join rolePermission in _dbContext.RolePermissions on role.Id equals rolePermission.RoleId
            join permission in _dbContext.Permissions on rolePermission.PermissionId equals permission.Id
            where normalizedRoleNames.Contains(role.NormalizedName!)
                && permission.Name == requirement.Permission
                && permission.IsActive
            select permission.Id)
            .AnyAsync();

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
