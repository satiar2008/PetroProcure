using PetroProcure.Contracts.V1.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Contracts.V1.Legal;

public sealed record LegalDocumentDto(Guid Id, string Title, string OriginalFileName, string FileHash,
    string? RelativePath, string? Description, LegalDocumentStatus Status, Guid UploadedByUserId,
    DateTime UploadedAt);

public sealed record LegalArticleDto(Guid Id, Guid LegalDocumentId, string ArticleNumber, string Title,
    string Body, int OrderNo, IReadOnlyList<LegalClauseDto> Clauses);

public sealed record LegalClauseDto(Guid Id, Guid LegalArticleId, string ClauseNumber, string Body,
    int OrderNo, string? Note);

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
    string Title, string Description, string LegalReference, Guid? LegalArticleId, Guid? LegalClauseId);

public sealed record LegalDocumentListRequest(string? SearchTerm = null, LegalDocumentStatus? Status = null,
    int PageNumber = 1, int PageSize = 20);

public sealed record UploadLegalDocumentRequest(string Title, string? Description);

public sealed record CreateLegalArticleRequest(Guid LegalDocumentId, string ArticleNumber,
    string Title, string Body, int OrderNo);

public sealed record CreateLegalClauseRequest(Guid LegalArticleId, string ClauseNumber,
    string Body, int OrderNo, string? Note);

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

public sealed record LegalRuleEvaluationSummaryDto(Guid EvaluationId, int PassCount, int FailCount,
    int WarningCount, int NotApplicableCount, int NeedHumanReviewCount);
