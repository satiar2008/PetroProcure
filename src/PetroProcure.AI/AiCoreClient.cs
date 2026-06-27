using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Ai;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PetroProcure.AI;

public sealed class AiCoreClient(HttpClient http, IAiCoreSettingsProvider settingsProvider, ILogger<AiCoreClient> logger) : IAiCoreClient
{
    public async Task<AiCoreAnalysisResponse> SendAnalysisAsync(AiCoreAnalysisRequest request, CancellationToken ct = default)
    {
        var settings = await settingsProvider.GetAsync(ct);
        if (!settings.IsEnabled) throw new AiCoreClientException("AiCore provider is disabled.");
        if (string.IsNullOrWhiteSpace(settings.BaseUrl)) throw new AiCoreClientException("AiCore BaseUrl is not configured.");
        using var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl(settings.BaseUrl, settings.AnalysisPath, "/api/ai/text"));
        ApplyHeaders(message, settings);
        //message.Content = JsonContent.Create(ToTextRequest(request, settings));
        var payload = ToTextRequest(request, settings);

        var payloadJson = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        message.Content = new StringContent(
            payloadJson,
            Encoding.UTF8,
            "application/json");

        var timeout = TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 10, 900));
        logger.LogInformation("Sending AiCore analysis request {RequestId} for {EntityType}/{EntityId}.",
            request.RequestId, request.EntityType, request.EntityId);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);
        try
        {
            using var response = await http.SendAsync(message, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
                throw Friendly(response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AiCoreTextResponse>(cancellationToken: timeoutCts.Token);
            return result is null
                ? throw new AiCoreClientException("AiCore returned an unsupported response.")
                : FromTextResponse(result);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AiCoreClientException($"AiCore request timed out after {timeout.TotalSeconds:0} seconds. Increase PetroProcure:AI:AiCore:TimeoutSeconds or check AI Server Core response time.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiCoreClientException($"AiCore request failed because AI Server Core is unreachable or closed the connection: {ex.Message}");
        }
    }

    public async Task<AiChatResponse> SendChatAsync(AiChatRequest request, CancellationToken ct = default)
    {
        //var response = await SendAnalysisAsync(new AiCoreAnalysisRequest(Guid.NewGuid().ToString("N"),
        //    "PetroProcure", "Chat", Guid.Empty, "Chat", null,
        //    [new("system", request.SystemPrompt), new("user", request.UserPrompt)], new { }), ct);
        var response = await SendAnalysisAsync(new AiCoreAnalysisRequest(Guid.NewGuid().ToString("N"),
            "PetroProcure", "Chat", Guid.Empty, "Chat", null,
            [new("system", request.SystemPrompt), new("user", request.UserPrompt)], new { }), ct);
        return new AiChatResponse(response.Summary, "AiCore", "AiCore");
    }

    public async Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default)
    {
        var settings = await settingsProvider.GetAsync(ct);
        if (!settings.IsEnabled) return new("AiCore", false, "Disabled", DateTime.UtcNow, settings.DefaultModel);
        if (string.IsNullOrWhiteSpace(settings.BaseUrl)) return new("AiCore", false, "BaseUrlNotConfigured", DateTime.UtcNow, settings.DefaultModel);
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, BuildUrl(settings.BaseUrl, settings.HealthPath, "/health/ready"));
            ApplyHeaders(message, settings);
            using var response = await http.SendAsync(message, ct);
            return new("AiCore", response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                DateTime.UtcNow, settings.DefaultModel, response.IsSuccessStatusCode ? null : "AiCore health check failed.");
        }
        catch
        {
            return new("AiCore", false, "Unavailable", DateTime.UtcNow, settings.DefaultModel,
                "AiCore is not reachable. Check BaseUrl and network access.");
        }
    }

    // AI-RAG-11: raw text/chat passthrough. Unlike SendAnalysisAsync, this sends the supplied
    // system/user messages and JsonMode flag UNCHANGED (no analysis-JSON wrapping), so grounded
    // prompts and their citation contract reach AiCore intact.
    public async Task<AiCoreTextResponse> SendTextAsync(AiCoreTextRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var settings = await settingsProvider.GetAsync(ct);
        if (!settings.IsEnabled) throw new AiCoreClientException("AiCore provider is disabled.");
        if (string.IsNullOrWhiteSpace(settings.BaseUrl)) throw new AiCoreClientException("AiCore BaseUrl is not configured.");

        var payload = string.IsNullOrWhiteSpace(request.Model) ? request with { Model = settings.DefaultModel } : request;
        using var message = new HttpRequestMessage(HttpMethod.Post, BuildUrl(settings.BaseUrl, settings.AnalysisPath, "/api/ai/text"));
        ApplyHeaders(message, settings);
        var payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        message.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

        var timeout = TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 10, 900));
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);
        try
        {
            using var response = await http.SendAsync(message, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
                throw Friendly(response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AiCoreTextResponse>(cancellationToken: timeoutCts.Token);
            return result ?? throw new AiCoreClientException("AiCore returned an unsupported response.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new AiCoreClientException($"AiCore request timed out after {timeout.TotalSeconds:0} seconds. Increase PetroProcure:AI:AiCore:TimeoutSeconds or check AI Server Core response time.");
        }
        catch (HttpRequestException ex)
        {
            throw new AiCoreClientException($"AiCore request failed because AI Server Core is unreachable or closed the connection: {ex.Message}");
        }
    }

    private static void ApplyHeaders(HttpRequestMessage message, AiCoreSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            message.Headers.TryAddWithoutValidation("X-AI-API-KEY", settings.ApiKey);
            message.Headers.Authorization = new("Bearer", settings.ApiKey);
        }
        if (!string.IsNullOrWhiteSpace(settings.Tenant))
            message.Headers.TryAddWithoutValidation("X-Tenant", settings.Tenant);
        if (!string.IsNullOrWhiteSpace(settings.ClientId))
            message.Headers.TryAddWithoutValidation("X-Client-Id", settings.ClientId);
    }

    private static AiCoreTextRequest ToTextRequest(AiCoreAnalysisRequest request, AiCoreSettings settings)
    {
        var contextJson = JsonSerializer.Serialize(request.Context, new JsonSerializerOptions { WriteIndented = false });
        var userMessage = request.Messages.LastOrDefault(x => x.Role.Equals("user", StringComparison.OrdinalIgnoreCase))?.Content;
        var prompt = $$"""
        تحلیل تدارکات پالایشگاهی را برای موجودیت زیر انجام بده.
        خروجی فقط JSON معتبر مطابق این ساختار باشد:
        {
          "summary": "string",
          "riskLevel": "Info|Low|Medium|High|Critical",
          "findings": [
            {
              "title": "string",
              "description": "string",
              "severity": "Info|Low|Medium|High|Critical",
              "code": "optional",
              "evidence": "optional",
              "recommendation": "optional",
              "relatedRuleClauseId": null,
              "legalReference": "optional"
            }
          ],
          "recommendations": [
            {
              "title": "string",
              "description": "string",
              "severity": "Info|Low|Medium|High|Critical",
              "relatedAction": "optional"
            }
          ]
        }

        تمام مقادیر متنی خروجی (summary، title، description، recommendation، evidence و ...) باید به زبان فارسی روان نوشته شوند. فقط کلیدهای JSON به انگلیسی بمانند.

        نوع تحلیل: {{request.AnalysisType}}
        پرسش کاربر: {{userMessage ?? request.AnalysisType}}
        Context:
        {{contextJson}}
        """;

        return new AiCoreTextRequest(settings.DefaultModel, [
            new("system", "شما دستیار تحلیل تدارکات هستید. فقط JSON معتبر برگردان و همهٔ مقادیر متنی را به زبان فارسی بنویس. هرگز تصمیم نهایی کسب‌وکار نگیر. (You are an advisory procurement analysis assistant. Reply only with valid JSON and write all textual values in Persian/Farsi.)"),
            new("user", prompt)
        ], MaxTokens: settings.MaxOutputTokens, Stream: false, JsonMode: true,
        Metadata: new Dictionary<string, string>
        {
            ["sourceSystem"] = request.SourceSystem,
            ["entityType"] = request.EntityType,
            ["entityId"] = request.EntityId.ToString(),
            ["analysisType"] = request.AnalysisType,
            ["requestId"] = request.RequestId
        });
    }

    private static AiCoreAnalysisResponse FromTextResponse(AiCoreTextResponse response)
    {
        var content = StripJsonFence(response.Content);
        try
        {
            var parsed = JsonSerializer.Deserialize<AiCoreAnalysisResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (parsed is not null)
                return parsed with { Usage = new AiUsageDto(response.InputTokens, response.OutputTokens) };
        }
        catch (JsonException)
        {
        }

        return new AiCoreAnalysisResponse(response.Content, "Info", [], [],
            new AiUsageDto(response.InputTokens, response.OutputTokens));
    }

    private static string StripJsonFence(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;
        var firstLineEnd = trimmed.IndexOf('\n');
        if (firstLineEnd < 0) return trimmed;
        var withoutStart = trimmed[(firstLineEnd + 1)..].Trim();
        return withoutStart.EndsWith("```", StringComparison.Ordinal)
            ? withoutStart[..^3].Trim()
            : withoutStart;
    }

    private static string BuildUrl(string? baseUrl, string path, string defaultPath)
    {
        var normalizedPath = string.IsNullOrWhiteSpace(path) ? defaultPath : NormalizeLegacyPath(path.Trim(), defaultPath);
        if (Uri.TryCreate(normalizedPath, UriKind.Absolute, out _))
            return normalizedPath;
        return $"{baseUrl!.TrimEnd('/')}/{normalizedPath.TrimStart('/')}";
    }

    private static string NormalizeLegacyPath(string path, string defaultPath) => path switch
    {
        "/api/analysis" => "/api/ai/text",
        "api/analysis" => "api/ai/text",
        "/api/health" => defaultPath,
        "api/health" => defaultPath.TrimStart('/'),
        _ => path
    };

    private static AiCoreClientException Friendly(HttpStatusCode statusCode) => new(statusCode switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => "AiCore authentication failed. Check X-AI-API-KEY secret and client capabilities.",
        HttpStatusCode.NotFound => "AiCore endpoint was not found. For AI Server Core use AnalysisPath=/api/ai/text and HealthPath=/health/ready.",
        HttpStatusCode.RequestTimeout => "AiCore request timed out.",
        HttpStatusCode.TooManyRequests => "AiCore rate limit was reached. Please retry later.",
        _ => $"AiCore request failed with status {(int)statusCode}."
    });
}

