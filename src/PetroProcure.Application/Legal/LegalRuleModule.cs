using PetroProcure.Application.Security;
using PetroProcure.Application.Documents;
using PetroProcure.Application.Rag;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Application.Legal;

public sealed record UploadLegalDocumentCommand(string Title, string OriginalFileName, Stream Content,
    string? MimeType, string? Description, string? SourceDocumentTitle = null,
    string? SourceDocumentNumber = null, DateTime? SourceDocumentDate = null);
public sealed record DeleteLegalDocumentCommand(Guid Id);
public sealed record CreateLegalArticleCommand(CreateLegalArticleRequest Request);
public sealed record CreateLegalClauseCommand(CreateLegalClauseRequest Request);
public sealed record CreateProcurementRuleCommand(CreateProcurementRuleRequest Request);
public sealed record CloneRuleAsDraftCommand(Guid RuleId);
public sealed record UpdateRuleDraftCommand(Guid RuleId, UpdateRuleDraftRequest Request);
public sealed record SubmitRuleForApprovalCommand(Guid RuleId, string? Comment);
public sealed record ApproveRuleVersionCommand(Guid RuleId, string? Comment);
public sealed record DeprecateRuleCommand(Guid RuleId, string Reason);
public sealed record EvaluatePurchaseFileRulesCommand(Guid PurchaseFileId);
public sealed record EvaluateTenderRulesCommand(Guid TenderId);

public sealed record GetLegalDocumentsQuery(LegalDocumentListRequest Request);
public sealed record GetLegalDocumentByIdQuery(Guid Id);
public sealed record DownloadLegalDocumentQuery(Guid Id);
public sealed record GetArticlesByDocumentQuery(Guid DocumentId);
public sealed record SearchLegalArticlesQuery(LegalArticleSearchRequest Request);
public sealed record SearchLegalClausesQuery(LegalClauseSearchRequest Request);
public sealed record GetLegalClauseContextQuery(Guid ClauseId);
public sealed record GetProcurementRulesQuery(ProcurementRuleListRequest Request);
public sealed record GetProcurementRuleByIdQuery(Guid Id);
public sealed record GetRuleVersionsQuery(Guid RuleId);
public sealed record GetPurchaseFileRuleEvaluationsQuery(Guid PurchaseFileId);

