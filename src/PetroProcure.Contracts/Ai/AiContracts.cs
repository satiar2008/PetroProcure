using System.Text.Json.Serialization;

namespace PetroProcure.Contracts.V1.Ai;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AiSeverity { Info = 1, Low, Medium, High, Critical }

public sealed class AiFindingDto
{
    public AiFindingDto()
    {
    }

    public AiFindingDto(string title, string description, AiSeverity severity, string? relatedClauseCode = null,
        Guid? relatedDocumentId = null)
    {
        Title = title;
        Description = description;
        Severity = severity;
        RelatedClauseCode = relatedClauseCode;
        RelatedDocumentId = relatedDocumentId;
    }

    public AiFindingDto(Guid id, string title, string description, AiSeverity severity, string? code,
        Guid? relatedRuleClauseId = null, string? evidence = null, string? recommendation = null,
        string? legalReference = null)
        : this(title, description, severity, code, null)
    {
        Id = id;
        Code = code;
        RelatedRuleClauseId = relatedRuleClauseId;
        Evidence = evidence;
        Recommendation = recommendation;
        LegalReference = legalReference;
    }

    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AiSeverity Severity { get; init; } = AiSeverity.Info;
    public string? RelatedClauseCode { get; init; }
    public Guid? RelatedDocumentId { get; init; }
    public string? Code { get; init; }
    public Guid? RelatedRuleClauseId { get; init; }
    public string? Evidence { get; init; }
    public string? Recommendation { get; init; }
    public string? LegalReference { get; init; }
}

public sealed class AiRecommendationDto
{
    public AiRecommendationDto()
    {
    }

    public AiRecommendationDto(string title, string description, string priority, string? suggestedAction = null)
    {
        Title = title;
        Description = description;
        Priority = priority;
        SuggestedAction = suggestedAction;
    }

    public AiRecommendationDto(Guid id, string title, string description, AiSeverity severity,
        string? relatedAction = null)
        : this(title, description, severity.ToString(), relatedAction)
    {
        Id = id;
        Severity = severity;
        RelatedAction = relatedAction;
    }

    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Priority { get; init; } = "Medium";
    public string? SuggestedAction { get; init; }
    public AiSeverity Severity { get; init; } = AiSeverity.Info;
    public string? RelatedAction { get; init; }
}

public sealed record AiEvaluationResultDto(
    Guid Id, Guid PurchaseFileId, string EvaluationType, string Summary,
    DateTime CreatedAt, IReadOnlyList<AiFindingDto> Findings,
    IReadOnlyList<AiRecommendationDto> Recommendations,
    string? Provider = null, string? Model = null, string? Status = null, string? RiskLevel = null,
    string AdvisoryDisclaimer = "تحلیل هوش مصنوعی صرفاً جنبه کمکی دارد و جایگزین تصمیم کارشناسی، حقوقی یا کمیسیون نیست.");
public sealed record AiPurchaseFileSummaryRequest(string? AdditionalInstructions = null);
public sealed record AiRuleEvaluationRequest(IReadOnlyList<Guid>? RuleIds = null);

public sealed record AiProviderDto(string Name, string ProviderType, bool IsEnabled, string? BaseUrl, string? DefaultModel);
public sealed record AiCoreProviderSettingsDto(string? BaseUrl, string? DefaultModel, int TimeoutSeconds,
    int? MaxInputTokens, int? MaxOutputTokens, bool IsEnabled, bool UseStreaming, string? Tenant,
    string? ClientId, string? ApiKeySecretName, bool HasApiKey,
    string AnalysisPath = "/api/ai/text", string HealthPath = "/health/ready");
public sealed record ConfigureAiCoreProviderRequest(string? BaseUrl, string? DefaultModel, int TimeoutSeconds = 60,
    int? MaxInputTokens = null, int? MaxOutputTokens = null, bool IsEnabled = false,
    bool UseStreaming = false, string? Tenant = null, string? ClientId = null,
    string? ApiKeySecretName = null, string AnalysisPath = "/api/ai/text", string HealthPath = "/health/ready",
    string Mode = "AsyncAiCoreJob", string SubmitJobPath = "/api/ai/jobs", string CallbackPublicUrl = "",
    int RequestTimeoutSeconds = 120, int WorkerBatchSize = 5, int MaxRetryCount = 3,
    int RetryDelaySeconds = 30, int CallbackTimestampToleranceSeconds = 300,
    int StuckJobTimeoutMinutes = 15, int RunningJobTimeoutMinutes = 120,
    int CompletedJobRetentionDays = 180, string[]? CallbackAllowedIpAddresses = null);
