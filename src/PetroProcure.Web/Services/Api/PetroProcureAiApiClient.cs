using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.Web.Services.Api;

/// <summary>
/// Dedicated, short-timeout client for the asynchronous AI job API. The Web layer only ever
/// creates jobs and polls status/result — it never calls long-running synchronous AI endpoints.
/// </summary>
public interface IPetroProcureAiApiClient
{
    Task<CreateAiJobResponse> CreateJobAsync(CreateAiJobRequest request, CancellationToken ct = default);
    Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct = default);
    Task<AiJobResultDto?> GetJobResultAsync(Guid jobId, CancellationToken ct = default);
    Task CancelJobAsync(Guid jobId, CancellationToken ct = default);
    Task<CreateAiJobResponse> CreatePurchaseFileSummaryJobAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<CreateAiJobResponse> CreateMissingDocumentsJobAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<CreateAiJobResponse> CreateRulesEvaluationJobAsync(Guid purchaseFileId, CancellationToken ct = default);
    Task<CreateAiJobResponse> CreateFullAnalysisJobAsync(Guid purchaseFileId, string? userQuestion = null, CancellationToken ct = default);
}

public sealed class PetroProcureAiApiClient(HttpClient http, AuthSession? session = null) : IPetroProcureAiApiClient
{
    public async Task<CreateAiJobResponse> CreateJobAsync(CreateAiJobRequest request, CancellationToken ct = default)
    {
        using var response = await Client().PostAsJsonAsync("/api/ai/jobs", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateAiJobResponse>(ct))!;
    }

    public async Task<AiJobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken ct = default)
    {
        using var response = await Client().GetAsync($"/api/ai/jobs/{jobId}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiJobStatusDto>(ct);
    }

    public async Task<AiJobResultDto?> GetJobResultAsync(Guid jobId, CancellationToken ct = default)
    {
        // The result endpoint returns 404 while the job is still pending; that is not an error.
        using var response = await Client().GetAsync($"/api/ai/jobs/{jobId}/result", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiJobResultDto>(ct);
    }

    public async Task CancelJobAsync(Guid jobId, CancellationToken ct = default)
    {
        using var response = await Client().PostAsync($"/api/ai/jobs/{jobId}/cancel", null, ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return;
        response.EnsureSuccessStatusCode();
    }

    public Task<CreateAiJobResponse> CreatePurchaseFileSummaryJobAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        PostShortcutAsync($"/api/ai/purchase-files/{purchaseFileId}/jobs/summarize", ct);

    public Task<CreateAiJobResponse> CreateMissingDocumentsJobAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        PostShortcutAsync($"/api/ai/purchase-files/{purchaseFileId}/jobs/check-missing-documents", ct);

    public Task<CreateAiJobResponse> CreateRulesEvaluationJobAsync(Guid purchaseFileId, CancellationToken ct = default) =>
        PostShortcutAsync($"/api/ai/purchase-files/{purchaseFileId}/jobs/evaluate-rules", ct);

    public async Task<CreateAiJobResponse> CreateFullAnalysisJobAsync(Guid purchaseFileId, string? userQuestion = null, CancellationToken ct = default)
    {
        var request = new AnalyzePurchaseFileRequest("RiskReview", userQuestion);
        using var response = await Client().PostAsJsonAsync($"/api/ai/purchase-files/{purchaseFileId}/jobs/analyze", request, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateAiJobResponse>(ct))!;
    }

    private async Task<CreateAiJobResponse> PostShortcutAsync(string url, CancellationToken ct)
    {
        using var response = await Client().PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreateAiJobResponse>(ct))!;
    }

    private HttpClient Client()
    {
        http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(session?.AccessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return http;
    }
}
