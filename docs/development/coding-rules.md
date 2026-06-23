# Coding Rules

- Keep business invariants inside domain entities and application handlers.
- Use immutable DTO records for API and query results.
- Persist historical MESC descriptions as snapshots in Indent and Purchase File items.
- Store only relative file paths; all physical path resolution belongs to `IFileStorageService`.
- Use cancellation tokens for database, HTTP, file, reporting, and AI operations.
- Treat AI output as advisory. It must never approve, reject, award, or finalize procurement decisions.
- Add tests for every numbering rule, workflow transition, snapshot rule, and access restriction.
- Do not expose secrets, absolute storage paths, or internal exception details through APIs.
- Preserve Persian labels and RTL behavior in official UI and reports.
- Use deterministic IDs and timestamps for seed data so migrations remain stable.
