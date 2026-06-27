using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class RagRetrieverTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PurchaseFileA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PurchaseFileB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task LegalQueryReturnsLegalChunks()
    {
        var service = CreateService(out _);

        var response = await service.RetrieveAsync(
            new RagRetrieveRequest("ضمانت نامه", RagRetrievalScope.LegalCorpus, TopK: 5),
            User(ApplicationPermissions.LegalDocumentView));

        Assert.Single(response.Results);
        Assert.Equal(AiDocumentSourceType.LegalClause, response.Results[0].SourceType);
        Assert.Contains("/api/legal/clauses/", response.Results[0].CitationReference);
        Assert.NotNull(response.Results[0].Text);
    }

    [Fact]
    public async Task PurchaseFileQueryReturnsOnlyChunksForThatFile()
    {
        var service = CreateService(out _);

        var response = await service.RetrieveAsync(
            new RagRetrieveRequest("تحویل کالا", RagRetrievalScope.PurchaseFile, PurchaseFileA, TopK: 10),
            User(ApplicationPermissions.PurchaseFileView));

        Assert.NotEmpty(response.Results);
        Assert.All(response.Results, result => Assert.Equal(PurchaseFileA, (Guid)result.Metadata["purchaseFileId"]!));
    }

    [Fact]
    public async Task UnauthorizedUserCannotRetrieveAnotherFilesChunks()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<CurrentUserForbiddenException>(() => service.RetrieveAsync(
            new RagRetrieveRequest("تحویل کالا", RagRetrievalScope.PurchaseFile, PurchaseFileB, TopK: 5),
            User(ApplicationPermissions.PurchaseFileView)));
    }

    [Fact]
    public async Task TopKIsRespected()
    {
        var service = CreateService(out _);

        var response = await service.RetrieveAsync(
            new RagRetrieveRequest("گزارش", RagRetrievalScope.PurchaseFile, PurchaseFileA, TopK: 2),
            User(ApplicationPermissions.PurchaseFileView));

        Assert.Equal(2, response.Results.Count);
        Assert.True(response.Results[0].Score >= response.Results[1].Score);
    }

    [Fact]
    public async Task EmptyQueryReturnsValidationError()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ArgumentException>(() => service.RetrieveAsync(
            new RagRetrieveRequest("   ", RagRetrievalScope.LegalCorpus, TopK: 5),
            User(ApplicationPermissions.LegalDocumentView)));
    }

    private static RagRetriever CreateService(out FakeRagSearchService search)
    {
        search = new FakeRagSearchService();
        return new RagRetriever(search, new FakeRagAccessDataSource());
    }

    private static RagUserContext User(params string[] permissions) =>
        new(UserId, IsSystemAdmin: false, permissions, DepartmentIds: []);

    private sealed class FakeRagSearchService : IRagSearchService
    {
        private readonly IReadOnlyList<RagSearchResultDto> _items =
        [
            Item(AiDocumentSourceType.LegalClause, Guid.Parse("30000000-0000-0000-0000-000000000001"),
                null, 0.98, "ارائه ضمانت نامه شرکت در مناقصه الزامی است.", "بند 1"),
            Item(AiDocumentSourceType.PurchaseFileDocument, Guid.Parse("40000000-0000-0000-0000-000000000001"),
                PurchaseFileA, 0.91, "شرایط تحویل کالای پرونده الف", "doc-a"),
            Item(AiDocumentSourceType.ReportDocument, Guid.Parse("40000000-0000-0000-0000-000000000002"),
                PurchaseFileA, 0.83, "گزارش نهایی پرونده الف", "report-a"),
            Item(AiDocumentSourceType.TenderDocument, Guid.Parse("40000000-0000-0000-0000-000000000003"),
                PurchaseFileA, 0.76, "اسناد مناقصه پرونده الف", "tender-a"),
            Item(AiDocumentSourceType.PurchaseFileDocument, Guid.Parse("50000000-0000-0000-0000-000000000001"),
                PurchaseFileB, 0.99, "شرایط محرمانه پرونده ب", "doc-b")
        ];

        public Task<IReadOnlyList<RagSearchResultDto>> SearchAsync(string query, string? model, int topK,
            EmbeddingSearchFilter? filter = null, CancellationToken ct = default)
        {
            var hits = _items
                .Where(x => filter?.SourceType is null || x.SourceType == filter.SourceType)
                .Where(x => filter?.PurchaseFileId is null || x.PurchaseFileId == filter.PurchaseFileId)
                .Where(x => filter?.AccessiblePurchaseFileIds is null
                    || x.PurchaseFileId is null
                    || filter.AccessiblePurchaseFileIds.Contains(x.PurchaseFileId.Value))
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToArray();

            return Task.FromResult<IReadOnlyList<RagSearchResultDto>>(hits);
        }

        private static RagSearchResultDto Item(
            AiDocumentSourceType sourceType,
            Guid sourceId,
            Guid? purchaseFileId,
            double score,
            string text,
            string citation)
        {
            var metadata = new Dictionary<string, object?>
            {
                ["title"] = citation,
                ["purchaseFileId"] = purchaseFileId
            };

            return new RagSearchResultDto(
                Guid.NewGuid(),
                sourceType.ToString(),
                sourceId,
                score,
                text,
                citation,
                metadata,
                text,
                purchaseFileId);
        }
    }

    private sealed class FakeRagAccessDataSource : IRagAccessDataSource
    {
        public Task<bool> CanAccessPurchaseFileAsync(
            Guid purchaseFileId, RagUserContext userContext, CancellationToken ct = default) =>
            Task.FromResult(purchaseFileId == PurchaseFileA);

        public Task<IReadOnlySet<Guid>> GetAllowedPurchaseFileIdsAsync(
            RagUserContext userContext, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlySet<Guid>>(new HashSet<Guid> { PurchaseFileA });

        public Task<Guid?> GetTenderPurchaseFileIdAsync(Guid tenderId, CancellationToken ct = default) =>
            Task.FromResult<Guid?>(PurchaseFileA);
    }
}
