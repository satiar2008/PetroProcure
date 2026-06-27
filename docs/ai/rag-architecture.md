# RAG Architecture

The RAG path is implemented inside the modular monolith.

Flow:
1. Source content is extracted from legal clauses or purchase-file documents.
2. `IChunkingService` creates `AiDocumentChunk` records with source ids, purchase file scope, content hash, metadata, and deletion state.
3. `IEmbeddingGenerator` calls AiCore `/api/ai/embeddings`.
4. `IEmbeddingIndex` stores vectors behind a replaceable interface.
5. `IRagRetriever` applies scope and user access checks before returning ranked chunks and citations.
6. Grounded AI analysis receives only retrieved chunks and must answer from that context.

Supported scopes:
- Legal corpus
- A single purchase file
- Tender context
- All allowed sources

Security boundary:
- Purchase-file chunks are filtered by the user access scope before search results are returned.
- Legal corpus requires AI/legal-rule/legal-document permissions.
- Cross-file retrieval is blocked by `IRagAccessDataSource` and `RagRetriever`.

