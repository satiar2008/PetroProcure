using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Legal;

public sealed class ProcurementRuleSet : Entity<Guid>
{
    private ProcurementRuleSet() : base(Guid.Empty) { Name = string.Empty; }
    public ProcurementRuleSet(Guid id, string name, string? description = null, bool isActive = true) : base(id)
    {
        Name = LegalDocument.Required(name, nameof(name));
        Description = LegalDocument.Trim(description);
        IsActive = isActive;
    }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
}

public sealed class ProcurementRule : Entity<Guid>
{
    private readonly List<ProcurementRuleVersion> _versions = [];
    private ProcurementRule() : base(Guid.Empty) { Code = Title = string.Empty; }
    public ProcurementRule(Guid id, string code, string title, Guid? ruleSetId, Guid createdByUserId) : base(id)
    {
        Code = LegalDocument.Required(code, nameof(code));
        Title = LegalDocument.Required(title, nameof(title));
        RuleSetId = ruleSetId;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }
    public string Code { get; private set; }
    public string Title { get; private set; }
    public Guid? RuleSetId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid? ActiveVersionId { get; private set; }
    public IReadOnlyCollection<ProcurementRuleVersion> Versions => _versions.AsReadOnly();
    public void SetActiveVersion(Guid versionId) => ActiveVersionId = versionId;
}

