using Microsoft.Extensions.Options;
using PetroProcure.AI;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class AiFoundationTests
{
    [Fact]
    public void ContextBuilderGroupingGroupsMescItemsCorrectly()
    {
        var groups = AiContextFactory.GroupItems([
            ("1234560001","123456","Pipes","Pipe","M",10),
            ("1234560002","123456","Pipes","Elbow","EA",2),
            ("6543210001","654321","Pumps","Pump","DEV",1)]);
        Assert.Equal(2, groups.Count);
        Assert.Equal(2, groups.Single(x => x.GeneralCode == "123456").Items.Count);
    }

    [Fact]
    public async Task MissingDocumentCheckerReturnsExpectedFindings()
    {
        var repo = new FakeRepository(); var service = Service(repo, new PurchaseFileAiContext(
            Guid.NewGuid(), "PF-1", "Draft", "Purchase", [], [new("Indent", "indent.pdf")], [], []));
        var result = await service.CheckMissingDocumentsAsync(repo.Context!.PurchaseFileId);
        Assert.Single(result.Findings);
        Assert.Contains("TechnicalSpecification", result.Findings[0].Title);
    }

    [Fact]
    public async Task RuleEvaluatorStoresResult()
    {
        var repo = new FakeRepository { Context = new(Guid.NewGuid(), "PF-1", "Draft", "Purchase", [], [], [], []) };
        await new ProcurementRuleEvaluator(repo).EvaluateAsync(repo.Context);
        Assert.Single(repo.Stored);
        Assert.Equal("Rules", repo.Stored[0].EvaluationType);
    }

    [Fact]
    public async Task MockProviderReturnsDeterministicSummary()
    {
        var provider = new MockAiProvider();
        var first = await provider.CompleteAsync(new("system", "one"));
        var second = await provider.CompleteAsync(new("system", "two"));
        Assert.Equal(first.Content, second.Content);
    }

    [Fact]
    public void AiCoreSettingsDtoDoesNotExposeApiKey()
    {
        var settings = new AiCoreSettings("https://aicore.local", "secret-value",
            "PETROPROCURE_AICORE_API_KEY", "model", 60, 12000, 2000, true, false, "tenant", "client");

        var dto = settings.ToDto();

        Assert.True(dto.HasApiKey);
        Assert.DoesNotContain(typeof(PetroProcure.Contracts.V1.Ai.AiCoreProviderSettingsDto).GetProperties(),
            property => property.Name.Contains("ApiKey", StringComparison.OrdinalIgnoreCase)
                && property.Name != nameof(PetroProcure.Contracts.V1.Ai.AiCoreProviderSettingsDto.ApiKeySecretName)
                && property.Name != nameof(PetroProcure.Contracts.V1.Ai.AiCoreProviderSettingsDto.HasApiKey));
    }

    [Fact]
    public async Task AiLegalContextFiltersClausesByAppliesTo()
    {
        var entityId = Guid.NewGuid();
        var legalBuilder = new FakeLegalRuleContextBuilder([
            new AiLegalRuleContextDto(Guid.NewGuid(), Guid.NewGuid(), "ماده ۱", "1", "الف",
                "قانون پرونده خرید", "Summary", "PurchaseFile", "Warning", ["purchase-file"]),
            new AiLegalRuleContextDto(Guid.NewGuid(), Guid.NewGuid(), "ماده ۲", "2", "ب",
                "قانون مناقصه", "Summary", "Tender", "Critical", ["tender"])
        ]);
        var repository = new FakeAiAnalysisRepository();
        var client = new FakeAiCoreClient();
        var service = new AiAnalysisService(new FakeAiContextBuilder(entityId), legalBuilder, client, repository,
            new FakeCurrentUser(Guid.NewGuid()), new FakeAiCoreSettingsProvider());

        await service.AnalyzeLegalComplianceAsync("PurchaseFile", entityId, "PurchaseFile", null);

        Assert.Equal("PurchaseFile", legalBuilder.LastAppliesTo);
        var context = Assert.IsType<AiPromptContextDto>(client.LastRequest!.Context);
        var rule = Assert.Single(context.LegalRules);
        Assert.Equal("PurchaseFile", rule.AppliesTo);
        Assert.Contains("purchase-file", rule.Tags);
    }

    [Fact]
    public async Task MockAiCoreAnalyzePurchaseFileStoresAiAnalysisEvaluation()
    {
        var entityId = Guid.NewGuid();
        var repository = new FakeAiAnalysisRepository();
        var service = new AiAnalysisService(new FakeAiContextBuilder(entityId), new FakeLegalRuleContextBuilder([]),
            new FakeAiCoreClient(), repository, new FakeCurrentUser(Guid.NewGuid()), new FakeAiCoreSettingsProvider());

        var result = await service.AnalyzePurchaseFileAsync(entityId, "Summary", null);

        var evaluation = Assert.Single(repository.Evaluations);
        Assert.Equal(result.Id, evaluation.Id);
        Assert.Equal("PurchaseFile", evaluation.EntityType);
        Assert.Equal(entityId, evaluation.EntityId);
        Assert.Equal("Completed", evaluation.Status);
        Assert.Contains("تحلیل هوش مصنوعی صرفاً جنبه کمکی دارد", evaluation.ResultSummary);
    }

    private static AiAgentService Service(FakeRepository repo, PurchaseFileAiContext context)
    {
        repo.Context = context;
        return new(repo, new MockAiProvider(), new ProcurementRuleEvaluator(repo), repo,
            Options.Create(new AiOptions { RequiredDocumentTypes = ["Indent", "TechnicalSpecification"] }));
    }
    private sealed class FakeRepository : IAiEvaluationRepository, IPurchaseFileAiContextBuilder
    {
        public PurchaseFileAiContext? Context; public List<AiEvaluationDto> Stored { get; } = [];
        public Task<PurchaseFileAiContext> BuildAsync(Guid purchaseFileId, CancellationToken ct = default) => Task.FromResult(Context!);
        public Task SaveAsync(AiEvaluationJob job, AiEvaluationResult result, CancellationToken ct) { Stored.Add(ProcurementRuleEvaluator.Map(result)); return Task.CompletedTask; }
        public Task<IReadOnlyList<AiEvaluationDto>> GetAsync(Guid purchaseFileId, CancellationToken ct) => Task.FromResult<IReadOnlyList<AiEvaluationDto>>(Stored);
    }

    private sealed class FakeAiCoreSettingsProvider : IAiCoreSettingsProvider
    {
        public Task<AiCoreSettings> GetAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiCoreSettings("https://aicore.local", "secret", "PETROPROCURE_AICORE_API_KEY",
                "model", 60, 12000, 2000, true, false, null, null));
    }

    private sealed class FakeAiCoreClient : IAiCoreClient
    {
        public AiCoreAnalysisRequest? LastRequest { get; private set; }
        public Task<AiCoreAnalysisResponse> SendAnalysisAsync(AiCoreAnalysisRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            return Task.FromResult(new AiCoreAnalysisResponse("خلاصه قطعی تست", "Low",
                [new AiCoreFinding("یافته تست", "شرح یافته", "Info")],
                [new AiCoreRecommendation("پیشنهاد تست", "شرح پیشنهاد", "Info")],
                new AiUsageDto(10, 20)));
        }

        public Task<AiChatResponse> SendChatAsync(AiChatRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiChatResponse("chat", "AiCore", "model"));

        public Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiProviderHealthDto("AiCore", true, "Healthy", DateTime.UtcNow, "model"));
    }

    private sealed class FakeAiContextBuilder(Guid expectedId) : IAiContextBuilder
    {
        public Task<AiPromptContextDto> BuildPurchaseFileContextAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Context("PurchaseFile", id));

        public Task<AiPromptContextDto> BuildTenderContextAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Context("Tender", id));

        public Task<AiPromptContextDto> BuildContractContextAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Context("Contract", id));

        public Task<AiPromptContextDto> BuildPurchaseOrderContextAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Context("PurchaseOrder", id));

        public Task<AiPromptContextDto> BuildWarehouseReceiptContextAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Context("WarehouseReceipt", id));

        private AiPromptContextDto Context(string entityType, Guid id)
        {
            Assert.Equal(expectedId, id);
            return new AiPromptContextDto(entityType, id, "NO-1", "Draft",
                new AiProcurementEntityContextDto("عنوان تست", null, new Dictionary<string, object?>()), []);
        }
    }

    private sealed class FakeLegalRuleContextBuilder(IReadOnlyList<AiLegalRuleContextDto> rules) : IAiLegalRuleContextBuilder
    {
        public string? LastAppliesTo { get; private set; }

        public Task<IReadOnlyList<AiLegalRuleContextDto>> BuildLegalRuleContextAsync(string entityType, Guid entityId,
            string? appliesTo, CancellationToken ct = default)
        {
            LastAppliesTo = appliesTo;
            return Task.FromResult<IReadOnlyList<AiLegalRuleContextDto>>(rules
                .Where(x => string.IsNullOrWhiteSpace(appliesTo) ||
                    string.IsNullOrWhiteSpace(x.AppliesTo) ||
                    x.AppliesTo.Contains(appliesTo, StringComparison.OrdinalIgnoreCase))
                .ToArray());
        }
    }

    private sealed class FakeAiAnalysisRepository : IAiAnalysisRepository
    {
        public List<AiAnalysisEvaluation> Evaluations { get; } = [];
        private readonly Dictionary<Guid, AiAnalysisResultDto> _results = [];

        public Task SaveAsync(AiAnalysisEvaluation evaluation, IReadOnlyList<AiAnalysisFinding> findings,
            IReadOnlyList<AiAnalysisRecommendation> recommendations, AiProviderRequestLog log, CancellationToken ct)
        {
            Evaluations.Add(evaluation);
            _results[evaluation.Id] = new AiAnalysisResultDto(evaluation.Id, evaluation.EntityType,
                evaluation.EntityId, evaluation.AnalysisType, evaluation.Provider, evaluation.Model, evaluation.Status,
                evaluation.ResultSummary, evaluation.RiskLevel, evaluation.CreatedAt, evaluation.CompletedAt,
                findings.Select(x => new PetroProcure.Contracts.V1.Ai.AiFindingDto(x.Id, x.Title, x.Description,
                    PetroProcure.Contracts.V1.Ai.AiSeverity.Info, null, x.RelatedRuleClauseId, x.Evidence, x.Recommendation, x.LegalReference)).ToArray(),
                recommendations.Select(x => new PetroProcure.Contracts.V1.Ai.AiRecommendationDto(x.Id, x.Title,
                    x.Description, PetroProcure.Contracts.V1.Ai.AiSeverity.Info, x.RelatedAction)).ToArray(),
                new AiUsageDto(10, 20));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AiAnalysisResultDto>> GetAsync(string? entityType, Guid? entityId, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<AiAnalysisResultDto>>(_results.Values
                .Where(x => (entityType is null || x.EntityType == entityType) && (!entityId.HasValue || x.EntityId == entityId))
                .ToArray());

        public Task<AiAnalysisResultDto?> GetByIdAsync(Guid id, CancellationToken ct) =>
            Task.FromResult(_results.GetValueOrDefault(id));
    }

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUserService
    {
        public Guid UserId => userId;
        public string? UserName => "ai-test";
        public string? Email => "ai-test@example.local";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public IReadOnlyCollection<Guid> DepartmentIds => [];
        public bool IsAuthenticated => true;
        public bool IsSystemAdmin => false;
    }
}
