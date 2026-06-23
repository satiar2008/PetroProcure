using System.Net;
using System.Text;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Contracts.V1.Identity;
using PetroProcure.Domain.Enums;
using PetroProcure.Web.Services;
using PetroProcure.Web.Services.Api;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.UnitTests.Web;

public sealed class PurchaseFileUiTests
{
    [Fact]
    public async Task PurchaseFileListSendsPagingAndFilterParameters()
    {
        var handler = new CaptureHandler("""{"items":[],"pageNumber":3,"pageSize":25,"totalCount":0,"totalPages":0}""");
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") });

        await client.GetPurchaseFilesAsync(new PurchaseFileListRequest(
            FileNumber: "PF-2026", Title: "پمپ", Status: PurchaseFileStatus.Draft,
            PageNumber: 3, PageSize: 25, SortBy: "FileNumber", SortDescending: false));

        Assert.Contains("PageNumber=3", handler.RequestUri!.Query);
        Assert.Contains("PageSize=25", handler.RequestUri.Query);
        Assert.Contains("FileNumber=PF-2026", handler.RequestUri.Query);
        Assert.Contains("SortBy=FileNumber", handler.RequestUri.Query);
    }

    [Fact]
    public async Task AddNotePostsSharedContract()
    {
        var handler = new CaptureHandler("""
        {"id":"00000000-0000-0000-0000-000000000001","departmentId":"00000000-0000-0000-0000-000000000002","userId":"00000000-0000-0000-0000-000000000003","noteText":"یادداشت","createdAt":"2026-01-01T00:00:00Z","isInternal":true}
        """);
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") });
        await client.AddNoteAsync(Guid.NewGuid(), new(Guid.NewGuid(), "یادداشت", true));
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Contains("/notes", handler.RequestUri!.AbsolutePath);
        Assert.Contains("\"noteText\"", handler.Body);
    }

    [Fact]
    public void UnauthorizedOrReadOnlyActionsAreHidden()
    {
        Assert.False(PurchaseFileUiPolicy.CanEdit(PurchaseFileStatus.Draft, _ => false));
        Assert.False(PurchaseFileUiPolicy.CanEdit(PurchaseFileStatus.Archived, _ => true));
        Assert.True(PurchaseFileUiPolicy.CanEdit(PurchaseFileStatus.Draft, permission => permission == "PurchaseFile.Edit"));
    }

    [Fact]
    public void TabStatesRemainIndependentAfterOneFailure()
    {
        var summary = new TabLoadState<string> { Data = "ok" };
        var documents = new TabLoadState<string> { Error = "failed" };
        Assert.Equal("ok", summary.Data);
        Assert.Null(summary.Error);
        Assert.Equal("failed", documents.Error);
    }

    [Fact]
    public async Task UploadDocumentUsesMultipartContract()
    {
        var handler = new CaptureHandler("""
        {"id":"00000000-0000-0000-0000-000000000001","purchaseFileId":"00000000-0000-0000-0000-000000000002","departmentId":null,"documentType":2,"originalFileName":"spec.pdf","storedFileName":"safe.pdf","relativePath":"PurchaseFiles/spec.pdf","extension":".pdf","mimeType":"application/pdf","size":3,"hash":"abc","versionNo":1,"uploadedByUserId":"00000000-0000-0000-0000-000000000003","uploadedAt":"2026-01-01T00:00:00Z","isDeleted":false,"description":"test"}
        """);
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") });
        await client.UploadDocumentAsync(Guid.NewGuid(), new MemoryStream([1,2,3]), "spec.pdf",
            "application/pdf", new(DocumentType.TechnicalSpecification, null, "test"));
        Assert.Contains("multipart/form-data", handler.ContentType);
        Assert.Contains("spec.pdf", handler.Body);
        Assert.Contains("documentType", handler.Body);
    }

    [Fact]
    public async Task ApiClientMaps404ToNotFoundError()
    {
        var handler = new CaptureHandler("{}", HttpStatusCode.NotFound);
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") });

        var ex = await Assert.ThrowsAsync<ApiClientException>(() => client.GetPurchaseFilesAsync(new PurchaseFileListRequest()));

        Assert.Equal(ApiErrorType.NotFound, ex.Error.Type);
        Assert.Contains("یافت نشد", ex.Message);
    }

