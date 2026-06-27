using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Ai;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class GroundedAiAnalysisDataSource(PetroProcureDbContext db) : IGroundedAiAnalysisDataSource
{
    public async Task<string?> GetPurchaseFileContextAsync(Guid purchaseFileId, CancellationToken ct = default)
    {
        var file = await db.PurchaseFiles.AsNoTracking()
            .Where(x => x.Id == purchaseFileId)
            .Select(x => new
            {
                x.FileNumber,
                x.Title,
                x.Description,
                x.Status,
                x.Priority,
                x.CreatedAt,
                x.CurrentDepartmentId
            })
            .SingleOrDefaultAsync(ct);
        if (file is null) return null;

        var documentCount = await db.FileDocuments.AsNoTracking()
            .CountAsync(x => x.PurchaseFileId == purchaseFileId && !x.IsDeleted, ct);
        var tenderCount = await db.Tenders.AsNoTracking()
            .CountAsync(x => x.PurchaseFileId == purchaseFileId, ct);
        var orderCount = await db.PurchaseOrders.AsNoTracking()
            .CountAsync(x => x.PurchaseFileId == purchaseFileId, ct);

        return $"PurchaseFile {file.FileNumber}: {file.Title}; Status={file.Status}; Priority={file.Priority}; " +
            $"CreatedAt={file.CreatedAt:O}; CurrentDepartmentId={file.CurrentDepartmentId}; " +
            $"Documents={documentCount}; Tenders={tenderCount}; PurchaseOrders={orderCount}; " +
            $"Description={file.Description}";
    }

    public async Task<GroundedRuleFindingContext?> GetRuleFindingContextAsync(
        Guid findingId, CancellationToken ct = default) =>
        await (
            from finding in db.LegalProcurementRuleFindings.AsNoTracking()
            join evaluation in db.LegalProcurementRuleEvaluations.AsNoTracking()
                on finding.ProcurementRuleEvaluationId equals evaluation.Id
            where finding.Id == findingId
            select new GroundedRuleFindingContext(
                finding.Id,
                evaluation.PurchaseFileId,
                finding.Title,
                finding.Description,
                finding.Result.ToString(),
                finding.Severity.ToString(),
                finding.LegalReference,
                finding.CitationReferences))
            .SingleOrDefaultAsync(ct);
}
