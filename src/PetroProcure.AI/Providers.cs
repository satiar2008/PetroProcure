using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PetroProcure.AI;

public sealed class MockAiProvider : IAiChatProvider
{
    public string Name => "Mock";
    public Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct = default) =>
        Task.FromResult(new AiChatResponse("خلاصه قطعی آزمایشی: پرونده بررسی شد. این خروجی صرفاً تحلیلی است و تصمیم نهایی با کاربران مجاز است.", Name, "mock-v1"));
}
public sealed class OllamaProvider(HttpClient http, IOptions<AiOptions> options) : IAiChatProvider
{
    public string Name => "Ollama";
    public async Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct = default)
    {
        var o = options.Value; using var response = await http.PostAsJsonAsync($"{o.OllamaBaseUrl.TrimEnd('/')}/api/generate", new { model = o.OllamaModel, prompt = $"{request.SystemPrompt}\n{request.UserPrompt}", stream = false }, ct);
        response.EnsureSuccessStatusCode(); var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return new(json.GetProperty("response").GetString() ?? "", Name, o.OllamaModel);
    }
}
public sealed class OpenAICompatibleProvider(HttpClient http, IOptions<AiOptions> options) : IAiChatProvider
{
    public string Name => "OpenAICompatible";
    public async Task<AiChatResponse> CompleteAsync(AiChatRequest request, CancellationToken ct = default)
    {
        var o = options.Value; using var message = new HttpRequestMessage(HttpMethod.Post, $"{o.OpenAiEndpoint.TrimEnd('/')}/chat/completions");
        if (!string.IsNullOrWhiteSpace(o.OpenAiApiKey)) message.Headers.Authorization = new("Bearer", o.OpenAiApiKey);
        message.Content = JsonContent.Create(new { model = o.OpenAiModel, messages = new[] { new { role = "system", content = request.SystemPrompt }, new { role = "user", content = request.UserPrompt } } });
        using var response = await http.SendAsync(message, ct); response.EnsureSuccessStatusCode(); var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return new(json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "", Name, o.OpenAiModel);
    }
}
