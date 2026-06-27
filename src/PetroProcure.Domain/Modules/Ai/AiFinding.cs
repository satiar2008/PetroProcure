using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Ai;

public sealed class AiFinding : Entity<Guid>
{
    public AiFinding(Guid id, Guid resultId, string title, string description, AiFindingSeverity severity,
        string? relatedClauseCode = null, Guid? relatedDocumentId = null)
        : base(id)
    {
        ResultId = resultId;
        Title = Required(title, nameof(title));
        Description = Required(description, nameof(description));
        Severity = severity;
        RelatedClauseCode = relatedClauseCode;
        RelatedDocumentId = relatedDocumentId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private AiFinding(Guid id) : base(id)
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Guid ResultId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public AiFindingSeverity Severity { get; private set; }
    public string? RelatedClauseCode { get; private set; }
    public Guid? RelatedDocumentId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();
}
