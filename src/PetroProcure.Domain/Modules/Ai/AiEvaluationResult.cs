using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Ai;

public sealed class AiEvaluationResult : Entity<Guid>
{
    private readonly List<AiFinding> _findings = [];
    private readonly List<AiRecommendation> _recommendations = [];

    public AiEvaluationResult(Guid id, Guid jobId, string entityType, Guid entityId, string analysisType,
        string summary, string rawResultJson, string? model, string? provider, int? inputTokens,
        int? outputTokens, int? totalTokens, long? durationMs)
        : base(id)
    {
        JobId = jobId;
        EntityType = Required(entityType, nameof(entityType));
        EntityId = entityId;
        AnalysisType = Required(analysisType, nameof(analysisType));
        Summary = Required(summary, nameof(summary));
        RawResultJson = Required(rawResultJson, nameof(rawResultJson));
        Model = model;
        Provider = provider;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens;
        DurationMs = durationMs;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private AiEvaluationResult(Guid id) : base(id)
    {
        EntityType = string.Empty;
        AnalysisType = string.Empty;
        Summary = string.Empty;
        RawResultJson = "{}";
    }

    public Guid JobId { get; private set; }
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string AnalysisType { get; private set; }
    public string Summary { get; private set; }
    public string RawResultJson { get; private set; }
    public string? Model { get; private set; }
    public string? Provider { get; private set; }
    public int? InputTokens { get; private set; }
    public int? OutputTokens { get; private set; }
    public int? TotalTokens { get; private set; }
    public long? DurationMs { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<AiFinding> Findings => _findings.AsReadOnly();
    public IReadOnlyCollection<AiRecommendation> Recommendations => _recommendations.AsReadOnly();

    public void AddFinding(AiFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        if (finding.ResultId != Id) throw new InvalidOperationException("Finding belongs to another AI result.");
        _findings.Add(finding);
    }

    public void AddRecommendation(AiRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        if (recommendation.ResultId != Id) throw new InvalidOperationException("Recommendation belongs to another AI result.");
        _recommendations.Add(recommendation);
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
