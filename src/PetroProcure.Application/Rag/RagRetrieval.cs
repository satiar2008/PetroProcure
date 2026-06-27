using PetroProcure.Application.Security;
using PetroProcure.Domain.Modules.Ai;

namespace PetroProcure.Application.Rag;

public enum RagRetrievalScope
{
    LegalCorpus = 1,
    PurchaseFile = 2,
    Tender = 3,
    AllAllowed = 4
}

public sealed record RagRetrieveRequest(
    string Query,
    RagRetrievalScope Scope,
    Guid? PurchaseFileId = null,
    int TopK = 5,
    IReadOnlyList<AiDocumentSourceType>? SourceTypes = null,
    IReadOnlyList<string>? Tags = null);

public sealed record RagRetrieveResponse(
    string Query,
    IReadOnlyList<RagRetrieveResultDto> Results);

public sealed record RagRetrieveResultDto(
    double Score,
    string TextPreview,
    string? Text,
    AiDocumentSourceType SourceType,
    Guid SourceId,
    string CitationTitle,
    string CitationReference,
    IReadOnlyDictionary<string, object?> Metadata,
    Guid? ChunkId = null);

public sealed record RagUserContext(
    Guid UserId,
    bool IsSystemAdmin,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<Guid> DepartmentIds);

public interface IRagRetriever
{
    Task<RagRetrieveResponse> RetrieveAsync(
        string query, RagRetrievalScope scope, int topK, RagUserContext userContext, CancellationToken ct = default);

    Task<RagRetrieveResponse> RetrieveAsync(
        RagRetrieveRequest request, RagUserContext userContext, CancellationToken ct = default);
}

public interface IRagAccessDataSource
{
    Task<bool> CanAccessPurchaseFileAsync(Guid purchaseFileId, RagUserContext userContext, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> GetAllowedPurchaseFileIdsAsync(RagUserContext userContext, CancellationToken ct = default);
    Task<Guid?> GetTenderPurchaseFileIdAsync(Guid tenderId, CancellationToken ct = default);
}

public sealed class RagRetriever(IRagSearchService search, IRagAccessDataSource access) : IRagRetriever
{
    private static readonly AiDocumentSourceType[] LegalSourceTypes = [AiDocumentSourceType.LegalClause];

    private static readonly AiDocumentSourceType[] PurchaseFileSourceTypes =
    [
        AiDocumentSourceType.PurchaseFileDocument,
        AiDocumentSourceType.TenderDocument,
        AiDocumentSourceType.ContractDocument,
        AiDocumentSourceType.ReportDocument
    ];

    public Task<RagRetrieveResponse> RetrieveAsync(
        string query, RagRetrievalScope scope, int topK, RagUserContext userContext, CancellationToken ct = default) =>
        RetrieveAsync(new RagRetrieveRequest(query, scope, TopK: topK), userContext, ct);

    public async Task<RagRetrieveResponse> RetrieveAsync(
        RagRetrieveRequest request, RagUserContext userContext, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(userContext);

        var query = request.Query?.Trim();
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query is required.", nameof(request));

        var topK = Math.Clamp(request.TopK <= 0 ? 5 : request.TopK, 1, 50);
        var filters = await BuildFiltersAsync(request with { Query = query }, userContext, ct);
        if (filters.Count == 0) return new RagRetrieveResponse(query, []);

        var results = new Dictionary<Guid, RagRetrieveResultDto>();
        foreach (var filter in filters)
        {
            var hits = await search.SearchAsync(query, model: null, topK, filter, ct);
            foreach (var hit in hits)
            {
                var dto = ToResult(hit);
                if (!results.TryGetValue(hit.ChunkId, out var existing) || dto.Score > existing.Score)
                    results[hit.ChunkId] = dto;
            }
        }

        return new RagRetrieveResponse(
            query,
            results.Values
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToArray());
    }

