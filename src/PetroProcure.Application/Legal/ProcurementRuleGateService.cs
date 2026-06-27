using PetroProcure.Application.Security;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Application.Legal;

public static class ProcurementRuleGateTransitions
{
    public const string TenderPublish = "Tender.Publish";
    public const string TenderApproveInvitation = "Tender.ApproveInvitation";
    public const string TenderApproveWinner = "Tender.ApproveWinner";
    public const string TenderSelectWinner = "Tender.SelectWinner";
    public const string PurchaseOrderIssue = "PurchaseOrder.Issue";
    public const string PurchaseFileComplete = "PurchaseFile.Complete";
    public const string PurchaseFileArchive = "PurchaseFile.Archive";
}

public sealed record ProcurementRuleGateUserContext(
    Guid UserId,
    bool IsSystemAdmin,
    IReadOnlyCollection<string> Permissions);

public sealed record ProcurementRuleGateFinding(
    Guid FindingId,
    Guid RuleId,
    Guid RuleVersionId,
    Guid PurchaseFileId,
    RuleResult Result,
    RuleSeverity Severity,
    string Title,
    string LegalReference);

public sealed record ProcurementRuleGateResult(
    Guid EntityId,
    Guid? PurchaseFileId,
    string TransitionName,
    bool IsBlocked,
    IReadOnlyList<ProcurementRuleGateFinding> BlockingFindings);

public interface IProcurementRuleGateService
{
    Task<ProcurementRuleGateResult> CheckTransitionAsync(
        Guid entityId, string transitionName, ProcurementRuleGateUserContext userContext, CancellationToken ct = default);

    Task<ProcurementRuleGateResult> OverrideTransitionAsync(
        Guid entityId, string transitionName, string reason, ProcurementRuleGateUserContext userContext, CancellationToken ct = default);
}

public interface IProcurementRuleGateDataSource
{
    Task<Guid?> ResolvePurchaseFileIdAsync(Guid entityId, string transitionName, CancellationToken ct = default);
    Task<IReadOnlyList<ProcurementRuleGateFinding>> GetBlockingFindingsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> GetOverriddenFindingIdsAsync(Guid purchaseFileId, string transitionName, CancellationToken ct = default);
    Task AddOverrideAuditAsync(Guid purchaseFileId, string transitionName, IReadOnlyList<ProcurementRuleGateFinding> findings,
        Guid userId, string reason, CancellationToken ct = default);
}

public sealed class ProcurementRuleGateService(IProcurementRuleGateDataSource dataSource) : IProcurementRuleGateService
{
    public async Task<ProcurementRuleGateResult> CheckTransitionAsync(
        Guid entityId, string transitionName, ProcurementRuleGateUserContext userContext, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        var purchaseFileId = await dataSource.ResolvePurchaseFileIdAsync(entityId, transitionName, ct);
        if (purchaseFileId is null)
            return new ProcurementRuleGateResult(entityId, null, transitionName, IsBlocked: false, []);

        var blocking = await ActiveBlockingFindingsAsync(purchaseFileId.Value, transitionName, ct);
        return new ProcurementRuleGateResult(entityId, purchaseFileId, transitionName, blocking.Count > 0, blocking);
    }

    public async Task<ProcurementRuleGateResult> OverrideTransitionAsync(
        Guid entityId, string transitionName, string reason, ProcurementRuleGateUserContext userContext, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        if (string.IsNullOrWhiteSpace(reason))
            throw new LegalRuleValidationException("Override reason is required.");
        if (!CanOverride(userContext))
            throw new CurrentUserForbiddenException("User is not allowed to override blocking legal findings.");

        var purchaseFileId = await dataSource.ResolvePurchaseFileIdAsync(entityId, transitionName, ct)
            ?? throw new LegalRuleValidationException("Purchase file could not be resolved for this transition.");
        var blocking = await ActiveBlockingFindingsAsync(purchaseFileId, transitionName, ct);
        if (blocking.Count > 0)
            await dataSource.AddOverrideAuditAsync(purchaseFileId, transitionName, blocking, userContext.UserId, reason.Trim(), ct);

        return new ProcurementRuleGateResult(entityId, purchaseFileId, transitionName, IsBlocked: false, []);
    }

    private async Task<IReadOnlyList<ProcurementRuleGateFinding>> ActiveBlockingFindingsAsync(
        Guid purchaseFileId, string transitionName, CancellationToken ct)
    {
        var findings = await dataSource.GetBlockingFindingsAsync(purchaseFileId, ct);
        if (findings.Count == 0) return [];

        var overridden = await dataSource.GetOverriddenFindingIdsAsync(purchaseFileId, transitionName, ct);
        return findings.Where(x => !overridden.Contains(x.FindingId)).ToArray();
    }

    private static bool CanOverride(ProcurementRuleGateUserContext userContext) =>
        userContext.IsSystemAdmin
        || userContext.Permissions.Contains(ApplicationPermissions.LegalRuleOverrideBlockingFinding);
}
