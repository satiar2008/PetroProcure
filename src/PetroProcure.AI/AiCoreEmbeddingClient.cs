using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PetroProcure.Application.Rag;

namespace PetroProcure.AI;

public sealed class AiCoreEmbeddingGenerator(
    HttpClient http,
    IAiCoreSettingsProvider settingsProvider,
    ILogger<AiCoreEmbeddingGenerator> logger) : IEmbeddingGenerator, IEmbeddingClient
{
    public async Task<float[]> GenerateAsync(string text, string? model, CancellationToken ct = default)
    {
        var vectors = await GenerateBatchAsync([text], model, ct);
        return vectors.Count == 0 ? [] : vectors[0];
    }

    public Task<IReadOnlyList<float[]>> GenerateBatchAsync(IReadOnlyList<string> texts, string? model, CancellationToken ct = default) =>
        EmbedAsync(texts, model, ct);

    public async Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, string? model, CancellationToken ct = default)
    {
        if (inputs.Count == 0) return [];

        var settings = await settingsProvider.GetAsync(ct);
        if (!settings.IsEnabled) throw new AiCoreClientException("AiCore provider is disabled.");
        if (string.IsNullOrWhiteSpace(settings.BaseUrl)) throw new AiCoreClientException("AiCore BaseUrl is not configured.");

        using var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl(settings.BaseUrl, "/api/ai/embeddings"));
        ApplyHeaders(message, settings);
        message.Content = new StringContent(
            JsonSerializer.Serialize(new EmbeddingRequest(model, inputs), JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 10, 900)));

        try
        {
            using var response = await http.SendAsync(message, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
                throw Friendly(response.StatusCode, await ReadErrorBodyAsync(response, timeoutCts.Token));

            await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeoutCts.Token);
            var vectors = ParseEmbeddings(document.RootElement);
            if (vectors.Count != inputs.Count)
                throw new AiCoreClientException($"AiCore returned {vectors.Count} embeddings for {inputs.Count} inputs.");

            logger.LogDebug("Received {Count} embeddings from AiCore model {Model}.", vectors.Count, model);
            return vectors;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AiCoreClientException($"AiCore embedding request timed out after {settings.TimeoutSeconds} seconds.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiCoreClientException($"AiCore embedding request failed: {ex.Message}");
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static IReadOnlyList<float[]> ParseEmbeddings(JsonElement root)
    {
        if (TryReadVectorArray(root, out var direct)) return direct;

        if (TryGetProperty(root, "embeddings", out var embeddings) && TryReadVectorArray(embeddings, out var vectors))
            return vectors;

        if (TryGetProperty(root, "data", out var data) && data.ValueKind == JsonValueKind.Array)
        {
            var result = new List<float[]>();
            foreach (var item in data.EnumerateArray())
            {
                if (TryGetProperty(item, "embedding", out var embedding) && TryReadVector(embedding, out var vector))
                    result.Add(vector);
            }
            return result;
        }

        throw new AiCoreClientException("AiCore returned an unsupported embedding response.");
    }

    private static bool TryReadVectorArray(JsonElement element, out IReadOnlyList<float[]> vectors)
    {
        vectors = [];
        if (element.ValueKind != JsonValueKind.Array) return false;

        var result = new List<float[]>();
        foreach (var item in element.EnumerateArray())
        {
            if (!TryReadVector(item, out var vector)) return false;
            result.Add(vector);
        }

        vectors = result;
        return true;
    }

    private static bool TryReadVector(JsonElement element, out float[] vector)
    {
        vector = [];
        if (element.ValueKind != JsonValueKind.Array) return false;

        var result = new List<float>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Number || !item.TryGetSingle(out var value)) return false;
            result.Add(value);
        }

        vector = result.ToArray();
        return vector.Length > 0;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static void ApplyHeaders(HttpRequestMessage message, AiCoreSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            message.Headers.TryAddWithoutValidation("X-AI-API-KEY", settings.ApiKey);
            message.Headers.Authorization = new("Bearer", settings.ApiKey);
        }
        if (!string.IsNullOrWhiteSpace(settings.Tenant))
            message.Headers.TryAddWithoutValidation("X-Tenant", settings.Tenant);
        if (!string.IsNullOrWhiteSpace(settings.ClientId))
            message.Headers.TryAddWithoutValidation("X-Client-Id", settings.ClientId);
    }

    private static string BuildUrl(string baseUrl, string path) =>
        $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    private static async Task<string?> ReadErrorBodyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body)) return null;
            body = body.Replace("\r", " ").Replace("\n", " ").Trim();
            return body.Length <= 1000 ? body : $"{body[..1000]}...";
        }
        catch
        {
            return null;
        }
    }

    private static AiCoreClientException Friendly(HttpStatusCode statusCode, string? responseBody) => new(AddBody(statusCode switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "AiCore embedding authentication failed. Check X-AI-API-KEY secret and client capabilities.",
        HttpStatusCode.NotFound => "AiCore embedding endpoint was not found. Expected /api/ai/embeddings.",
        HttpStatusCode.RequestTimeout => "AiCore embedding request timed out.",
        HttpStatusCode.TooManyRequests => "AiCore embedding rate limit was reached. Please retry later.",
        HttpStatusCode.InternalServerError => "AiCore embedding request failed with status 500. Check that AiCore has an active Embedding model/provider matching PetroProcure:AI:Rag:EmbeddingModel.",
        _ => $"AiCore embedding request failed with status {(int)statusCode}."
    }, responseBody));

    private static string AddBody(string message, string? responseBody) =>
        string.IsNullOrWhiteSpace(responseBody) ? message : $"{message} AiCore response: {responseBody}";

    private sealed record EmbeddingRequest(string? Model, IReadOnlyList<string> Inputs);
}
