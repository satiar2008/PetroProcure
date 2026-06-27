namespace PetroProcure.AI;

public enum AiCoreIntegrationMode
{
    LocalOllamaWorker,
    LocalOllamaDirect,
    SyncAiCoreDirect,
    AsyncAiCoreJob
}

public sealed class AiCoreIntegrationOptions
{
    public const string SectionName = "PetroProcure:AI:AiCore";

    public AiCoreIntegrationMode Mode { get; set; } = AiCoreIntegrationMode.AsyncAiCoreJob;
    public string? BaseUrl { get; set; }
    public string SubmitJobPath { get; set; } = "/api/ai/jobs";
    public string SyncAnalysisPath { get; set; } = "/api/ai/text";
    public string HealthPath { get; set; } = "/health/ready";
    public string CallbackPublicUrl { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? CallbackSecret { get; set; }
    public string? DefaultModel { get; set; }
    public int RequestTimeoutSeconds { get; set; } = 120;
    public int WorkerBatchSize { get; set; } = 5;
    public int MaxRetryCount { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 30;
    public int CallbackTimestampToleranceSeconds { get; set; } = 300;
    public int StuckJobTimeoutMinutes { get; set; } = 15;
    public int RunningJobTimeoutMinutes { get; set; } = 120;
    public int CompletedJobRetentionDays { get; set; } = 180;
    public string[] CallbackAllowedIpAddresses { get; set; } = [];
}
