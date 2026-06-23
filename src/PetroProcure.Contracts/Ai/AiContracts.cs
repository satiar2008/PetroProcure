namespace PetroProcure.Contracts.V1.Ai;

public enum AiSeverity { Info = 1, Low, Medium, High, Critical }
public sealed record AiFindingDto(Guid Id, string Title, string Description, AiSeverity Severity, string? Code);
public sealed record AiRecommendationDto(Guid Id, string Title, string Description, AiSeverity Severity);
public sealed record AiEvaluationResultDto(
    Guid Id, Guid PurchaseFileId, string EvaluationType, string Summary,
    DateTime CreatedAt, IReadOnlyList<AiFindingDto> Findings,
    IReadOnlyList<AiRecommendationDto> Recommendations);
public sealed record AiPurchaseFileSummaryRequest(string? AdditionalInstructions = null);
public sealed record AiRuleEvaluationRequest(IReadOnlyList<Guid>? RuleIds = null);