// AI-RAG-01: expanded evaluation context. The original eight positional members are preserved
// (backward compatible — existing constructions and the six built-in conditions still work).
// All new members are init-only with safe defaults / empty collections, so evaluation stays
// null-safe when tender, supplier, amount, deadline, approval, or workflow data is missing.
public sealed record RuleEvaluationContext(string EntityType, Guid EntityId, Guid? PurchaseFileId, Guid? TenderId,
    string? Status, bool HasTender, int ItemCount, IReadOnlySet<string> DocumentTypes)
{
    public string? FileNumber { get; init; }
    public Guid? CurrentDepartmentId { get; init; }
    public Guid? RequestingDepartmentId { get; init; }
    public Guid? ApplicantDepartmentId { get; init; }
    public decimal? EstimatedAmount { get; init; }
    public decimal? FinalAmount { get; init; }
    public string? Currency { get; init; }
    public decimal TotalRequestedQuantity { get; init; }
    public string? TenderType { get; init; }
    public int SupplierCount { get; init; }
    public int OfferCount { get; init; }
    public int ExistingDocumentCount { get; init; }
    public string? Priority { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? InquiryDeadline { get; init; }
    public DateTime? TenderDeadline { get; init; }
    public DateTime? TechnicalReviewDeadline { get; init; }
    public IReadOnlyList<string> ApprovalStatuses { get; init; } = [];
    public IReadOnlyList<string> WorkflowStatuses { get; init; } = [];
    public IReadOnlyList<string> LegalReferences { get; init; } = [];
    public IReadOnlyList<Guid> UserDepartmentIds { get; init; } = [];
}

public static class RuleEvaluationContextMapping
{
    public static RuleEvaluationContextDto ToDto(this RuleEvaluationContext c) => new(
        c.EntityType, c.EntityId, c.PurchaseFileId, c.TenderId, c.FileNumber, c.Status,
        c.CurrentDepartmentId, c.RequestingDepartmentId, c.ApplicantDepartmentId, c.EstimatedAmount,
        c.FinalAmount, c.Currency, c.ItemCount, c.TotalRequestedQuantity, c.HasTender, c.TenderType,
        c.SupplierCount, c.OfferCount, c.DocumentTypes.ToArray(), c.ExistingDocumentCount, c.CreatedAt,
        c.InquiryDeadline, c.TenderDeadline, c.TechnicalReviewDeadline, c.ApprovalStatuses,
        c.WorkflowStatuses, c.LegalReferences, c.UserDepartmentIds);
}

public interface ILegalClauseSearchService
{
    Task<IReadOnlyList<LegalClauseContextDto>> SearchAsync(LegalClauseSearchRequest request, CancellationToken ct = default);
}

public sealed record StoredLegalDocument(string OriginalFileName, string StoredFileName, string RelativePath,
    string Extension, string MimeType, long Size, string Hash);

public interface ILegalDocumentStorageService
{
    Task<StoredLegalDocument> SaveAsync(Guid legalDocumentId, string originalFileName, Stream stream,
        string? mimeType, CancellationToken ct = default);
    Task<StoredFileContent> OpenAsync(LegalDocument document, CancellationToken ct = default);
    Task DeletePhysicalAsync(string relativePath, CancellationToken ct = default);
}

public interface IPurchaseFileRuleContextBuilder
{
    Task<RuleEvaluationContext> BuildPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<RuleEvaluationContext> BuildTenderAsync(Guid tenderId, CancellationToken ct = default);
}

public interface IAiRuleExplanationService
{
    Task<string> ExplainAsync(ProcurementRuleFindingDto finding, CancellationToken ct = default);
}

public sealed record AiLegalEvaluationCitation(string Title, string Reference, string Preview);

public sealed record AiLegalEvaluationRequest(
    RuleEvaluationContext Context,
    ProcurementRuleVersion Rule,
    ConditionEvaluationResult DeterministicOutcome,
    IReadOnlyList<AiLegalEvaluationCitation> Citations);

public sealed record AiLegalEvaluationFinding(
    string Title,
    string Description,
    RuleSeverity Severity,
    decimal? Confidence = null,
    string? LegalReference = null,
    IReadOnlyList<string>? CitationReferences = null);

public interface IAiLegalEvaluationService
{
    Task<IReadOnlyList<AiLegalEvaluationFinding>> AnalyzeAsync(
        AiLegalEvaluationRequest request, CancellationToken ct = default);
}

public interface IProcurementRuleEvaluator
{
    Task<ProcurementRuleEvaluationDto> EvaluateAsync(RuleEvaluationContext context, CancellationToken ct = default);
}

public interface ILegalRuleRepository
{
    Task AddLegalDocumentAsync(LegalDocument document, CancellationToken ct);
    Task<LegalDocument?> FindLegalDocumentAsync(Guid id, CancellationToken ct);
    Task AddArticleAsync(LegalArticle article, CancellationToken ct);
    Task<LegalArticle?> FindArticleAsync(Guid id, CancellationToken ct);
    Task AddClauseAsync(LegalClause clause, CancellationToken ct);
    Task AddRuleAsync(ProcurementRule rule, ProcurementRuleVersion version, CancellationToken ct);
    Task<ProcurementRule?> FindRuleAsync(Guid id, bool includeVersions, CancellationToken ct);
    Task<ProcurementRule?> FindRuleByCodeAsync(string code, bool includeVersions, CancellationToken ct);
    Task<ProcurementRuleDto?> GetRuleAsync(Guid id, CancellationToken ct);
    Task<ProcurementRuleVersion?> FindRuleVersionAsync(Guid id, CancellationToken ct);
    Task<ProcurementRuleVersion?> FindLatestDraftVersionAsync(Guid ruleId, CancellationToken ct);
    Task<ProcurementRuleVersion?> FindPendingVersionAsync(Guid ruleId, CancellationToken ct);
    Task<int> NextRuleVersionNoAsync(Guid ruleId, CancellationToken ct);
    Task<IReadOnlyList<ProcurementRuleVersion>> GetActiveRuleVersionsAsync(CancellationToken ct);
    Task AddEvaluationAsync(ProcurementRuleEvaluation evaluation, CancellationToken ct);
    Task AddAuditAsync(LegalRuleAuditLog audit, CancellationToken ct);
    Task<PagedResult<LegalDocumentDto>> GetLegalDocumentsAsync(LegalDocumentListRequest request, CancellationToken ct);
    Task<LegalDocumentDto?> GetLegalDocumentAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<LegalArticleDto>> GetArticlesByDocumentAsync(Guid documentId, CancellationToken ct);
    Task<PagedResult<LegalArticleDto>> SearchArticlesAsync(LegalArticleSearchRequest request, CancellationToken ct);
    Task<PagedResult<LegalClauseContextDto>> SearchClauseContextsAsync(LegalClauseSearchRequest request, CancellationToken ct);
    Task<LegalClauseContextDto?> GetClauseContextAsync(Guid clauseId, CancellationToken ct);
    Task<PagedResult<ProcurementRuleDto>> GetRulesAsync(ProcurementRuleListRequest request, CancellationToken ct);
    Task<IReadOnlyList<ProcurementRuleVersionDto>> GetRuleVersionsAsync(Guid ruleId, CancellationToken ct);
    Task<IReadOnlyList<ProcurementRuleEvaluationDto>> GetEvaluationsByPurchaseFileAsync(Guid purchaseFileId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed class LegalRuleCommandHandler(
    ILegalRuleRepository repository,
    ICurrentUserService currentUser,
    ILegalDocumentStorageService legalDocumentStorage,
    IRagIngestionQueue ingestionQueue)
{
    public async Task<LegalDocumentDto> Handle(UploadLegalDocumentCommand command, CancellationToken ct = default)
    {
        var id = Guid.NewGuid();
        var stored = await legalDocumentStorage.SaveAsync(id, command.OriginalFileName, command.Content, command.MimeType, ct);
        try
        {
            var document = new LegalDocument(id, command.Title, stored.OriginalFileName, stored.StoredFileName,
                stored.RelativePath, stored.Extension, stored.MimeType, stored.Size, stored.Hash,
                command.Description, currentUser.UserId, command.SourceDocumentTitle,
                command.SourceDocumentNumber, command.SourceDocumentDate);
            await repository.AddLegalDocumentAsync(document, ct);
            await Audit("LegalDocument", document.Id, "Upload", $"Uploaded legal document {document.Title}", ct);
            await repository.SaveChangesAsync(ct);
            return (await repository.GetLegalDocumentAsync(document.Id, ct))!;
        }
        catch
        {
            await legalDocumentStorage.DeletePhysicalAsync(stored.RelativePath, ct);
            throw;
        }
    }

    public async Task Handle(DeleteLegalDocumentCommand command, CancellationToken ct = default)
    {
        var document = await repository.FindLegalDocumentAsync(command.Id, ct)
            ?? throw new LegalRuleNotFoundException("Legal document was not found.");
        document.SoftDelete(currentUser.UserId);
        await Audit("LegalDocument", document.Id, "SoftDelete", $"Deleted legal document {document.Title}", ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<LegalArticleDto> Handle(CreateLegalArticleCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        if (await repository.FindLegalDocumentAsync(r.LegalDocumentId, ct) is null)
            throw new LegalRuleNotFoundException("Legal document was not found.");
        var article = new LegalArticle(Guid.NewGuid(), r.LegalDocumentId, r.ArticleNumber, r.Title, r.Body, r.OrderNo);
        await repository.AddArticleAsync(article, ct);
        await Audit("LegalArticle", article.Id, "Create", $"Created article {article.ArticleNumber}", ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetArticlesByDocumentAsync(r.LegalDocumentId, ct)).Single(x => x.Id == article.Id);
    }

    public async Task<LegalClauseDto> Handle(CreateLegalClauseCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        var article = await repository.FindArticleAsync(r.LegalArticleId, ct)
            ?? throw new LegalRuleNotFoundException("Legal article was not found.");
        var clause = new LegalClause(Guid.NewGuid(), r.LegalArticleId, r.ClauseNumber, r.Body, r.OrderNo,
            r.Note, r.AppliesTo, r.Severity, r.Tags);
        await repository.AddClauseAsync(clause, ct);
        await Audit("LegalClause", clause.Id, "Create", $"Created clause {clause.ClauseNumber}", ct);
        await repository.SaveChangesAsync(ct);
        await ingestionQueue.EnqueueAsync(new EmbeddingIngestionPayload(
            AiDocumentSourceType.LegalClause,
            clause.Id), currentUser.UserId, ct);
        return (await repository.GetArticlesByDocumentAsync(article.LegalDocumentId, ct))
            .SelectMany(x => x.Clauses).Single(x => x.Id == clause.Id);
    }

    public async Task<ProcurementRuleDto> Handle(CreateProcurementRuleCommand command, CancellationToken ct = default)
    {
        var r = command.Request;
        if (!r.LegalArticleId.HasValue && !r.LegalClauseId.HasValue)
            throw new LegalRuleValidationException("Rule must reference a legal article or clause.");
        var rule = new ProcurementRule(Guid.NewGuid(), r.Code, r.Title, r.RuleSetId, currentUser.UserId);
        var version = NewVersion(rule.Id, 1, r.Title, r.RuleType, r.Severity, r.EvaluationMode,
            r.LegalArticleId, r.LegalClauseId, r.LegalReference, r.ConditionType, r.ConditionValue, r.ConditionDescription);
        await repository.AddRuleAsync(rule, version, ct);
        await Audit("ProcurementRule", rule.Id, "Create", $"Created procurement rule {rule.Code}", ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetRulesAsync(new ProcurementRuleListRequest(SearchTerm: rule.Code), ct)).Items.Single(x => x.Id == rule.Id);
    }

    public async Task<ProcurementRuleVersionDto> Handle(CloneRuleAsDraftCommand command, CancellationToken ct = default)
    {
        var rule = await RequiredRule(command.RuleId, true, ct);
        var active = rule.ActiveVersionId.HasValue
            ? await repository.FindRuleVersionAsync(rule.ActiveVersionId.Value, ct)
            : rule.Versions.OrderByDescending(x => x.VersionNo).FirstOrDefault();
        if (active is null) throw new LegalRuleValidationException("Rule has no version to clone.");
        var draft = active.CloneAsDraft(Guid.NewGuid(), await repository.NextRuleVersionNoAsync(rule.Id, ct), currentUser.UserId);
        await repository.AddRuleAsync(rule, draft, ct);
        await Audit("ProcurementRuleVersion", draft.Id, "CloneDraft", $"Cloned rule {rule.Code} as draft", ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetRuleVersionsAsync(rule.Id, ct)).Single(x => x.Id == draft.Id);
    }

    public async Task<ProcurementRuleVersionDto> Handle(UpdateRuleDraftCommand command, CancellationToken ct = default)
    {
        var draft = await repository.FindLatestDraftVersionAsync(command.RuleId, ct)
            ?? throw new LegalRuleValidationException("No draft version exists. Clone the active rule as draft first.");
        var r = command.Request;
        draft.UpdateDraft(r.Title, r.RuleType, r.Severity, r.EvaluationMode, r.LegalArticleId, r.LegalClauseId,
            r.LegalReference, r.ConditionType, r.ConditionValue, r.ConditionDescription);
        await Audit("ProcurementRuleVersion", draft.Id, "UpdateDraft", "Updated draft rule version", ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetRuleVersionsAsync(command.RuleId, ct)).Single(x => x.Id == draft.Id);
    }

    public async Task Handle(SubmitRuleForApprovalCommand command, CancellationToken ct = default)
    {
        var draft = await repository.FindLatestDraftVersionAsync(command.RuleId, ct)
            ?? throw new LegalRuleValidationException("No draft version exists.");
        draft.Submit();
        await Audit("ProcurementRuleVersion", draft.Id, "Submit", command.Comment ?? "Submitted rule version", ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task Handle(ApproveRuleVersionCommand command, CancellationToken ct = default)
    {
        var rule = await RequiredRule(command.RuleId, true, ct);
        var pending = await repository.FindPendingVersionAsync(rule.Id, ct)
            ?? throw new LegalRuleValidationException("No pending rule version exists.");
        if (rule.ActiveVersionId.HasValue && await repository.FindRuleVersionAsync(rule.ActiveVersionId.Value, ct) is { } old)
            old.Deprecate();
        pending.Approve(currentUser.UserId);
        rule.SetActiveVersion(pending.Id);
        await Audit("ProcurementRuleVersion", pending.Id, "Approve", command.Comment ?? "Approved rule version", ct);
        await repository.SaveChangesAsync(ct);
        if (pending.LegalClauseId is { } legalClauseId)
            await ingestionQueue.EnqueueAsync(new EmbeddingIngestionPayload(
                AiDocumentSourceType.LegalClause,
                legalClauseId), currentUser.UserId, ct);
    }

    public async Task Handle(DeprecateRuleCommand command, CancellationToken ct = default)
    {
        var rule = await RequiredRule(command.RuleId, true, ct);
        foreach (var version in rule.Versions.Where(x => x.Status == RuleStatus.Active))
            version.Deprecate();
        await Audit("ProcurementRule", rule.Id, "Deprecate", command.Reason, ct);
        await repository.SaveChangesAsync(ct);
    }

    private ProcurementRuleVersion NewVersion(Guid ruleId, int versionNo, string title, RuleType type, RuleSeverity severity,
        RuleEvaluationMode mode, Guid? articleId, Guid? clauseId, string legalReference, string conditionType,
        string conditionValue, string? conditionDescription) =>
        new(Guid.NewGuid(), ruleId, versionNo, title, type, severity, mode, articleId, clauseId, legalReference,
            conditionType, conditionValue, conditionDescription, currentUser.UserId);

    private async Task<ProcurementRule> RequiredRule(Guid id, bool includeVersions, CancellationToken ct) =>
        await repository.FindRuleAsync(id, includeVersions, ct) ?? throw new LegalRuleNotFoundException("Procurement rule was not found.");

    private async Task Audit(string entityType, Guid entityId, string action, string summary, CancellationToken ct) =>
        await repository.AddAuditAsync(new LegalRuleAuditLog(Guid.NewGuid(), entityType, entityId, action, summary, currentUser.UserId), ct);

}

public sealed class LegalRuleEvaluationHandler(IPurchaseFileRuleContextBuilder contextBuilder, IProcurementRuleEvaluator evaluator)
{
    public async Task<ProcurementRuleEvaluationDto> Handle(EvaluatePurchaseFileRulesCommand command, CancellationToken ct = default) =>
        await evaluator.EvaluateAsync(await contextBuilder.BuildPurchaseFileAsync(command.PurchaseFileId, ct), ct);
    public async Task<ProcurementRuleEvaluationDto> Handle(EvaluateTenderRulesCommand command, CancellationToken ct = default) =>
        await evaluator.EvaluateAsync(await contextBuilder.BuildTenderAsync(command.TenderId, ct), ct);
}

public sealed class LegalRuleQueryHandler(ILegalRuleRepository repository, ILegalDocumentStorageService legalDocumentStorage)
{
    public Task<PagedResult<LegalDocumentDto>> Handle(GetLegalDocumentsQuery query, CancellationToken ct = default) => repository.GetLegalDocumentsAsync(query.Request, ct);
    public Task<LegalDocumentDto?> Handle(GetLegalDocumentByIdQuery query, CancellationToken ct = default) => repository.GetLegalDocumentAsync(query.Id, ct);
    public async Task<StoredFileContent> Handle(DownloadLegalDocumentQuery query, CancellationToken ct = default)
    {
        var document = await repository.FindLegalDocumentAsync(query.Id, ct)
            ?? throw new LegalRuleNotFoundException("Legal document was not found.");
        if (document.IsDeleted) throw new LegalRuleNotFoundException("Legal document was deleted.");
        return await legalDocumentStorage.OpenAsync(document, ct);
    }
    public Task<IReadOnlyList<LegalArticleDto>> Handle(GetArticlesByDocumentQuery query, CancellationToken ct = default) => repository.GetArticlesByDocumentAsync(query.DocumentId, ct);
    public Task<PagedResult<LegalArticleDto>> Handle(SearchLegalArticlesQuery query, CancellationToken ct = default) => repository.SearchArticlesAsync(query.Request, ct);
    public Task<PagedResult<LegalClauseContextDto>> Handle(SearchLegalClausesQuery query, CancellationToken ct = default) => repository.SearchClauseContextsAsync(query.Request, ct);
    public Task<LegalClauseContextDto?> Handle(GetLegalClauseContextQuery query, CancellationToken ct = default) => repository.GetClauseContextAsync(query.ClauseId, ct);
    public Task<PagedResult<ProcurementRuleDto>> Handle(GetProcurementRulesQuery query, CancellationToken ct = default) => repository.GetRulesAsync(query.Request, ct);
    public Task<ProcurementRuleDto?> Handle(GetProcurementRuleByIdQuery query, CancellationToken ct = default) => repository.GetRuleAsync(query.Id, ct);
    public Task<IReadOnlyList<ProcurementRuleVersionDto>> Handle(GetRuleVersionsQuery query, CancellationToken ct = default) => repository.GetRuleVersionsAsync(query.RuleId, ct);
    public Task<IReadOnlyList<ProcurementRuleEvaluationDto>> Handle(GetPurchaseFileRuleEvaluationsQuery query, CancellationToken ct = default) => repository.GetEvaluationsByPurchaseFileAsync(query.PurchaseFileId, ct);
}

public sealed class LegalClauseSearchService(ILegalRuleRepository repository) : ILegalClauseSearchService
{
    public async Task<IReadOnlyList<LegalClauseContextDto>> SearchAsync(LegalClauseSearchRequest request, CancellationToken ct = default) =>
        (await repository.SearchClauseContextsAsync(request, ct)).Items;
}

public sealed class MockAiRuleExplanationService : IAiRuleExplanationService
{
    public Task<string> ExplainAsync(ProcurementRuleFindingDto finding, CancellationToken ct = default) =>
        Task.FromResult($"{finding.Title}: {finding.Description} مرجع: {finding.LegalReference}");
}

public sealed class NullAiLegalEvaluationService : IAiLegalEvaluationService
{
    public Task<IReadOnlyList<AiLegalEvaluationFinding>> AnalyzeAsync(
        AiLegalEvaluationRequest request, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AiLegalEvaluationFinding>>([]);
}

public sealed class DeterministicProcurementRuleEvaluator(
    ILegalRuleRepository repository,
    ICurrentUserService currentUser,
    IConditionEvaluator? conditionEvaluator = null) : IProcurementRuleEvaluator
{
    // AI-RAG-02: JSON-driven condition engine. Falls back to the legacy ConditionType switch
    // when ConditionValue is not a valid JSON condition (backward compatibility).
    private readonly IConditionEvaluator _conditions = conditionEvaluator ?? JsonRuleConditionEvaluator.CreateDefault();

    public async Task<ProcurementRuleEvaluationDto> EvaluateAsync(RuleEvaluationContext context, CancellationToken ct = default)
    {
        var versions = await repository.GetActiveRuleVersionsAsync(ct);
        var evaluation = new ProcurementRuleEvaluation(Guid.NewGuid(), context.EntityType, context.EntityId,
            context.PurchaseFileId, context.TenderId,
            "ارزیابی قوانین مناقصه انجام شد. نتیجه صرفاً پیشنهادی است و جایگزین تصمیم انسانی نیست.",
            currentUser.UserId);

        foreach (var version in versions)
        {
            var outcome = await ResolveAsync(version, context, ct);
            var severity = outcome.SeverityOverride ?? version.Severity;
            var description = outcome.Message ?? Description(version, outcome.Result);
            evaluation.AddFinding(new ProcurementRuleFinding(Guid.NewGuid(), evaluation.Id, version.ProcurementRuleId,
                version.Id, outcome.Result, severity, version.Title, description,
                version.LegalReference, version.LegalArticleId, version.LegalClauseId));
        }

        await repository.AddEvaluationAsync(evaluation, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetEvaluationsByPurchaseFileAsync(context.PurchaseFileId ?? context.EntityId, ct))
            .OrderByDescending(x => x.EvaluatedAt).First(x => x.Id == evaluation.Id);
    }

    private async Task<ConditionEvaluationResult> ResolveAsync(ProcurementRuleVersion version,
        RuleEvaluationContext context, CancellationToken ct)
    {
        if (version.EvaluationMode == RuleEvaluationMode.ManualReview)
            return new ConditionEvaluationResult(RuleResult.NeedHumanReview);

        if (_conditions.CanEvaluate(version))
            return await _conditions.EvaluateAsync(version, context, ct);

        return new ConditionEvaluationResult(EvaluateLegacy(version, context));
    }

    private static RuleResult EvaluateLegacy(ProcurementRuleVersion rule, RuleEvaluationContext context) =>
        rule.ConditionType.Trim().ToLowerInvariant() switch
        {
            "alwayspass" => RuleResult.Pass,
            "alwaysfail" => RuleResult.Fail,
            "requireddocumenttype" => context.DocumentTypes.Contains(rule.ConditionValue) ? RuleResult.Pass : RuleResult.Fail,
            "minimumitems" => int.TryParse(rule.ConditionValue, out var min) && context.ItemCount >= min ? RuleResult.Pass : RuleResult.Warning,
            "hastender" => context.HasTender ? RuleResult.Pass : RuleResult.Warning,
            "purchasefilestatus" => string.Equals(context.Status, rule.ConditionValue, StringComparison.OrdinalIgnoreCase) ? RuleResult.Pass : RuleResult.NotApplicable,
            _ => RuleResult.NotApplicable
        };

    private static string Description(ProcurementRuleVersion rule, RuleResult result) => result switch
    {
        RuleResult.Pass => $"قاعده «{rule.Title}» رعایت شده است.",
        RuleResult.Fail => $"قاعده «{rule.Title}» رعایت نشده است و نیازمند بررسی مسئول پرونده است.",
        RuleResult.Warning => $"قاعده «{rule.Title}» هشدار ایجاد کرده است.",
        RuleResult.NeedHumanReview => $"قاعده «{rule.Title}» نیازمند بررسی انسانی است.",
        _ => $"قاعده «{rule.Title}» برای این پرونده قابل اعمال نیست."
    };
}

public sealed class HybridProcurementRuleEvaluator(
    ILegalRuleRepository repository,
    ICurrentUserService currentUser,
    IAiLegalEvaluationService ai,
    IRagRetriever rag,
    IConditionEvaluator? conditionEvaluator = null) : IProcurementRuleEvaluator
{
    private readonly IConditionEvaluator _conditions = conditionEvaluator ?? JsonRuleConditionEvaluator.CreateDefault();

    public async Task<ProcurementRuleEvaluationDto> EvaluateAsync(RuleEvaluationContext context, CancellationToken ct = default)
    {
        var versions = await repository.GetActiveRuleVersionsAsync(ct);
        var evaluation = new ProcurementRuleEvaluation(Guid.NewGuid(), context.EntityType, context.EntityId,
            context.PurchaseFileId, context.TenderId,
            "ارزیابی ترکیبی قوانین انجام شد. نتایج هوش مصنوعی پیشنهادی هستند و جایگزین تصمیم انسانی یا قواعد قطعی نمی‌شوند.",
            currentUser.UserId);

        foreach (var version in versions)
        {
            var outcome = await ResolveDeterministicAsync(version, context, ct);
            var severity = outcome.SeverityOverride ?? version.Severity;
            var description = outcome.Message ?? Description(version, outcome.Result);
            evaluation.AddFinding(new ProcurementRuleFinding(Guid.NewGuid(), evaluation.Id, version.ProcurementRuleId,
                version.Id, outcome.Result, severity, version.Title, description,
                version.LegalReference, version.LegalArticleId, version.LegalClauseId,
                needHumanReview: outcome.Result == RuleResult.NeedHumanReview));

            if (!ShouldRequestAi(version)) continue;

            foreach (var finding in await TryAnalyzeWithAiAsync(version, context, outcome, ct))
            {
                evaluation.AddFinding(new ProcurementRuleFinding(Guid.NewGuid(), evaluation.Id, version.ProcurementRuleId,
                    version.Id, RuleResult.NeedHumanReview, AdvisorySeverity(finding.Severity),
                    finding.Title, finding.Description,
                    string.IsNullOrWhiteSpace(finding.LegalReference) ? version.LegalReference : finding.LegalReference,
                    version.LegalArticleId, version.LegalClauseId,
                    isAiGenerated: true, needHumanReview: true, confidence: finding.Confidence,
                    citationReferences: CitationReferences(finding.CitationReferences)));
            }
        }

        await repository.AddEvaluationAsync(evaluation, ct);
        await repository.SaveChangesAsync(ct);
        return (await repository.GetEvaluationsByPurchaseFileAsync(context.PurchaseFileId ?? context.EntityId, ct))
            .OrderByDescending(x => x.EvaluatedAt).First(x => x.Id == evaluation.Id);
    }

    private async Task<ConditionEvaluationResult> ResolveDeterministicAsync(ProcurementRuleVersion version,
        RuleEvaluationContext context, CancellationToken ct)
    {
        if (version.EvaluationMode == RuleEvaluationMode.ManualReview)
            return new ConditionEvaluationResult(RuleResult.NeedHumanReview);

        if (_conditions.CanEvaluate(version))
            return await _conditions.EvaluateAsync(version, context, ct);

        return new ConditionEvaluationResult(EvaluateLegacy(version, context));
    }

    private async Task<IReadOnlyList<AiLegalEvaluationFinding>> TryAnalyzeWithAiAsync(
        ProcurementRuleVersion version,
        RuleEvaluationContext context,
        ConditionEvaluationResult outcome,
        CancellationToken ct)
    {
        try
        {
            var citations = await RetrieveCitationsAsync(version, context, ct);
            return await ai.AnalyzeAsync(new AiLegalEvaluationRequest(context, version, outcome, citations), ct);
        }
        catch
        {
            return [];
        }
    }

    private async Task<IReadOnlyList<AiLegalEvaluationCitation>> RetrieveCitationsAsync(
        ProcurementRuleVersion version, RuleEvaluationContext context, CancellationToken ct)
    {
        var query = string.Join(' ', new[]
        {
            version.Title,
            version.LegalReference,
            version.ConditionDescription,
            context.FileNumber,
            context.TenderType
        }.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (string.IsNullOrWhiteSpace(query)) return [];

        var response = await rag.RetrieveAsync(new RagRetrieveRequest(
                query,
                RagRetrievalScope.LegalCorpus,
                TopK: 3,
                SourceTypes: [AiDocumentSourceType.LegalClause]),
            new RagUserContext(currentUser.UserId, currentUser.IsSystemAdmin,
                currentUser.Permissions, currentUser.DepartmentIds),
            ct);

        return response.Results
            .Select(x => new AiLegalEvaluationCitation(x.CitationTitle, x.CitationReference, x.TextPreview))
            .ToArray();
    }

    private static bool ShouldRequestAi(ProcurementRuleVersion version) =>
        version.EvaluationMode != RuleEvaluationMode.Automatic
        && (version.EvaluationMode is RuleEvaluationMode.SemiAutomatic or RuleEvaluationMode.ManualReview
            || version.RuleType == RuleType.Evaluation);

    private static RuleSeverity AdvisorySeverity(RuleSeverity severity) =>
        severity == RuleSeverity.Blocking ? RuleSeverity.Critical : severity;

    private static string? CitationReferences(IReadOnlyList<string>? references) =>
        references is { Count: > 0 } ? string.Join(Environment.NewLine, references.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()) : null;

    private static RuleResult EvaluateLegacy(ProcurementRuleVersion rule, RuleEvaluationContext context) =>
        rule.ConditionType.Trim().ToLowerInvariant() switch
        {
            "alwayspass" => RuleResult.Pass,
            "alwaysfail" => RuleResult.Fail,
            "requireddocumenttype" => context.DocumentTypes.Contains(rule.ConditionValue) ? RuleResult.Pass : RuleResult.Fail,
            "minimumitems" => int.TryParse(rule.ConditionValue, out var min) && context.ItemCount >= min ? RuleResult.Pass : RuleResult.Warning,
            "hastender" => context.HasTender ? RuleResult.Pass : RuleResult.Warning,
            "purchasefilestatus" => string.Equals(context.Status, rule.ConditionValue, StringComparison.OrdinalIgnoreCase) ? RuleResult.Pass : RuleResult.NotApplicable,
            _ => RuleResult.NotApplicable
        };

    private static string Description(ProcurementRuleVersion rule, RuleResult result) => result switch
    {
        RuleResult.Pass => $"قاعده «{rule.Title}» رعایت شده است.",
        RuleResult.Fail => $"قاعده «{rule.Title}» رعایت نشده است و نیازمند بررسی مسئول پرونده است.",
        RuleResult.Warning => $"قاعده «{rule.Title}» هشدار ایجاد کرده است.",
        RuleResult.NeedHumanReview => $"قاعده «{rule.Title}» نیازمند بررسی انسانی است.",
        _ => $"قاعده «{rule.Title}» برای این پرونده قابل اعمال نیست."
    };
}

public sealed class LegalRuleValidationException(string message) : InvalidOperationException(message);
public sealed class LegalRuleNotFoundException(string message) : InvalidOperationException(message);
