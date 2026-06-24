using Microsoft.Extensions.Options;
using PetroProcure.AI;

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
}
