// AI-RAG stabilization: LegalClauseEmbeddingService and ILegalClauseEmbeddingSource were removed.
// The single production embedding-ingestion path is now ChunkingService + CorpusSourceTextProvider
// (AI-RAG-05) driven by RagIngestionService / RagMaintenanceService (AI-RAG-07), which use a
// consistent content-hash. This file is intentionally empty and can be deleted from the project.
