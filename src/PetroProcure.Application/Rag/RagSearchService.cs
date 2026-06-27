using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace PetroProcure.Application.Rag;

public interface IRagSearchService
{
    Task<IReadOnlyList<RagSearchResultDto>> SearchAsync(string query, string? model, int topK,
        EmbeddingSearchFilter? filter = null, CancellationToken ct = default);
}

public sealed class RagEmbeddingUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class RagSearchService(
    IEmbeddingGenerator embeddingGenerator,
    IEmbeddingIndex index,
    IOptions<RagOptions> options,
    IMemoryCache cache) : IRagSearchService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RagOptions _options = options.Value;

    public async Task<IReadOnlyList<RagSearchResultDto>> SearchAsync(string query, string? model, int topK,
        EmbeddingSearchFilter? filter = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var resolvedModel = ResolveModel(model);
        float[] vector;
        try
        {
            vector = await GetQueryVectorAsync(query.Trim(), resolvedModel, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new RagEmbeddingUnavailableException(
                $"Embedding generation failed for model '{resolvedModel}'. Check AiCore embedding provider/model configuration.",
                ex);
        }
        if (vector.Length == 0) return [];

        var effectiveFilter = filter is null
            ? new EmbeddingSearchFilter(Model: resolvedModel)
            : filter with { Model = filter.Model ?? resolvedModel };
        var hits = await index.SearchAsync(vector, Math.Clamp(topK, 1, 50), effectiveFilter, ct);
        return hits.Select(ToDto).ToArray();
    }

    private async Task<float[]> GetQueryVectorAsync(string query, string model, CancellationToken ct)
    {
        if (!_options.EnableQueryEmbeddingCache)
            return await embeddingGenerator.GenerateAsync(query, model, ct);

        var key = $"rag:q:{Hash(model, query)}";
        if (cache.TryGetValue(key, out float[]? cached) && cached is { Length: > 0 })
            return cached;

        var vector = await embeddingGenerator.GenerateAsync(query, model, ct);
        if (vector.Length > 0)
            cache.Set(key, vector, TimeSpan.FromMinutes(Math.Clamp(_options.QueryEmbeddingCacheMinutes, 1, 1440)));
        return vector;
    }

    private string ResolveModel(string? model) =>
        string.IsNullOrWhiteSpace(model)
            ? (string.IsNullOrWhiteSpace(_options.EmbeddingModel) ? "default" : _options.EmbeddingModel.Trim())
            : model.Trim();

    private static string Hash(string model, string query)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{model}\n{query}"));
        return Convert.ToHexString(bytes);
    }

    private static RagSearchResultDto ToDto(EmbeddingSearchHit hit)
    {
        var metadata = ReadMetadata(hit.MetadataJson);
        return new RagSearchResultDto(
            hit.ChunkId,
            hit.SourceType,
            hit.SourceId,
            hit.Score,
            Preview(hit.Text),
            Citation(hit, metadata),
            metadata,
            hit.Text,
            hit.PurchaseFileId,
            hit.LegalClauseId);
    }

    private static string Preview(string text) =>
        text.Length <= 360 ? text : $"{text[..360].Trim()}...";

    private static string Citation(EmbeddingSearchHit hit, IReadOnlyDictionary<string, object?> metadata)
    {
        if (metadata.TryGetValue("clauseNumber", out var clauseNumber) && clauseNumber is not null)
            return $"{hit.SourceType}:{clauseNumber}";
        if (metadata.TryGetValue("originalFileName", out var fileName) && fileName is not null)
            return $"{hit.SourceType}:{fileName}:chunk-{hit.Ordinal + 1}";
        return $"{hit.SourceType}:{hit.SourceId}:chunk-{hit.Ordinal + 1}";
    }

    private static IReadOnlyDictionary<string, object?> ReadMetadata(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new Dictionary<string, object?>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json, JsonOptions)
                ?? new Dictionary<string, object?>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, object?>();
        }
    }
}
