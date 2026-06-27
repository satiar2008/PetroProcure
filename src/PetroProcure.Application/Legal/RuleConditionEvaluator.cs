using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Legal;

namespace PetroProcure.Application.Legal;

// AI-RAG-02: data-driven JSON condition engine. ConditionValue may hold a JSON condition tree;
// when it is not valid JSON, evaluation falls back to the legacy ConditionType switch.

public enum RuleConditionOperator
{
    GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Equal, NotEqual, Between,
    Exists, Missing, AnyOf, AllOf,
    CountGreaterThan, CountGreaterThanOrEqual, CountLessThan, CountLessThanOrEqual, CountEqual,
    Equals, NotEquals, In, NotIn,
    Before, After, Within, Overdue,
    ExceptWhen, OverrideWhen,
    Unknown
}

/// <summary>One condition node: either a leaf (op/field/value) or a group (all/any/none).</summary>
public sealed class RuleConditionDefinition
{
    public string? Op { get; set; }
    public string? Field { get; set; }
    public JsonElement? Value { get; set; }
    public string? Currency { get; set; }
    public string? SeverityOverride { get; set; }
    public string? Message { get; set; }
    public List<RuleConditionDefinition>? All { get; set; }
    public List<RuleConditionDefinition>? Any { get; set; }
    public List<RuleConditionDefinition>? None { get; set; }
    public RuleConditionDefinition? Condition { get; set; } // nested condition for exceptWhen/overrideWhen

    [JsonIgnore]
    public bool IsGroup => All is { Count: > 0 } || Any is { Count: > 0 } || None is { Count: > 0 };
}

public sealed record RuleConditionGroup(string Kind, IReadOnlyList<RuleConditionDefinition> Conditions);

public sealed record RuleConditionParseResult(bool IsJson, bool IsValid, RuleConditionDefinition? Root, string? Error)
{
    public static RuleConditionParseResult NotJson() => new(false, false, null, null);
    public static RuleConditionParseResult Invalid(string error) => new(true, false, null, error);
    public static RuleConditionParseResult Valid(RuleConditionDefinition root) => new(true, true, root, null);
}

public sealed record ConditionEvaluationResult(RuleResult Result, RuleSeverity? SeverityOverride = null, string? Message = null);

public interface IConditionEvaluator
{
    bool CanEvaluate(ProcurementRuleVersion ruleVersion);
    Task<ConditionEvaluationResult> EvaluateAsync(ProcurementRuleVersion ruleVersion, RuleEvaluationContext context,
        CancellationToken cancellationToken = default);
    RuleConditionParseResult Parse(string? conditionValue);
}

