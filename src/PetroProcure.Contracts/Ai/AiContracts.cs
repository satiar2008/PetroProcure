namespace PetroProcure.Contracts.V1.Ai;

public enum AiSeverity { Info = 1, Low, Medium, High, Critical }
public sealed record AiFindingDto(Guid Id, string Title, string Description, AiSeverity Severity, string? Code,
    Guid? RelatedRuleClauseId = null, string? Evidence = null, string? Recommendation = null,
    string? LegalReference = null);
public sealed record AiRecommendationDto(Guid Id, string Title, string Description, AiSeverity Severity,
    string? RelatedAction = null);
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
    string? ApiKeySecretName = null, string AnalysisPath = "/api/ai/text", string HealthPath = "/health/ready");
public sealed record TestAiProviderConnectionRequest(string? Provider = "AiCore");
public sealed record AiProviderHealthDto(string Provider, bool IsHealthy, string Status, DateTime CheckedAt,
    string? Model = null, string? ErrorMessage = null);
public sealed record AiUsageDto(int? InputTokens, int? OutputTokens, decimal? Cost = null);
public sealed record AiAnalysisRequestDto(string AnalysisType, string? UserQuestion = null);
public sealed record AnalyzePurchaseFileRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeTenderRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeContractRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzePurchaseOrderRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeWarehouseReceiptRequest(string AnalysisType = "Summary", string? UserQuestion = null);
public sealed record AnalyzeLegalComplianceRequest(string EntityType, Guid EntityId, string? AppliesTo = null,
    string AnalysisType = "LegalCompliance", string? UserQuestion = null);
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
