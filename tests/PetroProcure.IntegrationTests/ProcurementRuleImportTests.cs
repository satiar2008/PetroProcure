using Microsoft.Extensions.DependencyInjection;
using PetroProcure.Application.Legal;
using PetroProcure.Contracts.V1.Legal;
using PetroProcure.Domain.Enums;

namespace PetroProcure.IntegrationTests;

[Collection("sqlserver")]
public sealed class ProcurementRuleImportTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task ImportsValidJsonRuleAsActive()
    {
        var code = Code();
        await using var scope = fixture.Services.CreateAsyncScope();
        var import = scope.ServiceProvider.GetRequiredService<IProcurementRuleImportService>();
        var repository = scope.ServiceProvider.GetRequiredService<ILegalRuleRepository>();

        var result = await import.ImportAsync(
            Request(code, """{ "op": ">=", "field": "FinalAmount", "value": 1000000000 }"""), CancellationToken.None);

        Assert.Equal(1, result.Imported);
        Assert.Equal(0, result.Failed);

        var rule = await repository.FindRuleByCodeAsync(code, includeVersions: true, CancellationToken.None);
        Assert.NotNull(rule);
        Assert.NotNull(rule!.ActiveVersionId);
        Assert.Contains(rule.Versions, v => v.Status == RuleStatus.Active);
    }

    [Fact]
    public async Task RejectsInvalidConditionJson()
    {
        var code = Code();
        await using var scope = fixture.Services.CreateAsyncScope();
        var import = scope.ServiceProvider.GetRequiredService<IProcurementRuleImportService>();

        var result = await import.ImportAsync(Request(code, "{ invalid json without close"), CancellationToken.None);

        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Failed);
    }

    [Fact]
    public async Task SkipsDuplicateRuleCodeAndVersion()
    {
        var code = Code();
        const string json = """{ "op": "count>=", "field": "SupplierCount", "value": 3 }""";
        await using var scope = fixture.Services.CreateAsyncScope();
        var import = scope.ServiceProvider.GetRequiredService<IProcurementRuleImportService>();

        var first = await import.ImportAsync(Request(code, json), CancellationToken.None);
        var second = await import.ImportAsync(Request(code, json), CancellationToken.None);

        Assert.Equal(1, first.Imported);
        Assert.Equal(1, second.Skipped);
        Assert.Equal(0, second.Imported);
    }

    [Fact]
    public async Task ActiveImportedRuleAppearsInQuery()
    {
        var code = Code();
        await using var scope = fixture.Services.CreateAsyncScope();
        var import = scope.ServiceProvider.GetRequiredService<IProcurementRuleImportService>();
        var repository = scope.ServiceProvider.GetRequiredService<ILegalRuleRepository>();

        await import.ImportAsync(
            Request(code, """{ "op": "exists", "field": "DocumentTypes", "value": "Indent" }"""), CancellationToken.None);

        var page = await repository.GetRulesAsync(
            new ProcurementRuleListRequest(SearchTerm: code, Status: RuleStatus.Active), CancellationToken.None);

        Assert.Contains(page.Items, x => x.Code == code);
    }

    [Fact]
    public async Task ImportJsonStringParsesArrayPayload()
    {
        var code = Code();
        await using var scope = fixture.Services.CreateAsyncScope();
        var import = scope.ServiceProvider.GetRequiredService<IProcurementRuleImportService>();

        var json = $$"""
        [ { "code": "{{code}}", "title": "قاعده", "ruleType": "Document", "severity": "Critical",
            "evaluationMode": "Automatic", "conditionValue": "{ \"op\": \"exists\", \"field\": \"DocumentTypes\", \"value\": \"Indent\" }",
            "legalReference": "ماده ۲", "activate": true, "version": 1 } ]
        """;

        var result = await import.ImportJsonAsync(json, CancellationToken.None);
        Assert.Equal(1, result.Imported);
    }

    private static string Code() => $"RAG3-{Guid.NewGuid():N}"[..14];

    private static ProcurementRuleImportRequest Request(string code, string conditionValue,
        bool activate = true, int version = 1) =>
        new(null, [
            new ProcurementRuleImportItem(code, "قاعده آزمایشی", "Threshold", "Critical", "Automatic",
                conditionValue, "ماده ۱", Activate: activate, Version: version)
        ]);
}
