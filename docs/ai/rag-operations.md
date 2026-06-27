# RAG Operations Guide

Operational notes for the PetroProcure RAG subsystem (AI-RAG-04…13). This is the source of truth
for the difference between **building the corpus** and **reindexing**, and for the planned migration
away from the brute-force index.

## Build initial corpus vs. reindex existing chunks

These are two distinct operations. Confusing them leaves retrieval empty.

**Build initial corpus** (chunk + embed from sources)
- Turns raw sources (legal clauses, purchase-file / tender / contract / report documents) into
  `AiDocumentChunk` rows and then embeds them.
- Driven by `IRagIngestionQueue.EnqueueAsync(...)`, processed by the Worker
  (`AiJobProcessor` → `RagIngestionService.IngestAsync` → `ChunkingService.RebuildChunksAsync`).
- Happens automatically going forward on the relevant events:
  - legal clause created / rule version approved (`LegalRuleModule`),
  - document uploaded (`FileStorageService`).
- For an existing/migrated database that predates RAG, there are **no chunks yet**, so reindex does
  nothing useful until a corpus build has run at least once.

**Reindex existing chunks** (re-embed only)
- `RagMaintenanceService.ReindexAsync(force)` and `POST /api/admin/rag/reindex`.
- Re-embeds chunks that ALREADY exist — e.g. after changing `PetroProcure:AI:Rag:EmbeddingModel`.
- It does **not** read sources or create chunks.

### TODO — initial corpus build command (not yet implemented)

A one-shot/administrative "build corpus" entry point is still needed. Suggested plan:

1. Add `IRagCorpusBuilder.BuildAllAsync(force)` in Application that enumerates each source:
   - all `LegalClause` ids,
   - all non-deleted `FileDocument` ids (with their `AiDocumentSourceType`),
   - tender/contract document ids,
   and calls `IRagIngestionQueue.EnqueueAsync(...)` for each (idempotent: ingestion already skips
   unchanged content by hash + model).
2. Expose `POST /api/admin/rag/build-corpus` (permission `Ai.Admin`) that triggers it.
3. Optionally a CLI/worker maintenance task for first-time bootstrap.
4. After the corpus build completes, `GET /api/admin/rag/embedding-model-status` should report
   `MismatchedModelCount = 0`.

Until this exists, bootstrap a fresh environment by re-saving/re-uploading sources or by enqueuing
ingestion manually.

## Embedding model

- Pinned via `PetroProcure:AI:Rag:EmbeddingModel` (default `nomic-embed-text`).
- A model change is detectable: `GetEmbeddingModelStatusAsync` reports mismatches; run reindex
  (`force` if needed) to converge. The stored `AiEmbedding.Model` + `ContentHash` drive skip/re-embed.

## Index implementation and future migration

- The current store is `BruteForceEmbeddingIndex`: it loads candidate vectors (filtered by source
  type / model / purchase file / access) and scores them with cosine similarity in memory.
- This is fine for the initial corpus size but **O(n)** per query and will not scale.
- It is deliberately hidden behind `IEmbeddingIndex` so it can be swapped without touching the
  ingestion/retrieval/grounding services.
- **Planned migration:** replace with SQL Server 2025 native `VECTOR` columns/`VECTOR_DISTANCE`, or an
  external vector DB (Qdrant), implementing the same `IEmbeddingIndex` contract. Search filters
  (source type, model+dimensions, purchase-file scope, access list, tags, appliesTo) must be preserved.

## Single ingestion path (post-stabilization)

There is exactly **one** production embedding-ingestion path:
`ChunkingService` + `CorpusSourceTextProvider` → `RagIngestionService` / `RagMaintenanceService`,
all using `ChunkingService`'s content hash. The earlier `LegalClauseEmbeddingService` (a second,
inconsistent path) was removed during stabilization.