public sealed record TestAiProviderConnectionRequest(string? Provider = "AiCore");
public sealed record AiProviderHealthDto(string Provider, bool IsHealthy, string Status, DateTime CheckedAt,
    string? Model = null, string? ErrorMessage = null);
public sealed record AiUsageDto(int? InputTokens, int? OutputTokens, decimal? Cost = null,
    int? TotalTokens = null, long? DurationMs = null, string? Model = null, string? Provider = null);
public sealed record AiAnalysisRequestDto(string AnalysisType, string? UserQuestion = null);
public sealed record AnalyzePurchaseFileRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeTenderRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeContractRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzePurchaseOrderRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeWarehouseReceiptRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeLegalComplianceRequest(string EntityType, Guid EntityId, string? AppliesTo = null,
    string AnalysisType = "LegalCompliance", string? UserQuestion = null);
public static class RagRetrievalScopes
{
    public const int LegalCorpus = 1;
    public const int PurchaseFile = 2;
    public const int Tender = 3;
    public const int AllAllowed = 4;
}
public sealed record RagRetrieveRequestDto(
    string Query,
    int Scope,
    Guid? PurchaseFileId = null,
    int TopK = 5,
    IReadOnlyList<int>? SourceTypes = null,
    IReadOnlyList<string>? Tags = null);
public sealed record RagRetrieveResponseDto(string Query, IReadOnlyList<RagClientRetrieveResultDto> Results);
public sealed record RagClientRetrieveResultDto(
    double Score,
    string TextPreview,
    string? Text,
    int SourceType,
    Guid SourceId,
    string CitationTitle,
    string CitationReference,
    IReadOnlyDictionary<string, object?> Metadata,
    Guid? ChunkId = null);
public sealed record GroundedAiQuestionRequest(string? Question, int TopK = 5);
public sealed record GroundedAiAnalysisResponse(
    string Answer,
    IReadOnlyList<GroundedAiCitationDto> Citations,
    IReadOnlyList<GroundedAiRelatedChunkDto> RelatedChunks,
    bool NeedHumanReview,
    DateTime CreatedAt);
public sealed record GroundedAiCitationDto(
    string CitationId,
    string Title,
    string Reference,
    string SourceType,
    Guid SourceId,
    double Score);
public sealed record GroundedAiRelatedChunkDto(
    string CitationId,
    Guid? ChunkId,
    string SourceType,
    Guid SourceId,
    string TextPreview,
    IReadOnlyDictionary<string, object?> Metadata);
public sealed record SummarizeProcurementEntityRequest(string EntityType, Guid EntityId, string? UserQuestion = null);
public sealed record AiPromptContextDto(string EntityType, Guid EntityId, string? EntityNumber, string? EntityStatus,
    AiProcurementEntityContextDto Entity, IReadOnlyList<AiLegalRuleContextDto> LegalRules);
public sealed record AiProcurementEntityContextDto(string Title, string? Description, IReadOnlyDictionary<string, object?> Metadata);
public sealed record AiLegalRuleContextDto(Guid? RuleVersionId, Guid? ClauseId, string? LegalReference,
    string? ArticleNumber, string? ClauseNumber, string? ClauseText, string? Summary,
    string? AppliesTo, string? Severity, IReadOnlyList<string> Tags);
public sealed record AiAnalysisResultDto(Guid Id, string EntityType, Guid EntityId, string AnalysisType,
    string Provider, string? Model, string Status, string Summary, string RiskLevel, DateTime CreatedAt,
    DateTime? CompletedAt, IReadOnlyList<AiFindingDto> Findings, IReadOnlyList<AiRecommendationDto> Recommendations,
    AiUsageDto? Usage = null,
    string AdvisoryDisclaimer = "تحلیل هوش مصنوعی صرفاً جنبه کمکی دارد و جایگزین تصمیم کارشناسی، حقوقی یا کمیسیون نیست.");
