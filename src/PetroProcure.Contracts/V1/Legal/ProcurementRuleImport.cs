namespace PetroProcure.Contracts.V1.Legal;

/// <summary>
/// A single procurement/legal rule to import or seed (AI-RAG-03). <see cref="ConditionValue"/>
/// is a JSON condition (AI-RAG-02) when <see cref="ConditionType"/> is omitted or "json",
/// otherwise it is a legacy condition value paired with a legacy <see cref="ConditionType"/>.
/// </summary>
public sealed record ProcurementRuleImportItem(
    string Code,
    string Title,
    string RuleType,
    string Severity,
    string EvaluationMode,
    string ConditionValue,
    string LegalReference,
    string? ConditionType = null,
    string? ConditionDescription = null,
    Guid? LegalArticleId = null,
    Guid? LegalClauseId = null,
    DateTime? EffectiveFrom = null,
    bool Activate = true,
    int Version = 1);

public sealed record ProcurementRuleImportRequest(
    string? RuleSetTitle,
    IReadOnlyList<ProcurementRuleImportItem> Rules);

public sealed record ProcurementRuleImportItemResult(string Code, string Outcome, string? Message);

public sealed record ProcurementRuleImportResult(
    int Total,
    int Imported,
    int Skipped,
    int Failed,
    IReadOnlyList<ProcurementRuleImportItemResult> Items)
{
    public const string OutcomeImported = "Imported";
    public const string OutcomeSkipped = "Skipped";
    public const string OutcomeFailed = "Failed";
}
