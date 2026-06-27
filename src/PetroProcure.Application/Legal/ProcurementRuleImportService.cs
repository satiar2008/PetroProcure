using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Ai;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Application.Legal;

public interface IProcurementRuleImportService
{
    Task<ProcurementRuleImportResult> ImportAsync(ProcurementRuleImportRequest request, CancellationToken ct = default);
    Task<ProcurementRuleImportResult> ImportJsonAsync(string json, CancellationToken ct = default);
    Task<ProcurementRuleImportResult> ImportCsvAsync(string csv, CancellationToken ct = default);
}

// AI-RAG-03: imports/seeds versioned procurement rules. Validates fields and the JSON condition,
// dedupes by RuleCode + Version, and (when requested) submits + approves to reach Active status.
public sealed class ProcurementRuleImportService(
    ILegalRuleRepository repository,
    IConditionEvaluator conditions,
    ICurrentUserService currentUser,
    IRagIngestionQueue ingestionQueue,
    ILogger<ProcurementRuleImportService> logger) : IProcurementRuleImportService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ProcurementRuleImportResult> ImportJsonAsync(string json, CancellationToken ct = default)
    {
        ProcurementRuleImportRequest? request;
        try
        {
            request = Deserialize(json);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Procurement rule JSON import could not be parsed.");
            return new ProcurementRuleImportResult(0, 0, 0, 1,
                [new ProcurementRuleImportItemResult("(file)", ProcurementRuleImportResult.OutcomeFailed, "Invalid JSON: " + ex.Message)]);
        }

        return request is null || request.Rules.Count == 0
            ? new ProcurementRuleImportResult(0, 0, 0, 0, [])
            : await ImportAsync(request, ct);
    }

    public Task<ProcurementRuleImportResult> ImportCsvAsync(string csv, CancellationToken ct = default) =>
        ImportAsync(new ProcurementRuleImportRequest(null, ParseCsv(csv)), ct);

    public async Task<ProcurementRuleImportResult> ImportAsync(ProcurementRuleImportRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var results = new List<ProcurementRuleImportItemResult>();
        foreach (var item in request.Rules)
        {
            ct.ThrowIfCancellationRequested();
            results.Add(await ImportItemAsync(item, ct));
        }

        return new ProcurementRuleImportResult(
            results.Count,
            results.Count(x => x.Outcome == ProcurementRuleImportResult.OutcomeImported),
            results.Count(x => x.Outcome == ProcurementRuleImportResult.OutcomeSkipped),
            results.Count(x => x.Outcome == ProcurementRuleImportResult.OutcomeFailed),
            results);
    }

    private async Task<ProcurementRuleImportItemResult> ImportItemAsync(ProcurementRuleImportItem item, CancellationToken ct)
    {
        var code = item.Code?.Trim() ?? string.Empty;
        try
        {
            if (string.IsNullOrWhiteSpace(code)) return Failed(code, "Rule code is required.");
            if (string.IsNullOrWhiteSpace(item.Title)) return Failed(code, "Rule title is required.");
            if (string.IsNullOrWhiteSpace(item.LegalReference)) return Failed(code, "Legal reference is required.");
            if (string.IsNullOrWhiteSpace(item.ConditionValue)) return Failed(code, "Condition value is required.");
            if (!Enum.TryParse<RuleType>(item.RuleType, true, out var ruleType)) return Failed(code, $"Unknown rule type '{item.RuleType}'.");
            if (!Enum.TryParse<RuleSeverity>(item.Severity, true, out var severity)) return Failed(code, $"Unknown severity '{item.Severity}'.");
            if (!Enum.TryParse<RuleEvaluationMode>(item.EvaluationMode, true, out var mode)) return Failed(code, $"Unknown evaluation mode '{item.EvaluationMode}'.");

            var conditionType = string.IsNullOrWhiteSpace(item.ConditionType) ? "json" : item.ConditionType!.Trim();
            var looksJson = item.ConditionValue.TrimStart().StartsWith('{');
            if (looksJson || conditionType.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                var parsed = conditions.Parse(item.ConditionValue);
                if (!parsed.IsValid) return Failed(code, "Invalid condition JSON: " + (parsed.Error ?? "not a valid condition."));
                conditionType = "json";
            }

            var requestedVersion = item.Version <= 0 ? 1 : item.Version;
            var existing = await repository.FindRuleByCodeAsync(code, includeVersions: true, ct);
            if (existing is not null && existing.Versions.Any(v => v.VersionNo == requestedVersion))
                return Skipped(code, $"Version {requestedVersion} already exists.");

            var rule = existing ?? new ProcurementRule(Guid.NewGuid(), code, item.Title.Trim(), null, currentUser.UserId);
            var versionNo = existing is null
                ? requestedVersion
                : Math.Max(requestedVersion, await repository.NextRuleVersionNoAsync(rule.Id, ct));

            var ruleVersion = new ProcurementRuleVersion(Guid.NewGuid(), rule.Id, versionNo, item.Title.Trim(),
                ruleType, severity, mode, item.LegalArticleId, item.LegalClauseId, item.LegalReference.Trim(),
                conditionType, item.ConditionValue, item.ConditionDescription, currentUser.UserId);

            await repository.AddRuleAsync(rule, ruleVersion, ct);

            if (item.Activate)
            {
                if (existing?.ActiveVersionId is { } activeId &&
                    await repository.FindRuleVersionAsync(activeId, ct) is { } oldActive)
                    oldActive.Deprecate();
                ruleVersion.Submit();
                ruleVersion.Approve(currentUser.UserId);
                rule.SetActiveVersion(ruleVersion.Id);
            }

            await repository.AddAuditAsync(new LegalRuleAuditLog(Guid.NewGuid(), "ProcurementRule", rule.Id,
                "Import", $"Imported rule {code} v{versionNo} (activate={item.Activate}).", currentUser.UserId), ct);
            await repository.SaveChangesAsync(ct);
            if (item.Activate && item.LegalClauseId is { } legalClauseId)
                await ingestionQueue.EnqueueAsync(new EmbeddingIngestionPayload(
                    AiDocumentSourceType.LegalClause,
                    legalClauseId), currentUser.UserId, ct);

            return new ProcurementRuleImportItemResult(code, ProcurementRuleImportResult.OutcomeImported,
                item.Activate ? "Imported and activated." : "Imported as draft.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to import procurement rule {Code}.", code);
            return Failed(code, ex.Message);
        }
    }

    private static ProcurementRuleImportItemResult Failed(string code, string message) =>
        new(code, ProcurementRuleImportResult.OutcomeFailed, message);

    private static ProcurementRuleImportItemResult Skipped(string code, string message) =>
        new(code, ProcurementRuleImportResult.OutcomeSkipped, message);

    private static ProcurementRuleImportRequest? Deserialize(string json)
    {
        var trimmed = json.TrimStart();
        if (trimmed.StartsWith('['))
        {
            var items = JsonSerializer.Deserialize<List<ProcurementRuleImportItem>>(json, JsonOptions) ?? [];
            return new ProcurementRuleImportRequest(null, items);
        }
        return JsonSerializer.Deserialize<ProcurementRuleImportRequest>(json, JsonOptions);
    }

    private static List<ProcurementRuleImportItem> ParseCsv(string csv)
    {
        var rows = SplitCsvRows(csv);
        if (rows.Count < 2) return [];

        var header = rows[0].Select(h => h.Trim().ToLowerInvariant()).ToList();
        int Idx(string name) => header.IndexOf(name);
        int ci = Idx("code"), ti = Idx("title"), rt = Idx("ruletype"), se = Idx("severity"),
            em = Idx("evaluationmode"), ctx = Idx("conditiontype"), cv = Idx("conditionvalue"),
            lr = Idx("legalreference"), la = Idx("legalarticleid"), lc = Idx("legalclauseid"),
            ac = Idx("activate"), vr = Idx("version");

        var items = new List<ProcurementRuleImportItem>();
        for (var i = 1; i < rows.Count; i++)
        {
            var r = rows[i];
            if (r.Count == 0 || r.All(string.IsNullOrWhiteSpace)) continue;
            string? Get(int idx) => idx >= 0 && idx < r.Count ? r[idx] : null;
            Guid? AsGuid(int idx) => Guid.TryParse(Get(idx), out var g) ? g : null;
            var activate = !bool.TryParse(Get(ac), out var b) || b; // default true
            var version = int.TryParse(Get(vr), out var v) ? v : 1;
            items.Add(new ProcurementRuleImportItem(
                Get(ci) ?? string.Empty, Get(ti) ?? string.Empty, Get(rt) ?? string.Empty,
                Get(se) ?? string.Empty, Get(em) ?? string.Empty, Get(cv) ?? string.Empty,
                Get(lr) ?? string.Empty,
                string.IsNullOrWhiteSpace(Get(ctx)) ? null : Get(ctx),
                null, AsGuid(la), AsGuid(lc), null, activate, version));
        }
        return items;
    }

    private static List<List<string>> SplitCsvRows(string csv)
    {
        var rows = new List<List<string>>();
        var field = new StringBuilder();
        var row = new List<string>();
        var inQuotes = false;

        for (var i = 0; i < csv.Length; i++)
        {
            var c = csv[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                switch (c)
                {
                    case '"': inQuotes = true; break;
                    case ',': row.Add(field.ToString()); field.Clear(); break;
                    case '\r': break;
                    case '\n': row.Add(field.ToString()); field.Clear(); rows.Add(row); row = []; break;
                    default: field.Append(c); break;
                }
            }
        }
        if (field.Length > 0 || row.Count > 0) { row.Add(field.ToString()); rows.Add(row); }
        return rows;
    }
}
