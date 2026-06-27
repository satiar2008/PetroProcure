using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Rag;

public interface IChunkingService
{
    Task<IReadOnlyList<AiDocumentChunk>> ChunkTextAsync(AiDocumentSourceType sourceType, Guid sourceId,
        string text, ChunkMetadata? metadata, CancellationToken ct = default);

    Task<IReadOnlyList<AiDocumentChunk>> RebuildChunksAsync(AiDocumentSourceType sourceType, Guid sourceId,
        CancellationToken ct = default);
}

public interface IAiDocumentChunkRepository
{
    Task<IReadOnlyList<AiDocumentChunk>> GetChunksAsync(AiDocumentSourceType sourceType, Guid sourceId,
        bool includeDeleted, CancellationToken ct);
    Task<IReadOnlyList<AiDocumentChunk>> GetActiveChunksAsync(CancellationToken ct);
    Task AddAsync(AiDocumentChunk chunk, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface ICorpusSourceTextProvider
{
    Task<CorpusSourceText?> GetTextAsync(AiDocumentSourceType sourceType, Guid sourceId, CancellationToken ct = default);
}

public sealed record ChunkMetadata(
    Guid? PurchaseFileId = null,
    Guid? DocumentId = null,
    Guid? LegalClauseId = null,
    IReadOnlyDictionary<string, object?>? Values = null);

public sealed record CorpusSourceText(string Text, ChunkMetadata? Metadata = null);

public sealed class ChunkingService(
    IAiDocumentChunkRepository repository,
    ICorpusSourceTextProvider sourceTextProvider) : IChunkingService
{
    private const int TargetTokens = 800;
    private const int MinimumTokens = 500;
    private const int OverlapTokens = 120;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<AiDocumentChunk>> ChunkTextAsync(AiDocumentSourceType sourceType, Guid sourceId,
        string text, ChunkMetadata? metadata, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var chunks = SplitIntoChunks(text)
            .Select((chunkText, ordinal) => CreateChunk(sourceType, sourceId, ordinal, chunkText, metadata))
            .ToArray();

        if (chunks.Length == 0) return [];

        var existing = await repository.GetChunksAsync(sourceType, sourceId, includeDeleted: true, ct);
        var byOrdinal = existing.ToDictionary(x => x.Ordinal);

        foreach (var chunk in chunks)
        {
            if (byOrdinal.TryGetValue(chunk.Ordinal, out var current))
            {
                current.Update(chunk.Text, chunk.TokenCount, chunk.ContentHash, chunk.MetadataJson,
                    chunk.PurchaseFileId, chunk.DocumentId, chunk.LegalClauseId);
            }
            else
            {
                await repository.AddAsync(chunk, ct);
            }
        }

        foreach (var obsolete in existing.Where(x => x.Ordinal >= chunks.Length && !x.IsDeleted))
            obsolete.SoftDelete();

        await repository.SaveChangesAsync(ct);
        return (await repository.GetChunksAsync(sourceType, sourceId, includeDeleted: false, ct))
            .OrderBy(x => x.Ordinal)
            .ToArray();
    }

    public async Task<IReadOnlyList<AiDocumentChunk>> RebuildChunksAsync(AiDocumentSourceType sourceType,
        Guid sourceId, CancellationToken ct = default)
    {
        var source = await sourceTextProvider.GetTextAsync(sourceType, sourceId, ct);
        return source is null
            ? []
            : await ChunkTextAsync(sourceType, sourceId, source.Text, source.Metadata, ct);
    }

    private static AiDocumentChunk CreateChunk(AiDocumentSourceType sourceType, Guid sourceId, int ordinal,
        string text, ChunkMetadata? metadata)
    {
        var trimmed = text.Trim();
        var metadataJson = metadata?.Values is null ? null : JsonSerializer.Serialize(metadata.Values, JsonOptions);
        var legalClauseId = metadata?.LegalClauseId ?? (sourceType == AiDocumentSourceType.LegalClause ? sourceId : null);
        var documentId = metadata?.DocumentId ?? (sourceType != AiDocumentSourceType.LegalClause ? sourceId : null);

        return new AiDocumentChunk(Guid.NewGuid(), sourceType, sourceId, ordinal, trimmed, EstimateTokens(trimmed),
            Hash(trimmed), metadataJson, metadata?.PurchaseFileId, documentId, legalClauseId);
    }

    private static IReadOnlyList<string> SplitIntoChunks(string text)
    {
        var paragraphs = text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        if (paragraphs.Length == 0) return [];

        var result = new List<string>();
        var current = new List<string>();
        var currentTokens = 0;

        foreach (var paragraph in paragraphs)
        {
            var paragraphTokens = EstimateTokens(paragraph);
            if (paragraphTokens > TargetTokens)
            {
                Flush();
                AddLongParagraph(paragraph, result);
                continue;
            }

            if (currentTokens >= MinimumTokens && currentTokens + paragraphTokens > TargetTokens)
                Flush();

            current.Add(paragraph);
            currentTokens += paragraphTokens;
        }

        Flush();
        return result;

        void Flush()
        {
            if (current.Count == 0) return;
            var chunk = string.Join("\n\n", current).Trim();
            if (!string.IsNullOrWhiteSpace(chunk)) result.Add(chunk);

            var overlap = TakeOverlap(current);
            current.Clear();
            current.AddRange(overlap);
            currentTokens = current.Sum(EstimateTokens);
        }
    }

    private static void AddLongParagraph(string paragraph, List<string> result)
    {
        var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length == 0) return;

        var step = Math.Max(1, TargetTokens - OverlapTokens);
        for (var start = 0; start < words.Length; start += step)
        {
            var slice = words.Skip(start).Take(TargetTokens).ToArray();
            if (slice.Length == 0) break;
            result.Add(string.Join(' ', slice));
            if (start + TargetTokens >= words.Length) break;
        }
    }

    private static IReadOnlyList<string> TakeOverlap(IReadOnlyList<string> paragraphs)
    {
        var overlap = new List<string>();
        var tokens = 0;
        for (var i = paragraphs.Count - 1; i >= 0 && tokens < OverlapTokens; i--)
        {
            overlap.Insert(0, paragraphs[i]);
            tokens += EstimateTokens(paragraphs[i]);
        }
        return overlap;
    }

    private static int EstimateTokens(string text) =>
        Math.Max(1, text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

    private static string Hash(string text)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }
}
