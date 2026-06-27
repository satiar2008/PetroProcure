using System.Text.Json;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Ai;

public interface IGroundedAiAnalysisService
{
    Task<GroundedAiAnalysisResponse> AskAboutFileAsync(
        Guid purchaseFileId, string question, int topK, CancellationToken ct = default);

    Task<GroundedAiAnalysisResponse> FindRelevantRegulationsAsync(
        string question, int topK, CancellationToken ct = default);

    Task<GroundedAiAnalysisResponse> ExplainRuleFindingAsync(
        Guid findingId, string? question, int topK, CancellationToken ct = default);

    Task<GroundedAiAnalysisResponse> SummarizeLegalRiskAsync(
        Guid purchaseFileId, int topK, CancellationToken ct = default);
}

public sealed record GroundedAiPrompt(
    string AnalysisType,
    string Question,
    string? PurchaseFileContext,
    IReadOnlyList<GroundedAiPromptChunk> Chunks);

public sealed record GroundedAiPromptChunk(
    string CitationId,
    Guid? ChunkId,
    string SourceType,
    Guid SourceId,
    string Title,
    string Reference,
    string Text);

public interface IGroundedAiAnswerGenerator
{
    Task<string> GenerateAnswerAsync(GroundedAiPrompt prompt, CancellationToken ct = default);
}

public interface IGroundedAiAnalysisDataSource
{
    Task<string?> GetPurchaseFileContextAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<GroundedRuleFindingContext?> GetRuleFindingContextAsync(Guid findingId, CancellationToken ct = default);
}

public sealed record GroundedRuleFindingContext(
    Guid FindingId,
    Guid? PurchaseFileId,
    string Title,
    string Description,
    string Result,
    string Severity,
    string LegalReference,
    string? CitationReferences);

