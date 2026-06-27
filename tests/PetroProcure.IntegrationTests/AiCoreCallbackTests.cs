using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.IntegrationTests;

public sealed class AiCoreCallbackTests(AiCoreCallbackFactory factory)
    : IClassFixture<AiCoreCallbackFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task ValidCallbackCompletesJobAndStoresResult()
    {
        var (jobId, correlationId) = await SeedJobAsync("PurchaseFile", "Summary");

        var payload = new AiCoreCallbackRequest(correlationId, "ext-1", "Completed", 100, "done",
            new AiCoreCallbackResultDto("نتیجه تحلیل", new[]
            {
                new AiFindingDto("کمبود مدرک", "سند فنی موجود نیست", AiSeverity.High, "DOC-1")
            }, new[]
            {
                new AiRecommendationDto("تکمیل مدارک", "سند را بارگذاری کنید", "High", "Upload")
            }, "{\"summary\":\"نتیجه تحلیل\"}"),
            null, new AiUsageDto(10, 20, null, 30, 1200, "gemma3", "AiCore"), DateTime.UtcNow);

        var response = await PostCallbackAsync(payload, sign: true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var results = scope.ServiceProvider.GetRequiredService<IAiResultRepository>();
        var job = await jobs.GetJobAsync(jobId, CancellationToken.None);
        var result = await results.GetResultAsync(jobId, CancellationToken.None);

        Assert.Equal(AiJobStatus.Completed, job!.Status);
        Assert.NotNull(result);
        Assert.Equal("نتیجه تحلیل", result!.Summary);
        Assert.Single(result.Findings);
        Assert.Single(result.Recommendations);
        Assert.Equal(30, result.Usage?.TotalTokens);
    }

    [Fact]
    public async Task InvalidSignatureIsRejected()
    {
        var (_, correlationId) = await SeedJobAsync("Tender", "LegalCompliance");
        var payload = new AiCoreCallbackRequest(correlationId, "ext-2", "Completed", 100, "done",
            new AiCoreCallbackResultDto("x", [], [], null), null, null, DateTime.UtcNow);

        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/ai/aicore/callback") { Content = content };
        request.Headers.Add("X-AI-API-KEY", AiCoreCallbackFactory.ApiKey);
        request.Headers.Add("X-AiCore-Timestamp", timestamp);
        request.Headers.Add("X-AiCore-Signature", "sha256=deadbeef"); // wrong signature

        var response = await factory.CreateClient().SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReplayedSignedCallbackIsRejected()
    {
        var (_, correlationId) = await SeedJobAsync("Tender", "LegalCompliance");
        var payload = new AiCoreCallbackRequest(correlationId, "ext-replay", "Running", 25, "running",
            null, null, null, null);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var first = await PostCallbackAsync(payload, sign: true, timestamp);
        var replay = await PostCallbackAsync(payload, sign: true, timestamp);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, replay.StatusCode);
    }

    [Fact]
    public async Task OldTimestampIsRejected()
    {
        var (_, correlationId) = await SeedJobAsync("Tender", "LegalCompliance");
        var payload = new AiCoreCallbackRequest(correlationId, "ext-old", "Running", 25, "running",
            null, null, null, null);
        var timestamp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds().ToString();

        var response = await PostCallbackAsync(payload, sign: true, timestamp);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnknownCorrelationIdIsRejected()
    {
        var payload = new AiCoreCallbackRequest(Guid.NewGuid().ToString("N"), "ext-3", "Completed", 100, "done",
            new AiCoreCallbackResultDto("x", [], [], null), null, null, DateTime.UtcNow);

        var response = await PostCallbackAsync(payload, sign: true);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnknownExternalJobIdIsRejected()
    {
        var payload = new AiCoreCallbackRequest("", "unknown-external-job", "Failed", 0, "external failure",
            null, new AiCoreCallbackErrorDto("FAILED", "External job failed", false), null, DateTime.UtcNow);

        var response = await PostCallbackAsync(payload, sign: true);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task InvalidPayloadReturnsBadRequestWithDetails()
    {
        var json = "{\"externalJobId\":\"ext-invalid\"}";
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/ai/aicore/callback") { Content = content };
        request.Headers.Add("X-AI-API-KEY", AiCoreCallbackFactory.ApiKey);
        request.Headers.Add("X-AiCore-Timestamp", timestamp);
        request.Headers.Add("X-AiCore-Signature", "sha256=" + ComputeSignature($"{timestamp}.{json}"));

        var response = await factory.CreateClient().SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("details", body);
        Assert.Contains("status is required", body);
    }

    [Fact]
    public async Task DuplicateCompletedCallbackIsIdempotent()
    {
        var (jobId, correlationId) = await SeedJobAsync("Contract", "Summary");

        var first = new AiCoreCallbackRequest(correlationId, "ext-4", "Completed", 100, null,
            new AiCoreCallbackResultDto("اولین نتیجه", [], [], "{\"summary\":\"اولین نتیجه\"}"),
            null, null, DateTime.UtcNow);
        var second = first with { Result = new AiCoreCallbackResultDto("نتیجه دوم", [], [], "{}") };

        Assert.Equal(HttpStatusCode.OK, (await PostCallbackAsync(first, sign: true)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await PostCallbackAsync(second, sign: true)).StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var results = scope.ServiceProvider.GetRequiredService<IAiResultRepository>();
        var result = await results.GetResultAsync(jobId, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal("اولین نتیجه", result!.Summary);
    }

    [Fact]
    public async Task FailedCallbackStoresErrorMessage()
    {
        var (jobId, correlationId) = await SeedJobAsync("PurchaseOrder", "RiskReview");
        var payload = new AiCoreCallbackRequest(correlationId, "ext-5", "Failed", 0, null, null,
            new AiCoreCallbackErrorDto("PROVIDER_ERROR", "AiCore provider timed out", false), null, DateTime.UtcNow);

        var response = await PostCallbackAsync(payload, sign: true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var job = await jobs.GetJobAsync(jobId, CancellationToken.None);
        Assert.Equal(AiJobStatus.Failed, job!.Status);
        Assert.Equal("AiCore provider timed out", job.ErrorMessage);
    }

    [Fact]
    public async Task RunningCallbackUpdatesProgress()
    {
        var (jobId, correlationId) = await SeedJobAsync("WarehouseReceipt", "Summary");
        var payload = new AiCoreCallbackRequest(correlationId, "ext-6", "Running", 42, "در حال تحلیل",
            null, null, null, null);

        var response = await PostCallbackAsync(payload, sign: true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        var job = await jobs.GetJobAsync(jobId, CancellationToken.None);
        Assert.Equal(AiJobStatus.Running, job!.Status);
        Assert.Equal(42, job.ProgressPercent);
    }

    [Fact]
    public async Task UnknownStatusIsRejectedWith400()
    {
        var (_, correlationId) = await SeedJobAsync("PurchaseFile", "Summary");
        var payload = new AiCoreCallbackRequest(correlationId, "ext-7", "Sleeping", 0, null, null, null, null, null);

        var response = await PostCallbackAsync(payload, sign: true);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<(Guid JobId, string CorrelationId)> SeedJobAsync(string entityType, string analysisType)
    {
        var correlationId = Guid.NewGuid().ToString("N");
        var job = new AiEvaluationJob(Guid.NewGuid(), "PetroProcure", entityType, Guid.NewGuid(),
            analysisType, 0, correlationId, "{}", 3, null);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        db.AiEvaluationJobs.Add(job);
        await db.SaveChangesAsync();
        return (job.Id, correlationId);
    }

    private async Task<HttpResponseMessage> PostCallbackAsync(AiCoreCallbackRequest payload, bool sign, string? timestamp = null)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/ai/aicore/callback") { Content = content };
        request.Headers.Add("X-AI-API-KEY", AiCoreCallbackFactory.ApiKey);

        if (sign)
        {
            timestamp ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            request.Headers.Add("X-AiCore-Timestamp", timestamp);
            request.Headers.Add("X-AiCore-Signature", "sha256=" + ComputeSignature($"{timestamp}.{json}"));
        }

        return await factory.CreateClient().SendAsync(request);
    }

    private static string ComputeSignature(string payload)
    {
        var bytes = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(AiCoreCallbackFactory.CallbackSecret),
            Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes); // hex decode on the server is case-insensitive
    }
}

public sealed class AiCoreCallbackFactory : WebApplicationFactory<Program>
{
    public const string ApiKey = "test-aicore-callback-key";
    public const string CallbackSecret = "test-aicore-callback-secret-value";

    private readonly object _migrationLock = new();
    private bool _migrated;
    private readonly string _connectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=PetroProcureAiCallback_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Authentication:Jwt:Issuer"] = "PetroProcure.Tests",
                ["Authentication:Jwt:Audience"] = "PetroProcure.Api.Tests",
                ["Authentication:Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-chars",
                ["ConnectionStrings:PetroProcureDb"] = _connectionString,
                ["PetroProcure:FileStorage:RootPath"] = Path.Combine(Path.GetTempPath(), "PetroProcureAiCallback"),
                ["Security:BootstrapAdmin:Enabled"] = "false",
                ["PetroProcure:AI:AiCore:ApiKey"] = ApiKey,
                ["PetroProcure:AI:AiCore:CallbackSecret"] = CallbackSecret,
                ["PetroProcure:AI:AiCore:CallbackTimestampToleranceSeconds"] = "300"
            }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        lock (_migrationLock)
        {
            if (!_migrated)
            {
                using var scope = host.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
                db.Database.EnsureDeleted();
                db.Database.Migrate();
                _migrated = true;
            }
        }
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var options = new DbContextOptionsBuilder<PetroProcureDbContext>()
                .UseSqlServer(_connectionString).Options;
            using var db = new PetroProcureDbContext(options);
            db.Database.EnsureDeleted();
        }
        base.Dispose(disposing);
    }
}
