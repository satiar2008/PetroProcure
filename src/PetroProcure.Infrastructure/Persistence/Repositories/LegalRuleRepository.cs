using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Legal;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class LegalRuleRepository(PetroProcureDbContext db, ICurrentUserService currentUser)
    : ILegalRuleRepository, IPurchaseFileRuleContextBuilder
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
    public Task<ProcurementRule?> FindRuleByCodeAsync(string code, bool includeVersions, CancellationToken ct)
    {
        IQueryable<ProcurementRule> query = db.LegalProcurementRules;
        if (includeVersions) query = query.Include(x => x.Versions);
        return query.SingleOrDefaultAsync(x => x.Code == code, ct);
    }

    public async Task<ProcurementRuleDto?> GetRuleAsync(Guid id, CancellationToken ct)
    {
        var row = await RuleQuery().SingleOrDefaultAsync(x => x.Id == id, ct);
        return row is null ? null : ToDto(row);
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
        if (!r.IncludeDeleted) query = query.Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.SearchText.Contains(r.SearchTerm) || x.OriginalFileName.Contains(r.SearchTerm));
        if (r.Status.HasValue) query = query.Where(x => x.Status == r.Status);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderByDescending(x => x.UploadedAt).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new LegalDocumentDto(x.Id, x.Title, x.OriginalFileName, x.StoredFileName, x.FileHash,
                x.RelativePath, x.Extension, x.MimeType, x.Size, x.Description, x.Status, x.UploadedByUserId,
                x.UploadedAt, x.IsDeleted, x.DeletedAt, x.DeletedByUserId, x.SourceDocumentTitle,
                x.SourceDocumentNumber, x.SourceDocumentDate)).ToListAsync(ct);
        return new PagedResult<LegalDocumentDto>(items, r.PageNumber, r.PageSize, total);
    }

    public Task<LegalDocumentDto?> GetLegalDocumentAsync(Guid id, CancellationToken ct) =>
        db.LegalDocuments.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new LegalDocumentDto(x.Id, x.Title, x.OriginalFileName, x.StoredFileName, x.FileHash,
                x.RelativePath, x.Extension, x.MimeType, x.Size, x.Description, x.Status, x.UploadedByUserId,
                x.UploadedAt, x.IsDeleted, x.DeletedAt, x.DeletedByUserId, x.SourceDocumentTitle,
                x.SourceDocumentNumber, x.SourceDocumentDate)).SingleOrDefaultAsync(ct);

    public async Task<IReadOnlyList<LegalArticleDto>> GetArticlesByDocumentAsync(Guid documentId, CancellationToken ct)
    {
        var clauses = await db.LegalClauses.AsNoTracking()
            .Join(db.LegalArticles.AsNoTracking().Where(a => a.LegalDocumentId == documentId),
                c => c.LegalArticleId, a => a.Id, (c, a) => c)
            .OrderBy(x => x.OrderNo)
            .Select(x => new LegalClauseDto(x.Id, x.LegalArticleId, x.ClauseNumber, x.Body, x.OrderNo,
                x.Note, x.AppliesTo, x.Severity, x.Tags))
            .ToListAsync(ct);
        var lookup = clauses.GroupBy(x => x.LegalArticleId).ToDictionary(x => x.Key, x => (IReadOnlyList<LegalClauseDto>)x.ToArray());
        var articles = await db.LegalArticles.AsNoTracking().Where(x => x.LegalDocumentId == documentId).OrderBy(x => x.OrderNo)
            .Select(x => new { x.Id, x.LegalDocumentId, x.ArticleNumber, x.Title, x.Body, x.OrderNo }).ToListAsync(ct);
        return articles.Select(x => new LegalArticleDto(x.Id, x.LegalDocumentId, x.ArticleNumber, x.Title, x.Body,
            x.OrderNo, lookup.TryGetValue(x.Id, out var list) ? list : [])).ToArray();
    }

    public async Task<PagedResult<LegalArticleDto>> SearchArticlesAsync(LegalArticleSearchRequest r, CancellationToken ct)
    {
        var query = db.LegalArticles.AsNoTracking()
            .Join(db.LegalDocuments.AsNoTracking().Where(d => !d.IsDeleted), a => a.LegalDocumentId, d => d.Id, (a, d) => a);
        if (r.DocumentId.HasValue) query = query.Where(x => x.LegalDocumentId == r.DocumentId);
        if (!string.IsNullOrWhiteSpace(r.ArticleNumber)) query = query.Where(x => x.ArticleNumber.Contains(r.ArticleNumber));
        if (!string.IsNullOrWhiteSpace(r.AppliesTo))
            query = query.Where(x => db.LegalClauses.Any(c => c.LegalArticleId == x.Id && c.AppliesTo == r.AppliesTo));
        if (r.Severity.HasValue)
            query = query.Where(x => db.LegalClauses.Any(c => c.LegalArticleId == x.Id && c.Severity == r.Severity));
        if (!string.IsNullOrWhiteSpace(r.Tag))
            query = query.Where(x => db.LegalClauses.Any(c => c.LegalArticleId == x.Id && c.Tags != null && c.Tags.Contains(r.Tag)));
        if (!string.IsNullOrWhiteSpace(r.Term))
        {
            var term = r.Term.Trim();
            query = query.Where(x => x.SearchText.Contains(term));
        }
        var total = await query.LongCountAsync(ct);
        var rows = await query.OrderBy(x => x.ArticleNumber).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize)
            .Select(x => new { x.Id, x.LegalDocumentId, x.ArticleNumber, x.Title, x.Body, x.OrderNo }).ToListAsync(ct);
        return new PagedResult<LegalArticleDto>(rows.Select(x => new LegalArticleDto(x.Id, x.LegalDocumentId,
            x.ArticleNumber, x.Title, x.Body, x.OrderNo, [])).ToArray(), r.PageNumber, r.PageSize, total);
    }

    public async Task<PagedResult<LegalClauseContextDto>> SearchClauseContextsAsync(LegalClauseSearchRequest r, CancellationToken ct)
    {
        var query = ClauseContextRows();
        if (r.DocumentId.HasValue) query = query.Where(x => x.DocumentId == r.DocumentId);
        if (!string.IsNullOrWhiteSpace(r.ArticleNumber)) query = query.Where(x => x.ArticleNumber.Contains(r.ArticleNumber));
        if (!string.IsNullOrWhiteSpace(r.ClauseNumber)) query = query.Where(x => x.ClauseNumber.Contains(r.ClauseNumber));
        if (!string.IsNullOrWhiteSpace(r.AppliesTo)) query = query.Where(x => x.AppliesTo == r.AppliesTo);
        if (r.Severity.HasValue) query = query.Where(x => x.Severity == r.Severity);
        if (!string.IsNullOrWhiteSpace(r.Tag)) query = query.Where(x => x.Tags != null && x.Tags.Contains(r.Tag));
        if (!string.IsNullOrWhiteSpace(r.Term))
        {
            var term = r.Term.Trim();
            query = query.Where(x => x.DocumentTitle.Contains(term) || x.ArticleTitle.Contains(term)
                || x.ArticleNumber.Contains(term) || x.ClauseNumber.Contains(term) || x.ClauseText.Contains(term));
        }
        var total = await query.LongCountAsync(ct);
        var rows = await query.OrderBy(x => x.ArticleNumber).ThenBy(x => x.ClauseNumber)
            .Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
        return new PagedResult<LegalClauseContextDto>(rows.Select(x => x.ToDto()).ToArray(), r.PageNumber, r.PageSize, total);
    }

    public async Task<LegalClauseContextDto?> GetClauseContextAsync(Guid clauseId, CancellationToken ct) =>
        (await ClauseContextRows().SingleOrDefaultAsync(x => x.ClauseId == clauseId, ct))?.ToDto();

    public async Task<PagedResult<ProcurementRuleDto>> GetRulesAsync(ProcurementRuleListRequest r, CancellationToken ct)
    {
        var query = RuleQuery();
        if (!string.IsNullOrWhiteSpace(r.SearchTerm)) query = query.Where(x => x.Code.Contains(r.SearchTerm) || x.Title.Contains(r.SearchTerm) || (x.VersionSearchText != null && x.VersionSearchText.Contains(r.SearchTerm)));
        if (r.Status.HasValue) query = query.Where(x => x.VersionStatus == r.Status);
        if (r.RuleType.HasValue) query = query.Where(x => x.VersionRuleType == r.RuleType);
        if (r.Severity.HasValue) query = query.Where(x => x.VersionSeverity == r.Severity);
        var total = await query.LongCountAsync(ct);
        var items = await query.OrderBy(x => x.Code).Skip((r.PageNumber - 1) * r.PageSize).Take(r.PageSize).ToListAsync(ct);
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

    public async Task<RuleEvaluationContext> BuildPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct = default)
    {
        var file = await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == purchaseFileId, ct)
            ?? throw new LegalRuleNotFoundException("Purchase file was not found.");

        var quantities = await db.PurchaseFileItems.AsNoTracking()
            .Where(x => x.PurchaseFileId == purchaseFileId).Select(x => x.RequestedQuantity).ToListAsync(ct);
        var hasTender = await db.Tenders.AsNoTracking().AnyAsync(x => x.PurchaseFileId == purchaseFileId, ct);
        var docTypes = await db.FileDocuments.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId && !x.IsDeleted)
            .Select(x => x.DocumentType.ToString()).ToListAsync(ct);
        var supplierCount = await db.InquirySuppliers.AsNoTracking()
            .CountAsync(s => db.Inquiries.Any(i => i.Id == s.InquiryId && i.PurchaseFileId == purchaseFileId), ct);
        var offerCount = await db.SupplierQuotes.AsNoTracking()
            .CountAsync(q => db.Inquiries.Any(i => i.Id == q.InquiryId && i.PurchaseFileId == purchaseFileId), ct);

        Guid? requestingDepartmentId = null;
        Guid? applicantDepartmentId = null;
        if (file.SourceIndentId is { } indentId)
        {
            var indent = await db.Indents.AsNoTracking().Where(x => x.Id == indentId)
                .Select(x => new { x.RequestingDepartmentId, x.ApplicantDepartmentId }).SingleOrDefaultAsync(ct);
            if (indent is not null)
            {
                requestingDepartmentId = indent.RequestingDepartmentId;
                applicantDepartmentId = indent.ApplicantDepartmentId;
            }
        }

        var contract = await db.PurchaseContracts.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt).Select(x => new { x.FinalAmount, x.Currency }).FirstOrDefaultAsync(ct);
        var order = await db.PurchaseOrders.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt).Select(x => new { x.FinalAmount, x.Currency }).FirstOrDefaultAsync(ct);
        var tender = await db.Tenders.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.TenderType, x.SubmissionDeadline }).FirstOrDefaultAsync(ct);
        var inquiryDeadline = await db.Inquiries.AsNoTracking().Where(x => x.PurchaseFileId == purchaseFileId)
            .OrderByDescending(x => x.CreatedAt).Select(x => x.DeadlineDate).FirstOrDefaultAsync(ct);

        var approvalStatuses = await db.PurchaseOrderApprovals.AsNoTracking()
            .Where(a => db.PurchaseOrders.Any(p => p.Id == a.PurchaseOrderId && p.PurchaseFileId == purchaseFileId))
            .Select(a => a.Status.ToString()).ToListAsync(ct);
        var workflowStatuses = await db.WorkflowInstances.AsNoTracking()
            .Where(w => w.EntityType == "PurchaseFile" && w.EntityId == purchaseFileId)
            .Select(w => w.Status.ToString()).ToListAsync(ct);

        return new RuleEvaluationContext("PurchaseFile", purchaseFileId, purchaseFileId, null, file.Status.ToString(),
            hasTender, quantities.Count, docTypes.ToHashSet(StringComparer.OrdinalIgnoreCase))
        {
            FileNumber = file.FileNumber,
            CurrentDepartmentId = file.CurrentDepartmentId,
            RequestingDepartmentId = requestingDepartmentId,
            ApplicantDepartmentId = applicantDepartmentId,
            FinalAmount = contract?.FinalAmount ?? order?.FinalAmount,
            Currency = contract?.Currency ?? order?.Currency,
            TotalRequestedQuantity = quantities.Sum(),
            TenderType = tender?.TenderType.ToString(),
            SupplierCount = supplierCount,
            OfferCount = offerCount,
            ExistingDocumentCount = docTypes.Count,
            Priority = file.Priority.ToString(),
            CreatedAt = file.CreatedAt,
            InquiryDeadline = inquiryDeadline,
            TenderDeadline = tender?.SubmissionDeadline,
            ApprovalStatuses = approvalStatuses,
            WorkflowStatuses = workflowStatuses,
            UserDepartmentIds = currentUser.DepartmentIds.ToArray()
        };
    }

    public async Task<RuleEvaluationContext> BuildTenderAsync(Guid tenderId, CancellationToken ct = default)
    {
        var tender = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == tenderId, ct)
            ?? throw new LegalRuleNotFoundException("Tender was not found.");
        var itemCount = await db.TenderItems.AsNoTracking().CountAsync(x => x.TenderId == tenderId, ct);
        var docTypes = await db.TenderDocuments.AsNoTracking().Where(x => x.TenderId == tenderId)
            .Select(x => x.DocumentType).ToListAsync(ct);
        var participantCount = await db.TenderParticipants.AsNoTracking().CountAsync(x => x.TenderId == tenderId, ct);
        var bidCount = await db.TenderBids.AsNoTracking().CountAsync(x => x.TenderId == tenderId, ct);
        var workflowStatuses = await db.WorkflowInstances.AsNoTracking()
            .Where(w => w.EntityType == "Tender" && w.EntityId == tenderId)
            .Select(w => w.Status.ToString()).ToListAsync(ct);

        return new RuleEvaluationContext("Tender", tenderId, tender.PurchaseFileId, tenderId, tender.Status.ToString(),
            true, itemCount, docTypes.ToHashSet(StringComparer.OrdinalIgnoreCase))
        {
            TenderType = tender.TenderType.ToString(),
            SupplierCount = participantCount,
            OfferCount = bidCount,
            ExistingDocumentCount = docTypes.Count,
            CreatedAt = tender.CreatedAt,
            TenderDeadline = tender.SubmissionDeadline,
            WorkflowStatuses = workflowStatuses,
            UserDepartmentIds = currentUser.DepartmentIds.ToArray()
        };
    }

    private IQueryable<RuleProjection> RuleQuery() =>
        from rule in db.LegalProcurementRules.AsNoTracking()
        join active in db.LegalProcurementRuleVersions.AsNoTracking() on rule.ActiveVersionId equals active.Id into activeVersions
        from active in activeVersions.DefaultIfEmpty()
        select new RuleProjection
        {
            Id = rule.Id,
            Code = rule.Code,
            Title = rule.Title,
            RuleSetId = rule.RuleSetId,
            ActiveVersionId = rule.ActiveVersionId,
            CreatedAt = rule.CreatedAt,
            VersionId = active == null ? null : active.Id,
            VersionProcurementRuleId = active == null ? null : active.ProcurementRuleId,
            VersionNo = active == null ? null : active.VersionNo,
            VersionTitle = active == null ? null : active.Title,
            VersionRuleType = active == null ? null : active.RuleType,
            VersionSeverity = active == null ? null : active.Severity,
            VersionEvaluationMode = active == null ? null : active.EvaluationMode,
            VersionStatus = active == null ? null : active.Status,
            VersionLegalArticleId = active == null ? null : active.LegalArticleId,
            VersionLegalClauseId = active == null ? null : active.LegalClauseId,
            VersionLegalReference = active == null ? null : active.LegalReference,
            VersionConditionType = active == null ? null : active.ConditionType,
            VersionConditionValue = active == null ? null : active.ConditionValue,
            VersionConditionDescription = active == null ? null : active.ConditionDescription,
            VersionCreatedAt = active == null ? null : active.CreatedAt,
            VersionApprovedAt = active == null ? null : active.ApprovedAt,
            VersionSearchText = active == null ? null : active.SearchText
        };

    private IQueryable<LegalClauseContextRow> ClauseContextRows() =>
        from clause in db.LegalClauses.AsNoTracking()
        join article in db.LegalArticles.AsNoTracking() on clause.LegalArticleId equals article.Id
        join document in db.LegalDocuments.AsNoTracking().Where(x => !x.IsDeleted) on article.LegalDocumentId equals document.Id
        select new LegalClauseContextRow
        {
            ClauseId = clause.Id,
            ArticleId = article.Id,
            DocumentId = document.Id,
            DocumentTitle = document.Title,
            ArticleNumber = article.ArticleNumber,
            ArticleTitle = article.Title,
            ClauseNumber = clause.ClauseNumber,
            ClauseText = clause.Body,
            Note = clause.Note,
            AppliesTo = clause.AppliesTo,
            Severity = clause.Severity,
            Tags = clause.Tags
        };

    private static ProcurementRuleDto ToDto(RuleProjection p) =>
        new(p.Id, p.Code, p.Title, p.RuleSetId, p.ActiveVersionId, p.CreatedAt,
            p.VersionId is null ? null : new ProcurementRuleVersionDto(p.VersionId.Value,
                p.VersionProcurementRuleId!.Value, p.VersionNo!.Value, p.VersionTitle!,
                p.VersionRuleType!.Value, p.VersionSeverity!.Value, p.VersionEvaluationMode!.Value,
                p.VersionStatus!.Value, p.VersionLegalArticleId, p.VersionLegalClauseId,
                p.VersionLegalReference!, p.VersionConditionType!, p.VersionConditionValue!,
                p.VersionConditionDescription, p.VersionCreatedAt!.Value, p.VersionApprovedAt));

    private static ProcurementRuleVersionDto ToVersionDto(ProcurementRuleVersion x) =>
        new(x.Id, x.ProcurementRuleId, x.VersionNo, x.Title, x.RuleType, x.Severity, x.EvaluationMode,
            x.Status, x.LegalArticleId, x.LegalClauseId, x.LegalReference, x.ConditionType,
            x.ConditionValue, x.ConditionDescription, x.CreatedAt, x.ApprovedAt);

    private static ProcurementRuleFindingDto ToFindingDto(ProcurementRuleFinding x) =>
        new(x.Id, x.ProcurementRuleEvaluationId, x.ProcurementRuleId, x.RuleVersionId, x.Result, x.Severity,
            x.Title, x.Description, x.LegalReference, x.LegalArticleId, x.LegalClauseId,
            x.IsAiGenerated, x.NeedHumanReview, x.Confidence, x.CitationReferences);

    private sealed class RuleProjection
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public Guid? RuleSetId { get; init; }
        public Guid? ActiveVersionId { get; init; }
        public DateTime CreatedAt { get; init; }
        public Guid? VersionId { get; init; }
        public Guid? VersionProcurementRuleId { get; init; }
        public int? VersionNo { get; init; }
        public string? VersionTitle { get; init; }
        public RuleType? VersionRuleType { get; init; }
        public RuleSeverity? VersionSeverity { get; init; }
        public RuleEvaluationMode? VersionEvaluationMode { get; init; }
        public RuleStatus? VersionStatus { get; init; }
        public Guid? VersionLegalArticleId { get; init; }
        public Guid? VersionLegalClauseId { get; init; }
        public string? VersionLegalReference { get; init; }
        public string? VersionConditionType { get; init; }
        public string? VersionConditionValue { get; init; }
        public string? VersionConditionDescription { get; init; }
        public DateTime? VersionCreatedAt { get; init; }
        public DateTime? VersionApprovedAt { get; init; }
        public string? VersionSearchText { get; init; }
    }

    private sealed class LegalClauseContextRow
    {
        public Guid ClauseId { get; init; }
        public Guid ArticleId { get; init; }
        public Guid DocumentId { get; init; }
        public string DocumentTitle { get; init; } = string.Empty;
        public string ArticleNumber { get; init; } = string.Empty;
        public string ArticleTitle { get; init; } = string.Empty;
        public string ClauseNumber { get; init; } = string.Empty;
        public string ClauseText { get; init; } = string.Empty;
        public string? Note { get; init; }
        public string? AppliesTo { get; init; }
        public RuleSeverity? Severity { get; init; }
        public string? Tags { get; init; }
        public LegalClauseContextDto ToDto() => new(ClauseId, ArticleId, DocumentId, DocumentTitle,
            ArticleNumber, ArticleTitle, ClauseNumber, ClauseText,
            string.IsNullOrWhiteSpace(Note) ? (ClauseText.Length > 240 ? ClauseText[..240] : ClauseText) : Note,
            AppliesTo, Severity, string.IsNullOrWhiteSpace(Tags)
                ? []
                : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}