    [Fact]
    public async Task ApiClientAttachesBearerTokenFromAuthSession()
    {
        var handler = new CaptureHandler("""{"items":[],"pageNumber":1,"pageSize":10,"totalCount":0,"totalPages":0}""");
        var session = new AuthSession();
        session.Set(new LoginResponse("token-123", "refresh-123", DateTime.UtcNow.AddMinutes(30),
            new CurrentUserDto(Guid.NewGuid(), "admin", null, null, [], [], [], [], true)));
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") }, session);

        await client.GetPurchaseFilesAsync(new PurchaseFileListRequest());

        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("token-123", handler.AuthorizationParameter);
    }

    [Fact]
    public async Task UnauthorizedGetTriggersOneRefreshAndRetriesOriginalRequest()
    {
        var handler = new SequenceHandler(
            new(HttpStatusCode.Unauthorized, "{}"),
            new(HttpStatusCode.OK, """{"items":[],"pageNumber":1,"pageSize":10,"totalCount":0,"totalPages":0}"""));
        var session = new AuthSession();
        session.Set(new LoginResponse("expired-token", "refresh-token", DateTime.UtcNow.AddMinutes(30),
            new CurrentUserDto(Guid.NewGuid(), "admin", null, null, [], [], [], [], true)));
        var auth = new FakeAuthService(refreshSucceeds: true);
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") }, session, auth);

        await client.GetPurchaseFilesAsync(new PurchaseFileListRequest());

        Assert.Equal(1, auth.RefreshAttempts);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task FailedRefreshDoesNotRetryOriginalRequestForever()
    {
        var handler = new SequenceHandler((HttpStatusCode.Unauthorized, "{}"));
        var session = new AuthSession();
        session.Set(new LoginResponse("expired-token", "refresh-token", DateTime.UtcNow.AddMinutes(30),
            new CurrentUserDto(Guid.NewGuid(), "admin", null, null, [], [], [], [], true)));
        var auth = new FakeAuthService(refreshSucceeds: false);
        var client = new PetroProcureApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://api.test") }, session, auth);

        var ex = await Assert.ThrowsAsync<ApiClientException>(() => client.GetPurchaseFilesAsync(new PurchaseFileListRequest()));

        Assert.Equal(ApiErrorType.Unauthorized, ex.Error.Type);
        Assert.Equal(1, auth.RefreshAttempts);
        Assert.Equal(1, handler.RequestCount);
    }

    private sealed class CaptureHandler(string response, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public string Body { get; private set; } = "";
        public string ContentType { get; private set; } = "";
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            RequestUri = request.RequestUri; Method = request.Method;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            ContentType = request.Content?.Headers.ContentType?.MediaType ?? "";
            if (request.Content is not null) Body = await request.Content.ReadAsStringAsync(ct);
            return new HttpResponseMessage(statusCode)
            { Content = new StringContent(response, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class SequenceHandler(params (HttpStatusCode StatusCode, string Body)[] responses) : HttpMessageHandler
    {
        private int _index;
        public int RequestCount { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            RequestCount++;
            var response = responses[Math.Min(_index++, responses.Length - 1)];
            return Task.FromResult(new HttpResponseMessage(response.StatusCode)
            {
                Content = new StringContent(response.Body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class FakeAuthService(bool refreshSucceeds) : IAuthService
    {
        public int RefreshAttempts { get; private set; }
        public event Action? AuthenticationChanged;
        public CurrentUserDto? CurrentUser => null;
        public Task<(bool Succeeded, string? Error)> LoginAsync(string userNameOrEmail, string password, bool rememberMe) =>
            Task.FromResult((true, (string?)null));
        public Task LogoutAsync()
        {
            AuthenticationChanged?.Invoke();
            return Task.CompletedTask;
        }
        public Task<(bool Succeeded, string? Error)> ChangePasswordAsync(string currentPassword, string newPassword) =>
            Task.FromResult((true, (string?)null));
        public Task<bool> RefreshTokenAsync()
        {
            RefreshAttempts++;
            return Task.FromResult(refreshSucceeds);
        }
        public Task<CurrentUserDto?> GetCurrentUserAsync() => Task.FromResult<CurrentUserDto?>(null);
        public Task<bool> IsAuthenticatedAsync() => Task.FromResult(false);
        public Task InitializeAsync() => Task.CompletedTask;
        public Task<List<UserSessionDto>> GetSessionsAsync() => Task.FromResult(new List<UserSessionDto>());
        public Task RevokeCurrentRefreshTokenAsync() => Task.CompletedTask;
    }
}