public sealed class JsonRuleConditionEvaluator(ILogger<JsonRuleConditionEvaluator> logger) : IConditionEvaluator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private enum Outcome { Satisfied, NotSatisfied, Indeterminate }

    public RuleConditionParseResult Parse(string? conditionValue)
    {
        if (string.IsNullOrWhiteSpace(conditionValue)) return RuleConditionParseResult.NotJson();
        var trimmed = conditionValue.TrimStart();
        if (trimmed.Length == 0 || trimmed[0] != '{') return RuleConditionParseResult.NotJson();

        try
        {
            var root = JsonSerializer.Deserialize<RuleConditionDefinition>(conditionValue, JsonOptions);
            if (root is null) return RuleConditionParseResult.Invalid("Condition is empty.");
            if (root.IsGroup || !string.IsNullOrWhiteSpace(root.Op)) return RuleConditionParseResult.Valid(root);
            return RuleConditionParseResult.Invalid("Condition has neither an operator nor a group.");
        }
        catch (JsonException ex)
        {
            return RuleConditionParseResult.Invalid(ex.Message);
        }
    }

    public bool CanEvaluate(ProcurementRuleVersion ruleVersion)
    {
        var parsed = Parse(ruleVersion.ConditionValue);
        if (parsed.IsJson && !parsed.IsValid)
            logger.LogWarning("Invalid JSON rule condition for rule version {RuleVersionId}: {Error}",
                ruleVersion.Id, parsed.Error);
        return parsed.IsValid;
    }

    public Task<ConditionEvaluationResult> EvaluateAsync(ProcurementRuleVersion ruleVersion,
        RuleEvaluationContext context, CancellationToken cancellationToken = default)
    {
        var parsed = Parse(ruleVersion.ConditionValue);
        if (!parsed.IsValid || parsed.Root is null)
            return Task.FromResult(new ConditionEvaluationResult(RuleResult.NotApplicable));

        var root = parsed.Root;
        var fields = BuildFieldMap(context);
        var severityOverride = ParseSeverity(root.SeverityOverride);
        var effectiveSeverity = severityOverride ?? ruleVersion.Severity;

        // Exception operators are top-level result modifiers.
        var op = ParseOperator(root.Op);
        if (op is RuleConditionOperator.ExceptWhen or RuleConditionOperator.OverrideWhen)
        {
            var inner = root.Condition is null ? Outcome.Indeterminate : Evaluate(root.Condition, fields);
            var exceptionResult = op switch
            {
                RuleConditionOperator.ExceptWhen => inner switch
                {
                    Outcome.Satisfied => RuleResult.NotApplicable, // exception applies → rule does not apply
                    Outcome.NotSatisfied => RuleResult.Pass,
                    _ => Indeterminate(effectiveSeverity)
                },
                _ => inner switch // OverrideWhen
                {
                    Outcome.Satisfied => RuleResult.Pass, // override → forced pass
                    Outcome.NotSatisfied => RuleResult.NotApplicable,
                    _ => Indeterminate(effectiveSeverity)
                }
            };
            return Task.FromResult(new ConditionEvaluationResult(exceptionResult, severityOverride, root.Message));
        }

        var outcome = Evaluate(root, fields);
        var result = Map(outcome, effectiveSeverity);
        return Task.FromResult(new ConditionEvaluationResult(result, severityOverride, root.Message));
    }

    private Outcome Evaluate(RuleConditionDefinition node, IReadOnlyDictionary<string, object?> fields)
    {
        if (node.All is { Count: > 0 } all)
            return Combine(all, fields, requireAll: true, negate: false);
        if (node.Any is { Count: > 0 } any)
            return CombineAny(any, fields);
        if (node.None is { Count: > 0 } none)
            return Combine(none, fields, requireAll: true, negate: true);
        return EvaluateLeaf(node, fields);
    }

    private Outcome Combine(IReadOnlyList<RuleConditionDefinition> nodes,
        IReadOnlyDictionary<string, object?> fields, bool requireAll, bool negate)
    {
        var satisfiedCount = 0;
        foreach (var n in nodes)
        {
            var o = Evaluate(n, fields);
            if (o == Outcome.Indeterminate) return Outcome.Indeterminate;
            if (o == Outcome.Satisfied) satisfiedCount++;
        }
        var allSatisfied = satisfiedCount == nodes.Count;
        // "none": satisfied when no child is satisfied.
        if (negate) return satisfiedCount == 0 ? Outcome.Satisfied : Outcome.NotSatisfied;
        return allSatisfied ? Outcome.Satisfied : Outcome.NotSatisfied;
    }

    private Outcome CombineAny(IReadOnlyList<RuleConditionDefinition> nodes, IReadOnlyDictionary<string, object?> fields)
    {
        var anyIndeterminate = false;
        foreach (var n in nodes)
        {
            var o = Evaluate(n, fields);
            if (o == Outcome.Satisfied) return Outcome.Satisfied;
            if (o == Outcome.Indeterminate) anyIndeterminate = true;
        }
        return anyIndeterminate ? Outcome.Indeterminate : Outcome.NotSatisfied;
    }

    private Outcome EvaluateLeaf(RuleConditionDefinition node, IReadOnlyDictionary<string, object?> fields)
    {
        var op = ParseOperator(node.Op);
        if (op == RuleConditionOperator.Unknown || string.IsNullOrWhiteSpace(node.Field)) return Outcome.Indeterminate;
        var field = node.Field!;
        if (!fields.ContainsKey(field)) return Outcome.Indeterminate;

        return op switch
        {
            RuleConditionOperator.GreaterThan => CompareNumber(field, fields, node.Value, (a, b) => a > b),
            RuleConditionOperator.GreaterThanOrEqual => CompareNumber(field, fields, node.Value, (a, b) => a >= b),
            RuleConditionOperator.LessThan => CompareNumber(field, fields, node.Value, (a, b) => a < b),
            RuleConditionOperator.LessThanOrEqual => CompareNumber(field, fields, node.Value, (a, b) => a <= b),
            RuleConditionOperator.Equal or RuleConditionOperator.Equals => EvaluateEquality(field, fields, node.Value, negate: false),
            RuleConditionOperator.NotEqual or RuleConditionOperator.NotEquals => EvaluateEquality(field, fields, node.Value, negate: true),
            RuleConditionOperator.Between => EvaluateBetween(field, fields, node.Value),
            RuleConditionOperator.Exists => EvaluateContains(field, fields, node.Value, negate: false),
            RuleConditionOperator.Missing => EvaluateContains(field, fields, node.Value, negate: true),
            RuleConditionOperator.AnyOf => EvaluateSet(field, fields, node.Value, requireAll: false),
            RuleConditionOperator.AllOf => EvaluateSet(field, fields, node.Value, requireAll: true),
            RuleConditionOperator.In => EvaluateMembership(field, fields, node.Value, negate: false),
            RuleConditionOperator.NotIn => EvaluateMembership(field, fields, node.Value, negate: true),
            RuleConditionOperator.CountGreaterThan => CompareCount(field, fields, node.Value, (a, b) => a > b),
            RuleConditionOperator.CountGreaterThanOrEqual => CompareCount(field, fields, node.Value, (a, b) => a >= b),
            RuleConditionOperator.CountLessThan => CompareCount(field, fields, node.Value, (a, b) => a < b),
            RuleConditionOperator.CountLessThanOrEqual => CompareCount(field, fields, node.Value, (a, b) => a <= b),
            RuleConditionOperator.CountEqual => CompareCount(field, fields, node.Value, (a, b) => a == b),
            RuleConditionOperator.Before => EvaluateDeadline(field, fields, node.Value, RuleConditionOperator.Before),
            RuleConditionOperator.After => EvaluateDeadline(field, fields, node.Value, RuleConditionOperator.After),
            RuleConditionOperator.Within => EvaluateDeadline(field, fields, node.Value, RuleConditionOperator.Within),
            RuleConditionOperator.Overdue => EvaluateDeadline(field, fields, node.Value, RuleConditionOperator.Overdue),
            _ => Outcome.Indeterminate
        };
    }

    private static Outcome CompareNumber(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, Func<decimal, decimal, bool> compare)
    {
        var left = GetDecimal(fields, field);
        var right = DecimalFromValue(value);
        if (left is null || right is null) return Outcome.Indeterminate;
        return ToOutcome(compare(left.Value, right.Value));
    }

    private static Outcome CompareCount(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, Func<int, int, bool> compare)
    {
        var count = GetCount(fields, field);
        var right = DecimalFromValue(value);
        if (count is null || right is null) return Outcome.Indeterminate;
        return ToOutcome(compare(count.Value, (int)right.Value));
    }

    private static Outcome EvaluateEquality(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, bool negate)
    {
        var leftNum = GetDecimal(fields, field);
        var rightNum = DecimalFromValue(value);
        if (leftNum is not null && rightNum is not null)
            return ToOutcome((leftNum == rightNum) ^ negate);

        var left = GetString(fields, field);
        var right = StringFromValue(value);
        if (left is null || right is null) return Outcome.Indeterminate;
        return ToOutcome(string.Equals(left, right, StringComparison.OrdinalIgnoreCase) ^ negate);
    }

    private static Outcome EvaluateBetween(string field, IReadOnlyDictionary<string, object?> fields, JsonElement? value)
    {
        var v = GetDecimal(fields, field);
        if (v is null || value is not { ValueKind: JsonValueKind.Array }) return Outcome.Indeterminate;
        var array = value.Value;
        if (array.GetArrayLength() < 2) return Outcome.Indeterminate;
        var min = ElementToDecimal(array[0]);
        var max = ElementToDecimal(array[1]);
        if (min is null || max is null) return Outcome.Indeterminate;
        return ToOutcome(v >= min && v <= max);
    }

    private static Outcome EvaluateContains(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, bool negate)
    {
        var set = GetStringCollection(fields, field);
        var target = StringFromValue(value);
        if (set is null || target is null) return Outcome.Indeterminate;
        var contains = set.Any(x => string.Equals(x, target, StringComparison.OrdinalIgnoreCase));
        return ToOutcome(contains ^ negate);
    }

    private static Outcome EvaluateSet(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, bool requireAll)
    {
        var set = GetStringCollection(fields, field);
        var values = StringListFromValue(value);
        if (set is null || values.Count == 0) return Outcome.Indeterminate;
        bool Has(string v) => set.Any(x => string.Equals(x, v, StringComparison.OrdinalIgnoreCase));
        return ToOutcome(requireAll ? values.All(Has) : values.Any(Has));
    }

    private static Outcome EvaluateMembership(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, bool negate)
    {
        var left = GetString(fields, field);
        var values = StringListFromValue(value);
        if (left is null || values.Count == 0) return Outcome.Indeterminate;
        var member = values.Any(v => string.Equals(v, left, StringComparison.OrdinalIgnoreCase));
        return ToOutcome(member ^ negate);
    }

    private static Outcome EvaluateDeadline(string field, IReadOnlyDictionary<string, object?> fields,
        JsonElement? value, RuleConditionOperator op)
    {
        var deadline = GetDateTime(fields, field);
        if (deadline is null) return Outcome.Indeterminate;
        var now = DateTime.UtcNow;
        var window = DurationFromValue(value);

        return op switch
        {
            RuleConditionOperator.Overdue => ToOutcome(deadline.Value < now),
            RuleConditionOperator.Before when window is not null => ToOutcome(now < deadline.Value - window.Value),
            RuleConditionOperator.After when window is not null => ToOutcome(now > deadline.Value + window.Value),
            RuleConditionOperator.Within when window is not null =>
                ToOutcome(now >= deadline.Value - window.Value && now <= deadline.Value),
            // Without a duration, treat before/after as relative to the deadline itself.
            RuleConditionOperator.Before => ToOutcome(now < deadline.Value),
            RuleConditionOperator.After => ToOutcome(now > deadline.Value),
            _ => Outcome.Indeterminate
        };
    }

    private static RuleResult Map(Outcome outcome, RuleSeverity severity) => outcome switch
    {
        Outcome.Satisfied => RuleResult.Pass,
        Outcome.NotSatisfied => severity switch
        {
            RuleSeverity.Blocking or RuleSeverity.Critical => RuleResult.Fail,
            RuleSeverity.Warning => RuleResult.Warning,
            _ => RuleResult.NotApplicable
        },
        _ => Indeterminate(severity)
    };

    private static RuleResult Indeterminate(RuleSeverity severity) =>
        severity is RuleSeverity.Blocking or RuleSeverity.Critical ? RuleResult.NeedHumanReview : RuleResult.NotApplicable;

    private static Outcome ToOutcome(bool value) => value ? Outcome.Satisfied : Outcome.NotSatisfied;

    private static IReadOnlyDictionary<string, object?> BuildFieldMap(RuleEvaluationContext c) =>
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["EntityType"] = c.EntityType,
            ["Status"] = c.Status,
            ["FileNumber"] = c.FileNumber,
            ["Priority"] = c.Priority,
            ["TenderType"] = c.TenderType,
            ["Currency"] = c.Currency,
            ["HasTender"] = c.HasTender,
            ["ItemCount"] = c.ItemCount,
            ["SupplierCount"] = c.SupplierCount,
            ["OfferCount"] = c.OfferCount,
            ["ExistingDocumentCount"] = c.ExistingDocumentCount,
            ["TotalRequestedQuantity"] = c.TotalRequestedQuantity,
            ["EstimatedAmount"] = c.EstimatedAmount,
            ["FinalAmount"] = c.FinalAmount,
            ["CreatedAt"] = c.CreatedAt,
            ["InquiryDeadline"] = c.InquiryDeadline,
            ["TenderDeadline"] = c.TenderDeadline,
            ["TechnicalReviewDeadline"] = c.TechnicalReviewDeadline,
            ["CurrentDepartmentId"] = c.CurrentDepartmentId?.ToString(),
            ["RequestingDepartmentId"] = c.RequestingDepartmentId?.ToString(),
            ["ApplicantDepartmentId"] = c.ApplicantDepartmentId?.ToString(),
            ["DocumentTypes"] = c.DocumentTypes,
            ["ApprovalStatuses"] = c.ApprovalStatuses,
            ["WorkflowStatuses"] = c.WorkflowStatuses,
            ["LegalReferences"] = c.LegalReferences,
            ["UserDepartmentIds"] = c.UserDepartmentIds.Select(x => x.ToString()).ToArray()
        };

    private static decimal? GetDecimal(IReadOnlyDictionary<string, object?> fields, string field) =>
        fields.TryGetValue(field, out var v) ? v switch
        {
            decimal d => d,
            int i => i,
            long l => l,
            bool b => b ? 1 : 0,
            string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) => r,
            _ => null
        } : null;

    private static int? GetCount(IReadOnlyDictionary<string, object?> fields, string field)
    {
        if (!fields.TryGetValue(field, out var v) || v is null) return null;
        if (v is System.Collections.IEnumerable e and not string) return e.Cast<object?>().Count();
        if (v is int i) return i;
        if (v is decimal d) return (int)d;
        return null;
    }

    private static string? GetString(IReadOnlyDictionary<string, object?> fields, string field)
    {
        if (!fields.TryGetValue(field, out var v) || v is null) return null;
        if (v is string s) return s;
        if (v is System.Collections.IEnumerable) return null; // collections are not scalar strings
        return v.ToString();
    }

    private static IReadOnlyCollection<string>? GetStringCollection(IReadOnlyDictionary<string, object?> fields, string field)
    {
        if (!fields.TryGetValue(field, out var v) || v is null) return null;
        if (v is IEnumerable<string> ss) return ss.ToArray();
        if (v is System.Collections.IEnumerable e and not string) return e.Cast<object?>().Select(x => x?.ToString() ?? string.Empty).ToArray();
        return null;
    }

    private static DateTime? GetDateTime(IReadOnlyDictionary<string, object?> fields, string field) =>
        fields.TryGetValue(field, out var v) && v is DateTime d ? d : null;

    private static decimal? DecimalFromValue(JsonElement? value) =>
        value is { } v ? ElementToDecimal(v) : null;

    private static decimal? ElementToDecimal(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.Number => v.TryGetDecimal(out var d) ? d : null,
        JsonValueKind.String => decimal.TryParse(v.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : null,
        _ => null
    };

    private static string? StringFromValue(JsonElement? value) => value?.ValueKind switch
    {
        JsonValueKind.String => value.Value.GetString(),
        JsonValueKind.Number => value.Value.ToString(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => null
    };

    private static List<string> StringListFromValue(JsonElement? value)
    {
        if (value is not { } v) return [];
        if (v.ValueKind == JsonValueKind.Array)
            return v.EnumerateArray().Select(e => StringFromValue(e) ?? string.Empty)
                .Where(s => s.Length > 0).ToList();
        var single = StringFromValue(v);
        return single is null ? [] : [single];
    }

    private static TimeSpan? DurationFromValue(JsonElement? value)
    {
        var s = StringFromValue(value);
        if (string.IsNullOrWhiteSpace(s)) return null;
        try { return XmlConvert.ToTimeSpan(s); }
        catch (FormatException) { return null; }
    }

    private static RuleSeverity? ParseSeverity(string? value) =>
        Enum.TryParse<RuleSeverity>(value, ignoreCase: true, out var parsed) ? parsed : null;

    private static RuleConditionOperator ParseOperator(string? op) => op?.Trim().ToLowerInvariant() switch
    {
        ">" => RuleConditionOperator.GreaterThan,
        ">=" => RuleConditionOperator.GreaterThanOrEqual,
        "<" => RuleConditionOperator.LessThan,
        "<=" => RuleConditionOperator.LessThanOrEqual,
        "=" or "==" => RuleConditionOperator.Equal,
        "!=" or "<>" => RuleConditionOperator.NotEqual,
        "between" => RuleConditionOperator.Between,
        "exists" => RuleConditionOperator.Exists,
        "missing" => RuleConditionOperator.Missing,
        "anyof" => RuleConditionOperator.AnyOf,
        "allof" => RuleConditionOperator.AllOf,
        "count>" => RuleConditionOperator.CountGreaterThan,
        "count>=" => RuleConditionOperator.CountGreaterThanOrEqual,
        "count<" => RuleConditionOperator.CountLessThan,
        "count<=" => RuleConditionOperator.CountLessThanOrEqual,
        "count=" => RuleConditionOperator.CountEqual,
        "equals" => RuleConditionOperator.Equals,
        "notequals" => RuleConditionOperator.NotEquals,
        "in" => RuleConditionOperator.In,
        "notin" => RuleConditionOperator.NotIn,
        "before" => RuleConditionOperator.Before,
        "after" => RuleConditionOperator.After,
        "within" => RuleConditionOperator.Within,
        "overdue" => RuleConditionOperator.Overdue,
        "exceptwhen" => RuleConditionOperator.ExceptWhen,
        "overridewhen" => RuleConditionOperator.OverrideWhen,
        _ => RuleConditionOperator.Unknown
    };

    public static IConditionEvaluator CreateDefault() => new JsonRuleConditionEvaluator(NullLogger<JsonRuleConditionEvaluator>.Instance);
}
