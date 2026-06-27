using PetroProcure.Application.Legal;
using PetroProcure.Application.Security;
using PetroProcure.Domain.Enums;

namespace PetroProcure.UnitTests.Application;

public sealed class ProcurementRuleGateServiceTests
{
    private static readonly Guid PurchaseFileId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid EntityId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid UserId = Guid.Parse("30000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task BlockingFailPreventsTransition()
    {
        var source = new FakeGateDataSource();
        source.Findings.Add(Finding(RuleSeverity.Blocking, RuleResult.Fail));
        var service = new ProcurementRuleGateService(source);

        var result = await service.CheckTransitionAsync(EntityId, ProcurementRuleGateTransitions.TenderPublish, User());

        Assert.True(result.IsBlocked);
        Assert.Single(result.BlockingFindings);
    }

    [Fact]
    public async Task WarningDoesNotBlockTransition()
    {
        var source = new FakeGateDataSource();
        source.Findings.Add(Finding(RuleSeverity.Warning, RuleResult.Fail));
        var service = new ProcurementRuleGateService(source);

        var result = await service.CheckTransitionAsync(EntityId, ProcurementRuleGateTransitions.TenderPublish, User());

        Assert.False(result.IsBlocked);
        Assert.Empty(result.BlockingFindings);
    }

    [Fact]
    public async Task AuthorizedOverrideAllowsTransition()
    {
        var source = new FakeGateDataSource();
        source.Findings.Add(Finding(RuleSeverity.Blocking, RuleResult.Fail));
        var service = new ProcurementRuleGateService(source);

        await service.OverrideTransitionAsync(EntityId, ProcurementRuleGateTransitions.TenderPublish,
            "مدیر حقوقی مجوز عبور داد.", User(ApplicationPermissions.LegalRuleOverrideBlockingFinding));
        var result = await service.CheckTransitionAsync(EntityId, ProcurementRuleGateTransitions.TenderPublish, User());

        Assert.False(result.IsBlocked);
    }

    [Fact]
    public async Task UnauthorizedOverrideFails()
    {
        var source = new FakeGateDataSource();
        source.Findings.Add(Finding(RuleSeverity.Blocking, RuleResult.Fail));
        var service = new ProcurementRuleGateService(source);

        await Assert.ThrowsAsync<CurrentUserForbiddenException>(() =>
            service.OverrideTransitionAsync(EntityId, ProcurementRuleGateTransitions.TenderPublish,
                "ندارد", User()));
    }

    [Fact]
    public async Task OverrideCreatesAuditLog()
    {
        var source = new FakeGateDataSource();
        source.Findings.Add(Finding(RuleSeverity.Blocking, RuleResult.Fail));
        var service = new ProcurementRuleGateService(source);

        await service.OverrideTransitionAsync(EntityId, ProcurementRuleGateTransitions.TenderPublish,
            "رفع اضطرار عملیاتی", User(ApplicationPermissions.LegalRuleOverrideBlockingFinding));

        var audit = Assert.Single(source.Audits);
        Assert.Equal(PurchaseFileId, audit.PurchaseFileId);
        Assert.Equal(ProcurementRuleGateTransitions.TenderPublish, audit.TransitionName);
        Assert.Equal("رفع اضطرار عملیاتی", audit.Reason);
    }

    private static ProcurementRuleGateUserContext User(params string[] permissions) =>
        new(UserId, IsSystemAdmin: false, permissions);

    private static ProcurementRuleGateFinding Finding(RuleSeverity severity, RuleResult result) =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PurchaseFileId, result, severity,
            "عدم رعایت الزام", "ماده ۱");

    private sealed class FakeGateDataSource : IProcurementRuleGateDataSource
    {
        public List<ProcurementRuleGateFinding> Findings { get; } = [];
        public List<Audit> Audits { get; } = [];

        public Task<Guid?> ResolvePurchaseFileIdAsync(Guid entityId, string transitionName, CancellationToken ct = default) =>
            Task.FromResult<Guid?>(PurchaseFileId);

        public Task<IReadOnlyList<ProcurementRuleGateFinding>> GetBlockingFindingsAsync(
            Guid purchaseFileId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ProcurementRuleGateFinding>>(Findings
                .Where(x => x.PurchaseFileId == purchaseFileId
                    && x.Severity == RuleSeverity.Blocking
                    && x.Result == RuleResult.Fail)
                .ToArray());

        public Task<IReadOnlySet<Guid>> GetOverriddenFindingIdsAsync(
            Guid purchaseFileId, string transitionName, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlySet<Guid>>(Audits
                .Where(x => x.PurchaseFileId == purchaseFileId && x.TransitionName == transitionName)
                .Select(x => x.FindingId)
                .ToHashSet());

        public Task AddOverrideAuditAsync(
            Guid purchaseFileId,
            string transitionName,
            IReadOnlyList<ProcurementRuleGateFinding> findings,
            Guid userId,
            string reason,
            CancellationToken ct = default)
        {
            Audits.AddRange(findings.Select(x => new Audit(purchaseFileId, x.FindingId, transitionName, reason)));
            return Task.CompletedTask;
        }
    }

    private sealed record Audit(Guid PurchaseFileId, Guid FindingId, string TransitionName, string Reason);
}
