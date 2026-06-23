using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using PetroProcure.Contracts.Security;

namespace PetroProcure.Web.Services.Auth;

public sealed class PetroProcureAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthService _auth;
    public PetroProcureAuthenticationStateProvider(IAuthService auth)
    {
        _auth = auth;
        _auth.AuthenticationChanged += OnAuthenticationChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _auth.CurrentUser;
        if (user is null)
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName)
        };
        if (!string.IsNullOrWhiteSpace(user.Email)) claims.Add(new(ClaimTypes.Email, user.Email));
        claims.AddRange(user.Roles.Select(x => new Claim(ClaimTypes.Role, x)));
        claims.AddRange(user.Permissions.Select(x => new Claim(PetroProcureClaimTypes.Permission, x)));
        claims.AddRange(user.DepartmentIds.Select(x => new Claim(PetroProcureClaimTypes.DepartmentId, x.ToString())));
        return Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "PetroProcureJwt"))));
    }

    private void OnAuthenticationChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() => _auth.AuthenticationChanged -= OnAuthenticationChanged;
}
