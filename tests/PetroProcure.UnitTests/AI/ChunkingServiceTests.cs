using PetroProcure.Application.Rag;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class ChunkingServiceTests
{
    [Fact]
    public async Task LegalClauseCreatesOneOrMoreChunks()
    {
        var service = CreateService(out var repository, out _);
        var sourceId = Guid.NewGuid();

        var chunks = await service.ChunkTextAsync(AiDocumentSourceType.LegalClause, sourceId,
            "ارائه ضمانت نامه معتبر برای شرکت در مناقصه الزامی است.",
            new ChunkMetadata(LegalClauseId: sourceId));

        Assert.Single(chunks);
        Assert.Equal(AiDocumentSourceType.LegalClause, chunks[0].SourceType);
        Assert.Equal(sourceId, chunks[0].LegalClauseId);
        Assert.Single(repository.ActiveChunks(sourceId));
    }

    [Fact]
    public async Task LongDocumentCreatesMultipleChunksWithOverlap()
    {
        var service = CreateService(out _, out _);
        var sourceId = Guid.NewGuid();
        var paragraph = string.Join(' ', Enumerable.Range(1, 120).Select(i => $"word{i}"));
        var text = string.Join("\n\n", Enumerable.Range(1, 12).Select(_ => paragraph));

        var chunks = await service.ChunkTextAsync(AiDocumentSourceType.PurchaseFileDocument, sourceId, text,
            new ChunkMetadata(PurchaseFileId: Guid.NewGuid(), DocumentId: sourceId));

        Assert.True(chunks.Count > 1);
        Assert.Contains("word1", chunks[0].Text);
        Assert.Contains("word1", chunks[1].Text);
    }

    [Fact]
    public async Task RebuildChunksAsync_IsIdempotentForSameText()
    {
        var service = CreateService(out var repository, out var sourceProvider);
        var sourceId = Guid.NewGuid();
        sourceProvider.Sources[(AiDocumentSourceType.LegalClause, sourceId)] =
            new CorpusSourceText("متن ثابت بند قانونی", new ChunkMetadata(LegalClauseId: sourceId));

        var first = await service.RebuildChunksAsync(AiDocumentSourceType.LegalClause, sourceId);
        var second = await service.RebuildChunksAsync(AiDocumentSourceType.LegalClause, sourceId);

        Assert.Equal(first.Select(x => x.Id), second.Select(x => x.Id));
        Assert.Single(repository.ActiveChunks(sourceId));
    }

    [Fact]
    public async Task ContentHashChangesWhenTextChanges()
    {
        var service = CreateService(out _, out _);
        var sourceId = Guid.NewGuid();

        var first = await service.ChunkTextAsync(AiDocumentSourceType.PurchaseFileDocument, sourceId,
            "first document text", new ChunkMetadata(DocumentId: sourceId));
        var firstHash = first[0].ContentHash;
        var second = await service.ChunkTextAsync(AiDocumentSourceType.PurchaseFileDocument, sourceId,
            "changed document text", new ChunkMetadata(DocumentId: sourceId));

        Assert.NotEqual(firstHash, second[0].ContentHash);
        Assert.Equal(first[0].Id, second[0].Id);
    }

    [Fact]
    public async Task EmptyTextIsIgnoredSafely()
    {
        var service = CreateService(out var repository, out _);

        var chunks = await service.ChunkTextAsync(AiDocumentSourceType.PurchaseFileDocument, Guid.NewGuid(),
            "   ", null);

        Assert.Empty(chunks);
        Assert.Empty(repository.AllChunks);
    }

    private static ChunkingService CreateService(out FakeChunkRepository repository,
        out FakeCorpusSourceTextProvider sourceProvider)
    {
        repository = new FakeChunkRepository();
        sourceProvider = new FakeCorpusSourceTextProvider();
        return new ChunkingService(repository, sourceProvider);
    }

    private sealed class FakeCorpusSourceTextProvider : ICorpusSourceTextProvider
    {
        public Dictionary<(AiDocumentSourceType SourceType, Guid SourceId), CorpusSourceText> Sources { get; } = [];

        public Task<CorpusSourceText?> GetTextAsync(AiDocumentSourceType sourceType, Guid sourceId,
            CancellationToken ct = default) =>
            Task.FromResult(Sources.TryGetValue((sourceType, sourceId), out var source) ? source : null);
    }

    private sealed class FakeChunkRepository : IAiDocumentChunkRepository
    {
        private readonly List<AiDocumentChunk> _chunks = [];
        public IReadOnlyList<AiDocumentChunk> AllChunks => _chunks;
        public IReadOnlyList<AiDocumentChunk> ActiveChunks(Guid sourceId) =>
            _chunks.Where(x => x.SourceId == sourceId && !x.IsDeleted).OrderBy(x => x.Ordinal).ToArray();

        public Task<IReadOnlyList<AiDocumentChunk>> GetChunksAsync(AiDocumentSourceType sourceType, Guid sourceId,
            bool includeDeleted, CancellationToken ct)
        {
            var chunks = _chunks
                .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
                .Where(x => includeDeleted || !x.IsDeleted)
                .OrderBy(x => x.Ordinal)
                .ToArray();
            return Task.FromResult<IReadOnlyList<AiDocumentChunk>>(chunks);
        }

        public Task<IReadOnlyList<AiDocumentChunk>> GetActiveChunksAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AiDocumentChunk>>(_chunks.Where(x => !x.IsDeleted).ToArray());

        public Task AddAsync(AiDocumentChunk chunk, CancellationToken ct)
        {
            _chunks.Add(chunk);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