    private async Task<IReadOnlyList<EmbeddingSearchFilter>> BuildFiltersAsync(
        RagRetrieveRequest request, RagUserContext userContext, CancellationToken ct)
    {
        var tags = NormalizeTags(request.Tags);
        var requestedTypes = request.SourceTypes?.Distinct().ToArray();

        return request.Scope switch
        {
            RagRetrievalScope.LegalCorpus => BuildLegalFilters(requestedTypes, tags, userContext),
            RagRetrievalScope.PurchaseFile => await BuildPurchaseFileFiltersAsync(request, requestedTypes, tags, userContext, ct),
            RagRetrievalScope.Tender => await BuildTenderFiltersAsync(request, requestedTypes, tags, userContext, ct),
            RagRetrievalScope.AllAllowed => await BuildAllAllowedFiltersAsync(requestedTypes, tags, userContext, ct),
            _ => throw new ArgumentException("Unsupported RAG retrieval scope.", nameof(request))
        };
    }

    private static IReadOnlyList<EmbeddingSearchFilter> BuildLegalFilters(
        IReadOnlyCollection<AiDocumentSourceType>? requestedTypes, IReadOnlyList<string> tags, RagUserContext userContext)
    {
        EnsureLegalAccess(userContext);
        return BuildSourceTypeFilters(RestrictTypes(requestedTypes, LegalSourceTypes), tags, null, null);
    }

    private async Task<IReadOnlyList<EmbeddingSearchFilter>> BuildPurchaseFileFiltersAsync(
        RagRetrieveRequest request,
        IReadOnlyCollection<AiDocumentSourceType>? requestedTypes,
        IReadOnlyList<string> tags,
        RagUserContext userContext,
        CancellationToken ct)
    {
        var purchaseFileId = request.PurchaseFileId
            ?? throw new ArgumentException("PurchaseFileId is required for purchase file RAG retrieval.", nameof(request));
        if (!await access.CanAccessPurchaseFileAsync(purchaseFileId, userContext, ct))
            throw new CurrentUserForbiddenException("You are not allowed to retrieve RAG chunks for this purchase file.");

        return BuildSourceTypeFilters(RestrictTypes(requestedTypes, PurchaseFileSourceTypes), tags, purchaseFileId, null);
    }

    private async Task<IReadOnlyList<EmbeddingSearchFilter>> BuildTenderFiltersAsync(
        RagRetrieveRequest request,
        IReadOnlyCollection<AiDocumentSourceType>? requestedTypes,
        IReadOnlyList<string> tags,
        RagUserContext userContext,
        CancellationToken ct)
    {
        var tenderId = request.PurchaseFileId
            ?? throw new ArgumentException("Tender id is required for tender RAG retrieval.", nameof(request));
        var purchaseFileId = await access.GetTenderPurchaseFileIdAsync(tenderId, ct)
            ?? throw new ArgumentException("Tender was not found.", nameof(request));
        if (!await access.CanAccessPurchaseFileAsync(purchaseFileId, userContext, ct))
            throw new CurrentUserForbiddenException("You are not allowed to retrieve RAG chunks for this tender.");

        var allowedTypes = RestrictTypes(requestedTypes, [AiDocumentSourceType.TenderDocument]);
        return BuildSourceTypeFilters(allowedTypes, tags, purchaseFileId, null);
    }

    private async Task<IReadOnlyList<EmbeddingSearchFilter>> BuildAllAllowedFiltersAsync(
        IReadOnlyCollection<AiDocumentSourceType>? requestedTypes,
        IReadOnlyList<string> tags,
        RagUserContext userContext,
        CancellationToken ct)
    {
        var filters = new List<EmbeddingSearchFilter>();
        if (HasLegalAccess(userContext))
            filters.AddRange(BuildSourceTypeFilters(RestrictTypes(requestedTypes, LegalSourceTypes), tags, null, null));

        if (HasPurchaseFileSearchAccess(userContext))
        {
            var allowedPurchaseFileIds = await access.GetAllowedPurchaseFileIdsAsync(userContext, ct);
            if (allowedPurchaseFileIds.Count > 0)
            {
                filters.AddRange(BuildSourceTypeFilters(
                    RestrictTypes(requestedTypes, PurchaseFileSourceTypes), tags, null, allowedPurchaseFileIds));
            }
        }

        if (filters.Count == 0)
            throw new CurrentUserForbiddenException("You are not allowed to retrieve RAG chunks.");

        return filters;
    }

