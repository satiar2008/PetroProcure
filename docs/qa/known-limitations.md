# Known Limitations & Production-Readiness Risks (Phase 39)

## 0. Validation status (must read)
Phase 39.1 re-established the local build/test baseline on 2026-06-26:
- `dotnet restore PetroProcure.sln` succeeded.
- `dotnet build PetroProcure.sln --no-restore` succeeded with 0 warnings / 0 errors.
- Final `dotnet test PetroProcure.sln --no-build` succeeded with 308 passing tests: 227 Unit, 69 Integration, 12 Architecture.
- `GET /api/purchase-files/{id}/lifecycle` and the Web «گردش کامل خرید» tab were added, with a focused integration regression test passing.

Action: run the final full-suite validation after each additional Phase 39.1 regression file and record exact totals in `regression-test-matrix.md`.

## 1. Intentionally NOT implemented (out of Phase 39 scope, by instruction)
- Finance, Payment, Accounting, Invoice Matching.
- Advanced RAG / vector search.
- Stock reversal / adjustment, full inventory issue/reservation workflow.
- Advanced delivery tracking, electronic signature.
- New major business modules.

## 2. AI integration limitations (carried from AI-00..AI-09)
- Default `AI:AiCore:Mode` is `AsyncAiCoreJob`, but AiCore does **not** yet expose an async text-analysis job endpoint, a `TextAnalysis` job type, or an outbound signed callback. Until AiCore implements these (AI-11), only `SyncAiCoreDirect` completes end-to-end. **Recommendation:** keep the deployed default at `SyncAiCoreDirect` for QA.
- `LocalOllamaDirect` worker mode is declared but not implemented (throws “not supported”).
- `ProcurementRuleEvaluator` (AI side) is foundational; the deterministic legal-rule engine has a small condition set (~6 condition types) — see `docs/ai/ai-rules-and-rag-plan.md` for the deepening plan.
- `AiCoreClient` (sync path) has a timeout bug: a timeout `CancellationTokenSource` is created but `CancellationToken.None` is passed to `SendAsync`, so the timeout is never applied. Fix before production.

## 3. Data correctness risks
- `FakeDirectory` in the Web layer still hardcodes department/unit GUIDs and is used in `PurchaseFileDetail` (unit mapping on add-item, department names). Replace with `LookupCacheService`/API to avoid mismatches against real data.

## 4. Infrastructure placeholders
- `PetroProcure.Worker/Worker.cs` orphan-file cleanup is a placeholder.
- `Infrastructure/Storage/FileScanning.cs` antivirus/reconcile is a placeholder; uploads are not malware-scanned.

## 5. Production-readiness risks to track
- **SignalR scale-out:** the AI job hub works single-instance only; multi-instance needs a backplane (e.g. Redis).
- **Callback hardening:** if neither AiCore `ApiKey` nor `CallbackSecret` is configured, the callback endpoint currently accepts requests (dev posture). Require at least one in production; add rate limiting. (A replay-protection cache was added to the callback authenticator.)
- **Secrets:** ensure JWT signing key, bootstrap admin password, and AiCore keys come from a secret manager, not committed config.
- **Reports/documents:** confirm downloads stream (not buffered fully) and require authorization; confirm generated report file names are sanitized and stored as relative paths with hashes.
- **Paging:** confirm all major list endpoints enforce paging and search endpoints are bounded (no unbounded result sets).
- **Lifecycle endpoint:** implemented with lightweight projections. Remaining risk is breadth: the current regression covers the endpoint shape/authorization; the full end-to-end test still needs to assert every related stage after a complete procurement chain.
- **README** is stale (lists implemented modules as “not implemented”); update before release.

## 6. UI gaps to verify in the consistency pass
PageHeader / Breadcrumbs / DataStateView / RetryPanel / DateDisplay presence; no raw GUIDs; permission-gated buttons hidden correctly; empty-enum query strings (`Status=`) don’t crash; forms guard against double-submit; validation messages present; navigation refreshes after panel switch; friendly Persian empty states; consistent Persian labels.

## 7. Recommended order to reach green
1. Author the remaining regression suites incrementally, compiling after each, verifying each route/DTO against `*Endpoints.cs`.
2. Broaden the lifecycle integration coverage to assert Inquiry, Tender, Commission, Contract, PurchaseOrder, WarehouseReceipt, Documents, Legal, and AI counts after a full happy path.
3. Run full `dotnet test`; record exact counts; close the gaps listed in the matrix.
4. Do the UI consistency pass; re-run Web UI tests.
