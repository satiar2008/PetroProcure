using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace PetroProcure.Web.Services.Auth;

public sealed class ApiAuthorizationHandler(
    AuthSession session,
    NavigationManager navigation) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(session.AccessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            TryNavigateToLogin();
        else if (response.StatusCode == HttpStatusCode.Forbidden)
            TryNavigateToAccessDenied();
        return response;
    }

    private void TryNavigateToLogin()
    {
        try
        {
            var returnUrl = navigation.ToBaseRelativePath(navigation.Uri);
            navigation.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        catch (InvalidOperationException)
        {
            // NavigationManager is not initialized during server-side prerendering
            // or endpoint execution. The caller will receive the 401 response.
        }
    }

    private void TryNavigateToAccessDenied()
    {
        try
        {
            navigation.NavigateTo("/access-denied");
        }
        catch (InvalidOperationException)
        {
            // NavigationManager is not initialized during server-side prerendering
            // or endpoint execution. The caller will receive the 403 response.
        }
    }
}
