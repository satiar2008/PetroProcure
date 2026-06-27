using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.AI;

public interface ILocalOllamaAnalysisClient
{
    Task<AiCoreAnalysisResponse> AnalyzeAsync(AiCoreAnalysisRequest request, CancellationToken ct = default);
}

public sealed class LocalOllamaAnalysisClient(
    HttpClient http,
    IOptions<AiOptions> options,
    ILogger<LocalOllamaAnalysisClient> logger) : ILocalOllamaAnalysisClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AiCoreAnalysisResponse> AnalyzeAsync(AiCoreAnalysisRequest request, CancellationToken ct = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.OllamaBaseUrl))
            throw new AiCoreClientException("Local Ollama BaseUrl is not configured.");
        if (string.IsNullOrWhiteSpace(settings.OllamaModel))
            throw new AiCoreClientException("Local Ollama model is not configured.");

        var prompt = BuildPrompt(request);
        var stopwatch = Stopwatch.StartNew();
        using var response = await http.PostAsJsonAsync(
            $"{settings.OllamaBaseUrl.TrimEnd('/')}/api/generate",
            new
            {
                model = settings.OllamaModel,
                prompt,
                stream = false,
                format = "json"
            },
            JsonOptions,
            ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw new AiCoreClientException(
                $"Local Ollama analysis failed with status {(int)response.StatusCode}: {TrimBody(body)}");

        var ollama = JsonSerializer.Deserialize<OllamaGenerateResponse>(body, JsonOptions)
            ?? throw new AiCoreClientException("Local Ollama returned an unsupported response.");
        stopwatch.Stop();

        logger.LogInformation("Local Ollama completed AI analysis {RequestId} using model {Model}.",
            request.RequestId, settings.OllamaModel);

        return ParseAnalysisResponse(ollama.Response, settings.OllamaModel, stopwatch.ElapsedMilliseconds);
    }

    private static string BuildPrompt(AiCoreAnalysisRequest request)
    {
        var contextJson = JsonSerializer.Serialize(request.Context, JsonOptions);
        var userMessage = request.Messages.LastOrDefault(x =>
            x.Role.Equals("user", StringComparison.OrdinalIgnoreCase))?.Content;

        return $$"""
        You are an advisory refinery procurement analysis assistant.
        Write every textual value (summary, title, description, recommendation) in fluent Persian (Farsi), not English. Only JSON keys stay in English.
        Never make final business decisions. Human users and commissions remain responsible for approvals.
        Return only valid JSON with this shape:
        {
          "summary": "string",
          "riskLevel": "Info|Low|Medium|High|Critical",
          "findings": [
            { "title": "string", "description": "string", "severity": "Info|Low|Medium|High|Critical", "code": "optional" }
          ],
          "recommendations": [
            { "title": "string", "description": "string", "severity": "Info|Low|Medium|High|Critical", "relatedAction": "optional" }
          ]
        }

        Analysis type: {{request.AnalysisType}}
        User question: {{userMessage ?? request.AnalysisType}}
        Context:
        {{contextJson}}
        """;
    }

    private static AiCoreAnalysisResponse ParseAnalysisResponse(string? content, string model, long durationMs)
    {
        var text = StripJsonFence(content ?? string.Empty);
        try
        {
            var parsed = JsonSerializer.Deserialize<AiCoreAnalysisResponse>(text, JsonOptions);
            if (parsed is not null)
            {
                return parsed with
                {
                    Usage = parsed.Usage ?? new AiUsageDto(0, 0, null, 0, durationMs, model, "Ollama"),
                    RawResponseMetadata = new { provider = "Ollama", model }
                };
            }
        }
        catch (JsonException)
        {
        }

        return new AiCoreAnalysisResponse(
            string.IsNullOrWhiteSpace(content) ? "Local Ollama analysis completed." : content,
            "Info",
            [],
            [],
            new AiUsageDto(0, 0, null, 0, durationMs, model, "Ollama"),
            new { provider = "Ollama", model });
    }

    private static string StripJsonFence(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;
        var firstLineEnd = trimmed.IndexOf('\n');
        if (firstLineEnd < 0) return trimmed;
        var withoutStart = trimmed[(firstLineEnd + 1)..].Trim();
        return withoutStart.EndsWith("```", StringComparison.Ordinal)
            ? withoutStart[..^3].Trim()
            : withoutStart;
    }

    private static string TrimBody(string body) =>
        string.IsNullOrWhiteSpace(body) ? "<empty>" : body.Length <= 1000 ? body : body[..1000];

    private sealed record OllamaGenerateResponse(string? Response);
}
