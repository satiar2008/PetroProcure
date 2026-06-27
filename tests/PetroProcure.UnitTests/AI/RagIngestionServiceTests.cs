using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Rag;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class RagIngestionServiceTests
{
    [Fact]
    public async Task DuplicateContentDoesNotGenerateDuplicateEmbeddings()
    {
        var chunks = new FakeChunkingService();
        var embeddings = new FakeEmbeddingRepository();
        var generator = new FakeEmbeddingGenerator();
        var service = new RagIngestionService(chunks, generator, embeddings,
            Options.Create(new RagOptions { EmbeddingModel = "model-a" }),
            NullLogger<RagIngestionService>.Instance);
        var payload = new EmbeddingIngestionPayload(AiDocumentSourceType.LegalClause, Guid.NewGuid());

        var first = await service.IngestAsync(payload);
        var second = await service.IngestAsync(payload);

        Assert.Equal(1, first.EmbeddingCount);
        Assert.Equal(0, second.EmbeddingCount);
        Assert.Equal(1, second.SkippedEmbeddingCount);
        Assert.Equal(1, generator.CallCount);
        Assert.Single(embeddings.Items);
    }

    private sealed class FakeChunkingService : IChunkingService
    {
        private readonly Guid _chunkId = Guid.NewGuid();
        private const string Hash = "same-content";

        public Task<IReadOnlyList<AiDocumentChunk>> ChunkTextAsync(AiDocumentSourceType sourceType, Guid sourceId,
            string text, ChunkMetadata? metadata, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<AiDocumentChunk>> RebuildChunksAsync(AiDocumentSourceType sourceType, Guid sourceId,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<AiDocumentChunk>>([
                new AiDocumentChunk(_chunkId, sourceType, sourceId, 0, "stable text", 2, Hash, null)
            ]);

        public IReadOnlyList<AiDocumentChunk> ActiveChunks(AiDocumentSourceType sourceType, Guid sourceId) =>
        [
            new AiDocumentChunk(_chunkId, sourceType, sourceId, 0, "stable text", 2, Hash, null)
        ];
    }

    private sealed class FakeEmbeddingGenerator : IEmbeddingGenerator
    {
        public int CallCount { get; private set; }
        public Task<float[]> GenerateAsync(string text, string? model, CancellationToken ct = default)
        {
            CallCount++;
            return Task.FromResult(new[] { 1f, 0f });
        }

        public Task<IReadOnlyList<float[]>> GenerateBatchAsync(IReadOnlyList<string> texts, string? model,
            CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<float[]>>(texts.Select(_ => new[] { 1f, 0f }).ToArray());
    }

    private sealed class FakeEmbeddingRepository : IAiEmbeddingRepository
    {
        public Dictionary<Guid, AiEmbedding> Items { get; } = [];

        public Task<AiEmbedding?> GetByChunkIdAsync(Guid chunkId, CancellationToken ct = default) =>
            Task.FromResult(Items.TryGetValue(chunkId, out var embedding) ? embedding : null);

        public Task UpsertAsync(Guid chunkId, string model, float[] vector, string contentHash, CancellationToken ct = default)
        {
            Items[chunkId] = new AiEmbedding(Guid.NewGuid(), chunkId, model, "[1,0]", vector.Length, contentHash);
            return Task.CompletedTask;
        }

        public Task DeleteByChunkIdAsync(Guid chunkId, CancellationToken ct = default)
        {
            Items.Remove(chunkId);
            return Task.CompletedTask;
        }
    }
}
