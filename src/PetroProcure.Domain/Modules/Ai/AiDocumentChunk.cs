using PetroProcure.Domain.Common;

namespace PetroProcure.Domain.Modules.Ai;

// AI-RAG-04: a retrievable chunk of source content (e.g. a legal clause) for the RAG index.
// Source-agnostic so later phases can chunk full documents without schema changes.
public sealed class AiDocumentChunk : Entity<Guid>
{
    public AiDocumentChunk(Guid id, AiDocumentSourceType sourceType, Guid sourceId, int ordinal, string text,
        int tokenCount, string contentHash, string? metadataJson, Guid? purchaseFileId = null,
        Guid? documentId = null, Guid? legalClauseId = null) : base(id)
    {
        SourceType = sourceType;
        SourceId = sourceId == Guid.Empty ? throw new ArgumentException("Source is required.", nameof(sourceId)) : sourceId;
        PurchaseFileId = purchaseFileId;
        DocumentId = documentId;
        LegalClauseId = legalClauseId;
        Ordinal = ordinal < 0 ? throw new ArgumentOutOfRangeException(nameof(ordinal)) : ordinal;
        Text = Required(text, nameof(text));
        TokenCount = tokenCount < 0 ? 0 : tokenCount;
        ContentHash = Required(contentHash, nameof(contentHash));
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        CreatedAt = DateTime.UtcNow;
    }

    public AiDocumentChunk(Guid id, string sourceType, Guid sourceId, int ordinal, string text,
        int tokenCount, string contentHash, string? metadataJson) : base(id)
    {
        SourceType = ParseSourceType(sourceType);
        SourceId = sourceId == Guid.Empty ? throw new ArgumentException("Source is required.", nameof(sourceId)) : sourceId;
        LegalClauseId = SourceType == AiDocumentSourceType.LegalClause ? sourceId : null;
        DocumentId = SourceType == AiDocumentSourceType.LegalClause ? null : sourceId;
        Ordinal = ordinal < 0 ? throw new ArgumentOutOfRangeException(nameof(ordinal)) : ordinal;
        Text = Required(text, nameof(text));
        TokenCount = tokenCount < 0 ? 0 : tokenCount;
        ContentHash = Required(contentHash, nameof(contentHash));
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        CreatedAt = DateTime.UtcNow;
    }

    private AiDocumentChunk(Guid id) : base(id)
    {
        Text = string.Empty;
        ContentHash = string.Empty;
    }

    public AiDocumentSourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public Guid? PurchaseFileId { get; private set; }
    public Guid? DocumentId { get; private set; }
    public Guid? LegalClauseId { get; private set; }
    public int Ordinal { get; private set; }
    public string Text { get; private set; }
    public int TokenCount { get; private set; }
    public string ContentHash { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    public void Update(string text, int tokenCount, string contentHash, string? metadataJson,
        Guid? purchaseFileId = null, Guid? documentId = null, Guid? legalClauseId = null)
    {
        Text = Required(text, nameof(text));
        TokenCount = tokenCount < 0 ? 0 : tokenCount;
        ContentHash = Required(contentHash, nameof(contentHash));
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
        PurchaseFileId = purchaseFileId;
        DocumentId = documentId;
        LegalClauseId = legalClauseId;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void SoftDelete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    private static AiDocumentSourceType ParseSourceType(string sourceType) =>
        Enum.TryParse<AiDocumentSourceType>(Required(sourceType, nameof(sourceType)), ignoreCase: true, out var value)
            ? value
            : throw new ArgumentException("Unsupported source type.", nameof(sourceType));
}
