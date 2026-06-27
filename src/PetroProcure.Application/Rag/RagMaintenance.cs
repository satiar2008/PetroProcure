using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Rag;

public sealed record RagEmbeddingModelStatus(string CurrentModel, int TotalEmbeddings,
    int MatchingModelCount, int MismatchedModelCount, bool ReindexRequired);

public sealed record RagReindexResult(string Model, int ChunkCount, int EmbeddedCount, int SkippedCount);

public interface IRagMaintenanceService
{
    Task<RagEmbeddingModelStatus> GetEmbeddingModelStatusAsync(CancellationToken ct = default);
    Task<RagReindexResult> ReindexAsync(bool force, CancellationToken ct = default);
}

public sealed class RagMaintenanceService(
    IAiDocumentChunkRepository chunks,
    IAiEmbeddingRepository embeddings,
    IEmbeddingGenerator generator,
    IOptions<RagOptions> options,
    ILogger<RagMaintenanceService> logger) : IRagMaintenanceService
{
    public async Task<RagEmbeddingModelStatus> GetEmbeddingModelStatusAsync(CancellationToken ct = default)
    {
        var model = ResolveModel();
        var activeChunks = await chunks.GetActiveChunksAsync(ct);
        var matching = 0;
        var mismatched = 0;

        foreach (var chunk in activeChunks)
        {
            var embedding = await embeddings.GetByChunkIdAsync(chunk.Id, ct);
            if (embedding is null || !string.Equals(embedding.Model, model, StringComparison.OrdinalIgnoreCase))
                mismatched++;
            else
                matching++;
        }

        return new RagEmbeddingModelStatus(model, activeChunks.Count, matching, mismatched, mismatched > 0);
    }

    // NOTE: Reindex re-embeds chunks that ALREADY exist (e.g. after an embedding-model change).
    // It does NOT build the corpus from sources. Initial/whole-corpus population must enqueue
    // ingestion per source via IRagIngestionQueue (see docs/ai/rag-operations.md, "Build initial corpus").
    public async Task<RagReindexResult> ReindexAsync(bool force, CancellationToken ct = default)
    {
        var model = ResolveModel();
        var activeChunks = await chunks.GetActiveChunksAsync(ct);
        var embedded = 0;
        var skipped = 0;

        foreach (var batch in Batch(activeChunks, Math.Max(1, options.Value.EmbeddingBatchSize)))
        {
            var pending = new List<AiDocumentChunk>();
            foreach (var chunk in batch)
            {
                var existing = await embeddings.GetByChunkIdAsync(chunk.Id, ct);
                if (!force && existing is not null
                    && string.Equals(existing.Model, model, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(existing.ContentHash, chunk.ContentHash, StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    continue;
                }

                pending.Add(chunk);
            }

            if (pending.Count == 0) continue;
            var vectors = await generator.GenerateBatchAsync(pending.Select(x => x.Text).ToArray(), model, ct);
            if (vectors.Count != pending.Count)
                throw new InvalidOperationException($"Embedding provider returned {vectors.Count} vectors for {pending.Count} chunks.");

            for (var i = 0; i < pending.Count; i++)
            {
                await embeddings.UpsertAsync(pending[i].Id, model, vectors[i], pending[i].ContentHash, ct);
                embedded++;
            }
        }

        logger.LogInformation("RAG reindex completed for model {Model}: {Chunks} chunks, {Embedded} embedded, {Skipped} skipped.",
            model, activeChunks.Count, embedded, skipped);
        return new RagReindexResult(model, activeChunks.Count, embedded, skipped);
    }

    private string ResolveModel() =>
        string.IsNullOrWhiteSpace(options.Value.EmbeddingModel) ? "default" : options.Value.EmbeddingModel.Trim();

    private static IEnumerable<IReadOnlyList<T>> Batch<T>(IReadOnlyList<T> items, int size)
    {
        for (var i = 0; i < items.Count; i += size)
            yield return items.Skip(i).Take(size).ToArray();
    }
}