public sealed class AiAnalysisService(
    IAiContextBuilder contextBuilder,
    IAiLegalRuleContextBuilder legalRuleContextBuilder,
    IAiCoreClient client,
    IAiAnalysisRepository repository,
    ICurrentUserService currentUser,
    IAiCoreSettingsProvider settingsProvider,
    IOptions<RagOptions> ragOptions) : IAiAnalysisService
{
    private const string Disclaimer = "تحلیل هوش مصنوعی صرفاً جنبه کمکی دارد و جایگزین تصمیم کارشناسی، حقوقی یا کمیسیون نیست.";

    public Task<AiAnalysisResultDto> AnalyzePurchaseFileAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default) =>
        Analyze("PurchaseFile", id, analysisType, userQuestion, () => contextBuilder.BuildPurchaseFileContextAsync(id, ct), ct);
    public Task<AiAnalysisResultDto> AnalyzeTenderAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default) =>
        Analyze("Tender", id, analysisType, userQuestion, () => contextBuilder.BuildTenderContextAsync(id, ct), ct);
    public Task<AiAnalysisResultDto> AnalyzeContractAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default) =>
        Analyze("Contract", id, analysisType, userQuestion, () => contextBuilder.BuildContractContextAsync(id, ct), ct);
    public Task<AiAnalysisResultDto> AnalyzePurchaseOrderAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default) =>
        Analyze("PurchaseOrder", id, analysisType, userQuestion, () => contextBuilder.BuildPurchaseOrderContextAsync(id, ct), ct);
    public Task<AiAnalysisResultDto> AnalyzeWarehouseReceiptAsync(Guid id, string analysisType, string? userQuestion, CancellationToken ct = default) =>
        Analyze("WarehouseReceipt", id, analysisType, userQuestion, () => contextBuilder.BuildWarehouseReceiptContextAsync(id, ct), ct);
    public Task<AiAnalysisResultDto> AnalyzeLegalComplianceAsync(string entityType, Guid id, string? appliesTo, string? userQuestion, CancellationToken ct = default) =>
        Analyze(entityType, id, "LegalCompliance", userQuestion, async () =>
        {
            var legal = await legalRuleContextBuilder.BuildLegalRuleContextAsync(entityType, id, appliesTo, ct);
            return new AiPromptContextDto(entityType, id, null, null,
                new AiProcurementEntityContextDto("تحلیل انطباق قانونی", null, new Dictionary<string, object?>()),
                legal);
        }, ct);

    private async Task<AiAnalysisResultDto> Analyze(string entityType, Guid entityId, string analysisType,
        string? userQuestion, Func<Task<AiPromptContextDto>> buildContext, CancellationToken ct)
    {
        var settings = await settingsProvider.GetAsync(ct);
        var context = await buildContext();
        var requestId = Guid.NewGuid().ToString("N");
        var request = new AiCoreAnalysisRequest(requestId, "PetroProcure", entityType, entityId, analysisType,
            settings.DefaultModel, [new("system", Disclaimer), new("user", userQuestion ?? analysisType)],
            context, new { advisoryOnly = true });
        var stopwatch = Stopwatch.StartNew();
        var log = new AiProviderRequestLog(Guid.NewGuid(), "AiCore", entityType, entityId, analysisType);
        var promptSummary = PromptSummary(entityType, analysisType, userQuestion);
        AiCoreAnalysisResponse response;
        try
        {
            response = await client.SendAnalysisAsync(request, CancellationToken.None);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            stopwatch.Stop();
            log.Fail(stopwatch.ElapsedMilliseconds, "AiCoreError", ex is AiCoreClientException ? ex.Message : "AiCore analysis failed.");
            var evaluation = new AiAnalysisEvaluation(Guid.NewGuid(), entityType, entityId, analysisType,
                "AiCore", settings.DefaultModel, "Failed", promptSummary, Disclaimer, "Unknown", currentUser.UserId);
            evaluation.Fail(ex is AiCoreClientException ? ex.Message : "AiCore analysis failed.");
            await repository.SaveAsync(evaluation, [], [], log, CancellationToken.None);
            throw;
        }

        stopwatch.Stop();
        var completedEvaluation = new AiAnalysisEvaluation(Guid.NewGuid(), entityType, entityId, analysisType,
            "AiCore", settings.DefaultModel, "Completed", promptSummary,
            $"{response.Summary}\n\n{Disclaimer}", string.IsNullOrWhiteSpace(response.RiskLevel) ? "Info" : response.RiskLevel,
            currentUser.UserId, JsonSerializer.Serialize(new { requestId }));
        completedEvaluation.Complete();
        var findings = response.Findings.Select(x => new AiAnalysisFinding(Guid.NewGuid(), completedEvaluation.Id,
            x.Severity, x.Title, x.Description, x.RelatedRuleClauseId, x.Evidence, x.Recommendation, x.LegalReference)).ToArray();
        var recommendations = response.Recommendations.Select(x => new AiAnalysisRecommendation(Guid.NewGuid(),
            completedEvaluation.Id, x.Severity, x.Title, x.Description, x.RelatedAction)).ToArray();
        log.Complete(stopwatch.ElapsedMilliseconds, response.Usage?.InputTokens, response.Usage?.OutputTokens, response.Usage?.Cost);
        await repository.SaveAsync(completedEvaluation, findings, recommendations, log, CancellationToken.None);
        return (await repository.GetByIdAsync(completedEvaluation.Id, CancellationToken.None))!;
    }

    private string PromptSummary(string entityType, string analysisType, string? userQuestion)
    {
        if (ragOptions.Value.EnableSensitivePromptLogging)
            return $"{entityType}:{analysisType}:{userQuestion}".TrimEnd(':');
        return $"{entityType}:{analysisType}:prompt-redacted";
    }
}
