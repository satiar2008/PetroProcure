using PetroProcure.Application.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.UnitTests.Application;

public sealed class RuleConditionEvaluatorTests
{
    private static readonly IConditionEvaluator Engine = JsonRuleConditionEvaluator.CreateDefault();

    private static RuleEvaluationContext Context() =>
        new("PurchaseFile", Guid.NewGuid(), Guid.NewGuid(), null, "Open", true, 5,
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Indent", "TechnicalSpecification" })
        {
            FinalAmount = 1_500_000_000m,
            SupplierCount = 3,
            OfferCount = 2,
            TenderType = "PublicTender",
            TenderDeadline = DateTime.UtcNow.AddDays(-1),   // overdue
            InquiryDeadline = DateTime.UtcNow.AddDays(5),
            WorkflowStatuses = ["InProgress"],
            ApprovalStatuses = ["Approved"]
        };

    private static ProcurementRuleVersion Rule(string conditionValue,
        RuleSeverity severity = RuleSeverity.Critical, RuleType type = RuleType.Threshold,
        string conditionType = "json") =>
        new(Guid.NewGuid(), Guid.NewGuid(), 1, "قاعده آزمایشی", type, severity,
            RuleEvaluationMode.Automatic, null, null, "ماده ۱", conditionType, conditionValue, null, Guid.NewGuid());

    private static async Task<RuleResult> Eval(string json, RuleSeverity severity = RuleSeverity.Critical) =>
        (await Engine.EvaluateAsync(Rule(json, severity), Context())).Result;

    [Fact]
    public async Task AmountThresholdPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""{ "op": ">=", "field": "FinalAmount", "value": 1000000000, "currency": "IRR" }"""));

    [Fact]
    public async Task AmountThresholdFails() =>
        Assert.Equal(RuleResult.Fail, await Eval("""{ "op": ">=", "field": "FinalAmount", "value": 2000000000 }"""));

    [Fact]
    public async Task RequiredDocumentExistsPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""{ "op": "exists", "field": "DocumentTypes", "value": "TechnicalSpecification" }"""));

    [Fact]
    public async Task RequiredDocumentMissingFails() =>
        Assert.Equal(RuleResult.Fail, await Eval("""{ "op": "exists", "field": "DocumentTypes", "value": "Contract" }"""));

    [Fact]
    public async Task SupplierCountPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""{ "op": "count>=", "field": "SupplierCount", "value": 3 }"""));

    [Fact]
    public async Task SupplierCountFailsAsWarningWhenWarningSeverity() =>
        Assert.Equal(RuleResult.Warning, await Eval("""{ "op": "count>=", "field": "SupplierCount", "value": 5 }""", RuleSeverity.Warning));

    [Fact]
    public async Task StatusEqualsPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""{ "op": "equals", "field": "Status", "value": "Open" }"""));

    [Fact]
    public async Task StatusEqualsFails() =>
        Assert.Equal(RuleResult.Fail, await Eval("""{ "op": "equals", "field": "Status", "value": "Closed" }"""));

    [Fact]
    public async Task GroupedAllPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""
        { "all": [
            { "op": ">=", "field": "FinalAmount", "value": 1000000000 },
            { "op": "count>=", "field": "SupplierCount", "value": 3 }
        ] }
        """));

    [Fact]
    public async Task GroupedAnyPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""
        { "any": [
            { "op": ">=", "field": "FinalAmount", "value": 9000000000 },
            { "op": "count>=", "field": "SupplierCount", "value": 3 }
        ] }
        """));

    [Fact]
    public async Task GroupedNoneFailsWhenAnyChildSatisfied() =>
        Assert.Equal(RuleResult.Fail, await Eval("""
        { "none": [
            { "op": "exists", "field": "DocumentTypes", "value": "Indent" }
        ] }
        """));

    [Fact]
    public async Task DeadlineOverduePasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""{ "op": "overdue", "field": "TenderDeadline" }"""));

    [Fact]
    public async Task BetweenPasses() =>
        Assert.Equal(RuleResult.Pass, await Eval("""{ "op": "between", "field": "SupplierCount", "value": [1, 5] }"""));

    [Fact]
    public async Task InvalidFieldYieldsNeedHumanReviewForCriticalSeverity() =>
        Assert.Equal(RuleResult.NeedHumanReview,
            await Eval("""{ "op": ">=", "field": "NotARealField", "value": 1 }""", RuleSeverity.Critical));

    [Fact]
    public async Task SeverityOverrideIsApplied()
    {
        var outcome = await Engine.EvaluateAsync(
            Rule("""{ "op": ">=", "field": "FinalAmount", "value": 9000000000, "severityOverride": "Warning" }"""),
            Context());
        Assert.Equal(RuleResult.Warning, outcome.Result);
        Assert.Equal(RuleSeverity.Warning, outcome.SeverityOverride);
    }

    [Fact]
    public async Task ExceptWhenSkipsRuleWhenInnerConditionMatches() =>
        Assert.Equal(RuleResult.NotApplicable, await Eval("""
        { "op": "exceptWhen", "condition": { "op": "equals", "field": "Status", "value": "Open" } }
        """));

    [Fact]
    public async Task InvalidJsonIsNotEngineEvaluatedSoLegacyFallbackApplies()
    {
        var malformed = Rule("{ not valid json", conditionType: "alwayspass");
        Assert.False(Engine.CanEvaluate(malformed));
    }

    [Fact]
    public void LegacyConditionIsNotEngineEvaluated()
    {
        Assert.False(Engine.CanEvaluate(Rule("Indent", conditionType: "requireddocumenttype")));
        Assert.False(Engine.CanEvaluate(Rule("true", conditionType: "alwayspass")));
    }

    [Fact]
    public void ValidJsonConditionIsEngineEvaluated() =>
        Assert.True(Engine.CanEvaluate(Rule("""{ "op": ">=", "field": "FinalAmount", "value": 1 }""")));

    [Theory]
    [MemberData(nameof(RuleTypeCases))]
    public async Task AtLeastOneConditionPerRuleType(RuleType type, string json, RuleResult expected)
    {
        var outcome = await Engine.EvaluateAsync(Rule(json, RuleSeverity.Critical, type), Context());
        Assert.Equal(expected, outcome.Result);
    }

    public static IEnumerable<object[]> RuleTypeCases() => new[]
    {
        new object[] { RuleType.Threshold, """{ "op": ">=", "field": "FinalAmount", "value": 1000000000 }""", RuleResult.Pass },
        new object[] { RuleType.Document, """{ "op": "exists", "field": "DocumentTypes", "value": "Indent" }""", RuleResult.Pass },
        new object[] { RuleType.Checklist, """{ "op": "allOf", "field": "DocumentTypes", "value": ["Indent", "TechnicalSpecification"] }""", RuleResult.Pass },
        new object[] { RuleType.Workflow, """{ "op": "equals", "field": "Status", "value": "Open" }""", RuleResult.Pass },
        new object[] { RuleType.Deadline, """{ "op": "overdue", "field": "TenderDeadline" }""", RuleResult.Pass },
        new object[] { RuleType.Evaluation, """{ "op": "count>=", "field": "SupplierCount", "value": 3 }""", RuleResult.Pass },
        new object[] { RuleType.Exception, """{ "op": "exceptWhen", "condition": { "op": "equals", "field": "Status", "value": "Open" } }""", RuleResult.NotApplicable }
    };
}
