using PetroProcure.Domain.Common;
using PetroProcure.Domain.Enums;

namespace PetroProcure.Domain.Modules.Legal;

public sealed class LegalDocument : Entity<Guid>
{
    private readonly List<LegalArticle> _articles = [];
    private LegalDocument() : base(Guid.Empty)
    {
        Title = OriginalFileName = StoredFileName = FileHash = Extension = MimeType = SearchText = string.Empty;
    }

    public LegalDocument(Guid id, string title, string originalFileName, string fileHash, string? relativePath,
        string? description, Guid uploadedByUserId)
        : this(id, title, originalFileName, originalFileName, relativePath, Path.GetExtension(originalFileName),
            "application/octet-stream", 0, fileHash, description, uploadedByUserId)
    {
    }

    public LegalDocument(Guid id, string title, string originalFileName, string storedFileName, string? relativePath,
        string extension, string mimeType, long size, string fileHash, string? description, Guid uploadedByUserId,
        string? sourceDocumentTitle = null, string? sourceDocumentNumber = null, DateTime? sourceDocumentDate = null) : base(id)
    {
        Title = Required(title, nameof(title));
        OriginalFileName = Required(originalFileName, nameof(originalFileName));
        StoredFileName = Required(storedFileName, nameof(storedFileName));
        FileHash = Required(fileHash, nameof(fileHash));
        RelativePath = Trim(relativePath);
        Extension = Required(extension, nameof(extension)).ToLowerInvariant();
        MimeType = Required(mimeType, nameof(mimeType));
        Size = size < 0 ? throw new ArgumentOutOfRangeException(nameof(size)) : size;
        Description = Trim(description);
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
        SourceDocumentTitle = Trim(sourceDocumentTitle);
        SourceDocumentNumber = Trim(sourceDocumentNumber);
        SourceDocumentDate = sourceDocumentDate;
        Status = LegalDocumentStatus.Draft;
        SearchText = $"{Title} {Description} {SourceDocumentTitle} {SourceDocumentNumber} {OriginalFileName}".Trim();
    }

    public string Title { get; private set; }
    public string OriginalFileName { get; private set; }
    public string StoredFileName { get; private set; }
    public string FileHash { get; private set; }
    public string? RelativePath { get; private set; }
    public string Extension { get; private set; }
    public string MimeType { get; private set; }
    public long Size { get; private set; }
    public string? Description { get; private set; }
    public LegalDocumentStatus Status { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedByUserId { get; private set; }
    public string? SourceDocumentTitle { get; private set; }
    public string? SourceDocumentNumber { get; private set; }
    public DateTime? SourceDocumentDate { get; private set; }
    public string SearchText { get; private set; }
    public IReadOnlyCollection<LegalArticle> Articles => _articles.AsReadOnly();

    public void Activate() => Status = LegalDocumentStatus.Active;
    public void Deprecate() => Status = LegalDocumentStatus.Deprecated;
    public void SoftDelete(Guid deletedByUserId)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedByUserId = deletedByUserId;
    }

    internal static string Required(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Value is required.", name) : value.Trim();

    internal static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class LegalArticle : Entity<Guid>
{
    private readonly List<LegalClause> _clauses = [];
    private LegalArticle() : base(Guid.Empty) { ArticleNumber = Title = Body = SearchText = string.Empty; }

    public LegalArticle(Guid id, Guid legalDocumentId, string articleNumber, string title, string body, int orderNo) : base(id)
    {
        LegalDocumentId = legalDocumentId == Guid.Empty ? throw new ArgumentException("Legal document is required.", nameof(legalDocumentId)) : legalDocumentId;
        ArticleNumber = LegalDocument.Required(articleNumber, nameof(articleNumber));
        Title = LegalDocument.Required(title, nameof(title));
        Body = LegalDocument.Required(body, nameof(body));
        OrderNo = orderNo;
        SearchText = $"{ArticleNumber} {Title} {Body}".Trim();
    }

    public Guid LegalDocumentId { get; private set; }
    public string ArticleNumber { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public int OrderNo { get; private set; }
    public string SearchText { get; private set; }
    public IReadOnlyCollection<LegalClause> Clauses => _clauses.AsReadOnly();
}

public sealed class LegalClause : Entity<Guid>
{
    private LegalClause() : base(Guid.Empty) { ClauseNumber = Body = SearchText = string.Empty; }

    public LegalClause(Guid id, Guid legalArticleId, string clauseNumber, string body, int orderNo, string? note = null,
        string? appliesTo = null, RuleSeverity? severity = null, string? tags = null) : base(id)
    {
        LegalArticleId = legalArticleId == Guid.Empty ? throw new ArgumentException("Legal article is required.", nameof(legalArticleId)) : legalArticleId;
        ClauseNumber = LegalDocument.Required(clauseNumber, nameof(clauseNumber));
        Body = LegalDocument.Required(body, nameof(body));
        OrderNo = orderNo;
        Note = LegalDocument.Trim(note);
        AppliesTo = LegalDocument.Trim(appliesTo);
        Severity = severity;
        Tags = LegalDocument.Trim(tags);
        SearchText = $"{ClauseNumber} {Body} {Note} {AppliesTo} {Tags}".Trim();
    }

    public Guid LegalArticleId { get; private set; }
    public string ClauseNumber { get; private set; }
    public string Body { get; private set; }
    public int OrderNo { get; private set; }
    public string? Note { get; private set; }
    public string? AppliesTo { get; private set; }
    public RuleSeverity? Severity { get; private set; }
    public string? Tags { get; private set; }
    public string SearchText { get; private set; }
}
