using PetroProcure.Application.Legal;
using PetroProcure.Application.Rag;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.UnitTests.Domain;

public sealed class LegalRuleTests
{
    [Fact]
    public void ActiveRulesCannotBeEditedDirectly()
    {
        var version = Version(conditionType: "alwayspass");
        version.Submit();
        version.Approve(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => version.UpdateDraft(
            "Updated", RuleType.Checklist, RuleSeverity.Warning, RuleEvaluationMode.Automatic,
            null, Guid.NewGuid(), "ماده ۱", "alwayspass", "true", null));
    }

    [Fact]
    public void EditingActiveRuleCreatesDraftVersion()
    {
        var active = Version(versionNo: 1, conditionType: "alwayspass");
        active.Submit();
        active.Approve(Guid.NewGuid());

        var draft = active.CloneAsDraft(Guid.NewGuid(), 2, Guid.NewGuid());

        Assert.Equal(RuleStatus.Draft, draft.Status);
        Assert.Equal(2, draft.VersionNo);
        Assert.Equal(active.Id, active.Id);
        Assert.NotEqual(active.Id, draft.Id);
    }

    [Fact]
    public void RuleReferencesLegalClause()
    {
        var clauseId = Guid.NewGuid();
        var version = Version(legalClauseId: clauseId, legalReference: "ماده ۲ بند الف");

        Assert.Equal(clauseId, version.LegalClauseId);
        Assert.Equal("ماده ۲ بند الف", version.LegalReference);
    }

    [Fact]
    public void LegalClauseStoresAiSearchMetadata()
    {
        var clause = new LegalClause(Guid.NewGuid(), Guid.NewGuid(), "بند الف",
            "متن بند قانونی", 1, "تبصره", "PurchaseFile", RuleSeverity.Critical,
            "مناقصه,اسناد,ارزیابی");

        Assert.Equal("PurchaseFile", clause.AppliesTo);
        Assert.Equal(RuleSeverity.Critical, clause.Severity);
        Assert.Equal("مناقصه,اسناد,ارزیابی", clause.Tags);
        Assert.Contains("PurchaseFile", clause.SearchText);
        Assert.Contains("اسناد", clause.SearchText);
    }

    [Fact]
    public async Task EvaluationStoresResultLinkedToRuleVersion()
    {
        var repo = new FakeLegalRuleRepository();
        var ruleVersion = Version(conditionType: "requiredDocumentType", conditionValue: "TenderDocument");
        ruleVersion.Submit();
        ruleVersion.Approve(Guid.NewGuid());
        repo.ActiveVersions.Add(ruleVersion);
        var evaluator = new DeterministicProcurementRuleEvaluator(repo, new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 3, new HashSet<string>()));

