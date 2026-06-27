namespace PetroProcure.Contracts.V1.Ai;

public sealed record CreateAiJobRequest(
    string EntityType,
    Guid EntityId,
    string AnalysisType,
    string? UserQuestion = null,
    string? RequestedModel = null,
    int Priority = 0,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record CreateAiJobResponse(
    Guid JobId,
    string Status,
    string Message,
    DateTime CreatedAtUtc);

public sealed record AiJobStatusDto(
    Guid JobId,
    string EntityType,
    Guid EntityId,
    string AnalysisType,
    string Status,
    int ProgressPercent,
    string Message,
    string? ExternalJobId,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? ErrorMessage,
    bool HasResult);

public sealed record AiJobResultDto(
    Guid JobId,
    string Status,
    string Summary,
    IReadOnlyList<AiFindingDto> Findings,
    IReadOnlyList<AiRecommendationDto> Recommendations,
    string? RawResultJson,
    AiUsageDto? Usage,
    DateTime? CompletedAtUtc);

public sealed record AiCoreSubmitJobRequest(
    string CorrelationId,
    string SourceSystem,
    string EntityType,
    Guid EntityId,
    string AnalysisType,
    string CallbackUrl,
    string? Model,
    IReadOnlyList<AiCoreMessageDto> Messages,
    object? Context,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record AiCoreMessageDto(
    string Role,
    string Content,
    string? Name = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

public sealed record AiCoreSubmitJobResponse(
    string ExternalJobId,
    string Status,
    string Message,
    DateTime AcceptedAtUtc);

public sealed record AiCoreCallbackRequest(
    string CorrelationId,
    string ExternalJobId,
    string Status,
    int ProgressPercent,
    string? Message,
    AiCoreCallbackResultDto? Result,
    AiCoreCallbackErrorDto? Error,
    AiUsageDto? Usage,
    DateTime? CompletedAtUtc);

public sealed record AiCoreCallbackResultDto(
    string? Summary,
    IReadOnlyList<AiFindingDto> Findings,
    IReadOnlyList<AiRecommendationDto> Recommendations,
    string? RawResultJson = null);

public sealed record AiCoreCallbackErrorDto(
    string Code,
    string Message,
    bool Retryable = false);
