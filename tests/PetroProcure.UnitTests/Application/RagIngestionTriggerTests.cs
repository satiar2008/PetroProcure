using PetroProcure.Application.Documents;
using PetroProcure.Application.Legal;
using PetroProcure.Application.Rag;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.UnitTests.Application;

public sealed class RagIngestionTriggerTests
{
    [Fact]
    public async Task CreateLegalClause_EnqueuesEmbeddingIngestionJob()
    {
        var articleId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var repository = new FakeLegalRuleRepository(articleId, documentId);
        var queue = new FakeIngestionQueue();
        var handler = new LegalRuleCommandHandler(repository, new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true),
            new FakeLegalDocumentStorage(), queue);

        var dto = await handler.Handle(new CreateLegalClauseCommand(new CreateLegalClauseRequest(
            articleId, "بند ۱", "متن بند قانونی", 1, null, "PurchaseFile", RuleSeverity.Critical, "test")));

        var payload = Assert.Single(queue.Payloads);
        Assert.Equal(AiDocumentSourceType.LegalClause, payload.SourceType);
        Assert.Equal(dto.Id, payload.SourceId);
    }

    private sealed class FakeIngestionQueue : IRagIngestionQueue
    {
        public List<EmbeddingIngestionPayload> Payloads { get; } = [];

        public Task<CreateAiJobResponse> EnqueueAsync(EmbeddingIngestionPayload payload, Guid? createdByUserId,
            CancellationToken ct = default)
        {
            Payloads.Add(payload);
            return Task.FromResult(new CreateAiJobResponse(Guid.NewGuid(), "Queued", "queued", DateTime.UtcNow));
        }
    }

    private sealed class FakeLegalRuleRepository(Guid articleId, Guid documentId) : ILegalRuleRepository
    {
        private LegalClause? _clause;

        public Task<LegalArticle?> FindArticleAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(id == articleId
                ? new LegalArticle(articleId, documentId, "1", "article", "body", 1)
                : null);

        public Task AddClauseAsync(LegalClause clause, CancellationToken ct)
        {
            _clause = clause;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<LegalArticleDto>> GetArticlesByDocumentAsync(Guid id, CancellationToken ct)
        {
            var clause = _clause!;
            IReadOnlyList<LegalClauseDto> clauses =
            [
                new(clause.Id, clause.LegalArticleId, clause.ClauseNumber, clause.Body, clause.OrderNo,
                    clause.Note, clause.AppliesTo, clause.Severity, clause.Tags)
            ];
            return Task.FromResult<IReadOnlyList<LegalArticleDto>>([
                new(articleId, documentId, "1", "article", "body", 1, clauses)
            ]);
        }

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
        public Task AddAuditAsync(LegalRuleAuditLog audit, CancellationToken ct) => Task.CompletedTask;

        public Task AddLegalDocumentAsync(LegalDocument document, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalDocument?> FindLegalDocumentAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task AddArticleAsync(LegalArticle article, CancellationToken ct) => throw new NotSupportedException();
        public Task AddRuleAsync(ProcurementRule rule, ProcurementRuleVersion version, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRule?> FindRuleAsync(Guid id, bool includeVersions, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRule?> FindRuleByCodeAsync(string code, bool includeVersions, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleDto?> GetRuleAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleVersion?> FindRuleVersionAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleVersion?> FindLatestDraftVersionAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleVersion?> FindPendingVersionAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task<int> NextRuleVersionNoAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<ProcurementRuleVersion>> GetActiveRuleVersionsAsync(CancellationToken ct) => throw new NotSupportedException();
        public Task AddEvaluationAsync(ProcurementRuleEvaluation evaluation, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<LegalDocumentDto>> GetLegalDocumentsAsync(LegalDocumentListRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalDocumentDto?> GetLegalDocumentAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<LegalArticleDto>> SearchArticlesAsync(LegalArticleSearchRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<LegalClauseContextDto>> SearchClauseContextsAsync(LegalClauseSearchRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalClauseContextDto?> GetClauseContextAsync(Guid clauseId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<ProcurementRuleDto>> GetRulesAsync(ProcurementRuleListRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<ProcurementRuleVersionDto>> GetRuleVersionsAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<ProcurementRuleEvaluationDto>> GetEvaluationsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeLegalDocumentStorage : ILegalDocumentStorageService
    {
        public Task<StoredLegalDocument> SaveAsync(Guid legalDocumentId, string originalFileName, Stream stream,
            string? mimeType, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StoredFileContent> OpenAsync(LegalDocument document, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeletePhysicalAsync(string relativePath, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
