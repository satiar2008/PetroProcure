using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Rag;

public sealed record EmbeddingIngestionPayload(
    AiDocumentSourceType SourceType,
    Guid SourceId,
    Guid? PurchaseFileId = null,
    Guid? DocumentId = null,
    bool ForceRebuild = false);

public sealed record RagIngestionResult(int ChunkCount, int EmbeddingCount, int SkippedEmbeddingCount);

public interface IRagIngestionQueue
{
    Task<CreateAiJobResponse> EnqueueAsync(EmbeddingIngestionPayload payload, Guid? createdByUserId,
        CancellationToken ct = default);
}

public interface IRagIngestionService
{
    Task<RagIngestionResult> IngestAsync(EmbeddingIngestionPayload payload, CancellationToken ct = default);
}

public sealed class RagIngestionQueue(IAiJobQueueService queue) : IRagIngestionQueue
{
    public Task<CreateAiJobResponse> EnqueueAsync(EmbeddingIngestionPayload payload, Guid? createdByUserId,
        CancellationToken ct = default)
    {
        var metadata = new Dictionary<string, string>
        {
            ["sourceType"] = payload.SourceType.ToString(),
            ["sourceId"] = payload.SourceId.ToString(),
            ["forceRebuild"] = payload.ForceRebuild.ToString()
        };
        if (payload.PurchaseFileId is { } purchaseFileId)
            metadata["purchaseFileId"] = purchaseFileId.ToString();
        if (payload.DocumentId is { } documentId)
            metadata["documentId"] = documentId.ToString();

        return queue.CreateJobAsync(new CreateAiJobRequest(
            payload.SourceType.ToString(),
            payload.SourceId,
            AiAnalysisType.EmbeddingIngestion.ToString(),
            Priority: 10,
            Metadata: metadata), createdByUserId, ct);
    }
}

public sealed class RagIngestionService(
    IChunkingService chunking,
    IEmbeddingGenerator embeddingGenerator,
    IAiEmbeddingRepository embeddings,
    IOptions<RagOptions> options,
    ILogger<RagIngestionService> logger) : IRagIngestionService
{
    public async Task<RagIngestionResult> IngestAsync(EmbeddingIngestionPayload payload, CancellationToken ct = default)
    {
        var model = string.IsNullOrWhiteSpace(options.Value.EmbeddingModel) ? "default" : options.Value.EmbeddingModel.Trim();
        logger.LogInformation("RAG ingestion started for {SourceType}/{SourceId} (force={ForceRebuild}).",
            payload.SourceType, payload.SourceId, payload.ForceRebuild);

        var chunks = await chunking.RebuildChunksAsync(payload.SourceType, payload.SourceId, ct);
        var embedded = 0;
        var skipped = 0;

        foreach (var chunk in chunks)
        {
            ct.ThrowIfCancellationRequested();
            var existing = await embeddings.GetByChunkIdAsync(chunk.Id, ct);
            if (!payload.ForceRebuild && existing is not null &&
                string.Equals(existing.Model, model, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(existing.ContentHash, chunk.ContentHash, StringComparison.OrdinalIgnoreCase))
            {
                skipped++;
                continue;
            }

            var vector = await embeddingGenerator.GenerateAsync(chunk.Text, model, ct);
            if (vector.Length == 0)
            {
                skipped++;
                continue;
            }

            await embeddings.UpsertAsync(chunk.Id, model, vector, chunk.ContentHash, ct);
            embedded++;
        }

        logger.LogInformation("RAG ingestion completed for {SourceType}/{SourceId}: {ChunkCount} chunks, {Embedded} embeddings, {Skipped} skipped.",
            payload.SourceType, payload.SourceId, chunks.Count, embedded, skipped);
        return new RagIngestionResult(chunks.Count, embedded, skipped);
    }

    public static EmbeddingIngestionPayload ParsePayload(CreateAiJobRequest request)
    {
        var metadata = request.Metadata ?? throw new InvalidOperationException("Embedding ingestion payload is missing.");
        string Required(string key) => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"Embedding ingestion payload is missing '{key}'.");

        return new EmbeddingIngestionPayload(
            Enum.Parse<AiDocumentSourceType>(Required("sourceType"), ignoreCase: true),
            Guid.Parse(Required("sourceId")),
            TryGuid(metadata, "purchaseFileId"),
            TryGuid(metadata, "documentId"),
            metadata.TryGetValue("forceRebuild", out var force) && bool.TryParse(force, out var parsedForce) && parsedForce);
    }

    public static string SerializeResult(RagIngestionResult result) =>
        JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static Guid? TryGuid(IReadOnlyDictionary<string, string> metadata, string key) =>
        metadata.TryGetValue(key, out var value) && Guid.TryParse(value, out var parsed) ? parsed : null;
}