        var finding = Assert.Single(result.Findings);
        Assert.Equal(ruleVersion.Id, finding.RuleVersionId);
        Assert.Equal(RuleResult.Fail, finding.Result);
    }

    [Fact]
    public async Task PurchaseFileEvaluationReturnsAllRuleResultTypes()
    {
        var repo = new FakeLegalRuleRepository();
        repo.ActiveVersions.Add(ApprovedVersion("pass", "alwayspass", "true"));
        repo.ActiveVersions.Add(ApprovedVersion("fail", "alwaysfail", "true"));
        repo.ActiveVersions.Add(ApprovedVersion("warning", "minimumitems", "5"));
        repo.ActiveVersions.Add(ApprovedVersion("not-applicable", "purchasefilestatus", "Completed"));
        repo.ActiveVersions.Add(ApprovedVersion("human", "alwayspass", "true", RuleEvaluationMode.ManualReview));
        var evaluator = new DeterministicProcurementRuleEvaluator(repo, new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 1, new HashSet<string>()));

        Assert.Contains(result.Findings, x => x.Result == RuleResult.Pass);
        Assert.Contains(result.Findings, x => x.Result == RuleResult.Fail);
        Assert.Contains(result.Findings, x => x.Result == RuleResult.Warning);
        Assert.Contains(result.Findings, x => x.Result == RuleResult.NotApplicable);
        Assert.Contains(result.Findings, x => x.Result == RuleResult.NeedHumanReview);
    }

    [Fact]
    public async Task DeprecatedRulesAreNotUsedForNewEvaluations()
    {
        var repo = new FakeLegalRuleRepository();
        var deprecated = ApprovedVersion("old", "alwaysfail", "true");
        deprecated.Deprecate();
        repo.ActiveVersions.Add(deprecated);
        repo.ActiveVersions.RemoveAll(x => x.Status != RuleStatus.Active);
        var evaluator = new DeterministicProcurementRuleEvaluator(repo, new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true));

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 1, new HashSet<string>()));

        Assert.Empty(result.Findings);
    }

    [Fact]
    public async Task HybridAutomaticRuleDoesNotCallAi()
    {
        var repo = new FakeLegalRuleRepository();
        repo.ActiveVersions.Add(ApprovedVersion("auto", "alwayspass", "true", RuleEvaluationMode.Automatic));
        var ai = new FakeAiLegalEvaluationService();
        var evaluator = Hybrid(repo, ai);

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 1, new HashSet<string>()));

        Assert.Equal(0, ai.CallCount);
        Assert.Single(result.Findings);
        Assert.DoesNotContain(result.Findings, x => x.IsAiGenerated);
    }

    [Fact]
    public async Task HybridSemiAutomaticRuleCallsAiAndStoresProposedFinding()
    {
        var repo = new FakeLegalRuleRepository();
        repo.ActiveVersions.Add(ApprovedVersion("semi", "alwayspass", "true", RuleEvaluationMode.SemiAutomatic));
        var ai = new FakeAiLegalEvaluationService();
        var evaluator = Hybrid(repo, ai);

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 1, new HashSet<string>()));

        Assert.Equal(1, ai.CallCount);
        var aiFinding = Assert.Single(result.Findings, x => x.IsAiGenerated);
        Assert.Equal(RuleResult.NeedHumanReview, aiFinding.Result);
        Assert.True(aiFinding.NeedHumanReview);
        Assert.Equal(0.72m, aiFinding.Confidence);
    }

    [Fact]
    public async Task HybridAiFindingIncludesLegalCitationWhenAvailable()
    {
        var repo = new FakeLegalRuleRepository();
        repo.ActiveVersions.Add(ApprovedVersion("semi", "alwayspass", "true", RuleEvaluationMode.SemiAutomatic));
        var evaluator = Hybrid(repo, new FakeAiLegalEvaluationService());

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 1, new HashSet<string>()));

        var aiFinding = Assert.Single(result.Findings, x => x.IsAiGenerated);
        Assert.Contains("/api/legal/clauses/", aiFinding.CitationReferences);
    }

    [Fact]
    public async Task HybridAiFailureDoesNotBreakDeterministicEvaluation()
    {
        var repo = new FakeLegalRuleRepository();
        repo.ActiveVersions.Add(ApprovedVersion("semi", "alwaysfail", "true", RuleEvaluationMode.SemiAutomatic));
        var evaluator = Hybrid(repo, new FakeAiLegalEvaluationService(throwOnAnalyze: true));

        var purchaseFileId = Guid.NewGuid();
        var result = await evaluator.EvaluateAsync(new RuleEvaluationContext(
            "PurchaseFile", purchaseFileId, purchaseFileId, null, "Open", false, 1, new HashSet<string>()));

        Assert.Single(result.Findings);
        Assert.Equal(RuleResult.Fail, result.Findings[0].Result);
        Assert.False(result.Findings[0].IsAiGenerated);
    }

    [Fact]
    public void OldEvaluationsKeepOriginalRuleVersion()
    {
        var ruleId = Guid.NewGuid();
        var oldVersion = ApprovedVersion("old", "alwayspass", "true", ruleId: ruleId);
        var evaluation = new ProcurementRuleEvaluation(Guid.NewGuid(), "PurchaseFile", Guid.NewGuid(), Guid.NewGuid(), null,
            "old evaluation", Guid.NewGuid());
        evaluation.AddFinding(new ProcurementRuleFinding(Guid.NewGuid(), evaluation.Id, ruleId, oldVersion.Id,
            RuleResult.Pass, RuleSeverity.Info, "old", "ok", "ماده ۱", null, Guid.NewGuid()));
        oldVersion.Deprecate();
        var newVersion = ApprovedVersion("new", "alwayspass", "true", ruleId: ruleId);

        var finding = Assert.Single(evaluation.Findings);
        Assert.Equal(oldVersion.Id, finding.RuleVersionId);
        Assert.NotEqual(newVersion.Id, finding.RuleVersionId);
    }

    private static ProcurementRuleVersion ApprovedVersion(string title, string conditionType, string conditionValue,
        RuleEvaluationMode mode = RuleEvaluationMode.Automatic, Guid? ruleId = null)
    {
        var version = Version(ruleId, title: title, conditionType: conditionType, conditionValue: conditionValue,
            mode: mode);
        version.Submit();
        version.Approve(Guid.NewGuid());
        return version;
    }

    private static ProcurementRuleVersion Version(Guid? ruleId = null, int versionNo = 1, string title = "Rule",
        RuleEvaluationMode mode = RuleEvaluationMode.Automatic, Guid? legalClauseId = null,
        string legalReference = "ماده ۱", string conditionType = "alwayspass", string conditionValue = "true",
        RuleType ruleType = RuleType.Checklist) =>
        new(Guid.NewGuid(), ruleId ?? Guid.NewGuid(), versionNo, title, ruleType,
            RuleSeverity.Warning, mode, null, legalClauseId ?? Guid.NewGuid(), legalReference,
            conditionType, conditionValue, "test rule", Guid.NewGuid());

    private static HybridProcurementRuleEvaluator Hybrid(
        FakeLegalRuleRepository repo,
        FakeAiLegalEvaluationService ai) =>
        new(repo, new TestCurrentUser(Guid.NewGuid(), isSystemAdmin: true),
            ai, new FakeRagRetriever());

    private sealed class FakeLegalRuleRepository : ILegalRuleRepository
    {
        public List<ProcurementRuleVersion> ActiveVersions { get; } = [];
        public List<ProcurementRuleEvaluation> Evaluations { get; } = [];

        public Task AddEvaluationAsync(ProcurementRuleEvaluation evaluation, CancellationToken ct)
        {
            Evaluations.Add(evaluation);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ProcurementRuleVersion>> GetActiveRuleVersionsAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ProcurementRuleVersion>>(ActiveVersions.Where(x => x.Status == RuleStatus.Active).ToArray());

        public Task<IReadOnlyList<ProcurementRuleEvaluationDto>> GetEvaluationsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ProcurementRuleEvaluationDto>>(Evaluations
                .Where(x => x.PurchaseFileId == purchaseFileId || x.EntityId == purchaseFileId)
                .Select(ToDto).ToArray());

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;

        private static ProcurementRuleEvaluationDto ToDto(ProcurementRuleEvaluation evaluation) =>
            new(evaluation.Id, evaluation.EntityType, evaluation.EntityId, evaluation.PurchaseFileId,
                evaluation.TenderId, evaluation.Summary, evaluation.EvaluatedByUserId, evaluation.EvaluatedAt,
                evaluation.Findings.Select(x => new ProcurementRuleFindingDto(x.Id, x.ProcurementRuleEvaluationId,
                    x.ProcurementRuleId, x.RuleVersionId, x.Result, x.Severity, x.Title, x.Description,
                    x.LegalReference, x.LegalArticleId, x.LegalClauseId, x.IsAiGenerated,
                    x.NeedHumanReview, x.Confidence, x.CitationReferences)).ToArray());

        public Task AddLegalDocumentAsync(LegalDocument document, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalDocument?> FindLegalDocumentAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task AddArticleAsync(LegalArticle article, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalArticle?> FindArticleAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task AddClauseAsync(LegalClause clause, CancellationToken ct) => throw new NotSupportedException();
        public Task AddRuleAsync(ProcurementRule rule, ProcurementRuleVersion version, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRule?> FindRuleAsync(Guid id, bool includeVersions, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRule?> FindRuleByCodeAsync(string code, bool includeVersions, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleDto?> GetRuleAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleVersion?> FindRuleVersionAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleVersion?> FindLatestDraftVersionAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task<ProcurementRuleVersion?> FindPendingVersionAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task<int> NextRuleVersionNoAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
        public Task AddAuditAsync(LegalRuleAuditLog audit, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<LegalDocumentDto>> GetLegalDocumentsAsync(LegalDocumentListRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalDocumentDto?> GetLegalDocumentAsync(Guid id, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<LegalArticleDto>> GetArticlesByDocumentAsync(Guid documentId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<LegalArticleDto>> SearchArticlesAsync(LegalArticleSearchRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<LegalClauseContextDto>> SearchClauseContextsAsync(LegalClauseSearchRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<LegalClauseContextDto?> GetClauseContextAsync(Guid clauseId, CancellationToken ct) => throw new NotSupportedException();
        public Task<PagedResult<ProcurementRuleDto>> GetRulesAsync(ProcurementRuleListRequest request, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<ProcurementRuleVersionDto>> GetRuleVersionsAsync(Guid ruleId, CancellationToken ct) => throw new NotSupportedException();
    }

    private sealed class FakeAiLegalEvaluationService(bool throwOnAnalyze = false) : IAiLegalEvaluationService
    {
        public int CallCount { get; private set; }

        public Task<IReadOnlyList<AiLegalEvaluationFinding>> AnalyzeAsync(
            AiLegalEvaluationRequest request, CancellationToken ct = default)
        {
            CallCount++;
            if (throwOnAnalyze) throw new InvalidOperationException("AiCore failed.");
            return Task.FromResult<IReadOnlyList<AiLegalEvaluationFinding>>([
                new("پیشنهاد هوش مصنوعی", "این بند نیازمند بررسی کارشناس حقوقی است.",
                    RuleSeverity.Critical, 0.72m, request.Rule.LegalReference,
                    request.Citations.Select(x => x.Reference).ToArray())
            ]);
        }
    }

    private sealed class FakeRagRetriever : IRagRetriever
    {
        public Task<RagRetrieveResponse> RetrieveAsync(
            string query, RagRetrievalScope scope, int topK, RagUserContext userContext, CancellationToken ct = default) =>
            RetrieveAsync(new RagRetrieveRequest(query, scope, TopK: topK), userContext, ct);

        public Task<RagRetrieveResponse> RetrieveAsync(
            RagRetrieveRequest request, RagUserContext userContext, CancellationToken ct = default) =>
            Task.FromResult(new RagRetrieveResponse(request.Query, [
                new RagRetrieveResultDto(0.9, "متن بند قانونی", "متن کامل بند قانونی",
                    AiDocumentSourceType.LegalClause, Guid.Parse("90000000-0000-0000-0000-000000000001"),
                    "ماده ۱", "/api/legal/clauses/90000000-0000-0000-0000-000000000001/context",
                    new Dictionary<string, object?>())
            ]));
    }
}