public sealed class GroundedAiAnalysisService(
    IRagRetriever retriever,
    IGroundedAiAnswerGenerator answerGenerator,
    IGroundedAiAnalysisDataSource dataSource,
    ICurrentUserService currentUser) : IGroundedAiAnalysisService
{
    private const string Insufficient =
        "مدارک و بندهای بازیابی‌شده برای پاسخ مستند کافی نیستند.";

    public async Task<GroundedAiAnalysisResponse> AskAboutFileAsync(
        Guid purchaseFileId, string question, int topK, CancellationToken ct = default)
    {
        var normalized = RequiredQuestion(question);
        var response = await RetrieveOrInsufficientAsync(new RagRetrieveRequest(
                normalized,
                RagRetrievalScope.PurchaseFile,
                purchaseFileId,
                topK,
                SourceTypes:
                [
                    AiDocumentSourceType.PurchaseFileDocument,
                    AiDocumentSourceType.TenderDocument,
                    AiDocumentSourceType.ContractDocument,
                    AiDocumentSourceType.ReportDocument
                ]),
            UserContext(),
            needHumanReview: false,
            ct);
        if (response is null)
            return new GroundedAiAnalysisResponse(Insufficient, [], [], false, DateTime.UtcNow);

        return await AnswerAsync(AiAnalysisType.AskAboutFile.ToString(), normalized,
            await dataSource.GetPurchaseFileContextAsync(purchaseFileId, ct), response.Results, false, ct);
    }

    public async Task<GroundedAiAnalysisResponse> FindRelevantRegulationsAsync(
        string question, int topK, CancellationToken ct = default)
    {
        var normalized = RequiredQuestion(question);
        var response = await RetrieveOrInsufficientAsync(new RagRetrieveRequest(
                normalized,
                RagRetrievalScope.LegalCorpus,
                TopK: topK,
                SourceTypes: [AiDocumentSourceType.LegalClause]),
            UserContext(),
            needHumanReview: false,
            ct);
        if (response is null)
            return new GroundedAiAnalysisResponse(Insufficient, [], [], false, DateTime.UtcNow);

        return await AnswerAsync(AiAnalysisType.FindRelevantRegulations.ToString(), normalized,
            null, response.Results, false, ct);
    }

    public async Task<GroundedAiAnalysisResponse> ExplainRuleFindingAsync(
        Guid findingId, string? question, int topK, CancellationToken ct = default)
    {
        var finding = await dataSource.GetRuleFindingContextAsync(findingId, ct)
            ?? throw new ArgumentException("Rule finding was not found.", nameof(findingId));
        var normalized = RequiredQuestion(string.IsNullOrWhiteSpace(question)
            ? $"Explain this procurement rule finding: {finding.Title}. {finding.Description}. {finding.LegalReference}"
            : question);

        var response = await RetrieveOrInsufficientAsync(new RagRetrieveRequest(
                $"{finding.Title} {finding.Description} {finding.LegalReference} {normalized}",
                RagRetrievalScope.LegalCorpus,
                TopK: topK,
                SourceTypes: [AiDocumentSourceType.LegalClause]),
            UserContext(),
            needHumanReview: true,
            ct);
        if (response is null)
            return new GroundedAiAnalysisResponse(Insufficient, [], [], true, DateTime.UtcNow);

        var context = JsonSerializer.Serialize(finding, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return await AnswerAsync(AiAnalysisType.ExplainRuleFinding.ToString(), normalized, context,
            response.Results, true, ct);
    }

    public async Task<GroundedAiAnalysisResponse> SummarizeLegalRiskAsync(
        Guid purchaseFileId, int topK, CancellationToken ct = default)
    {
        var question = "Summarize the legal risk for this purchase file using only retrieved legal and document context.";
        var response = await RetrieveOrInsufficientAsync(new RagRetrieveRequest(
                question,
                RagRetrievalScope.AllAllowed,
                purchaseFileId,
                topK),
            UserContext(),
            needHumanReview: true,
            ct);
        if (response is null)
            return new GroundedAiAnalysisResponse(Insufficient, [], [], true, DateTime.UtcNow);

        return await AnswerAsync(AiAnalysisType.SummarizeLegalRisk.ToString(), question,
            await dataSource.GetPurchaseFileContextAsync(purchaseFileId, ct), response.Results, true, ct);
    }

    private async Task<GroundedAiAnalysisResponse> AnswerAsync(
        string analysisType,
        string question,
        string? purchaseFileContext,
        IReadOnlyList<RagRetrieveResultDto> chunks,
        bool needHumanReview,
        CancellationToken ct)
    {
        var promptChunks = chunks.Select((chunk, index) => new GroundedAiPromptChunk(
            $"C{index + 1}",
            chunk.ChunkId,
            chunk.SourceType.ToString(),
            chunk.SourceId,
            chunk.CitationTitle,
            chunk.CitationReference,
            chunk.Text ?? chunk.TextPreview)).ToArray();

        if (promptChunks.Length == 0)
            return new GroundedAiAnalysisResponse(Insufficient, [], [], needHumanReview, DateTime.UtcNow);

        var answer = await answerGenerator.GenerateAnswerAsync(
            new GroundedAiPrompt(analysisType, question, purchaseFileContext, promptChunks), ct);
        if (string.IsNullOrWhiteSpace(answer))
            answer = Insufficient;
        answer = EnsureCitation(answer.Trim(), promptChunks);

        return new GroundedAiAnalysisResponse(
            answer,
            chunks.Select((chunk, index) => new GroundedAiCitationDto(
                $"C{index + 1}",
                chunk.CitationTitle,
                chunk.CitationReference,
                chunk.SourceType.ToString(),
                chunk.SourceId,
                chunk.Score)).ToArray(),
            chunks.Select((chunk, index) => new GroundedAiRelatedChunkDto(
                $"C{index + 1}",
                chunk.ChunkId,
                chunk.SourceType.ToString(),
                chunk.SourceId,
                chunk.TextPreview,
                chunk.Metadata)).ToArray(),
            needHumanReview,
            DateTime.UtcNow);
    }

    private static string EnsureCitation(string answer, IReadOnlyList<GroundedAiPromptChunk> chunks)
    {
        if (chunks.Count == 0 || chunks.Any(x => answer.Contains($"[{x.CitationId}]", StringComparison.OrdinalIgnoreCase)))
            return answer;
        return $"{answer} [{chunks[0].CitationId}]";
    }

    private RagUserContext UserContext() =>
        new(currentUser.UserId, currentUser.IsSystemAdmin, currentUser.Permissions, currentUser.DepartmentIds);

    private async Task<RagRetrieveResponse?> RetrieveOrInsufficientAsync(
        RagRetrieveRequest request,
        RagUserContext userContext,
        bool needHumanReview,
        CancellationToken ct)
    {
        try
        {
            return await retriever.RetrieveAsync(request, userContext, ct);
        }
        catch (RagEmbeddingUnavailableException)
        {
            return null;
        }
    }

    private static string RequiredQuestion(string? question) =>
        string.IsNullOrWhiteSpace(question)
            ? throw new ArgumentException("Question is required.", nameof(question))
            : question.Trim();
}

public sealed class NullGroundedAiAnswerGenerator : IGroundedAiAnswerGenerator
{
    public Task<string> GenerateAnswerAsync(GroundedAiPrompt prompt, CancellationToken ct = default) =>
        Task.FromResult("مدارک و بندهای بازیابی‌شده برای پاسخ مستند کافی نیستند.");
}
