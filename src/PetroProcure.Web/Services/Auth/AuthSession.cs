using PetroProcure.Contracts.V1.Identity;

namespace PetroProcure.Web.Services.Auth;

public sealed class AuthSession
{
    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public CurrentUserDto? User { get; private set; }
    public bool IsAuthenticated => User is not null && !string.IsNullOrWhiteSpace(AccessToken);

    public void Set(LoginResponse response)
    {
        AccessToken = response.AccessToken;
        RefreshToken = response.RefreshToken;
        ExpiresAt = response.ExpiresAt;
        User = response.User;
    }

    public void UpdateUser(CurrentUserDto user)
    {
        User = user;
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        ExpiresAt = default;
        User = null;
    }
}
