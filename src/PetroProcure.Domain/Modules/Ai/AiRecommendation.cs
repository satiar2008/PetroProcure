using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Ai;

public sealed class AiRecommendation : Entity<Guid>
{
    public AiRecommendation(Guid id, Guid resultId, string title, string description,
        AiRecommendationPriority priority, string? suggestedAction = null)
        : base(id)
    {
        ResultId = resultId;
        Title = Required(title, nameof(title));
        Description = Required(description, nameof(description));
        Priority = priority;
        SuggestedAction = suggestedAction;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private AiRecommendation(Guid id) : base(id)
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Guid ResultId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public AiRecommendationPriority Priority { get; private set; }
    public string? SuggestedAction { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
