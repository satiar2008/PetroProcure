using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PetroProcure.Contracts.V1.Identity;

namespace PetroProcure.Web.Services.Auth;

public static class AuthErrorMessages
{
    public const string InvalidCredentials = "نام کاربری یا رمز عبور صحیح نیست.";
}

public interface IAuthService
{
    event Action? AuthenticationChanged;
    CurrentUserDto? CurrentUser { get; }
    Task<(bool Succeeded, string? Error)> LoginAsync(string userNameOrEmail, string password, bool rememberMe);
    Task LogoutAsync();
    Task<(bool Succeeded, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword);
    Task<bool> RefreshTokenAsync();
    Task<List<UserSessionDto>> GetSessionsAsync();
    Task RevokeCurrentRefreshTokenAsync();
    Task<CurrentUserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task InitializeAsync();
}

public sealed class AuthService(
    IHttpClientFactory httpClientFactory,
    ProtectedLocalStorage localStorage,
    ProtectedSessionStorage sessionStorage,
    AuthSession session) : IAuthService
{
    private const string StorageKey = "petroprocure.auth";
    private bool _initialized;

    public event Action? AuthenticationChanged;
    public CurrentUserDto? CurrentUser => session.User;

    public async Task<(bool Succeeded, string? Error)> LoginAsync(
        string userNameOrEmail, string password, bool rememberMe)
    {
        var response = await Client().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(userNameOrEmail, password, rememberMe));
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return (false, AuthErrorMessages.InvalidCredentials);
        if (!response.IsSuccessStatusCode)
            return (false, "ارتباط با سرویس احراز هویت ناموفق بود.");

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (login is null) return (false, "پاسخ سرویس احراز هویت معتبر نیست.");

        session.Set(login);
        if (rememberMe)
            await localStorage.SetAsync(StorageKey, login);
        else
            await sessionStorage.SetAsync(StorageKey, login);
        AuthenticationChanged?.Invoke();
        return (true, null);
    }

    public async Task LogoutAsync()
    {
        await RevokeCurrentRefreshTokenAsync();
        session.Clear();
        await localStorage.DeleteAsync(StorageKey);
        await sessionStorage.DeleteAsync(StorageKey);
        AuthenticationChanged?.Invoke();
    }

    public async Task<(bool Succeeded, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-password")
        {
            Content = JsonContent.Create(new ChangePasswordRequest(currentPassword, newPassword))
        };
        request.Headers.Authorization = new("Bearer", session.AccessToken);
        var response = await Client().SendAsync(request);
        return response.IsSuccessStatusCode
            ? (true, null)
            : (false, "تغییر رمز عبور ناموفق بود. رمز فعلی و سیاست رمز را بررسی کنید.");
    }

    public async Task<bool> RefreshTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(session.RefreshToken)) return false;
        var response = await Client().PostAsJsonAsync("/api/auth/refresh",
            new RefreshTokenRequest(session.RefreshToken));
        if (!response.IsSuccessStatusCode)
        {
            await LogoutAsync();
            return false;
        }
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (login is null) return false;
        session.Set(login);
        await localStorage.SetAsync(StorageKey, login);
        AuthenticationChanged?.Invoke();
        return true;
    }

    public async Task<List<UserSessionDto>> GetSessionsAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/sessions");
        request.Headers.Authorization = new("Bearer", session.AccessToken);
        return await (await Client().SendAsync(request)).Content.ReadFromJsonAsync<List<UserSessionDto>>() ?? [];
    }

    public async Task RevokeCurrentRefreshTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(session.RefreshToken)) return;
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/revoke-refresh-token")
        {
            Content = JsonContent.Create(new RevokeRefreshTokenRequest(session.RefreshToken))
        };
        request.Headers.Authorization = new("Bearer", session.AccessToken);
        await Client().SendAsync(request);
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        if (!session.IsAuthenticated) return null;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new("Bearer", session.AccessToken);
        var response = await Client().SendAsync(request);
        if (response.StatusCode == HttpStatusCode.Unauthorized && await RefreshTokenAsync())
            return await GetCurrentUserAsync();
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CurrentUserDto>();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        await InitializeAsync();
        return session.IsAuthenticated;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        var persistent = await localStorage.GetAsync<LoginResponse>(StorageKey);
        var transient = persistent.Success
            ? default
            : await sessionStorage.GetAsync<LoginResponse>(StorageKey);
        var login = persistent.Success ? persistent.Value : transient.Success ? transient.Value : null;
        if (login is null) return;
        session.Set(login);
        if (session.ExpiresAt <= DateTime.UtcNow && !await RefreshTokenAsync())
        {
            session.Clear();
            await ClearStoredSessionAsync();
            AuthenticationChanged?.Invoke();
            return;
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser is null)
        {
            session.Clear();
            await ClearStoredSessionAsync();
            AuthenticationChanged?.Invoke();
            return;
        }

        session.UpdateUser(currentUser);
        AuthenticationChanged?.Invoke();
    }

    private HttpClient Client() => httpClientFactory.CreateClient("PetroProcure.Auth");

    private async Task ClearStoredSessionAsync()
    {
        await localStorage.DeleteAsync(StorageKey);
        await sessionStorage.DeleteAsync(StorageKey);
    }
}