    private static IReadOnlyList<EmbeddingSearchFilter> BuildSourceTypeFilters(
        IReadOnlyCollection<AiDocumentSourceType> sourceTypes,
        IReadOnlyList<string> tags,
        Guid? purchaseFileId,
        IReadOnlySet<Guid>? accessiblePurchaseFileIds)
    {
        if (sourceTypes.Count == 0) return [];
        var effectiveTags = tags.Count == 0 ? [null] : tags.Select<string, string?>(x => x).ToArray();
        return sourceTypes
            .SelectMany(type => effectiveTags.Select(tag => new EmbeddingSearchFilter(
                SourceType: type.ToString(),
                PurchaseFileId: purchaseFileId,
                Tags: tag,
                AccessiblePurchaseFileIds: accessiblePurchaseFileIds)))
            .ToArray();
    }

    private static AiDocumentSourceType[] RestrictTypes(
        IReadOnlyCollection<AiDocumentSourceType>? requestedTypes,
        IReadOnlyCollection<AiDocumentSourceType> allowedTypes)
    {
        if (requestedTypes is null || requestedTypes.Count == 0)
            return allowedTypes.ToArray();
        return requestedTypes.Where(allowedTypes.Contains).Distinct().ToArray();
    }

    private static IReadOnlyList<string> NormalizeTags(IReadOnlyCollection<string>? tags) =>
        tags?.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        ?? [];

    private static RagRetrieveResultDto ToResult(RagSearchResultDto hit)
    {
        var sourceType = Enum.TryParse<AiDocumentSourceType>(hit.SourceType, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ArgumentException("Unsupported source type in RAG search result.");

        return new RagRetrieveResultDto(
            hit.Score,
            hit.TextPreview,
            hit.Text,
            sourceType,
            hit.SourceId,
            CitationTitle(hit, sourceType),
            CitationReference(hit, sourceType),
            hit.Metadata,
            hit.ChunkId);
    }

    private static string CitationTitle(RagSearchResultDto hit, AiDocumentSourceType sourceType)
    {
        if (hit.Metadata.TryGetValue("title", out var title) && title is not null)
            return title.ToString() ?? sourceType.ToString();
        if (hit.Metadata.TryGetValue("clauseNumber", out var clauseNumber) && clauseNumber is not null)
            return $"Clause {clauseNumber}";
        if (hit.Metadata.TryGetValue("originalFileName", out var fileName) && fileName is not null)
            return fileName.ToString() ?? sourceType.ToString();
        return sourceType.ToString();
    }

    private static string CitationReference(RagSearchResultDto hit, AiDocumentSourceType sourceType) =>
        sourceType switch
        {
            AiDocumentSourceType.LegalClause => $"/api/legal/clauses/{hit.SourceId}/context",
            AiDocumentSourceType.PurchaseFileDocument when hit.PurchaseFileId is { } purchaseFileId =>
                $"/api/purchase-files/{purchaseFileId}/documents/{hit.SourceId}",
            AiDocumentSourceType.TenderDocument => $"/api/tenders/{hit.SourceId}/documents",
            AiDocumentSourceType.ContractDocument => $"/api/contracts/{hit.SourceId}/documents",
            AiDocumentSourceType.ReportDocument => $"/api/reports/{hit.SourceId}",
            _ => hit.Citation
        };

    private static void EnsureLegalAccess(RagUserContext userContext)
    {
        if (!HasLegalAccess(userContext))
            throw new CurrentUserForbiddenException("You are not allowed to retrieve legal RAG chunks.");
    }

    private static bool HasLegalAccess(RagUserContext userContext) =>
        userContext.IsSystemAdmin
        || userContext.Permissions.Contains(ApplicationPermissions.AiAgentUse)
        || userContext.Permissions.Contains(ApplicationPermissions.LegalDocumentView)
        || userContext.Permissions.Contains(ApplicationPermissions.ProcurementRuleView)
        || userContext.Permissions.Contains(ApplicationPermissions.ProcurementRuleEvaluate);

    private static bool HasPurchaseFileSearchAccess(RagUserContext userContext) =>
        userContext.IsSystemAdmin
        || userContext.Permissions.Contains(ApplicationPermissions.AiAgentUse)
        || userContext.Permissions.Contains(ApplicationPermissions.PurchaseFileView);
}
