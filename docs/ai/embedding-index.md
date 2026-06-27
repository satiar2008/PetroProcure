# Embedding Index

The current index is a brute-force cosine implementation over persisted `AiEmbedding` vectors and `AiDocumentChunk` rows.

Key interfaces:
- `IEmbeddingIndex`
- `IAiEmbeddingRepository`
- `IEmbeddingGenerator`
- `IRagMaintenanceService`

Cost controls:
- Embeddings are generated in batches where supported.
- Existing embeddings are skipped when model and content hash are unchanged.
- The embedding model is pinned by `PetroProcure:AI:Rag:EmbeddingModel`.
- Query embedding cache is optional and disabled by default.

Operational commands:
- `GET /api/admin/rag/embedding-model-status`
- `POST /api/admin/rag/reindex`

When the pinned model changes, `embedding-model-status` reports mismatches. Run reindex to regenerate vectors with the new model.

