using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Rag;

// AI-RAG-04: storage abstraction for chunk vectors. The current implementation is a brute-force
// cosine search over SQL Server; this interface lets us switch to SQL Server Vector or Qdrant later
// without touching the embedding/ingestion services.
public interface IEmbeddingIndex
{
    // Inserts or updates the chunk and its vector together, keyed by chunk identity.
    Task UpsertAsync(Guid chunkId, float[] vector, EmbeddingChunkMetadata metadata, CancellationToken ct = default);

    // Brute-force top-K nearest neighbours by cosine similarity, honouring an optional filter.
    Task<IReadOnlyList<EmbeddingSearchHit>> SearchAsync(float[] queryVector, int topK,
        EmbeddingSearchFilter? filter = null, CancellationToken ct = default);

    Task DeleteAsync(Guid chunkId, CancellationToken ct = default);

    // Lightweight projection used to decide what needs (re)embedding without loading vectors.
    Task<IReadOnlyList<EmbeddingFingerprint>> ListFingerprintsAsync(string sourceType, CancellationToken ct = default);
}

public interface IAiEmbeddingRepository
{
    Task<AiEmbedding?> GetByChunkIdAsync(Guid chunkId, CancellationToken ct = default);
    Task UpsertAsync(Guid chunkId, string model, float[] vector, string contentHash, CancellationToken ct = default);
    Task DeleteByChunkIdAsync(Guid chunkId, CancellationToken ct = default);
}

public interface IEmbeddingGenerator
{
    Task<float[]> GenerateAsync(string text, string? model, CancellationToken ct = default);
    Task<IReadOnlyList<float[]>> GenerateBatchAsync(IReadOnlyList<string> texts, string? model, CancellationToken ct = default);
}

// Everything the index needs to persist a chunk + its embedding in one call.
public sealed record EmbeddingChunkMetadata(
    string SourceType,
    Guid SourceId,
    int Ordinal,
    string Text,
    int TokenCount,
    string ContentHash,
    string Model,
    string? AppliesTo = null,
    string? MetadataJson = null,
    Guid? PurchaseFileId = null,
    Guid? DocumentId = null,
    Guid? LegalClauseId = null);

public sealed record EmbeddingSearchFilter(
    string? SourceType = null,
    string? Model = null,
    string? AppliesTo = null,
    Guid? PurchaseFileId = null,
    Guid? LegalClauseId = null,
    string? Tags = null,
    IReadOnlySet<Guid>? AccessiblePurchaseFileIds = null);

public sealed record EmbeddingSearchHit(
    Guid ChunkId,
    string SourceType,
    Guid SourceId,
    int Ordinal,
    string Text,
    double Score,
    string? AppliesTo,
    string? MetadataJson,
    Guid? PurchaseFileId = null,
    Guid? LegalClauseId = null);

public sealed record RagSearchResultDto(
    Guid ChunkId,
    string SourceType,
    Guid SourceId,
    double Score,
    string TextPreview,
    string Citation,
    IReadOnlyDictionary<string, object?> Metadata,
    string? Text = null,
    Guid? PurchaseFileId = null,
    Guid? LegalClauseId = null);

public sealed record EmbeddingFingerprint(
    Guid ChunkId,
    string SourceType,
    Guid SourceId,
    int Ordinal,
    string ContentHash,
    string Model);

// AI-RAG-04: produces embeddings via the existing AiCore client. Implemented in the AI layer.
public interface IEmbeddingClient
{
    Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, string? model, CancellationToken ct = default);
}
