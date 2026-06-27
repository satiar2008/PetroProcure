using PetroProcure.Application.Ai;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class GroundedAiAnalysisServiceTests
{
    private static readonly Guid PurchaseFileId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task AnswerIncludesCitation()
    {
        var service = CreateService(new FakeRagRetriever(), new FakeAnswerGenerator("پاسخ مستند"));

        var response = await service.AskAboutFileAsync(PurchaseFileId, "شرایط ضمانت چیست؟", 3);

        Assert.Contains("[C1]", response.Answer);
        Assert.Single(response.Citations);
    }

    [Fact]
    public async Task ModelProvidedCitationIsPreservedAndNotDuplicated()
    {
        // When the model already cites [C1], EnsureCitation must NOT append a second one.
        var service = CreateService(new FakeRagRetriever(), new FakeAnswerGenerator("ضمانت‌نامه الزامی است [C1]"));

        var response = await service.AskAboutFileAsync(PurchaseFileId, "شرایط ضمانت چیست؟", 3);

        Assert.Equal("ضمانت‌نامه الزامی است [C1]", response.Answer);
        Assert.Equal(1, CountOccurrences(response.Answer, "[C1]"));
    }

    private static int CountOccurrences(string text, string token)
    {
        int count = 0, index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0) { count++; index += token.Length; }
        return count;
    }

    [Fact]
    public async Task MissingContextReturnsInsufficientInformation()
    {
        var service = CreateService(new FakeRagRetriever(returnNoChunks: true), new FakeAnswerGenerator("نباید فراخوانی شود"));

        var response = await service.FindRelevantRegulationsAsync("حکم قانونی چیست؟", 3);

        Assert.Contains("کافی نیست", response.Answer);
        Assert.Empty(response.Citations);
    }

    [Fact]
    public async Task RetrievedChunksAreIncludedInPrompt()
    {
        var generator = new FakeAnswerGenerator("بر اساس متن [C1]");
        var service = CreateService(new FakeRagRetriever(), generator);

        await service.AskAboutFileAsync(PurchaseFileId, "شرایط ضمانت چیست؟", 3);

        Assert.NotNull(generator.LastPrompt);
        Assert.Single(generator.LastPrompt!.Chunks);
        Assert.Contains("ضمانت نامه", generator.LastPrompt.Chunks[0].Text);
    }

    [Fact]
    public async Task UserCannotAskAboutInaccessibleFile()
    {
        var service = CreateService(new FakeRagRetriever(throwForbidden: true), new FakeAnswerGenerator("پاسخ"));

        await Assert.ThrowsAsync<CurrentUserForbiddenException>(() =>
            service.AskAboutFileAsync(PurchaseFileId, "این پرونده چیست؟", 3));
    }

    [Fact]
    public async Task EmbeddingProviderFailureReturnsInsufficientInformation()
    {
        var generator = new FakeAnswerGenerator("نباید فراخوانی شود");
        var service = CreateService(new FakeRagRetriever(throwEmbeddingUnavailable: true), generator);

        var response = await service.AskAboutFileAsync(PurchaseFileId, "شرایط ضمانت چیست؟", 3);

        Assert.Contains("کافی نیست", response.Answer);
        Assert.Empty(response.Citations);
        Assert.Null(generator.LastPrompt);
    }

    private static GroundedAiAnalysisService CreateService(
        IRagRetriever retriever,
        IGroundedAiAnswerGenerator generator) =>
        new(retriever, generator, new FakeGroundedDataSource(), new TestCurrentUser(
            UserId,
            isSystemAdmin: true));

    private sealed class FakeRagRetriever(
        bool returnNoChunks = false,
        bool throwForbidden = false,
        bool throwEmbeddingUnavailable = false) : IRagRetriever
    {
        public Task<RagRetrieveResponse> RetrieveAsync(
            string query, RagRetrievalScope scope, int topK, RagUserContext userContext, CancellationToken ct = default) =>
            RetrieveAsync(new RagRetrieveRequest(query, scope, TopK: topK), userContext, ct);

        public Task<RagRetrieveResponse> RetrieveAsync(
            RagRetrieveRequest request, RagUserContext userContext, CancellationToken ct = default)
        {
            if (throwForbidden) throw new CurrentUserForbiddenException("Forbidden.");
            if (throwEmbeddingUnavailable) throw new RagEmbeddingUnavailableException("Embedding unavailable.");
            if (returnNoChunks) return Task.FromResult(new RagRetrieveResponse(request.Query, []));

            return Task.FromResult(new RagRetrieveResponse(request.Query, [
                new RagRetrieveResultDto(
                    0.91,
                    "ارائه ضمانت نامه معتبر الزامی است.",
                    "ارائه ضمانت نامه معتبر برای شرکت در مناقصه الزامی است.",
                    AiDocumentSourceType.LegalClause,
                    Guid.Parse("30000000-0000-0000-0000-000000000001"),
                    "ماده ۱",
                    "/api/legal/clauses/30000000-0000-0000-0000-000000000001/context",
                    new Dictionary<string, object?>(),
                    Guid.Parse("40000000-0000-0000-0000-000000000001"))
            ]));
        }
    }

    private sealed class FakeAnswerGenerator(string answer) : IGroundedAiAnswerGenerator
    {
        public GroundedAiPrompt? LastPrompt { get; private set; }

        public Task<string> GenerateAnswerAsync(GroundedAiPrompt prompt, CancellationToken ct = default)
        {
            LastPrompt = prompt;
            return Task.FromResult(answer);
        }
    }

    private sealed class FakeGroundedDataSource : IGroundedAiAnalysisDataSource
    {
        public Task<string?> GetPurchaseFileContextAsync(Guid purchaseFileId, CancellationToken ct = default) =>
            Task.FromResult<string?>("PF-1405-001; پرونده خرید نمونه");

        public Task<GroundedRuleFindingContext?> GetRuleFindingContextAsync(Guid findingId, CancellationToken ct = default) =>
            Task.FromResult<GroundedRuleFindingContext?>(new GroundedRuleFindingContext(
                findingId,
                PurchaseFileId,
                "عدم رعایت ضمانت نامه",
                "ضمانت نامه ارائه نشده است.",
                "Fail",
                "Blocking",
                "ماده ۱",
                null));
    }
}
