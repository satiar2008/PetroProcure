using Microsoft.AspNetCore.Authorization;

namespace PetroProcure.Infrastructure.Identity;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
