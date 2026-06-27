using Microsoft.Extensions.DependencyInjection;
using PetroProcure.Application.Rag;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.IntegrationTests;

[Collection("sqlserver")]
public sealed class EmbeddingStoreSearchTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task SaveEmbedding_PersistsVector()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IAiEmbeddingRepository>();
        var chunk = await AddChunkAsync(db, repository, AiDocumentSourceType.LegalClause, Guid.NewGuid(), [1f, 0f],
            purchaseFileId: null, createEmbedding: false);

        await repository.UpsertAsync(chunk.Id, "model-a", [1f, 0f], "hash-a");

        var embedding = await repository.GetByChunkIdAsync(chunk.Id);
        Assert.NotNull(embedding);
        Assert.Equal("model-a", embedding.Model);
        Assert.Equal(2, embedding.Dimensions);
        Assert.Equal("hash-a", embedding.ContentHash);
    }

    [Fact]
    public async Task UpdateEmbedding_WhenHashChanges_UpdatesExistingRow()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IAiEmbeddingRepository>();
        var chunk = await AddChunkAsync(db, repository, AiDocumentSourceType.LegalClause, Guid.NewGuid(), [1f, 0f],
            purchaseFileId: null, createEmbedding: false);

        await repository.UpsertAsync(chunk.Id, "model-a", [1f, 0f], "hash-a");
        var first = await repository.GetByChunkIdAsync(chunk.Id);
        await repository.UpsertAsync(chunk.Id, "model-a", [0f, 1f], "hash-b");
        var second = await repository.GetByChunkIdAsync(chunk.Id);

        Assert.Equal(first!.Id, second!.Id);
        Assert.Equal("hash-b", second.ContentHash);
        Assert.NotNull(second.UpdatedAt);
    }

    [Fact]
    public async Task Search_ReturnsTopKSortedByScore()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        var index = scope.ServiceProvider.GetRequiredService<IEmbeddingIndex>();
        var repository = scope.ServiceProvider.GetRequiredService<IAiEmbeddingRepository>();
        await AddChunkAsync(db, repository, AiDocumentSourceType.LegalClause, Guid.NewGuid(), [1f, 0f], purchaseFileId: null);
        await AddChunkAsync(db, repository, AiDocumentSourceType.LegalClause, Guid.NewGuid(), [0.7f, 0.3f], purchaseFileId: null);
        await AddChunkAsync(db, repository, AiDocumentSourceType.LegalClause, Guid.NewGuid(), [0f, 1f], purchaseFileId: null);

        var hits = await index.SearchAsync([1f, 0f], topK: 2);

        Assert.Equal(2, hits.Count);
        Assert.True(hits[0].Score >= hits[1].Score);
        Assert.Contains("vector:1,0", hits[0].Text);
    }

    [Fact]
    public async Task Search_RespectsSourceTypeFilter()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        var index = scope.ServiceProvider.GetRequiredService<IEmbeddingIndex>();
        var repository = scope.ServiceProvider.GetRequiredService<IAiEmbeddingRepository>();
        await AddChunkAsync(db, repository, AiDocumentSourceType.LegalClause, Guid.NewGuid(), [1f, 0f], purchaseFileId: null);
        var documentSourceId = Guid.NewGuid();
        await AddChunkAsync(db, repository, AiDocumentSourceType.PurchaseFileDocument, documentSourceId, [1f, 0f], purchaseFileId: Guid.NewGuid());

        var hits = await index.SearchAsync([1f, 0f], 10,
            new EmbeddingSearchFilter(SourceType: AiDocumentSourceType.PurchaseFileDocument.ToString()));

        Assert.Single(hits);
        Assert.Equal(documentSourceId, hits[0].SourceId);
        Assert.Equal(AiDocumentSourceType.PurchaseFileDocument.ToString(), hits[0].SourceType);
    }

    [Fact]
    public async Task Search_RespectsPurchaseFileIdFilter()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
        var index = scope.ServiceProvider.GetRequiredService<IEmbeddingIndex>();
        var repository = scope.ServiceProvider.GetRequiredService<IAiEmbeddingRepository>();
        var wantedPurchaseFileId = Guid.NewGuid();
        await AddChunkAsync(db, repository, AiDocumentSourceType.PurchaseFileDocument, Guid.NewGuid(), [1f, 0f], wantedPurchaseFileId);
        await AddChunkAsync(db, repository, AiDocumentSourceType.PurchaseFileDocument, Guid.NewGuid(), [1f, 0f], Guid.NewGuid());

        var hits = await index.SearchAsync([1f, 0f], 10,
            new EmbeddingSearchFilter(PurchaseFileId: wantedPurchaseFileId));

        Assert.Single(hits);
        Assert.Equal(wantedPurchaseFileId, hits[0].PurchaseFileId);
    }

    [Fact]
    public async Task MetadataUpsert_MapsPurchaseFileId_AndFilterReturnsOnlyMatching()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var index = scope.ServiceProvider.GetRequiredService<IEmbeddingIndex>();
        var wantedPurchaseFileId = Guid.NewGuid();
        var wantedSourceId = Guid.NewGuid();

        // Upsert through the metadata path (the interface that previously dropped PurchaseFileId).
        await index.UpsertAsync(Guid.NewGuid(), [1f, 0f], new EmbeddingChunkMetadata(
            SourceType: AiDocumentSourceType.PurchaseFileDocument.ToString(),
            SourceId: wantedSourceId, Ordinal: 0, Text: "matching document", TokenCount: 2,
            ContentHash: Guid.NewGuid().ToString("N"), Model: "model-a",
            PurchaseFileId: wantedPurchaseFileId, DocumentId: wantedSourceId));

        await index.UpsertAsync(Guid.NewGuid(), [1f, 0f], new EmbeddingChunkMetadata(
            SourceType: AiDocumentSourceType.PurchaseFileDocument.ToString(),
            SourceId: Guid.NewGuid(), Ordinal: 0, Text: "other document", TokenCount: 2,
            ContentHash: Guid.NewGuid().ToString("N"), Model: "model-a",
            PurchaseFileId: Guid.NewGuid(), DocumentId: Guid.NewGuid()));

        var hits = await index.SearchAsync([1f, 0f], 10,
            new EmbeddingSearchFilter(PurchaseFileId: wantedPurchaseFileId));

        Assert.Single(hits);
        Assert.Equal(wantedPurchaseFileId, hits[0].PurchaseFileId);
        Assert.Equal(wantedSourceId, hits[0].SourceId);
    }

    private static async Task<AiDocumentChunk> AddChunkAsync(PetroProcureDbContext db, IAiEmbeddingRepository repository,
        AiDocumentSourceType sourceType, Guid sourceId, float[] vector, Guid? purchaseFileId,
        bool createEmbedding = true)
    {
        var text = $"vector:{string.Join(',', vector.Select(x => x.ToString("0.###")))}";
        var chunk = new AiDocumentChunk(Guid.NewGuid(), sourceType, sourceId, 0, text, 1,
            Guid.NewGuid().ToString("N"), """{"appliesTo":"PurchaseFile","tags":"test,embedding"}""",
            purchaseFileId, sourceType == AiDocumentSourceType.LegalClause ? null : sourceId,
            sourceType == AiDocumentSourceType.LegalClause ? sourceId : null);
        await db.AiDocumentChunks.AddAsync(chunk);
        await db.SaveChangesAsync();

        if (createEmbedding)
            await repository.UpsertAsync(chunk.Id, "model-a", vector, chunk.ContentHash);
        return chunk;
    }
}
