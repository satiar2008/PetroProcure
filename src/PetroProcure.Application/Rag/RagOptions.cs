namespace PetroProcure.Application.Rag;

// AI-RAG-04: RAG settings. Pins the embedding model so the corpus stays consistent and a model
// change is detectable (re-embedding is triggered when the stored model differs).
public sealed class RagOptions
{
    public const string SectionName = "PetroProcure:AI:Rag";

    // Embedding model id sent to AiCore /api/ai/embeddings. Falls back to AiCore's default when empty.
    public string EmbeddingModel { get; set; } = "nomic-embed-text";

    // Inputs per embedding request to AiCore.
    public int EmbeddingBatchSize { get; set; } = 16;

    // Default number of results returned by a corpus query.
    public int DefaultTopK { get; set; } = 5;

    // Cache query embeddings only for the current process. Disabled by default for conservative deployments.
    public bool EnableQueryEmbeddingCache { get; set; }

    public int QueryEmbeddingCacheMinutes { get; set; } = 15;

    // Prompt bodies are sensitive. Keep false unless an administrator explicitly enables full prompt logging.
    public bool EnableSensitivePromptLogging { get; set; }
}
