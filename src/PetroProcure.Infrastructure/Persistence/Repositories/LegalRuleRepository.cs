using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Legal;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class LegalRuleRepository(PetroProcureDbContext db) : ILegalRuleRepository, IPurchaseFileRuleContextBuilder
{
    public async Task AddLegalDocumentAsync(LegalDocument document, CancellationToken ct) => await db.LegalDocuments.AddAsync(document, ct);
    public Task<LegalDocument?> FindLegalDocumentAsync(Guid id, CancellationToken ct) => db.LegalDocuments.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddArticleAsync(LegalArticle article, CancellationToken ct) => await db.LegalArticles.AddAsync(article, ct);
    public Task<LegalArticle?> FindArticleAsync(Guid id, CancellationToken ct) => db.LegalArticles.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddClauseAsync(LegalClause clause, CancellationToken ct) => await db.LegalClauses.AddAsync(clause, ct);
    public async Task AddRuleAsync(ProcurementRule rule, ProcurementRuleVersion version, CancellationToken ct)
    {
        if (db.Entry(rule).State == EntityState.Detached && !await db.LegalProcurementRules.AnyAsync(x => x.Id == rule.Id, ct))
            await db.LegalProcurementRules.AddAsync(rule, ct);
        await db.LegalProcurementRuleVersions.AddAsync(version, ct);
    }
    public Task<ProcurementRule?> FindRuleAsync(Guid id, bool includeVersions, CancellationToken ct)
    {
        IQueryable<ProcurementRule> query = db.LegalProcurementRules;
        if (includeVersions) query = query.Include(x => x.Versions);
        return query.SingleOrDefaultAsync(x => x.Id == id, ct);
    }
    public Task<ProcurementRuleVersion?> FindRuleVersionAsync(Guid id, CancellationToken ct) =>
        db.LegalProcurementRuleVersions.SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<ProcurementRuleVersion?> FindLatestDraftVersionAsync(Guid ruleId, CancellationToken ct) =>
        db.LegalProcurementRuleVersions.Where(x => x.ProcurementRuleId == ruleId && x.Status == RuleStatus.Draft)
            .OrderByDescending(x => x.VersionNo).FirstOrDefaultAsync(ct);
    public Task<ProcurementRuleVersion?> FindPendingVersionAsync(Guid ruleId, CancellationToken ct) =>
        db.LegalProcurementRuleVersions.Where(x => x.ProcurementRuleId == ruleId && x.Status == RuleStatus.PendingApproval)
            .OrderByDescending(x => x.VersionNo).FirstOrDefaultAsync(ct);
    public async Task<int> NextRuleVersionNoAsync(Guid ruleId, CancellationToken ct) =>
        (await db.LegalProcurementRuleVersions.Where(x => x.ProcurementRuleId == ruleId).MaxAsync(x => (int?)x.VersionNo, ct) ?? 0) + 1;
    public async Task<IReadOnlyList<ProcurementRuleVersion>> GetActiveRuleVersionsAsync(CancellationToken ct) =>
        await db.LegalProcurementRuleVersions.AsNoTracking().Where(x => x.Status == RuleStatus.Active).ToListAsync(ct);
    public async Task AddEvaluationAsync(ProcurementRuleEvaluation evaluation, CancellationToken ct) => await db.LegalProcurementRuleEvaluations.AddAsync(evaluation, ct);
    public async Task AddAuditAsync(LegalRuleAuditLog audit, CancellationToken ct) => await db.LegalRuleAuditLogs.AddAsync(audit, ct);
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<PagedResult<LegalDocumentDto>> GetLegalDocumentsAsync(LegalDocumentListRequest r, CancellationToken ct)
    {
        var query = db.LegalDocuments.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.SearchText.Contains(r.SearchTerm) || x.OriginalFileName.Contains(r.SearchTerm));
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderByDescending(x => x.UploadedAt).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new LegalDocumentDto(x.Id, x.Title, x.OriginalFileName, x.FileHash, x.RelativePath,
                x.Description, x.Status, x.UploadedByUserId, x.UploadedAt)).ToListAsync(ct);
        return new PagedResult<LegalDocumentDto>(items, r.PageNumber, r.PageSize, total);
    }

    public Task<LegalDocumentDto?> GetLegalDocumentAsync(Guid id, CancellationToken ct) =>
        db.LegalDocuments.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new LegalDocumentDto(x.Id, x.Title, x.OriginalFileName, x.FileHash, x.RelativePath,
                x.Description, x.Status, x.UploadedByUserId, x.UploadedAt)).SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyList<LegalArticleDto>> GetArticlesByDocumentAsync(Guid documentId, CancellationToken ct)
    {
        var clauses = await db.LegalClauses.AsNoTracking()
            .Join(db.LegalArticles.AsNoTracking().Where(a => a.LegalDocumentId == documentId),
                c => c.LegalArticleId, a => a.Id, (c, a) => c)
            .OrderBy(x => x.OrderNo)
            .Select(x => new LegalClauseDto(x.Id, x.LegalArticleId, x.ClauseNumber, x.Body, x.OrderNo, x.Note))
            .ToListAsync(ct);
        var lookup = clauses.GroupBy(x => x.LegalArticleId).ToDictionary(x => x.Key, x => (IReadOnlyList<LegalClauseDto>)x.ToArray());
        var articles = await db.LegalArticles.AsNoTracking().Where(x => x.LegalDocumentId == documentId).OrderBy(x => x.OrderNo)
            .Select(x => new { x.Id, x.LegalDocumentId, x.ArticleNumber, x.Title, x.Body, x.OrderNo }).ToListAsync(ct);
        return articles.Select(x => new LegalArticleDto(x.Id, x.LegalDocumentId, x.ArticleNumber, x.Title, x.Body,
            x.OrderNo, lookup.TryGetValue(x.Id, out var list) ? list : [])).ToArray();
    }

    public async Task<PagedResult<ProcurementRuleDto>> GetRulesAsync(ProcurementRuleListRequest r, CancellationToken ct)
    {
        var query = RuleQuery();
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.Rule.Code.Contains(r.SearchTerm) || x.Rule.Title.Contains(r.SearchTerm) || (x.Version != null && x.Version.SearchText.Contains(r.SearchTerm)));
        if (r.Status.HasValue) query = query.Where(x => x.Version != null && x.Version.Status == r.Status);
        if (r.RuleType.HasValue) query = query.Where(x => x.Version != null && x.Version.RuleType == r.RuleType);
        if (r.Severity.HasValue) query = query.Where(x => x.Version != null && x.Version.Severity == r.Severity);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(x => x.Rule.Code).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new PagedResult<ProcurementRuleDto>(items.Select(ToDto).ToArray(), r.PageNumber, r.PageSize, total);
    }

    public async Task<IReadOnlyList<ProcurementRuleVersionDto>> GetRuleVersionsAsync(Guid ruleId, CancellationToken ct) =>
        await db.LegalProcurementRuleVersions.AsNoTracking().Where(x => x.ProcurementRuleId == ruleId)
            .OrderByDescending(x => x.VersionNo).Select(x => ToVersionDto(x)).ToListAsync(ct);

    public async Task<IReadOnlyList<ProcurementRuleEvaluationDto>> GetEvaluationsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct)
    {
        var evaluations = await db.LegalProcurementRuleEvaluations.AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId || x.EntityId == purchaseFileId)
            .OrderByDescending(x => x.EvaluatedAt)
            .ToListAsync(ct);
        var ids = evaluations.Select(x => x.Id).ToArray();
        var findings = await db.LegalProcurementRuleFindings.AsNoTracking()
            .Where(x => ids.Contains(x.ProcurementRuleEvaluationId))
            .Select(x => ToFindingDto(x)).ToListAsync(ct);
        var lookup = findings.GroupBy(x => x.ProcurementRuleEvaluationId).ToDictionary(x => x.Key, x => (IReadOnlyList<ProcurementRuleFindingDto>)x.ToArray());
        return evaluations.Select(x => new ProcurementRuleEvaluationDto(x.Id, x.EntityType, x.EntityId,
            x.PurchaseFileId, x.TenderId, x.Summary, x.EvaluatedByUserId, x.EvaluatedAt,
            lookup.TryGetValue(x.Id, out var list) ? list : [])).ToArray();
    }

    public async Task<IReadOnlyList<LegalClauseDto>> SearchClausesAsync(string term, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(term)) return [];
        return await db.LegalClauses.AsNoTracking().Where(x => x.SearchText.Contains(term))
            .OrderBy(x => x.ClauseNumber).Take(20)
            .Select(x => new LegalClauseDto(x.Id, x.LegalArticleId, x.ClauseNumber, x.Body, x.OrderNo, x.Note))
            .ToListAsync(ct);
    }

    public async Task<RuleEvaluationContext> BuildPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct = default)
    {
        var file = await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == purchaseFileId, ct)
            ?? throw new LegalRuleNotFoundException("Purchase file was not found.");
        var itemCount = await db.PurchaseFileItems.AsNoTracking().CountAsync(x => x.PurchaseFileId == purchaseFileId, ct);
        var hasTender = await db.Tenders.AsNoTracking().AnyAsync(x => x.PurchaseFileId == purchaseFileId, ct);
        var docs = await db.FileDocuments.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId && !x.IsDeleted)
            .Select(x => x.DocumentType.ToString()).ToListAsync(ct);
        return new RuleEvaluationContext("PurchaseFile", purchaseFileId, purchaseFileId, null, file.Status.ToString(),
            hasTender, itemCount, docs.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    public async Task<RuleEvaluationContext> BuildTenderAsync(Guid tenderId, CancellationToken ct = default)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == tenderId, ct)
            ?? throw new LegalRuleNotFoundException("Tender was not found.");
        var itemCount = await db.TenderItems.AsNoTracking().CountAsync(x => x.TenderId == tenderId, ct);
        var docs = await db.TenderDocuments.AsNoTracking().Where(x => x.TenderId == tenderId)
            .Select(x => x.DocumentType).ToListAsync(ct);
        return new RuleEvaluationContext("Tender", tenderId, tender.PurchaseFileId, tenderId, tender.Status.ToString(),
            true, itemCount, docs.ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private IQueryable<RuleProjection> RuleQuery() =>
        from rule in db.LegalProcurementRules.AsNoTracking()
        join active in db.LegalProcurementRuleVersions.AsNoTracking() on rule.ActiveVersionId equals active.Id into activeVersions
        from active in activeVersions.DefaultIfEmpty()
        select new RuleProjection(rule, active);

    private static ProcurementRuleDto ToDto(RuleProjection p) =>
        new(p.Rule.Id, p.Rule.Code, p.Rule.Title, p.Rule.RuleSetId, p.Rule.ActiveVersionId, p.Rule.CreatedAt,
            p.Version is null ? null : ToVersionDto(p.Version));

    private static ProcurementRuleVersionDto ToVersionDto(ProcurementRuleVersion x) =>
        new(x.Id, x.ProcurementRuleId, x.VersionNo, x.Title, x.RuleType, x.Severity, x.EvaluationMode,
            x.Status, x.LegalArticleId, x.LegalClauseId, x.LegalReference, x.ConditionType,
            x.ConditionValue, x.ConditionDescription, x.CreatedAt, x.ApprovedAt);

    private static ProcurementRuleFindingDto ToFindingDto(ProcurementRuleFinding x) =>
        new(x.Id, x.ProcurementRuleEvaluationId, x.ProcurementRuleId, x.RuleVersionId, x.Result, x.Severity,
            x.Title, x.Description, x.LegalReference, x.LegalArticleId, x.LegalClauseId);

    private sealed record RuleProjection(ProcurementRule Rule, ProcurementRuleVersion? Version);
}
