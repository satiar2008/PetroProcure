namespace PetroProcure.Worker;

public sealed class AiJobWorkerOptions
{
    public const string SectionName = "PetroProcure:AI:Worker";

    public int PollIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 5;
    public int MaxRetryCount { get; set; } = 3;
    public string? WorkerId { get; set; }
}
