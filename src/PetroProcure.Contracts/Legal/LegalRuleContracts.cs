using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Legal;

public sealed record LegalDocumentDto(Guid Id, string Title, string OriginalFileName, string StoredFileName,
    string FileHash, string? RelativePath, string Extension, string MimeType, long Size,
    string? Description, LegalDocumentStatus Status, Guid UploadedByUserId, DateTime UploadedAt,
    bool IsDeleted, DateTime? DeletedAt, Guid? DeletedByUserId,
    string? SourceDocumentTitle, string? SourceDocumentNumber, DateTime? SourceDocumentDate);

public sealed record LegalArticleDto(Guid Id, Guid LegalDocumentId, string ArticleNumber, string Title,
    string Body, int OrderNo, IReadOnlyList<LegalClauseDto> Clauses);

public sealed record LegalClauseDto(Guid Id, Guid LegalArticleId, string ClauseNumber, string Body,
    int OrderNo, string? Note, string? AppliesTo = null, RuleSeverity? Severity = null, string? Tags = null);

public sealed record LegalClauseContextDto(Guid ClauseId, Guid ArticleId, Guid DocumentId,
    string DocumentTitle, string ArticleNumber, string ArticleTitle, string ClauseNumber,
    string ClauseText, string Summary, string? AppliesTo, RuleSeverity? Severity,
    IReadOnlyList<string> Tags);

public sealed record ProcurementRuleDto(Guid Id, string Code, string Title, Guid? RuleSetId,
    Guid? ActiveVersionId, DateTime CreatedAt, ProcurementRuleVersionDto? ActiveVersion);

public sealed record ProcurementRuleVersionDto(Guid Id, Guid ProcurementRuleId, int VersionNo, string Title,
    RuleType RuleType, RuleSeverity Severity, RuleEvaluationMode EvaluationMode, RuleStatus Status,
    Guid? LegalArticleId, Guid? LegalClauseId, string LegalReference, string ConditionType,
    string ConditionValue, string? ConditionDescription, DateTime CreatedAt, DateTime? ApprovedAt);

public sealed record ProcurementRuleEvaluationDto(Guid Id, string EntityType, Guid EntityId,
    Guid? PurchaseFileId, Guid? TenderId, string Summary, Guid EvaluatedByUserId,
    DateTime EvaluatedAt, IReadOnlyList<ProcurementRuleFindingDto> Findings);

public sealed record ProcurementRuleFindingDto(Guid Id, Guid ProcurementRuleEvaluationId,
    Guid ProcurementRuleId, Guid RuleVersionId, RuleResult Result, RuleSeverity Severity,
    string Title, string Description, string LegalReference, Guid? LegalArticleId, Guid? LegalClauseId,
    bool IsAiGenerated = false, bool NeedHumanReview = false, decimal? Confidence = null,
    string? CitationReferences = null);

public sealed record LegalDocumentListRequest(string? SearchTerm = null, LegalDocumentStatus? Status = null,
    bool IncludeDeleted = false, int PageNumber = 1, int PageSize = 20);

public sealed record UploadLegalDocumentRequest(string Title, string? Description,
    string? SourceDocumentTitle = null, string? SourceDocumentNumber = null, DateTime? SourceDocumentDate = null);

public sealed record CreateLegalArticleRequest(Guid LegalDocumentId, string ArticleNumber,
    string Title, string Body, int OrderNo);

public sealed record CreateLegalClauseRequest(Guid LegalArticleId, string ClauseNumber,
    string Body, int OrderNo, string? Note, string? AppliesTo = null, RuleSeverity? Severity = null,
    string? Tags = null);

public sealed record LegalArticleSearchRequest(string? Term = null, Guid? DocumentId = null,
    string? ArticleNumber = null, string? AppliesTo = null, RuleSeverity? Severity = null,
    string? Tag = null, bool IsActive = true, int PageNumber = 1, int PageSize = 20);

public sealed record LegalClauseSearchRequest(string? Term = null, Guid? DocumentId = null,
    string? ArticleNumber = null, string? ClauseNumber = null, string? AppliesTo = null,
    RuleSeverity? Severity = null, string? Tag = null, bool IsActive = true,
    int PageNumber = 1, int PageSize = 20);

public sealed record ProcurementRuleListRequest(string? SearchTerm = null, RuleStatus? Status = null,
    RuleType? RuleType = null, RuleSeverity? Severity = null, int PageNumber = 1, int PageSize = 20);

public sealed record CreateProcurementRuleRequest(string Code, string Title, Guid? RuleSetId,
    RuleType RuleType, RuleSeverity Severity, RuleEvaluationMode EvaluationMode,
    Guid? LegalArticleId, Guid? LegalClauseId, string LegalReference,
    string ConditionType, string ConditionValue, string? ConditionDescription);

public sealed record UpdateRuleDraftRequest(string Title, RuleType RuleType, RuleSeverity Severity,
    RuleEvaluationMode EvaluationMode, Guid? LegalArticleId, Guid? LegalClauseId,
    string LegalReference, string ConditionType, string ConditionValue, string? ConditionDescription);

public sealed record SubmitRuleForApprovalRequest(string? Comment = null);
public sealed record ApproveRuleVersionRequest(string? Comment = null);
public sealed record DeprecateRuleRequest(string Reason);
public sealed record EvaluateRulesRequest(string? Scope = null);
public sealed record OverrideProcurementRuleGateRequest(string Reason, string TransitionName = "PurchaseFile.Complete");

public sealed record LegalRuleEvaluationSummaryDto(Guid EvaluationId, int PassCount, int FailCount,
    int WarningCount, int NotApplicableCount, int NeedHumanReviewCount);
