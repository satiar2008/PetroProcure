using PetroProcure.AI;
using PetroProcure.Application.Ai;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.UnitTests.AI;

public sealed class AiCoreGroundedAnswerGeneratorTests
{
    // AI-RAG-11: the grounded generator must send its grounding system prompt + citation contract
    // to AiCore UNCHANGED (raw text path, JsonMode=false) — not wrapped as analysis-JSON.
    [Fact]
    public async Task GenerateAnswer_SendsGroundingContractAndCitationsToAiCore()
    {
        var capture = new CapturingAiCoreClient("بر اساس متن بازیابی‌شده ضمانت‌نامه الزامی است [C1]");
        var generator = new AiCoreGroundedAnswerGenerator(capture, new FakeSettings());

        var prompt = new GroundedAiPrompt(
            "AskAboutFile",
            "شرایط ضمانت چیست؟",
            "PF-1405-001",
            [new GroundedAiPromptChunk("C1", Guid.NewGuid(), "LegalClause", Guid.NewGuid(),
                "ماده ۱", "/api/legal/clauses/x/context", "ارائه ضمانت‌نامه معتبر الزامی است.")]);

        var answer = await generator.GenerateAnswerAsync(prompt);

        Assert.NotNull(capture.LastRequest);
        // JsonMode must be false (cited prose, not analysis JSON).
        Assert.False(capture.LastRequest!.JsonMode);

        var system = capture.LastRequest.Messages.Single(m => m.Role == "system").Content;
        var user = capture.LastRequest.Messages.Single(m => m.Role == "user").Content;

        // Grounding contract reaches AiCore verbatim.
        Assert.Contains("only from the provided context", system, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[C1]", system, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("insufficient", system, StringComparison.OrdinalIgnoreCase);

        // The retrieved chunk and its citation id are present in the user prompt.
        Assert.Contains("[C1]", user, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ضمانت", user);

        // The model's own cited answer is returned raw (citation comes from the model, not appended later).
        Assert.Equal("بر اساس متن بازیابی‌شده ضمانت‌نامه الزامی است [C1]", answer);
    }

    private sealed class CapturingAiCoreClient(string content) : IAiCoreClient
    {
        public AiCoreTextRequest? LastRequest { get; private set; }

        public Task<AiCoreTextResponse> SendTextAsync(AiCoreTextRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            return Task.FromResult(new AiCoreTextResponse(request.Model ?? "model", content));
        }

        public Task<AiCoreAnalysisResponse> SendAnalysisAsync(AiCoreAnalysisRequest request, CancellationToken ct = default) =>
            throw new InvalidOperationException("Grounded generation must use the raw text path, not SendAnalysisAsync.");

        public Task<AiChatResponse> SendChatAsync(AiChatRequest request, CancellationToken ct = default) =>
            Task.FromResult(new AiChatResponse("chat", "AiCore", "model"));

        public Task<AiProviderHealthDto> GetHealthAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiProviderHealthDto("AiCore", true, "Healthy", DateTime.UtcNow, "model"));
    }

    private sealed class FakeSettings : IAiCoreSettingsProvider
    {
        public Task<AiCoreSettings> GetAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiCoreSettings("https://aicore.local", "secret", "PETROPROCURE_AICORE_API_KEY",
                "model", 60, 12000, 2000, true, false, null, null));
    }
}