public sealed class ProcurementRuleVersion : Entity<Guid>
{
    private ProcurementRuleVersion() : base(Guid.Empty) { Title = ConditionType = ConditionValue = LegalReference = SearchText = string.Empty; }
    public ProcurementRuleVersion(Guid id, Guid procurementRuleId, int versionNo, string title, RuleType ruleType,
        RuleSeverity severity, RuleEvaluationMode evaluationMode, Guid? legalArticleId, Guid? legalClauseId,
        string legalReference, string conditionType, string conditionValue, string? conditionDescription,
        Guid createdByUserId) : base(id)
    {
        ProcurementRuleId = procurementRuleId == Guid.Empty ? throw new ArgumentException("Rule is required.", nameof(procurementRuleId)) : procurementRuleId;
        VersionNo = versionNo <= 0 ? throw new ArgumentOutOfRangeException(nameof(versionNo)) : versionNo;
        Title = LegalDocument.Required(title, nameof(title));
        RuleType = ruleType;
        Severity = severity;
        EvaluationMode = evaluationMode;
        LegalArticleId = legalArticleId;
        LegalClauseId = legalClauseId;
        LegalReference = LegalDocument.Required(legalReference, nameof(legalReference));
        ConditionType = LegalDocument.Required(conditionType, nameof(conditionType));
        ConditionValue = LegalDocument.Required(conditionValue, nameof(conditionValue));
        ConditionDescription = LegalDocument.Trim(conditionDescription);
        Status = RuleStatus.Draft;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        SearchText = BuildSearchText();
    }
    public Guid ProcurementRuleId { get; private set; }
    public int VersionNo { get; private set; }
    public string Title { get; private set; }
    public RuleType RuleType { get; private set; }
    public RuleSeverity Severity { get; private set; }
    public RuleEvaluationMode EvaluationMode { get; private set; }
    public RuleStatus Status { get; private set; }
    public Guid? LegalArticleId { get; private set; }
    public Guid? LegalClauseId { get; private set; }
    public string LegalReference { get; private set; }
    public string ConditionType { get; private set; }
    public string ConditionValue { get; private set; }
    public string? ConditionDescription { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public string SearchText { get; private set; }

    public void UpdateDraft(string title, RuleType type, RuleSeverity severity, RuleEvaluationMode mode,
        Guid? articleId, Guid? clauseId, string legalReference, string conditionType, string conditionValue, string? conditionDescription)
    {
        EnsureDraft();
        Title = LegalDocument.Required(title, nameof(title));
        RuleType = type;
        Severity = severity;
        EvaluationMode = mode;
        LegalArticleId = articleId;
        LegalClauseId = clauseId;
        LegalReference = LegalDocument.Required(legalReference, nameof(legalReference));
        ConditionType = LegalDocument.Required(conditionType, nameof(conditionType));
        ConditionValue = LegalDocument.Required(conditionValue, nameof(conditionValue));
        ConditionDescription = LegalDocument.Trim(conditionDescription);
        SearchText = BuildSearchText();
    }

    public ProcurementRuleVersion CloneAsDraft(Guid id, int versionNo, Guid createdByUserId) =>
        new(id, ProcurementRuleId, versionNo, Title, RuleType, Severity, EvaluationMode, LegalArticleId,
            LegalClauseId, LegalReference, ConditionType, ConditionValue, ConditionDescription, createdByUserId);

    public void Submit()
    {
        EnsureDraft();
        Status = RuleStatus.PendingApproval;
    }

    public void Approve(Guid userId)
    {
        if (Status != RuleStatus.PendingApproval) throw new InvalidOperationException("Only pending rule versions can be approved.");
        Status = RuleStatus.Active;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = userId;
    }

    public void Deprecate()
    {
        if (Status == RuleStatus.Deprecated) return;
        Status = RuleStatus.Deprecated;
    }

    public void Disable() => Status = RuleStatus.Disabled;
    public void EnsureDraft()
    {
        if (Status != RuleStatus.Draft) throw new InvalidOperationException("Only draft rule versions can be edited.");
    }

    private string BuildSearchText() => $"{Title} {LegalReference} {ConditionType} {ConditionValue} {ConditionDescription}".Trim();
}

public sealed class ProcurementRuleEvaluation : Entity<Guid>
{
    private readonly List<ProcurementRuleFinding> _findings = [];
    private ProcurementRuleEvaluation() : base(Guid.Empty) { EntityType = Summary = string.Empty; }
    public ProcurementRuleEvaluation(Guid id, string entityType, Guid entityId, Guid? purchaseFileId, Guid? tenderId,
        string summary, Guid evaluatedByUserId) : base(id)
    {
        EntityType = LegalDocument.Required(entityType, nameof(entityType));
        EntityId = entityId;
        PurchaseFileId = purchaseFileId;
        TenderId = tenderId;
        Summary = LegalDocument.Required(summary, nameof(summary));
        EvaluatedByUserId = evaluatedByUserId;
        EvaluatedAt = DateTime.UtcNow;
    }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid? PurchaseFileId { get; private set; }
    public Guid? TenderId { get; private set; }
    public string Summary { get; private set; }
    public Guid EvaluatedByUserId { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public IReadOnlyCollection<ProcurementRuleFinding> Findings => _findings.AsReadOnly();
    public void AddFinding(ProcurementRuleFinding finding) => _findings.Add(finding);
}

public sealed class ProcurementRuleFinding : Entity<Guid>
{
    private ProcurementRuleFinding() : base(Guid.Empty) { Title = Description = LegalReference = string.Empty; }
    public ProcurementRuleFinding(Guid id, Guid evaluationId, Guid procurementRuleId, Guid ruleVersionId,
        RuleResult result, RuleSeverity severity, string title, string description, string legalReference,
        Guid? legalArticleId, Guid? legalClauseId) : base(id)
    {
        ProcurementRuleEvaluationId = evaluationId;
        ProcurementRuleId = procurementRuleId;
        RuleVersionId = ruleVersionId;
        Result = result;
        Severity = severity;
        Title = LegalDocument.Required(title, nameof(title));
        Description = LegalDocument.Required(description, nameof(description));
        LegalReference = LegalDocument.Required(legalReference, nameof(legalReference));
        LegalArticleId = legalArticleId;
        LegalClauseId = legalClauseId;
    }
    public Guid ProcurementRuleEvaluationId { get; private set; }
    public Guid ProcurementRuleId { get; private set; }
    public Guid RuleVersionId { get; private set; }
    public RuleResult Result { get; private set; }
    public RuleSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string LegalReference { get; private set; }
    public Guid? LegalArticleId { get; private set; }
    public Guid? LegalClauseId { get; private set; }
}

public sealed class LegalRuleAuditLog : Entity<Guid>
{
    private LegalRuleAuditLog() : base(Guid.Empty) { Action = EntityType = Summary = string.Empty; }
    public LegalRuleAuditLog(Guid id, string entityType, Guid entityId, string action, string summary, Guid userId) : base(id)
    {
        EntityType = LegalDocument.Required(entityType, nameof(entityType));
        EntityId = entityId;
        Action = LegalDocument.Required(action, nameof(action));
        Summary = LegalDocument.Required(summary, nameof(summary));
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string Action { get; private set; }
    public string Summary { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
