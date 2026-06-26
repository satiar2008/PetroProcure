using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.AI;

public sealed class AiCoreOptions
{
    public const string SectionName = "PetroProcure:AI:AiCore";
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiKeySecretName { get; set; } = "PETROPROCURE_AICORE_API_KEY";
    public string? DefaultModel { get; set; } = "gemma3";
    public int TimeoutSeconds { get; set; } = 300;
    public int? MaxInputTokens { get; set; }
    public int? MaxOutputTokens { get; set; }
    public bool IsEnabled { get; set; }
    public bool UseStreaming { get; set; }
    public string? Tenant { get; set; }
    public string? ClientId { get; set; }
    public string AnalysisPath { get; set; } = "/api/ai/text";
    public string HealthPath { get; set; } = "/health/ready";
}

public sealed record AiCoreSettings(string? BaseUrl, string? ApiKey, string? ApiKeySecretName,
    string? DefaultModel, int TimeoutSeconds, int? MaxInputTokens, int? MaxOutputTokens,
    bool IsEnabled, bool UseStreaming, string? Tenant, string? ClientId,
    string AnalysisPath = "/api/ai/text", string HealthPath = "/health/ready")
{
    public AiCoreProviderSettingsDto ToDto() => new(BaseUrl, DefaultModel, TimeoutSeconds, MaxInputTokens,
        MaxOutputTokens, IsEnabled, UseStreaming, Tenant, ClientId, ApiKeySecretName,
        !string.IsNullOrWhiteSpace(ApiKey), AnalysisPath, HealthPath);
}

public sealed record AiCoreAnalysisRequest(string RequestId, string SourceSystem, string EntityType,
    Guid EntityId, string AnalysisType, string? Model, IReadOnlyList<AiCoreMessage> Messages,
    object Context, object? Metadata = null);
public sealed record AiCoreMessage(string Role, string Content);
public sealed record AiCoreAnalysisResponse(string Summary, string RiskLevel,
    IReadOnlyList<AiCoreFinding> Findings, IReadOnlyList<AiCoreRecommendation> Recommendations,
    AiUsageDto? Usage = null, object? RawResponseMetadata = null);
public sealed record AiCoreFinding(string Title, string Description, string Severity, string? Code = null,
    string? Evidence = null, string? Recommendation = null, Guid? RelatedRuleClauseId = null,
    string? LegalReference = null);
public sealed record AiCoreRecommendation(string Title, string Description, string Severity,
    string? RelatedAction = null);

public sealed record AiCoreTextRequest(string? Model, IReadOnlyCollection<AiCoreTextMessage> Messages,
    double? Temperature = null, int? MaxTokens = null, bool Stream = false, bool JsonMode = true,
    Dictionary<string, string>? Metadata = null);
public sealed record AiCoreTextMessage(string Role, string Content, string? Name = null,
    Dictionary<string, string>? Metadata = null);
public sealed record AiCoreTextResponse(string Model, string Content, string? FinishReason = null,
    int InputTokens = 0, int OutputTokens = 0, int TotalTokens = 0, long DurationMs = 0);

public interface IAiCoreSettingsProvider
{
    Task<AiCoreSettings> GetAsync(CancellationToken ct = default);
}

public interface IAiCoreClient
{
    Task<AiCoreAnalysisResponse> SendAnalysisAsync(AiCoreAnalysisRequest request, CancellationToken ct = default);
    Task<AiChatResponse> SendChatAsync(AiChatRequest request, CancellationToken ct = default);
    Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default);
}

public interface IAiContextBuilder
{
    Task<AiPromptContextDto> BuildPurchaseFileContextAsync(Guid id, CancellationToken ct = default);
    Task<AiPromptContextDto> BuildTenderContextAsync(Guid id, CancellationToken ct = default);
    Task<AiPromptContextDto> BuildContractContextAsync(Guid id, CancellationToken ct = default);
    Task<AiPromptContextDto> BuildPurchaseOrderContextAsync(Guid id, CancellationToken ct = default);
    Task<AiPromptContextDto> BuildWarehouseReceiptContextAsync(Guid id, CancellationToken ct = default);
}

public interface IAiLegalRuleContextBuilder
{
    Task<IReadOnlyList<AiLegalRuleContextDto>> BuildLegalRuleContextAsync(string entityType, Guid entityId,
        string? appliesTo, CancellationToken ct = default);
}

public interface IAiAnalysisRepository
{
    Task SaveAsync(AiAnalysisEvaluation evaluation, IReadOnlyList<AiAnalysisFinding> findings,
        IReadOnlyList<AiAnalysisRecommendation> recommendations, AiProviderRequestLog log, CancellationToken ct);
    Task<IReadOnlyList<AiAnalysisResultDto>> GetAsync(string? entityType, Guid? entityId, CancellationToken ct);
    Task<AiAnalysisResultDto?> GetByIdAsync(Guid id, CancellationToken ct);
}

public interface IAiAnalysisService
{
    Task<AiAnalysisResultDto> AnalyzePurchaseFileAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default);
    Task<AiAnalysisResultDto> AnalyzeTenderAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default);
    Task<AiAnalysisResultDto> AnalyzeContractAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default);
    Task<AiAnalysisResultDto> AnalyzePurchaseOrderAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default);
    Task<AiAnalysisResultDto> AnalyzeWarehouseReceiptAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default);
    Task<AiAnalysisResultDto> AnalyzeLegalComplianceAsync(string entityType, Guid id, string? appliesTo, string? userQuestion, CancellationToken ct = default);
}

public sealed class AiCoreClientException(string message) : InvalidOperationException(message);
