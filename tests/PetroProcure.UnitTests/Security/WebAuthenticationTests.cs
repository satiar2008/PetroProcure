using System.Net;
using Microsoft.AspNetCore.Components;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.UnitTests.Security;

public sealed class WebAuthenticationTests
{
    [Fact]
    public async Task UnauthorizedApiResponseRedirectsToLogin()
    {
        var navigation = new TestNavigationManager();
        var handler = new ApiAuthorizationHandler(new AuthSession(), navigation)
        {
            InnerHandler = new StatusHandler(HttpStatusCode.Unauthorized)
        };
        await new HttpClient(handler).GetAsync("https://api.test/protected");
        Assert.StartsWith("http://localhost/login?returnUrl=", navigation.LastUri);
    }

    [Fact]
    public async Task ForbiddenApiResponseRedirectsToAccessDenied()
    {
        var navigation = new TestNavigationManager();
        var handler = new ApiAuthorizationHandler(new AuthSession(), navigation)
        {
            InnerHandler = new StatusHandler(HttpStatusCode.Forbidden)
        };
        await new HttpClient(handler).GetAsync("https://api.test/protected");
        Assert.Equal("http://localhost/access-denied", navigation.LastUri);
    }

    [Fact]
    public void LoginFailureMessageIsPersian() =>
        Assert.Equal("نام کاربری یا رمز عبور صحیح نیست.", AuthErrorMessages.InvalidCredentials);

    private sealed class StatusHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode));
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => Initialize("http://localhost/", "http://localhost/current");
        public string LastUri { get; private set; } = "";
        protected override void NavigateToCore(string uri, NavigationOptions options) =>
            LastUri = ToAbsoluteUri(uri).ToString();
    }
}
