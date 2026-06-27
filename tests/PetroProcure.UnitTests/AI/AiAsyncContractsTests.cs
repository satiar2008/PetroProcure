using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PetroProcure.AI;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class AiAsyncContractsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void AiCoreIntegrationOptionsBindCorrectly()
    {
        var values = new Dictionary<string, string?>
        {
            ["PetroProcure:AI:AiCore:Mode"] = "SyncAiCoreDirect",
            ["PetroProcure:AI:AiCore:BaseUrl"] = "https://aicore.example",
            ["PetroProcure:AI:AiCore:SubmitJobPath"] = "/api/ai/analysis-jobs",
            ["PetroProcure:AI:AiCore:SyncAnalysisPath"] = "/api/ai/text",
            ["PetroProcure:AI:AiCore:HealthPath"] = "/health/ready",
            ["PetroProcure:AI:AiCore:CallbackPublicUrl"] = "https://petroprocure.example/api/ai/aicore/callback",
            ["PetroProcure:AI:AiCore:ApiKey"] = "test-api-key",
            ["PetroProcure:AI:AiCore:CallbackSecret"] = "test-callback-secret",
            ["PetroProcure:AI:AiCore:DefaultModel"] = "gemma3",
            ["PetroProcure:AI:AiCore:RequestTimeoutSeconds"] = "90",
            ["PetroProcure:AI:AiCore:WorkerBatchSize"] = "7",
            ["PetroProcure:AI:AiCore:MaxRetryCount"] = "4",
            ["PetroProcure:AI:AiCore:RetryDelaySeconds"] = "15",
            ["PetroProcure:AI:AiCore:CallbackTimestampToleranceSeconds"] = "180",
            ["PetroProcure:AI:AiCore:StuckJobTimeoutMinutes"] = "20",
            ["PetroProcure:AI:AiCore:RunningJobTimeoutMinutes"] = "240",
            ["PetroProcure:AI:AiCore:CompletedJobRetentionDays"] = "365",
            ["PetroProcure:AI:AiCore:CallbackAllowedIpAddresses:0"] = "10.0.0.10"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();

        services.Configure<AiCoreIntegrationOptions>(
            configuration.GetSection(AiCoreIntegrationOptions.SectionName));

        var options = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AiCoreIntegrationOptions>>()
            .Value;

        Assert.Equal(AiCoreIntegrationMode.SyncAiCoreDirect, options.Mode);
        Assert.Equal("https://aicore.example", options.BaseUrl);
        Assert.Equal("/api/ai/analysis-jobs", options.SubmitJobPath);
        Assert.Equal("/api/ai/text", options.SyncAnalysisPath);
        Assert.Equal("/health/ready", options.HealthPath);
        Assert.Equal("https://petroprocure.example/api/ai/aicore/callback", options.CallbackPublicUrl);
        Assert.Equal("test-api-key", options.ApiKey);
        Assert.Equal("test-callback-secret", options.CallbackSecret);
        Assert.Equal("gemma3", options.DefaultModel);
        Assert.Equal(90, options.RequestTimeoutSeconds);
        Assert.Equal(7, options.WorkerBatchSize);
        Assert.Equal(4, options.MaxRetryCount);
        Assert.Equal(15, options.RetryDelaySeconds);
        Assert.Equal(180, options.CallbackTimestampToleranceSeconds);
        Assert.Equal(20, options.StuckJobTimeoutMinutes);
        Assert.Equal(240, options.RunningJobTimeoutMinutes);
        Assert.Equal(365, options.CompletedJobRetentionDays);
        Assert.Equal("10.0.0.10", Assert.Single(options.CallbackAllowedIpAddresses));
    }

    [Fact]
    public void AiCoreIntegrationOptionsExposeRequiredDefaults()
    {
        var options = new AiCoreIntegrationOptions();

        Assert.Equal(AiCoreIntegrationMode.AsyncAiCoreJob, options.Mode);
        Assert.Equal("/api/ai/jobs", options.SubmitJobPath);
        Assert.Equal("/api/ai/text", options.SyncAnalysisPath);
        Assert.Equal("/health/ready", options.HealthPath);
        Assert.Equal(string.Empty, options.CallbackPublicUrl);
        Assert.Equal(120, options.RequestTimeoutSeconds);
        Assert.Equal(5, options.WorkerBatchSize);
        Assert.Equal(3, options.MaxRetryCount);
        Assert.Equal(300, options.CallbackTimestampToleranceSeconds);
        Assert.Equal(15, options.StuckJobTimeoutMinutes);
        Assert.Equal(120, options.RunningJobTimeoutMinutes);
        Assert.Equal(180, options.CompletedJobRetentionDays);
        Assert.Empty(options.CallbackAllowedIpAddresses);
    }

    [Fact]
    public void LocalOllamaWorkerModeAndOptionsBindCorrectly()
    {
        var values = new Dictionary<string, string?>
        {
            ["PetroProcure:AI:AiCore:Mode"] = "LocalOllamaWorker",
            ["PetroProcure:AI:OllamaBaseUrl"] = "http://localhost:11434",
            ["PetroProcure:AI:OllamaModel"] = "gemma3"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var services = new ServiceCollection();

        services.Configure<AiCoreIntegrationOptions>(
            configuration.GetSection(AiCoreIntegrationOptions.SectionName));
        services.Configure<AiOptions>(
            configuration.GetSection(AiOptions.SectionName));

        var provider = services.BuildServiceProvider();
        var aiCore = provider.GetRequiredService<IOptions<AiCoreIntegrationOptions>>().Value;
        var ai = provider.GetRequiredService<IOptions<AiOptions>>().Value;

        Assert.Equal(AiCoreIntegrationMode.LocalOllamaWorker, aiCore.Mode);
        Assert.Equal("http://localhost:11434", ai.OllamaBaseUrl);
        Assert.Equal("gemma3", ai.OllamaModel);
    }

    [Fact]
    public void AsyncDtoSerializationUsesCamelCase()
    {
        var jobId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var entityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var dto = new AiJobStatusDto(jobId, "PurchaseFile", entityId, "Summary", "Queued",
            0, "Job queued.", null, new DateTime(2026, 6, 26, 0, 0, 0, DateTimeKind.Utc),
            null, null, null, false);

        var json = JsonSerializer.Serialize(dto, JsonOptions);

        Assert.Contains("\"jobId\"", json);
        Assert.Contains("\"entityType\"", json);
        Assert.Contains("\"progressPercent\"", json);
        Assert.DoesNotContain("\"JobId\"", json);
        Assert.DoesNotContain("\"EntityType\"", json);
        Assert.DoesNotContain("\"ProgressPercent\"", json);
    }

    [Fact]
    public void CallbackDtoDeserializesSampleAiCorePayload()
    {
        const string json = """
        {
          "correlationId": "corr-123",
          "externalJobId": "aicore-job-456",
          "status": "Completed",
          "progressPercent": 100,
          "message": "Analysis completed.",
          "result": {
            "summary": "Advisory summary.",
            "findings": [
              {
                "title": "Missing document",
                "description": "Technical specification is not attached.",
                "severity": "High",
                "relatedClauseCode": "DOC-TECH",
                "relatedDocumentId": "33333333-3333-3333-3333-333333333333"
              }
            ],
            "recommendations": [
              {
                "title": "Upload document",
                "description": "Ask the responsible department to attach the technical specification.",
                "priority": "High",
                "suggestedAction": "RequestDocument"
              }
            ],
            "rawResultJson": "{\"riskLevel\":\"High\"}"
          },
          "error": null,
          "usage": {
            "inputTokens": 10,
            "outputTokens": 20,
            "totalTokens": 30,
            "durationMs": 1500,
            "model": "gemma3",
            "provider": "AiCore"
          },
          "completedAtUtc": "2026-06-26T10:00:00Z"
        }
        """;

        var callback = JsonSerializer.Deserialize<AiCoreCallbackRequest>(json, JsonOptions);

        Assert.NotNull(callback);
        Assert.Equal("corr-123", callback.CorrelationId);
        Assert.Equal("aicore-job-456", callback.ExternalJobId);
        Assert.Equal("Completed", callback.Status);
        Assert.Equal(100, callback.ProgressPercent);
        Assert.NotNull(callback.Result);
        Assert.Equal("Advisory summary.", callback.Result.Summary);
        var finding = Assert.Single(callback.Result.Findings);
        Assert.Equal("Missing document", finding.Title);
        Assert.Equal(PetroProcure.Contracts.V1.Ai.AiSeverity.High, finding.Severity);
        Assert.Equal("DOC-TECH", finding.RelatedClauseCode);
        Assert.Equal(Guid.Parse("33333333-3333-3333-3333-333333333333"), finding.RelatedDocumentId);
        var recommendation = Assert.Single(callback.Result.Recommendations);
        Assert.Equal("High", recommendation.Priority);
        Assert.Equal("RequestDocument", recommendation.SuggestedAction);
        Assert.Equal(10, callback.Usage?.InputTokens);
        Assert.Equal(20, callback.Usage?.OutputTokens);
        Assert.Equal(30, callback.Usage?.TotalTokens);
        Assert.Equal(1500, callback.Usage?.DurationMs);
        Assert.Equal("gemma3", callback.Usage?.Model);
        Assert.Equal("AiCore", callback.Usage?.Provider);
        Assert.Equal(new DateTime(2026, 6, 26, 10, 0, 0, DateTimeKind.Utc), callback.CompletedAtUtc);
    }
}
