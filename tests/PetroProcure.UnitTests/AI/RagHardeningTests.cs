using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class RagHardeningTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ClauseId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task QueryEmbeddingCacheReusesVectorWhenEnabled()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var generator = new CountingEmbeddingGenerator();
        var index = new CapturingEmbeddingIndex();
        var service = new RagSearchService(generator, index,
            Options.Create(new RagOptions
            {
                EmbeddingModel = "model-a",
                EnableQueryEmbeddingCache = true,
                QueryEmbeddingCacheMinutes = 10
            }),
            cache);

        await service.SearchAsync("ضمانت نامه", null, 5);
        await service.SearchAsync("ضمانت نامه", null, 5);

        Assert.Equal(1, generator.GenerateCallCount);
        Assert.Equal("model-a", index.LastFilter?.Model);
    }

    [Fact]
    public async Task ModelChangeRequiresReindex()
    {
        var chunks = new FakeChunkRepository();
        var chunk = new AiDocumentChunk(Guid.NewGuid(), AiDocumentSourceType.LegalClause, ClauseId, 0,
            "متن بند ضمانت", 4, "hash-a", null);
        chunks.Items.Add(chunk);
        var embeddings = new FakeEmbeddingRepository();
        await embeddings.UpsertAsync(chunk.Id, "old-model", [1f, 0f], "hash-a");
        var service = new RagMaintenanceService(chunks, embeddings, new CountingEmbeddingGenerator(),
            Options.Create(new RagOptions { EmbeddingModel = "new-model" }),
            NullLogger<RagMaintenanceService>.Instance);

        var status = await service.GetEmbeddingModelStatusAsync();

        Assert.True(status.ReindexRequired);
        Assert.Equal(1, status.MismatchedModelCount);
    }

    [Fact]
    public async Task ReindexEmbedsChunksWithPinnedModel()
    {
        var chunks = new FakeChunkRepository();
        var chunk = new AiDocumentChunk(Guid.NewGuid(), AiDocumentSourceType.LegalClause, ClauseId, 0,
            "متن بند ضمانت", 4, "hash-a", null);
        chunks.Items.Add(chunk);
        var embeddings = new FakeEmbeddingRepository();
        var generator = new CountingEmbeddingGenerator();
        var service = new RagMaintenanceService(chunks, embeddings, generator,
            Options.Create(new RagOptions { EmbeddingModel = "model-b", EmbeddingBatchSize = 8 }),
            NullLogger<RagMaintenanceService>.Instance);

        var result = await service.ReindexAsync(force: false);

        Assert.Equal(1, result.EmbeddedCount);
        Assert.Equal(1, generator.BatchCallCount);
        Assert.Equal("model-b", embeddings.Items[chunk.Id].Model);
    }

    [Fact]
    public async Task QualityEvaluatorCalculatesPrecisionAtK()
    {
        var evaluator = new RagQualityEvaluator(new FakeRetriever());

        var result = await evaluator.EvaluateAsync(new RagQualityEvaluationRequest([
            new GoldenRagQuestion("ضمانت نامه", ExpectedLegalClauseId: ClauseId, Scope: RagRetrievalScope.LegalCorpus),
            new GoldenRagQuestion("سند ناموجود", ExpectNoAnswer: true, Scope: RagRetrievalScope.LegalCorpus)
        ], TopK: 3), User(ApplicationPermissions.LegalDocumentView));

        Assert.Equal(2, result.TotalQuestions);
        Assert.Equal(1d, result.PrecisionAtK);
        Assert.Equal(1d, result.CitationAccuracy);
        Assert.Equal(1d, result.NoAnswerCorrectness);
    }

    private static RagUserContext User(params string[] permissions) =>
        new(UserId, IsSystemAdmin: false, permissions, DepartmentIds: []);

    private sealed class CountingEmbeddingGenerator : IEmbeddingGenerator
    {
        public int GenerateCallCount { get; private set; }
        public int BatchCallCount { get; private set; }

        public Task<float[]> GenerateAsync(string text, string? model, CancellationToken ct = default)
        {
            GenerateCallCount++;
            return Task.FromResult(new[] { 1f, 0f });
        }

        public Task<IReadOnlyList<float[]>> GenerateBatchAsync(IReadOnlyList<string> texts, string? model,
            CancellationToken ct = default)
        {
            BatchCallCount++;
            return Task.FromResult<IReadOnlyList<float[]>>(texts.Select(_ => new[] { 1f, 0f }).ToArray());
        }
    }

    private sealed class CapturingEmbeddingIndex : IEmbeddingIndex
    {
        public EmbeddingSearchFilter? LastFilter { get; private set; }

        public Task UpsertAsync(Guid chunkId, float[] vector, EmbeddingChunkMetadata metadata, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<EmbeddingSearchHit>> SearchAsync(float[] queryVector, int topK,
            EmbeddingSearchFilter? filter = null, CancellationToken ct = default)
        {
            LastFilter = filter;
            return Task.FromResult<IReadOnlyList<EmbeddingSearchHit>>([
                new EmbeddingSearchHit(Guid.NewGuid(), AiDocumentSourceType.LegalClause.ToString(), ClauseId,
                    0, "متن بند ضمانت", 0.9, null, """{"clauseNumber":"1"}""", null, ClauseId)
            ]);
        }

        public Task DeleteAsync(Guid chunkId, CancellationToken ct = default) => Task.CompletedTask;
        public Task<IReadOnlyList<EmbeddingFingerprint>> ListFingerprintsAsync(string sourceType, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<EmbeddingFingerprint>>([]);
    }

    private sealed class FakeChunkRepository : IAiDocumentChunkRepository
    {
        public List<AiDocumentChunk> Items { get; } = [];

        public Task<IReadOnlyList<AiDocumentChunk>> GetChunksAsync(AiDocumentSourceType sourceType, Guid sourceId,
            bool includeDeleted, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AiDocumentChunk>>(Items
                .Where(x => x.SourceType == sourceType && x.SourceId == sourceId && (includeDeleted || !x.IsDeleted))
                .ToArray());

        public Task<IReadOnlyList<AiDocumentChunk>> GetActiveChunksAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AiDocumentChunk>>(Items.Where(x => !x.IsDeleted).ToArray());

        public Task AddAsync(AiDocumentChunk chunk, CancellationToken ct)
        {
            Items.Add(chunk);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class FakeEmbeddingRepository : IAiEmbeddingRepository
    {
        public Dictionary<Guid, AiEmbedding> Items { get; } = [];

        public Task<AiEmbedding?> GetByChunkIdAsync(Guid chunkId, CancellationToken ct = default) =>
            Task.FromResult(Items.TryGetValue(chunkId, out var embedding) ? embedding : null);

        public Task UpsertAsync(Guid chunkId, string model, float[] vector, string contentHash, CancellationToken ct = default)
        {
            if (Items.TryGetValue(chunkId, out var existing))
                existing.Update(model, "[1,0]", vector.Length, contentHash);
            else
                Items[chunkId] = new AiEmbedding(Guid.NewGuid(), chunkId, model, "[1,0]", vector.Length, contentHash);
            return Task.CompletedTask;
        }

        public Task DeleteByChunkIdAsync(Guid chunkId, CancellationToken ct = default)
        {
            Items.Remove(chunkId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRetriever : IRagRetriever
    {
        public Task<RagRetrieveResponse> RetrieveAsync(
            string query, RagRetrievalScope scope, int topK, RagUserContext userContext, CancellationToken ct = default) =>
            RetrieveAsync(new RagRetrieveRequest(query, scope, TopK: topK), userContext, ct);

        public Task<RagRetrieveResponse> RetrieveAsync(
            RagRetrieveRequest request, RagUserContext userContext, CancellationToken ct = default)
        {
            if (request.Query.Contains("ناموجود", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(new RagRetrieveResponse(request.Query, []));

            return Task.FromResult(new RagRetrieveResponse(request.Query, [
                new RagRetrieveResultDto(0.91, "متن بند", "متن کامل بند", AiDocumentSourceType.LegalClause,
                    ClauseId, "Clause 1", "/api/legal/clauses/1/context", new Dictionary<string, object?>(),
                    Guid.NewGuid())
            ]));
        }
    }
}
