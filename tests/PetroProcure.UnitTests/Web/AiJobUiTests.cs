using System.Net;
using System.Text;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Web.Services.Api;

namespace PetroProcure.UnitTests.Web;

public sealed class AiJobUiTests
{
    private static readonly Uri BaseAddress = new("https://api.test");

    [Fact]
    public async Task CreateJobCallsJobEndpointWithPost()
    {
        var handler = new CaptureHandler(
            """{"jobId":"11111111-1111-1111-1111-111111111111","status":"Queued","message":"queued","createdAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = new PetroProcureAiApiClient(new HttpClient(handler) { BaseAddress = BaseAddress });

        var response = await client.CreateJobAsync(new CreateAiJobRequest("PurchaseFile", Guid.NewGuid(), "Summary"));

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/api/ai/jobs", handler.RequestUri!.AbsolutePath);
        Assert.Equal("Queued", response.Status);
    }

    [Fact]
    public async Task SummaryShortcutCallsSummarizeEndpoint()
    {
        var handler = new CaptureHandler(
            """{"jobId":"11111111-1111-1111-1111-111111111111","status":"Queued","message":"queued","createdAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = new PetroProcureAiApiClient(new HttpClient(handler) { BaseAddress = BaseAddress });
        var id = Guid.NewGuid();

        await client.CreatePurchaseFileSummaryJobAsync(id);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal($"/api/ai/purchase-files/{id}/jobs/summarize", handler.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task CreatingJobDoesNotFetchResultSynchronously()
    {
        var handler = new CaptureHandler(
            """{"jobId":"11111111-1111-1111-1111-111111111111","status":"Queued","message":"queued","createdAtUtc":"2026-01-01T00:00:00Z"}""");
        var client = new PetroProcureAiApiClient(new HttpClient(handler) { BaseAddress = BaseAddress });

        await client.CreateRulesEvaluationJobAsync(Guid.NewGuid());

        // The only call is the short job-creation POST; the result endpoint is never hit synchronously.
        Assert.Equal(1, handler.RequestCount);
        Assert.DoesNotContain("/result", handler.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task CancelJobCallsCancelEndpoint()
    {
        var handler = new CaptureHandler("", HttpStatusCode.NoContent);
        var client = new PetroProcureAiApiClient(new HttpClient(handler) { BaseAddress = BaseAddress });
        var id = Guid.NewGuid();

        await client.CancelJobAsync(id);

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal($"/api/ai/jobs/{id}/cancel", handler.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetJobResultReturnsNullWhilePending()
    {
        var handler = new CaptureHandler("{}", HttpStatusCode.NotFound);
        var client = new PetroProcureAiApiClient(new HttpClient(handler) { BaseAddress = BaseAddress });

        var result = await client.GetJobResultAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task PollerStopsAndReportsResultOnCompleted()
    {
        var fake = new FakeAiApiClient(Status("Running", 40), Status("Completed", 100))
        {
            Result = new AiJobResultDto(Guid.NewGuid(), "Completed", "خلاصه", [], [], null, null, DateTime.UtcNow)
        };
        var poller = new AiJobPoller(fake) { Interval = TimeSpan.FromMilliseconds(5) };

        var statuses = new List<string>();
        AiJobResultDto? completedResult = null;
        await poller.PollAsync(Guid.NewGuid(),
            status => { statuses.Add(status.Status); return Task.CompletedTask; },
            result => { completedResult = result; return Task.CompletedTask; },
            CancellationToken.None);

        Assert.Equal(new[] { "Running", "Completed" }, statuses);
        Assert.NotNull(completedResult);
        Assert.Equal(2, fake.StatusCalls);
        Assert.Equal(1, fake.ResultCalls);
    }

    [Fact]
    public async Task PollerStopsWhenCancelledWithoutReportingResult()
    {
        var fake = new FakeAiApiClient(Status("Running", 40));
        var poller = new AiJobPoller(fake) { Interval = TimeSpan.FromMilliseconds(5) };
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // simulates component disposal before any poll

        var completedCalled = false;
        await poller.PollAsync(Guid.NewGuid(),
            _ => Task.CompletedTask,
            _ => { completedCalled = true; return Task.CompletedTask; },
            cts.Token);

        Assert.False(completedCalled);
        Assert.Equal(0, fake.StatusCalls); // no polling happened after cancellation
    }

    [Fact]
    public void TerminalStatusDetection()
    {
        Assert.True(AiJobPoller.IsTerminal("Completed"));
        Assert.True(AiJobPoller.IsTerminal("failed"));
        Assert.True(AiJobPoller.IsTerminal("Cancelled"));
        Assert.False(AiJobPoller.IsTerminal("Running"));
        Assert.False(AiJobPoller.IsTerminal("Queued"));
        Assert.True(AiJobPoller.IsCompleted("completed"));
    }

    private static AiJobStatusDto Status(string status, int progress) =>
        new(Guid.NewGuid(), "PurchaseFile", Guid.NewGuid(), "Summary", status, progress,
            status, "ext", DateTime.UtcNow, DateTime.UtcNow, null, null, status == "Completed");

    private sealed class CaptureHandler(string response, HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            RequestCount++;
            RequestUri = request.RequestUri;
            Method = request.Method;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class FakeAiApiClient(params AiJobStatusDto[] statuses) : IPetroProcureAiApiClient
    {
        private readonly Queue<AiJobStatusDto> _statuses = new(statuses);
        public AiJobResultDto? Result { get; set; }
        public int StatusCalls { get; private set; }
        public int ResultCalls { get; private set; }
        public int CancelCalls { get; private set; }

        public Task<CreateAiJobResponse> CreateJobAsync(CreateAiJobRequest request, CancellationToken ct = default) =>
            Task.FromResult(new CreateAiJobResponse(Guid.NewGuid(), "Queued", "queued", DateTime.UtcNow));

        public Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct = default)
        {
            StatusCalls++;
            return Task.FromResult<AiJobStatusDto?>(_statuses.Count > 0 ? _statuses.Dequeue() : null);
        }

        public Task<AiJobResultDto?> GetJobResultAsync(Guid jobId, CancellationToken ct = default)
        {
            ResultCalls++;
            return Task.FromResult(Result);
        }

        public Task CancelJobAsync(Guid jobId, CancellationToken ct = default)
        {
            CancelCalls++;
            return Task.CompletedTask;
        }

        public Task<CreateAiJobResponse> CreatePurchaseFileSummaryJobAsync(Guid purchaseFileId, CancellationToken ct = default) =>
            CreateJobAsync(new CreateAiJobRequest("PurchaseFile", purchaseFileId, "Summary"), ct);
        public Task<CreateAiJobResponse> CreateMissingDocumentsJobAsync(Guid purchaseFileId, CancellationToken ct = default) =>
            CreateJobAsync(new CreateAiJobRequest("PurchaseFile", purchaseFileId, "MissingDocuments"), ct);
        public Task<CreateAiJobResponse> CreateRulesEvaluationJobAsync(Guid purchaseFileId, CancellationToken ct = default) =>
            CreateJobAsync(new CreateAiJobRequest("PurchaseFile", purchaseFileId, "LegalCompliance"), ct);
        public Task<CreateAiJobResponse> CreateFullAnalysisJobAsync(Guid purchaseFileId, string? userQuestion = null, CancellationToken ct = default) =>
            CreateJobAsync(new CreateAiJobRequest("PurchaseFile", purchaseFileId, "RiskReview"), ct);
    }
}
