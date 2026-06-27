using System.Text;
using PetroProcure.Application.Ai;

namespace PetroProcure.AI;

public sealed class AiCoreGroundedAnswerGenerator(IAiCoreClient client, IAiCoreSettingsProvider settingsProvider)
    : IGroundedAiAnswerGenerator
{
    public async Task<string> GenerateAnswerAsync(GroundedAiPrompt prompt, CancellationToken ct = default)
    {
        var settings = await settingsProvider.GetAsync(ct);
        // AI-RAG-11: use the raw text path so the grounding system prompt + citation contract reach
        // AiCore unchanged. JsonMode is false — we want a cited prose answer, not analysis JSON.
        var response = await client.SendTextAsync(new AiCoreTextRequest(
            settings.DefaultModel,
            [
                new AiCoreTextMessage("system", SystemPrompt()),
                new AiCoreTextMessage("user", BuildPrompt(prompt))
            ],
            MaxTokens: settings.MaxOutputTokens,
            Stream: false,
            JsonMode: false,
            Metadata: new Dictionary<string, string>
            {
                ["sourceSystem"] = "PetroProcure",
                ["analysisType"] = prompt.AnalysisType,
                ["grounded"] = "true",
                ["citationRequired"] = "true"
            }),
            ct);

        return response.Content;
    }

    private static string SystemPrompt() => """
        You are PetroProcure's grounded procurement/legal assistant.
        Always write the entire answer in fluent Persian (Farsi). Do not answer in English.
        Answer only from the provided context chunks.
        Cite every factual claim using the supplied citation ids like [C1].
        If the provided context is insufficient, say in Persian that the available documents are insufficient.
        Do not use outside knowledge.
        پاسخ را کامل و فقط به زبان فارسی روان بنویس و برای هر ادعا citation مانند [C1] بیاور.
        """;

    private static string BuildPrompt(GroundedAiPrompt prompt)
    {
        var builder = new StringBuilder();
        builder.AppendLine("پرسش کاربر:");
        builder.AppendLine(prompt.Question);
        builder.AppendLine();
        builder.AppendLine("قاعده الزام‌آور: فقط از متن‌های زیر پاسخ بده و برای هر ادعا citation بیاور.");
        builder.AppendLine("اگر متن‌های زیر کافی نیستند، بگو: مدارک موجود برای پاسخ مستند کافی نیستند.");
        builder.AppendLine();
        if (!string.IsNullOrWhiteSpace(prompt.PurchaseFileContext))
        {
            builder.AppendLine("زمینه پرونده خرید:");
            builder.AppendLine(prompt.PurchaseFileContext);
            builder.AppendLine();
        }

        builder.AppendLine("متن‌های بازیابی‌شده:");
        foreach (var chunk in prompt.Chunks)
        {
            builder.AppendLine($"[{chunk.CitationId}] {chunk.Title} | {chunk.Reference}");
            builder.AppendLine(chunk.Text);
            builder.AppendLine();
        }

        builder.AppendLine("خروجی را کوتاه، فارسی، مستند و همراه citation ارائه کن.");
        return builder.ToString();
    }
}
