using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.AI;

public interface IAiCoreJobClient
{
    Task<AiCoreSubmitJobResponse> SubmitJobAsync(AiCoreSubmitJobRequest request, CancellationToken ct = default);
    Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default);
}

public sealed class AiCoreJobClient(
    HttpClient http,
    IOptions<AiCoreIntegrationOptions> options,
    ILogger<AiCoreJobClient> logger) : IAiCoreJobClient
{
    public async Task<AiCoreSubmitJobResponse> SubmitJobAsync(AiCoreSubmitJobRequest request, CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            throw new AiCoreClientException("AiCore BaseUrl is not configured.");

        using var message = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl(settings.BaseUrl, settings.SubmitJobPath, "/api/ai/jobs"));
        ApplyHeaders(message, settings);
        message.Content = JsonContent.Create(request, options: new(JsonSerializerDefaults.Web));

        var timeout = TimeSpan.FromSeconds(Math.Clamp(settings.RequestTimeoutSeconds, 5, 120));
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        logger.LogInformation("Submitting AI job {CorrelationId} to AiCore for {EntityType}/{EntityId}.",
            request.CorrelationId, request.EntityType, request.EntityId);

        try
        {
            using var response = await http.SendAsync(message, timeoutCts.Token);
            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
                throw new AiCoreClientException(
                    $"AiCore job submission failed with status {(int)response.StatusCode}: {TrimBody(body)}");

            var result = await response.Content.ReadFromJsonAsync<AiCoreSubmitJobResponse>(
                new JsonSerializerOptions(JsonSerializerDefaults.Web), timeoutCts.Token);
            if (result is null || string.IsNullOrWhiteSpace(result.ExternalJobId))
                throw new AiCoreClientException("AiCore returned an invalid job submission response.");

            logger.LogInformation("AiCore accepted AI job {CorrelationId} as external job {ExternalJobId}.",
                request.CorrelationId, result.ExternalJobId);
            return result;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AiCoreClientException($"AiCore job submission timed out after {timeout.TotalSeconds:0} seconds.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiCoreClientException($"AiCore job submission failed because AiCore is unavailable: {ex.Message}", ex);
        }
    }

    public async Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            return new("AiCore", false, "BaseUrlNotConfigured", DateTime.UtcNow, settings.DefaultModel);

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Get,
                BuildUrl(settings.BaseUrl, settings.HealthPath, "/health/ready"));
            ApplyHeaders(message, settings);
            using var response = await http.SendAsync(message, ct);
            return new("AiCore", response.IsSuccessStatusCode,
                response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                DateTime.UtcNow, settings.DefaultModel,
                response.IsSuccessStatusCode ? null : "AiCore health check failed.");
        }
        catch
        {
            return new("AiCore", false, "Unavailable", DateTime.UtcNow, settings.DefaultModel,
                "AiCore is not reachable. Check BaseUrl and network access.");
        }
    }

    private static void ApplyHeaders(HttpRequestMessage message, AiCoreIntegrationOptions settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            return;

        message.Headers.TryAddWithoutValidation("X-AI-API-KEY", settings.ApiKey);
        message.Headers.Authorization = new("Bearer", settings.ApiKey);
    }

    private static string BuildUrl(string baseUrl, string path, string defaultPath)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(path) ? defaultPath : path.Trim();
        if (Uri.TryCreate(normalizedPath, UriKind.Absolute, out _))
            return normalizedPath;
        return $"{baseUrl.TrimEnd('/')}/{normalizedPath.TrimStart('/')}";
    }

    private static string TrimBody(string body) =>
        string.IsNullOrWhiteSpace(body) ? "<empty>" : body.Length <= 1000 ? body : body[..1000];
}
