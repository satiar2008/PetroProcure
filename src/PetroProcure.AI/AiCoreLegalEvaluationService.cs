using System.Text.Json;
using PetroProcure.Application.Legal;
using PetroProcure.Domain.Enums;

namespace PetroProcure.AI;

public sealed class AiCoreLegalEvaluationService(IAiCoreClient client, IAiCoreSettingsProvider settingsProvider)
    : IAiLegalEvaluationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task<IReadOnlyList<AiLegalEvaluationFinding>> AnalyzeAsync(
        AiLegalEvaluationRequest request, CancellationToken ct = default)
    {
        var settings = await settingsProvider.GetAsync(ct);
        var response = await client.SendAnalysisAsync(new AiCoreAnalysisRequest(
            Guid.NewGuid().ToString("N"),
            "PetroProcure",
            request.Context.EntityType,
            request.Context.EntityId,
            "HybridLegalEvaluation",
            settings.DefaultModel,
            [
                new AiCoreMessage("system", SystemPrompt()),
                new AiCoreMessage("user", UserPrompt(request))
            ],
            new
            {
                request.Context,
                Rule = new
                {
                    request.Rule.Id,
                    request.Rule.ProcurementRuleId,
                    request.Rule.Title,
                    request.Rule.RuleType,
                    request.Rule.EvaluationMode,
                    request.Rule.Severity,
                    request.Rule.LegalReference,
                    request.Rule.ConditionType,
                    request.Rule.ConditionValue,
                    request.Rule.ConditionDescription
                },
                DeterministicOutcome = request.DeterministicOutcome,
                Citations = request.Citations
            },
            new { advisoryOnly = true }),
            ct);

        var fallbackReferences = request.Citations.Select(x => x.Reference).ToArray();
        return response.Findings
            .Select(x => new AiLegalEvaluationFinding(
                string.IsNullOrWhiteSpace(x.Title) ? request.Rule.Title : x.Title.Trim(),
                string.IsNullOrWhiteSpace(x.Description) ? response.Summary : x.Description.Trim(),
                MapSeverity(x.Severity),
                Confidence: null,
                LegalReference: string.IsNullOrWhiteSpace(x.LegalReference) ? request.Rule.LegalReference : x.LegalReference,
                CitationReferences: CitationReferences(x, fallbackReferences)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Description))
            .ToArray();
    }

    private static string SystemPrompt() => """
        You are a legal procurement analysis assistant for PetroProcure.
        Return advisory findings only. Never make final blocking decisions.
        Every finding must require human review and should cite supplied legal references when relevant.
        Keep the analysis grounded in the deterministic rule outcome and supplied citations.
        """;

    private static string UserPrompt(AiLegalEvaluationRequest request) =>
        JsonSerializer.Serialize(new
        {
            Task = "Prepare advisory legal findings for human review.",
            Rule = new
            {
                request.Rule.Title,
                request.Rule.RuleType,
                request.Rule.EvaluationMode,
                request.Rule.Severity,
                request.Rule.LegalReference,
                request.Rule.ConditionDescription
            },
            DeterministicOutcome = request.DeterministicOutcome,
            LegalCitations = request.Citations,
            EvaluationContext = request.Context.ToDto()
        }, JsonOptions);

    private static RuleSeverity MapSeverity(string severity) =>
        severity.Trim().ToLowerInvariant() switch
        {
            "blocking" => RuleSeverity.Critical,
            "critical" or "high" => RuleSeverity.Critical,
            "warning" or "medium" => RuleSeverity.Warning,
            _ => RuleSeverity.Info
        };

    private static IReadOnlyList<string> CitationReferences(AiCoreFinding finding, IReadOnlyList<string> fallbackReferences)
    {
        var references = new List<string>();
        if (!string.IsNullOrWhiteSpace(finding.LegalReference)) references.Add(finding.LegalReference);
        if (finding.RelatedRuleClauseId is { } clauseId) references.Add($"/api/legal/clauses/{clauseId}/context");
        references.AddRange(fallbackReferences);
        return references.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
