using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Rag;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

// AI-RAG-04: brute-force cosine implementation of IEmbeddingIndex over SQL Server.
// Stores chunks + vectors as rows and scores candidates in memory. Replaceable with a
// native vector store (SQL Server Vector / Qdrant) behind the same interface.
internal sealed class BruteForceEmbeddingIndex(PetroProcureDbContext db) : IEmbeddingIndex, IAiEmbeddingRepository
{
    public Task<AiEmbedding?> GetByChunkIdAsync(Guid chunkId, CancellationToken ct = default) =>
        db.AiEmbeddings.SingleOrDefaultAsync(x => x.ChunkId == chunkId, ct);

    public async Task UpsertAsync(Guid chunkId, string model, float[] vector, string contentHash, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vector);
        var vectorJson = JsonSerializer.Serialize(vector);
        var embedding = await db.AiEmbeddings.FirstOrDefaultAsync(e => e.ChunkId == chunkId, ct);
        if (embedding is null)
        {
            embedding = new AiEmbedding(Guid.NewGuid(), chunkId, model, vectorJson, vector.Length, contentHash);
            await db.AiEmbeddings.AddAsync(embedding, ct);
        }
        else
        {
            embedding.Update(model, vectorJson, vector.Length, contentHash);
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteByChunkIdAsync(Guid chunkId, CancellationToken ct = default)
    {
        var embedding = await db.AiEmbeddings.FirstOrDefaultAsync(e => e.ChunkId == chunkId, ct);
        if (embedding is null) return;
        db.AiEmbeddings.Remove(embedding);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpsertAsync(Guid chunkId, float[] vector, EmbeddingChunkMetadata metadata, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vector);
        ArgumentNullException.ThrowIfNull(metadata);

        var metadataSourceType = ParseSourceType(metadata.SourceType);
        var isLegalClause = metadataSourceType == AiDocumentSourceType.LegalClause;
        // Honour explicit ids from metadata; otherwise infer from source type for backward compatibility.
        var legalClauseId = metadata.LegalClauseId ?? (isLegalClause ? metadata.SourceId : null);
        var documentId = metadata.DocumentId ?? (isLegalClause ? null : metadata.SourceId);
        var purchaseFileId = metadata.PurchaseFileId;

        var chunk = await db.AiDocumentChunks
            .FirstOrDefaultAsync(c => c.SourceType == metadataSourceType
                && c.SourceId == metadata.SourceId && c.Ordinal == metadata.Ordinal, ct);

        if (chunk is null)
        {
            chunk = new AiDocumentChunk(chunkId, metadataSourceType, metadata.SourceId, metadata.Ordinal,
                metadata.Text, metadata.TokenCount, metadata.ContentHash, metadata.MetadataJson,
                purchaseFileId, documentId, legalClauseId);
            await db.AiDocumentChunks.AddAsync(chunk, ct);
        }
        else
        {
            chunk.Update(metadata.Text, metadata.TokenCount, metadata.ContentHash, metadata.MetadataJson,
                purchaseFileId, documentId, legalClauseId);
        }

        await db.SaveChangesAsync(ct);
        await UpsertAsync(chunk.Id, metadata.Model, vector, metadata.ContentHash, ct);
    }

    public async Task<IReadOnlyList<EmbeddingSearchHit>> SearchAsync(float[] queryVector, int topK,
        EmbeddingSearchFilter? filter = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(queryVector);
        if (queryVector.Length == 0 || topK <= 0) return [];

        var query =
            from e in db.AiEmbeddings.AsNoTracking()
            join c in db.AiDocumentChunks.AsNoTracking().Where(x => !x.IsDeleted) on e.ChunkId equals c.Id
            select new
            {
                e.VectorJson,
                e.Dimensions,
                e.Model,
                c.Id,
                c.SourceType,
                c.SourceId,
                c.PurchaseFileId,
                c.LegalClauseId,
                c.Ordinal,
                c.Text,
                c.MetadataJson
            };

        if (filter?.SourceType is { Length: > 0 } sourceType)
        {
            var parsedSourceType = ParseSourceType(sourceType);
            query = query.Where(x => x.SourceType == parsedSourceType);
        }
        if (filter?.Model is { Length: > 0 } model)
            query = query.Where(x => x.Model == model);
        if (filter?.PurchaseFileId is { } purchaseFileId)
            query = query.Where(x => x.PurchaseFileId == purchaseFileId);
        if (filter?.LegalClauseId is { } legalClauseId)
            query = query.Where(x => x.LegalClauseId == legalClauseId);
        if (filter?.AccessiblePurchaseFileIds is { Count: > 0 } allowed)
            query = query.Where(x => x.PurchaseFileId == null || allowed.Contains(x.PurchaseFileId.Value));

        var candidates = await query.ToListAsync(ct);
        var scored = new List<EmbeddingSearchHit>(candidates.Count);
        foreach (var candidate in candidates)
        {
            if (candidate.Dimensions != queryVector.Length) continue; // incompatible model/dimension
            var appliesTo = ReadAppliesTo(candidate.MetadataJson);
            if (filter?.AppliesTo is { Length: > 0 } wanted &&
                !string.Equals(appliesTo, wanted, StringComparison.OrdinalIgnoreCase))
                continue;
            if (filter?.Tags is { Length: > 0 } tags && !MetadataContainsTag(candidate.MetadataJson, tags))
                continue;

            var vector = JsonSerializer.Deserialize<float[]>(candidate.VectorJson);
            if (vector is null || vector.Length != queryVector.Length) continue;

            var score = CosineSimilarity.Compute(queryVector, vector);
            scored.Add(new EmbeddingSearchHit(candidate.Id, candidate.SourceType.ToString(), candidate.SourceId,
                candidate.Ordinal, candidate.Text, score, appliesTo, candidate.MetadataJson,
                candidate.PurchaseFileId, candidate.LegalClauseId));
        }

        return scored.OrderByDescending(x => x.Score).Take(topK).ToList();
    }

    public async Task DeleteAsync(Guid chunkId, CancellationToken ct = default)
    {
        var embedding = await db.AiEmbeddings.FirstOrDefaultAsync(e => e.ChunkId == chunkId, ct);
        if (embedding is not null) db.AiEmbeddings.Remove(embedding);
        var chunk = await db.AiDocumentChunks.FirstOrDefaultAsync(c => c.Id == chunkId, ct);
        if (chunk is not null) db.AiDocumentChunks.Remove(chunk);
        if (embedding is not null || chunk is not null) await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<EmbeddingFingerprint>> ListFingerprintsAsync(string sourceType, CancellationToken ct = default)
    {
        var parsedSourceType = ParseSourceType(sourceType);
        return await (
            from e in db.AiEmbeddings.AsNoTracking()
            join c in db.AiDocumentChunks.AsNoTracking() on e.ChunkId equals c.Id
            where c.SourceType == parsedSourceType
            select new EmbeddingFingerprint(c.Id, c.SourceType.ToString(), c.SourceId, c.Ordinal, e.ContentHash, e.Model))
            .ToListAsync(ct);
    }

    private static AiDocumentSourceType ParseSourceType(string sourceType) =>
        Enum.TryParse<AiDocumentSourceType>(sourceType, ignoreCase: true, out var value)
            ? value
            : throw new ArgumentException("Unsupported source type.", nameof(sourceType));

    private static string? ReadAppliesTo(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            foreach (var prop in doc.RootElement.EnumerateObject())
                if (string.Equals(prop.Name, "appliesTo", StringComparison.OrdinalIgnoreCase)
                    && prop.Value.ValueKind == JsonValueKind.String)
                    return prop.Value.GetString();
        }
        catch (JsonException)
        {
        }
        return null;
    }

    private static bool MetadataContainsTag(string? metadataJson, string wantedTag)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return false;
        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (!string.Equals(prop.Name, "tags", StringComparison.OrdinalIgnoreCase)) continue;
                if (prop.Value.ValueKind == JsonValueKind.String)
                    return prop.Value.GetString()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Any(x => string.Equals(x, wantedTag, StringComparison.OrdinalIgnoreCase)) == true;
                if (prop.Value.ValueKind == JsonValueKind.Array)
                    return prop.Value.EnumerateArray().Any(x => x.ValueKind == JsonValueKind.String
                        && string.Equals(x.GetString(), wantedTag, StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (JsonException)
        {
        }

        return false;
    }
}
