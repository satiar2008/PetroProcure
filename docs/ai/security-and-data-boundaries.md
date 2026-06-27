# Security And Data Boundaries

RAG and AI features must follow least disclosure.

Rules:
- Retrieval must enforce user permissions before returning chunks.
- Purchase-file retrieval must never return chunks from another file.
- Grounded AI prompts include only the retrieved chunks and necessary purchase-file context.
- Do not log sensitive prompt text by default.

Settings:
- `PetroProcure:AI:Rag:EmbeddingModel`
- `PetroProcure:AI:Rag:EmbeddingBatchSize`
- `PetroProcure:AI:Rag:EnableQueryEmbeddingCache`
- `PetroProcure:AI:Rag:QueryEmbeddingCacheMinutes`
- `PetroProcure:AI:Rag:EnableSensitivePromptLogging`

Prompt logging:
- Default: store a non-sensitive prompt summary such as `PurchaseFile:Summary:prompt-redacted`.
- Opt-in: when `EnableSensitivePromptLogging` is true, raw user question text may be included in the prompt summary. Use only in controlled environments.

Quality evaluation:
- Admins can run `POST /api/admin/rag/evaluate-quality` with golden questions.
- Metrics include precision@k, recall@k, citation accuracy, and no-answer correctness.

