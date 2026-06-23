using System.Security.Claims;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.Security;

namespace PetroProcure.Api.Security;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal User => httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();

    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return IsAuthenticated && Guid.TryParse(value, out var id)
                ? id
                : throw new CurrentUserNotAuthenticatedException();
        }
    }

    public string? UserName => User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
    public string? Email => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
    public IReadOnlyCollection<string> Roles => Values(ClaimTypes.Role);
    public IReadOnlyCollection<string> Permissions => Values(PetroProcureClaimTypes.Permission);
    public IReadOnlyCollection<Guid> DepartmentIds => User.FindAll(PetroProcureClaimTypes.DepartmentId)
        .Select(claim => Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty)
        .Where(id => id != Guid.Empty)
        .Distinct()
        .ToArray();
    public bool IsSystemAdmin => Roles.Contains(ApplicationRoles.SystemAdmin, StringComparer.OrdinalIgnoreCase);

    private string[] Values(string claimType) => User.FindAll(claimType)
        .Select(claim => claim.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
