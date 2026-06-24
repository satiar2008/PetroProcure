namespace PetroProcure.AI;

public enum AiSeverity { Info = 1, Low, Medium, High, Critical }
public enum RuleEvaluationOutcome { Pass = 1, Warning, Fail, NotApplicable }

public sealed record AiChatRequest(string SystemPrompt, string UserPrompt);
public sealed record AiChatResponse(string Content, string Provider, string Model);
public sealed record AiContextItemGroup(string GeneralCode, string GeneralDescription, IReadOnlyList<AiContextItem> Items);
public sealed record AiContextItem(string MescCode, string SpecificDescription, string Unit, decimal Quantity);
public sealed record AiContextDocument(string DocumentType, string FileName);
public sealed record AiContextWorkflowStep(string Action, string FromDepartment, string ToDepartment, DateTime CreatedAt);
public sealed record PurchaseFileAiContext(Guid PurchaseFileId, string FileNumber, string Status, string CurrentDepartment,
    IReadOnlyList<AiContextItemGroup> ItemGroups, IReadOnlyList<AiContextDocument> Documents,
    IReadOnlyList<AiContextWorkflowStep> WorkflowTimeline, IReadOnlyList<string> Notes);
public sealed record AiFindingDto(Guid Id, string Title, string Description, AiSeverity Severity, string? Code);
public sealed record AiRecommendationDto(Guid Id, string Title, string Description, AiSeverity Severity);
public sealed record AiEvaluationDto(Guid Id, Guid PurchaseFileId, string EvaluationType, string Summary,
    DateTime CreatedAt, IReadOnlyList<AiFindingDto> Findings, IReadOnlyList<AiRecommendationDto> Recommendations);

public interface IAiChatProvider { string Name { get; } Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct = default); }
public interface IPurchaseFileAiContextBuilder { Task<PurchaseFileAiContext> BuildAsync(Guid purchaseFileId, CancellationToken ct = default); }
public interface IProcurementRuleEvaluator { Task<AiEvaluationDto> EvaluateAsync(PurchaseFileAiContext context, CancellationToken ct = default); }
public interface IAiAgentService
{
    Task<AiEvaluationDto> SummarizeAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<AiEvaluationDto> CheckMissingDocumentsAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<AiEvaluationDto> EvaluateRulesAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<IReadOnlyList<AiEvaluationDto>> GetEvaluationsAsync(Guid purchaseFileId, CancellationToken ct = default);
}
public interface IAiEvaluationRepository
{
    Task SaveAsync(AiEvaluationJob job, AiEvaluationResult result, CancellationToken ct);
    Task<IReadOnlyList<AiEvaluationDto>> GetAsync(Guid purchaseFileId, CancellationToken ct);
}

public sealed class AiOptions
{
    public const string SectionName = "PetroProcure:AI";
    public string Provider { get; set; } = "Mock";
    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "llama3.2";
    public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1";
    public string? OpenAiApiKey { get; set; }
    public string OpenAiModel { get; set; } = "gpt-4o-mini";
    public string[] RequiredDocumentTypes { get; set; } = ["Indent", "TechnicalSpecification"];
}
