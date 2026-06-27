using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class ProcurementRuleGateDataSource(PetroProcureDbContext db) : IProcurementRuleGateDataSource
{
    public Task<Guid?> ResolvePurchaseFileIdAsync(Guid entityId, string transitionName, CancellationToken ct = default) =>
        transitionName switch
        {
            ProcurementRuleGateTransitions.TenderPublish
                or ProcurementRuleGateTransitions.TenderApproveInvitation
                or ProcurementRuleGateTransitions.TenderApproveWinner
                or ProcurementRuleGateTransitions.TenderSelectWinner =>
                db.Tenders.AsNoTracking()
                    .Where(x => x.Id == entityId)
                    .Select(x => (Guid?)x.PurchaseFileId)
                    .SingleOrDefaultAsync(ct),

            ProcurementRuleGateTransitions.PurchaseOrderIssue =>
                db.PurchaseOrders.AsNoTracking()
                    .Where(x => x.Id == entityId)
                    .Select(x => (Guid?)x.PurchaseFileId)
                    .SingleOrDefaultAsync(ct),

            ProcurementRuleGateTransitions.PurchaseFileComplete
                or ProcurementRuleGateTransitions.PurchaseFileArchive =>
                db.PurchaseFiles.AsNoTracking()
                    .Where(x => x.Id == entityId)
                    .Select(x => (Guid?)x.Id)
                    .SingleOrDefaultAsync(ct),

            _ => Task.FromResult<Guid?>(null)
        };

    public async Task<IReadOnlyList<ProcurementRuleGateFinding>> GetBlockingFindingsAsync(
        Guid purchaseFileId, CancellationToken ct = default) =>
        await (
            from finding in db.LegalProcurementRuleFindings.AsNoTracking()
            join evaluation in db.LegalProcurementRuleEvaluations.AsNoTracking()
                on finding.ProcurementRuleEvaluationId equals evaluation.Id
            where (evaluation.PurchaseFileId == purchaseFileId || evaluation.EntityId == purchaseFileId)
                && finding.Severity == RuleSeverity.Blocking
                && finding.Result == RuleResult.Fail
            select new ProcurementRuleGateFinding(
                finding.Id,
                finding.ProcurementRuleId,
                finding.RuleVersionId,
                evaluation.PurchaseFileId ?? evaluation.EntityId,
                finding.Result,
                finding.Severity,
                finding.Title,
                finding.LegalReference))
            .ToListAsync(ct);

    public async Task<IReadOnlySet<Guid>> GetOverriddenFindingIdsAsync(
        Guid purchaseFileId, string transitionName, CancellationToken ct = default)
    {
        var action = OverrideAction(transitionName);
        return (await db.LegalRuleAuditLogs.AsNoTracking()
                .Where(x => x.PurchaseFileId == purchaseFileId
                    && x.Action == action
                    && x.FindingId.HasValue)
                .Select(x => x.FindingId!.Value)
                .ToListAsync(ct))
            .ToHashSet();
    }

    public async Task AddOverrideAuditAsync(
        Guid purchaseFileId,
        string transitionName,
        IReadOnlyList<ProcurementRuleGateFinding> findings,
        Guid userId,
        string reason,
        CancellationToken ct = default)
    {
        var action = OverrideAction(transitionName);
        foreach (var finding in findings)
        {
            await db.LegalRuleAuditLogs.AddAsync(new LegalRuleAuditLog(
                Guid.NewGuid(),
                purchaseFileId,
                finding.RuleId,
                finding.FindingId,
                action,
                finding.Result.ToString(),
                "OverrideAllowed",
                userId,
                reason), ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static string OverrideAction(string transitionName) => $"Override:{transitionName}";
}
