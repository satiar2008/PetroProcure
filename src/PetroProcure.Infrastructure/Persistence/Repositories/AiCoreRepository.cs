using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PetroProcure.AI;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Domain.Enums;
using ContractAiSeverity = PetroProcure.Contracts.V1.Ai.AiSeverity;
using ContractAiFindingDto = PetroProcure.Contracts.V1.Ai.AiFindingDto;
using ContractAiRecommendationDto = PetroProcure.Contracts.V1.Ai.AiRecommendationDto;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class AiCoreRepository(PetroProcureDbContext db, IOptions<AiCoreOptions> options)
    : IAiCoreSettingsProvider, IAiContextBuilder, IAiLegalRuleContextBuilder, IAiAnalysisRepository
{
    private static readonly string[] SettingKeys =
    [
        "AI:AiCore:BaseUrl",
        "AI:AiCore:DefaultModel",
        "AI:AiCore:TimeoutSeconds",
        "AI:AiCore:MaxInputTokens",
        "AI:AiCore:MaxOutputTokens",
        "AI:AiCore:IsEnabled",
        "AI:AiCore:UseStreaming",
        "AI:AiCore:Tenant",
        "AI:AiCore:ClientId",
        "AI:AiCore:ApiKeySecretName",
        "AI:AiCore:AnalysisPath",
        "AI:AiCore:HealthPath"
    ];

    public async Task<AiCoreSettings> GetAsync(CancellationToken ct = default)
    {
        var settings = await db.SystemSettings.AsNoTracking()
            .Where(x => SettingKeys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, x => x.Value, ct);

        var configuredSecretName = Get(settings, "AI:AiCore:ApiKeySecretName", options.Value.ApiKeySecretName);
        var apiKey = ResolveApiKey(configuredSecretName, options.Value.ApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = options.Value.ApiKey;

        return new AiCoreSettings(
            Get(settings, "AI:AiCore:BaseUrl", options.Value.BaseUrl),
            apiKey,
            configuredSecretName,
            Get(settings, "AI:AiCore:DefaultModel", options.Value.DefaultModel),
            GetInt(settings, "AI:AiCore:TimeoutSeconds", options.Value.TimeoutSeconds),
            GetNullableInt(settings, "AI:AiCore:MaxInputTokens", options.Value.MaxInputTokens),
            GetNullableInt(settings, "AI:AiCore:MaxOutputTokens", options.Value.MaxOutputTokens),
            GetBool(settings, "AI:AiCore:IsEnabled", options.Value.IsEnabled),
            GetBool(settings, "AI:AiCore:UseStreaming", options.Value.UseStreaming),
            Get(settings, "AI:AiCore:Tenant", options.Value.Tenant),
            Get(settings, "AI:AiCore:ClientId", options.Value.ClientId),
            NormalizePath(Get(settings, "AI:AiCore:AnalysisPath", options.Value.AnalysisPath), "/api/ai/text"),
            NormalizePath(Get(settings, "AI:AiCore:HealthPath", options.Value.HealthPath), "/health/ready"));
    }

    public async Task<AiPromptContextDto> BuildPurchaseFileContextAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.PurchaseFiles.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new InvalidOperationException("Purchase file not found.");
        var items = await db.PurchaseFileItems.AsNoTracking().Where(x => x.PurchaseFileId == id).ToListAsync(ct);
        var documents = await db.FileDocuments.AsNoTracking()
            .Where(x => x.PurchaseFileId == id && !x.IsDeleted)
            .Select(x => new { x.DocumentType, x.OriginalFileName, x.VersionNo, x.UploadedAt })
            .ToListAsync(ct);
        var workflow = await db.WorkflowSteps.AsNoTracking()
            .Where(x => db.WorkflowInstances.Any(w => w.Id == x.WorkflowInstanceId && w.EntityType == "PurchaseFile" && w.EntityId == id))
            .Select(x => new { x.ActionName, x.Comment, x.CreatedAt, x.CompletedAt })
            .ToListAsync(ct);
        var notesCount = await db.PurchaseFileNotes.AsNoTracking().CountAsync(x => x.PurchaseFileId == id, ct);

        var legalRules = await BuildLegalRuleContextAsync("PurchaseFile", id, "PurchaseFile", ct);
        return new AiPromptContextDto("PurchaseFile", id, file.FileNumber, file.Status.ToString(),
            new AiProcurementEntityContextDto(file.Title, file.Description, new Dictionary<string, object?>
            {
                ["priority"] = file.Priority.ToString(),
                ["createdAt"] = file.CreatedAt,
                ["currentDepartmentId"] = file.CurrentDepartmentId,
                ["sourceIndentId"] = file.SourceIndentId,
                ["itemGroups"] = items.GroupBy(x => new { x.MescGeneralGroupCode, x.GeneralDescription })
                    .Select(g => new
                    {
                        g.Key.MescGeneralGroupCode,
                        g.Key.GeneralDescription,
                        Count = g.Count(),
                        Items = g.Select(i => new
                        {
                            i.MescCode,
                            i.SpecificDescription,
                            i.RequestedQuantity,
                            i.ApprovedQuantity,
                            i.TechnicalDescription
                        }).ToArray()
                    }).ToArray(),
                ["documents"] = documents,
                ["workflow"] = workflow,
                ["notesCount"] = notesCount
            }),
            legalRules);
    }

    public Task<AiPromptContextDto> BuildTenderContextAsync(Guid id, CancellationToken ct = default) =>
        BuildSimpleContext("Tender", id, "Tender", async () =>
        {
            var entity = await db.Tenders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Tender not found.");
            var itemCount = await db.TenderItems.AsNoTracking().CountAsync(x => x.TenderId == id, ct);
            var bidCount = await db.TenderBids.AsNoTracking().CountAsync(x => x.TenderId == id, ct);
            return (entity.TenderNumber, entity.Status.ToString(), entity.Title, entity.Description,
                new Dictionary<string, object?>
                {
                    ["purchaseFileId"] = entity.PurchaseFileId,
                    ["tenderType"] = entity.TenderType.ToString(),
                    ["issueDate"] = entity.IssueDate,
                    ["submissionDeadline"] = entity.SubmissionDeadline,
                    ["itemCount"] = itemCount,
                    ["bidCount"] = bidCount
                });
        }, ct);

    public Task<AiPromptContextDto> BuildContractContextAsync(Guid id, CancellationToken ct = default) =>
        BuildSimpleContext("Contract", id, "Contract", async () =>
        {
            var entity = await db.PurchaseContracts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Contract not found.");
            var itemCount = await db.PurchaseContractItems.AsNoTracking().CountAsync(x => x.ContractId == id, ct);
            return (entity.ContractNumber, entity.Status.ToString(), entity.Title, entity.Description,
                new Dictionary<string, object?>
                {
                    ["purchaseFileId"] = entity.PurchaseFileId,
                    ["supplierId"] = entity.SupplierId,
                    ["contractType"] = entity.ContractType.ToString(),
                    ["currency"] = entity.Currency,
                    ["finalAmount"] = entity.FinalAmount,
                    ["startDate"] = entity.StartDate,
                    ["endDate"] = entity.EndDate,
                    ["itemCount"] = itemCount
                });
        }, ct);

    public Task<AiPromptContextDto> BuildPurchaseOrderContextAsync(Guid id, CancellationToken ct = default) =>
        BuildSimpleContext("PurchaseOrder", id, "PurchaseOrder", async () =>
        {
            var entity = await db.PurchaseOrders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Purchase order not found.");
            var itemCount = await db.PurchaseOrderItems.AsNoTracking().CountAsync(x => x.PurchaseOrderId == id, ct);
            return (entity.PurchaseOrderNumber, entity.Status.ToString(), entity.Title, entity.Description,
                new Dictionary<string, object?>
                {
                    ["purchaseFileId"] = entity.PurchaseFileId,
                    ["supplierId"] = entity.SupplierId,
                    ["contractId"] = entity.ContractId,
                    ["purchaseOrderType"] = entity.PurchaseOrderType.ToString(),
                    ["currency"] = entity.Currency,
                    ["finalAmount"] = entity.FinalAmount,
                    ["orderDate"] = entity.OrderDate,
                    ["expectedDeliveryDate"] = entity.ExpectedDeliveryDate,
                    ["itemCount"] = itemCount
                });
        }, ct);

    public Task<AiPromptContextDto> BuildWarehouseReceiptContextAsync(Guid id, CancellationToken ct = default) =>
        BuildSimpleContext("WarehouseReceipt", id, "WarehouseReceipt", async () =>
        {
            var entity = await db.WarehouseReceipts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Warehouse receipt not found.");
            var itemCount = await db.WarehouseReceiptItems.AsNoTracking().CountAsync(x => x.WarehouseReceiptId == id, ct);
            return (entity.ReceiptNumber, entity.Status.ToString(), "رسید انبار", entity.Description,
                new Dictionary<string, object?>
                {
                    ["purchaseFileId"] = entity.PurchaseFileId,
                    ["purchaseOrderId"] = entity.PurchaseOrderId,
                    ["warehouseId"] = entity.WarehouseId,
                    ["supplierId"] = entity.SupplierId,
                    ["receiptDate"] = entity.ReceiptDate,
                    ["deliveryNoteNumber"] = entity.DeliveryNoteNumber,
                    ["itemCount"] = itemCount
                });
        }, ct);

    public async Task<IReadOnlyList<AiLegalRuleContextDto>> BuildLegalRuleContextAsync(string entityType, Guid entityId,
        string? appliesTo, CancellationToken ct = default)
    {
        var versions = await db.LegalProcurementRuleVersions.AsNoTracking()
            .Where(x => x.Status == RuleStatus.Active)
            .ToListAsync(ct);
        if (!versions.Any()) return [];

        var clauseIds = versions.Where(x => x.LegalClauseId.HasValue).Select(x => x.LegalClauseId!.Value).Distinct().ToArray();
        var clauses = await db.LegalClauses.AsNoTracking()
            .Where(x => clauseIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var articleIds = versions.Where(x => x.LegalArticleId.HasValue).Select(x => x.LegalArticleId!.Value)
            .Concat(clauses.Values.Select(x => x.LegalArticleId)).Distinct().ToArray();
        var articles = await db.LegalArticles.AsNoTracking()
            .Where(x => articleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);
        var documentIds = articles.Values.Select(x => x.LegalDocumentId).Distinct().ToArray();
        var documents = await db.LegalDocuments.AsNoTracking()
            .Where(x => documentIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        return versions.Select(v =>
            {
                clauses.TryGetValue(v.LegalClauseId ?? Guid.Empty, out var clause);
                articles.TryGetValue(v.LegalArticleId ?? clause?.LegalArticleId ?? Guid.Empty, out var article);
                documents.TryGetValue(article?.LegalDocumentId ?? Guid.Empty, out var document);
                return new { Version = v, Clause = clause, Article = article, Document = document };
            })
            .Where(x => x.Clause is null || string.IsNullOrWhiteSpace(appliesTo) ||
                string.IsNullOrWhiteSpace(x.Clause.AppliesTo) ||
                x.Clause.AppliesTo.Contains(appliesTo, StringComparison.OrdinalIgnoreCase))
            .Select(x => new AiLegalRuleContextDto(
                x.Version.Id,
                x.Clause?.Id,
                x.Version.LegalReference,
                x.Article?.ArticleNumber,
                x.Clause?.ClauseNumber,
                x.Clause?.Body,
                x.Version.ConditionDescription ?? x.Version.Title,
                x.Clause?.AppliesTo,
                x.Version.Severity.ToString(),
                SplitTags(x.Clause?.Tags)))
            .ToArray();
    }

    public async Task SaveAsync(AiAnalysisEvaluation evaluation, IReadOnlyList<AiAnalysisFinding> findings,
        IReadOnlyList<AiAnalysisRecommendation> recommendations, AiProviderRequestLog log, CancellationToken ct)
    {
        db.AiAnalysisEvaluations.Add(evaluation);
        db.AiAnalysisFindings.AddRange(findings);
        db.AiAnalysisRecommendations.AddRange(recommendations);
        db.AiProviderRequestLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AiAnalysisResultDto>> GetAsync(string? entityType, Guid? entityId, CancellationToken ct)
    {
        var query = db.AiAnalysisEvaluations.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType)) query = query.Where(x => x.EntityType == entityType);
        if (entityId.HasValue) query = query.Where(x => x.EntityId == entityId);
        var evaluations = await query.OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct);
        return await MapAsync(evaluations, ct);
    }

    public async Task<AiAnalysisResultDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var evaluation = await db.AiAnalysisEvaluations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (evaluation is null) return null;
        return (await MapAsync([evaluation], ct)).Single();
    }

    private async Task<AiPromptContextDto> BuildSimpleContext(string entityType, Guid id, string appliesTo,
        Func<Task<(string? Number, string Status, string Title, string? Description, Dictionary<string, object?> Metadata)>> load,
        CancellationToken ct)
    {
        var data = await load();
        var legalRules = await BuildLegalRuleContextAsync(entityType, id, appliesTo, ct);
        return new AiPromptContextDto(entityType, id, data.Number, data.Status,
            new AiProcurementEntityContextDto(data.Title, data.Description, data.Metadata), legalRules);
    }

    private async Task<IReadOnlyList<AiAnalysisResultDto>> MapAsync(IReadOnlyList<AiAnalysisEvaluation> evaluations, CancellationToken ct)
    {
        var ids = evaluations.Select(x => x.Id).ToArray();
        var findings = await db.AiAnalysisFindings.AsNoTracking().Where(x => ids.Contains(x.EvaluationId)).ToListAsync(ct);
        var recommendations = await db.AiAnalysisRecommendations.AsNoTracking().Where(x => ids.Contains(x.EvaluationId)).ToListAsync(ct);

        return evaluations.Select(e => new AiAnalysisResultDto(e.Id, e.EntityType, e.EntityId, e.AnalysisType,
            e.Provider, e.Model, e.Status, e.ResultSummary, e.RiskLevel, e.CreatedAt, e.CompletedAt,
            findings.Where(f => f.EvaluationId == e.Id)
                .Select(f => new ContractAiFindingDto(f.Id, f.Title, f.Description, ToSeverity(f.Severity), null,
                    f.RelatedRuleClauseId, f.Evidence, f.Recommendation, f.LegalReference)).ToArray(),
            recommendations.Where(r => r.EvaluationId == e.Id)
                .Select(r => new ContractAiRecommendationDto(r.Id, r.Title, r.Description, ToSeverity(r.Severity), r.RelatedAction)).ToArray(),
            null)).ToArray();
    }

    private static string? Get(IReadOnlyDictionary<string, string?> settings, string key, string? fallback) =>
        settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    private static string? ResolveApiKey(string? configuredSecretName, string? configuredFallback)
    {
        if (string.IsNullOrWhiteSpace(configuredSecretName))
            return configuredFallback;

        var trimmed = configuredSecretName.Trim();
        if (LooksLikeRawAiCoreKey(trimmed))
            return trimmed;

        var fromEnvironment = Environment.GetEnvironmentVariable(trimmed);
        return string.IsNullOrWhiteSpace(fromEnvironment) ? configuredFallback : fromEnvironment;
    }

    private static bool LooksLikeRawAiCoreKey(string value) =>
        value.StartsWith("aic_", StringComparison.OrdinalIgnoreCase) ||
        value.StartsWith("sk-", StringComparison.OrdinalIgnoreCase);

    private static int GetInt(IReadOnlyDictionary<string, string?> settings, string key, int fallback) =>
        int.TryParse(Get(settings, key, null), out var value) ? value : fallback;
    private static int? GetNullableInt(IReadOnlyDictionary<string, string?> settings, string key, int? fallback) =>
        int.TryParse(Get(settings, key, null), out var value) ? value : fallback;
    private static bool GetBool(IReadOnlyDictionary<string, string?> settings, string key, bool fallback) =>
        bool.TryParse(Get(settings, key, null), out var value) ? value : fallback;
    private static string NormalizePath(string? path, string fallback) => string.IsNullOrWhiteSpace(path)
        ? fallback
        : path.Trim() switch
        {
            "/api/analysis" => "/api/ai/text",
            "api/analysis" => "/api/ai/text",
            "/api/health" => fallback,
            "api/health" => fallback,
            var value => value
        };
    private static IReadOnlyList<string> SplitTags(string? tags) =>
        string.IsNullOrWhiteSpace(tags) ? [] : tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    private static ContractAiSeverity ToSeverity(string severity) =>
        Enum.TryParse<ContractAiSeverity>(severity, true, out var value) ? value : ContractAiSeverity.Info;
}
