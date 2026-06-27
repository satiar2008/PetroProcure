using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Rag;

public sealed record GoldenRagQuestion(
    string Question,
    Guid? ExpectedSourceId = null,
    Guid? ExpectedLegalClauseId = null,
    IReadOnlyList<string>? ExpectedAnswerKeywords = null,
    RagRetrievalScope Scope = RagRetrievalScope.AllAllowed,
    Guid? PurchaseFileId = null,
    bool ExpectNoAnswer = false);

public sealed record RagQualityEvaluationRequest(IReadOnlyList<GoldenRagQuestion> Questions, int TopK = 5);

public sealed record RagQualityEvaluationResult(
    int TotalQuestions,
    double PrecisionAtK,
    double RecallAtK,
    double CitationAccuracy,
    double NoAnswerCorrectness,
    IReadOnlyList<RagQualityQuestionResult> Results);

public sealed record RagQualityQuestionResult(
    string Question,
    bool ExpectedHitFound,
    bool CitationCorrect,
    bool NoAnswerCorrect,
    IReadOnlyList<Guid> ReturnedSourceIds);

public interface IRagQualityEvaluator
{
    Task<RagQualityEvaluationResult> EvaluateAsync(
        RagQualityEvaluationRequest request,
        RagUserContext userContext,
        CancellationToken ct = default);
}

public sealed class RagQualityEvaluator(IRagRetriever retriever) : IRagQualityEvaluator
{
    public async Task<RagQualityEvaluationResult> EvaluateAsync(
        RagQualityEvaluationRequest request,
        RagUserContext userContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Questions.Count == 0)
            return new RagQualityEvaluationResult(0, 0, 0, 0, 0, []);

        var topK = Math.Clamp(request.TopK <= 0 ? 5 : request.TopK, 1, 50);
        var results = new List<RagQualityQuestionResult>();

        foreach (var golden in request.Questions)
        {
            var response = await retriever.RetrieveAsync(new RagRetrieveRequest(
                golden.Question,
                golden.Scope,
                golden.PurchaseFileId,
                topK,
                SourceTypesFor(golden),
                null), userContext, ct);

            var expectedId = golden.ExpectedLegalClauseId ?? golden.ExpectedSourceId;
            var returnedIds = response.Results.Select(x => x.SourceId).ToArray();
            var expectedHit = expectedId.HasValue && returnedIds.Contains(expectedId.Value);
            var noAnswerCorrect = golden.ExpectNoAnswer && response.Results.Count == 0;
            var citationCorrect = golden.ExpectNoAnswer
                ? noAnswerCorrect
                : expectedHit && response.Results.Any(x => x.SourceId == expectedId && !string.IsNullOrWhiteSpace(x.CitationReference));

            results.Add(new RagQualityQuestionResult(
                golden.Question,
                expectedHit,
                citationCorrect,
                noAnswerCorrect,
                returnedIds));
        }

        var answerable = results.Where((_, i) => !request.Questions[i].ExpectNoAnswer).ToArray();
        var noAnswer = results.Where((_, i) => request.Questions[i].ExpectNoAnswer).ToArray();
        var precision = answerable.Length == 0 ? 0 : answerable.Count(x => x.ExpectedHitFound) / (double)answerable.Length;
        var recall = precision; // One expected source per golden question in the initial dataset.
        var citation = answerable.Length == 0 ? 0 : answerable.Count(x => x.CitationCorrect) / (double)answerable.Length;
        var noAnswerCorrectness = noAnswer.Length == 0 ? 0 : noAnswer.Count(x => x.NoAnswerCorrect) / (double)noAnswer.Length;

        return new RagQualityEvaluationResult(request.Questions.Count, precision, recall, citation, noAnswerCorrectness, results);
    }

    private static IReadOnlyList<AiDocumentSourceType>? SourceTypesFor(GoldenRagQuestion golden) =>
        golden.ExpectedLegalClauseId.HasValue ? [AiDocumentSourceType.LegalClause] : null;
}
