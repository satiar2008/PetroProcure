using PetroProcure.Domain.Common;

namespace PetroProcure.AI;

public sealed class AiAnalysisEvaluation(Guid id, string entityType, Guid entityId, string analysisType,
    string provider, string? model, string status, string? promptSummary, string resultSummary,
    string riskLevel, Guid createdByUserId, string? metadataJson = null) : Entity<Guid>(id)
{
    public string EntityType { get; private set; } = entityType;
    public Guid EntityId { get; private set; } = entityId;
    public string AnalysisType { get; private set; } = analysisType;
    public string Provider { get; private set; } = provider;
    public string? Model { get; private set; } = model;
    public string Status { get; private set; } = status;
    public string? PromptSummary { get; private set; } = promptSummary;
    public string ResultSummary { get; private set; } = resultSummary;
    public string RiskLevel { get; private set; } = riskLevel;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; private set; } = createdByUserId;
    public DateTime? CompletedAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? MetadataJson { get; private set; } = metadataJson;
    public void Complete() { Status = "Completed"; CompletedAt = DateTime.UtcNow; }
    public void Fail(string error) { Status = "Failed"; ErrorMessage = error; CompletedAt = DateTime.UtcNow; }
}

public sealed class AiAnalysisFinding(Guid id, Guid evaluationId, string severity, string title, string description,
    Guid? relatedRuleClauseId = null, string? evidence = null, string? recommendation = null, string? legalReference = null) : Entity<Guid>(id)
{
    public Guid EvaluationId { get; private set; } = evaluationId;
    public string Severity { get; private set; } = severity;
    public string Title { get; private set; } = title;
    public string Description { get; private set; } = description;
    public Guid? RelatedRuleClauseId { get; private set; } = relatedRuleClauseId;
    public string? Evidence { get; private set; } = evidence;
    public string? Recommendation { get; private set; } = recommendation;
    public string? LegalReference { get; private set; } = legalReference;
}

public sealed class AiAnalysisRecommendation(Guid id, Guid evaluationId, string severity, string title,
    string description, string? relatedAction = null) : Entity<Guid>(id)
{
    public Guid EvaluationId { get; private set; } = evaluationId;
    public string Severity { get; private set; } = severity;
    public string Title { get; private set; } = title;
    public string Description { get; private set; } = description;
    public string? RelatedAction { get; private set; } = relatedAction;
}

public sealed class AiProviderRequestLog(Guid id, string provider, string entityType, Guid entityId,
    string analysisType) : Entity<Guid>(id)
{
    public string Provider { get; private set; } = provider;
    public string EntityType { get; private set; } = entityType;
    public Guid EntityId { get; private set; } = entityId;
    public string AnalysisType { get; private set; } = analysisType;
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string Status { get; private set; } = "Started";
    public int? TokenInput { get; private set; }
    public int? TokenOutput { get; private set; }
    public decimal? Cost { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public void Complete(long durationMs, int? input, int? output, decimal? cost = null)
    {
        Status = "Completed"; CompletedAt = DateTime.UtcNow; DurationMs = durationMs;
        TokenInput = input; TokenOutput = output; Cost = cost;
    }
    public void Fail(long durationMs, string? errorCode, string? errorMessage)
    {
        Status = "Failed"; CompletedAt = DateTime.UtcNow; DurationMs = durationMs;
        ErrorCode = errorCode; ErrorMessage = errorMessage;
    }
}
