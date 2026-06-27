# Phase AI-00 — AiCore Integration Discovery

Status: Discovery only. No business behavior changed in this phase.
Date: 2026-06-26
Sources inspected (actual code, not assumptions):
- PetroProcure: `E:\PetroProcure\src` (PetroProcure.AI, PetroProcure.Api/Endpoints/AiEndpoints.cs, PetroProcure.Worker, PetroProcure.Contracts/V1/Ai, persistence)
- AiCore: `E:\AI Server Core\src` (AiCore.Api, AiCore.Application, AiCore.Worker, AiCore.Shared, AiCore.Domain)

---

## 1. Executive summary

PetroProcure has **already built the full async adapter layer** for AiCore, including a job queue, a worker dispatcher, a `SubmitJob` client, callback/HMAC contracts, and three integration modes. Its **default mode is `AsyncAiCoreJob`**, which expects AiCore to (a) accept an async analysis-job submission at `POST /api/ai/jobs`, and (b) push results back to a PetroProcure webhook at `/api/ai/callbacks/aicore`.

**AiCore does not implement either capability.** AiCore today supports:
- Synchronous text/chat (`POST /api/ai/text`) — which PetroProcure’s `SyncAiCoreDirect` mode already uses correctly.
- Asynchronous jobs, but **only for media job types** (audio/vision/ocr/image), and even those are **not processed** — the AiCore worker only implements `EmbeddingBatch`. There is **no text/analysis job type** and **no callback/webhook** mechanism (jobs are poll-only).

Net effect: with the shipped default configuration (`Mode = AsyncAiCoreJob`), the integration cannot complete a round trip. The only working path today is `SyncAiCoreDirect`. The recommended path is to **keep PetroProcure’s adapter as-is**, switch the default to the working sync mode now, and implement the missing async-job + callback contract on the AiCore side as a separate, additive change.

---

## 2. Current PetroProcure AI flow

PetroProcure has **two parallel AI stacks** that both ultimately call AiCore:

### 2a. Legacy/local agent (in-process, no AiCore required)
- `PetroProcure.AI/AiServices.cs` — `AiAgentService` + `ProcurementRuleEvaluator` produce summaries, missing-document checks, and simple rule findings using an in-process `IAiChatProvider` (`MockAiProvider`, `OllamaProvider`, or `OpenAICompatibleProvider` in `Providers.cs`).
- Exposed via **obsolete, dev-only synchronous endpoints** in `AiEndpoints.cs` (`POST /api/ai/purchase-files/{id}/summarize|check-missing-documents|evaluate-rules`, `/analyze`). All are marked `[Obsolete]` and point callers to the job-based endpoints.

### 2b. AiCore analysis stack (the strategic path)
- **Context building:** `IAiContextBuilder` / `IPurchaseFileAiContextBuilder` build an `AiPromptContextDto` for PurchaseFile, Tender, Contract, PurchaseOrder, WarehouseReceipt, and legal-compliance.
- **Sync client:** `AiCoreClient` (`AiCoreClient.cs`) → `POST {BaseUrl}/api/ai/text` with `AiCoreTextRequest`, parses JSON-mode response into `AiCoreAnalysisResponse` (summary + findings + recommendations + usage). Health via `GET /health/ready`.
- **Async queue (PetroProcure-side):** `IAiJobQueueService` + domain `AiEvaluationJob`/`AiEvaluationResult` persist jobs and results in PetroProcure’s own database. Public endpoints in `AiEndpoints.cs`:
  - `POST /api/ai/jobs`, `GET /api/ai/jobs/{id}`, `GET /api/ai/jobs/{id}/result`, `POST /api/ai/jobs/{id}/cancel`
  - `GET /api/ai/entities/{entityType}/{entityId}/jobs`
  - `POST /api/ai/purchase-files/{id}/jobs/{summarize|check-missing-documents|evaluate-rules|analyze}` → returns `202 Accepted`.
- **Worker dispatcher:** `PetroProcure.Worker/AiJobProcessor.cs` claims queued jobs and branches on `AiCoreIntegrationOptions.Mode`:
  - `SyncAiCoreDirect` → calls `IAiCoreClient.SendAnalysisAsync` (AiCore `/api/ai/text`) inline and stores the result. **Working.**
  - `AsyncAiCoreJob` → calls `IAiCoreJobClient.SubmitJobAsync` (`POST /api/ai/jobs` on AiCore) with a `CallbackUrl`, then waits for AiCore to call back. **Depends on AiCore features that do not exist.**
  - `LocalOllamaDirect` → enum value exists but is not handled by the dispatcher (throws "not supported").

### 2c. Web UI
- `PetroProcure.Web/Components/Pages/Admin/Ai/AiProvidersPage.razor` (AiCore provider settings: BaseUrl, model, timeouts, enable, paths) and an `AiEvaluationPanel` plus the "دستیار هوشمند" tab in `PurchaseFileDetail.razor` (run analysis, list evaluations/findings/recommendations).

### 2d. Configuration (`appsettings.example.json`)
```jsonc
"AI": {
  "Provider": "Mock",
  "AiCore": {
    "Mode": "AsyncAiCoreJob",          // default → currently non-functional end-to-end
    "BaseUrl": "https://127.0.0.1:5203",
    "ApiKey": "", "ApiKeySecretName": "PETROPROCURE_AICORE_API_KEY",
    "CallbackSecret": "", "CallbackPublicUrl": "",
    "DefaultModel": "gemma3",
    "SubmitJobPath": "/api/ai/jobs", "SyncAnalysisPath": "/api/ai/text",
    "AnalysisPath": "/api/ai/text", "HealthPath": "/health/ready",
    "RequestTimeoutSeconds": 120, "TimeoutSeconds": 300,
    "MaxRetryCount": 3, "RetryDelaySeconds": 30, "CallbackTimestampToleranceSeconds": 300
  }
}
```

---

## 3. Current AiCore available endpoints

Host: `AiCore.Api/Program.cs`. Auth scheme `CoreApiKey`; controllers are `[Authorize]`; admin uses policy `Admin` (requires `capability=admin`).

### Client API — `AiController` (`/api/ai`, `[Authorize]`)
| Method & path | Request | Response | Sync/Async |
|---|---|---|---|
| `POST /api/ai/text` | `ChatRequestDto` | `ChatResponseDto` | **Sync** |
| `POST /api/ai/chat/stream` | `ChatRequestDto` | SSE `ChatStreamChunkDto` | Sync (streaming) |
| `POST /api/ai/embeddings` | `EmbeddingRequestDto` | `EmbeddingResponseDto` | **Sync** |
| `POST /api/ai/speech/transcriptions` | `AudioTranscriptionRequestDto` | `AiJobDto` | Async job |
| `POST /api/ai/speech/synthesis` | `TextToSpeechRequestDto` | `AiJobDto` | Async job |
| `POST /api/ai/vision` | `VisionAnalyzeRequestDto` | `AiJobDto` | Async job |
| `POST /api/ai/ocr` | `VisionAnalyzeRequestDto` | `AiJobDto` | Async job |
| `POST /api/ai/images/generations` | `ImageGenerationRequestDto` | `AiJobDto` | Async job |
| `POST /api/ai/images/edits` | `ImageEditRequestDto` | `AiJobDto` | Async job |
| `GET /api/ai/jobs/{id}` | – | `AiJobDto` | **Poll** |

> There is **no generic `POST /api/ai/jobs`** and **no text/analysis job type**. Async jobs are created only by the fixed media endpoints above, and the job result is retrieved by **polling** `GET /api/ai/jobs/{id}`.

### Management API — `ManagementController` (`/api/management`, `Admin`)
`GET providers`, `GET providers/health`, `GET models`, `GET usage`, `GET api-keys`.

### Admin compatibility API — `AdminCompatibilityController` (`/api/v1`, `Admin`)
Providers CRUD + test-health; models CRUD/capabilities/set-default; `GET usage|usage/recent|usage/summary`; `GET jobs`, `POST jobs/{id}/retry`, `POST jobs/{id}/cancel`; api-clients CRUD; api-keys list/create/deactivate; `GET files` (list only); `GET assets` (empty); `GET settings` (empty); `GET health/details`, `GET health/providers`.

### Health (anonymous)
`GET /health/live` (tag `live`), `GET /health/ready` (tags `ready`: database, file-storage, ai-providers, worker-heartbeat, job-backlog), `GET /health/details` (Admin).

---

## 4. Existing request/response contracts

### AiCore — sync text (matches what PetroProcure already sends)
`AiCore.Shared/Text/ChatDtos.cs`:
```csharp
ChatRequestDto  { string? Model; IReadOnlyCollection<ChatMessageDto> Messages; double? Temperature;
                  int? MaxTokens; bool Stream; bool JsonMode; Dictionary<string,string>? Metadata }
ChatMessageDto  { string Role; string Content; string? Name; Dictionary<string,string>? Metadata }
ChatResponseDto { string Model; string Content; string? FinishReason;
                  int InputTokens; int OutputTokens; int TotalTokens; long DurationMs }
```
PetroProcure’s mirror (`PetroProcure.AI/AiCoreContracts.cs` → `AiCoreTextRequest`/`AiCoreTextMessage`/`AiCoreTextResponse`) is **field-for-field compatible**. PetroProcure wraps the model’s JSON output into `AiCoreAnalysisResponse { Summary, RiskLevel, Findings[], Recommendations[], Usage }` (the prompt instructs the model to return that JSON).

### AiCore — jobs (media only)
`AiCore.Shared/Jobs/JobDtos.cs`:
```csharp
AiJobDto { Guid Id; string JobType; string Status; int Progress; Guid? InputFileId; Guid? OutputFileId;
           Dictionary<string,string>? Parameters; string? ErrorMessage; int RetryCount; int AttemptCount;
           DateTime? ClaimedAtUtc; string? ClaimedBy; DateTime? LockExpiresAtUtc; string? LastError;
           DateTime CreatedAtUtc; DateTime? StartedAtUtc; DateTime? CompletedAtUtc }
CreateJobRequestDto { string JobType; Guid? InputFileId; Dictionary<string,string>? Parameters }
UpdateJobRequestDto { string Status; int Progress; Guid? OutputFileId; string? ErrorMessage }
```
- `AiJobType` enum: `AudioTranscription, TextToSpeech, ImageGeneration, ImageEditing, FileProcessing, VideoProcessing, EmbeddingBatch` — **no text/analysis type**.
- `AiJobStatus` enum: `Pending, Queued, Processing, Completed, Failed, Cancelled`.

### PetroProcure — async adapter contract it already expects AiCore to honor
`PetroProcure.Contracts/V1/Ai/AsyncAiContracts.cs`:
```csharp
// PetroProcure → AiCore submission
AiCoreSubmitJobRequest { string CorrelationId; string SourceSystem; string EntityType; Guid EntityId;
                         string AnalysisType; string CallbackUrl; string? Model;
                         IReadOnlyList<AiCoreMessageDto> Messages; object? Context;
                         IReadOnlyDictionary<string,string>? Metadata }
AiCoreSubmitJobResponse { string ExternalJobId; string Status; string Message; DateTime AcceptedAtUtc }

// AiCore → PetroProcure callback (POST {CallbackPublicUrl}/api/ai/callbacks/aicore)
AiCoreCallbackRequest { string CorrelationId; string ExternalJobId; string Status; int ProgressPercent;
                        string? Message; AiCoreCallbackResultDto? Result; AiCoreCallbackErrorDto? Error;
                        AiUsageDto? Usage; DateTime? CompletedAtUtc }
AiCoreCallbackResultDto { string? Summary; IReadOnlyList<AiFindingDto> Findings;
                          IReadOnlyList<AiRecommendationDto> Recommendations; string? RawResultJson }
AiCoreCallbackErrorDto  { string Code; string Message; bool Retryable }
```
Submission is performed by `AiCoreJobClient.SubmitJobAsync` → `POST {BaseUrl}{SubmitJobPath=/api/ai/jobs}`.

---

## 5. Does AiCore support async job submission?

**Partially — and not for analysis/text.**
- Async job infrastructure exists and is solid: `AiJob` entity with claim/lock fields, `IJobService` (create/get/status/update/cancel/retry), `IJobClaimService.ClaimNextAsync` (worker lease with lock + heartbeat), `AiCore.Worker/JobWorker` polling loop.
- BUT: (1) jobs can only be created through the **fixed media endpoints**; there is no generic submit endpoint and no `TextAnalysis`/`Chat` job type. (2) The worker **only implements `EmbeddingBatch`** — `JobWorker.ExecuteJobAsync` throws `"Job type '<x>' is not supported by this worker."` for everything else. So even audio/vision/ocr/image jobs are accepted but never processed.

Conclusion: AiCore **cannot** currently satisfy PetroProcure’s `AsyncAiCoreJob` mode.

## 6. Does AiCore support callback / webhook?

**No.** There is no callback, webhook, or push notification anywhere in AiCore application code (grep across `E:\AI Server Core\src` returns only Blazor UI `EventCallback` and bundled bootstrap JS). Job completion is **poll-only** via `GET /api/ai/jobs/{id}`. AiCore has no concept of a caller-supplied `CallbackUrl` or `CorrelationId`.

## 7. Authentication style

- **AiCore:** API key in header **`X-AI-API-KEY`**, SHA-256 hashed and matched to `AiApiKey` → `AiApiClient`; client capabilities become `capability` claims. Admin endpoints require `capability=admin`. No JWT/OAuth/HMAC. (`CoreApiKeyAuthenticationHandler`, `Program.cs`.)
- **PetroProcure → AiCore:** both clients send `X-AI-API-KEY: <ApiKey>` **and** a redundant `Authorization: Bearer <ApiKey>` (AiCore ignores the bearer). PetroProcure also has a `CallbackSecret` (intended for HMAC-signed callbacks) and `CallbackTimestampToleranceSeconds`, but **no callback verification code exists yet** because there is no callback receiver.

## 8. Missing pieces

**On AiCore (to support PetroProcure’s default async mode):**
1. A generic **analysis/text job submission** endpoint, e.g. `POST /api/ai/jobs` accepting an analysis payload (messages + context + model) and returning an external job id — equivalent to `AiCoreSubmitJobRequest`/`AiCoreSubmitJobResponse`.
2. A new **`TextAnalysis` (or `ChatAnalysis`) job type** plus **worker handling** for it (the worker currently only does `EmbeddingBatch`).
3. A **callback/webhook dispatcher**: persist the caller’s `CallbackUrl` + `CorrelationId` on the job and POST results on completion/failure, ideally HMAC-signed (`X-Signature` over body + timestamp) using a shared secret.
4. Persisting/echoing `CorrelationId` and accepting arbitrary `Context`/`Metadata` on jobs.

**On PetroProcure (to make either mode fully work):**
5. **Callback receiver endpoint** `POST /api/ai/callbacks/aicore` is **not mapped** in `AiEndpoints.cs` (only the URL is built in `AiJobProcessor` and a config key exists). The async mode cannot complete without it.
6. HMAC verification of inbound callbacks using `CallbackSecret` + timestamp tolerance (contracts/config exist; logic does not).
7. The shipped **default mode (`AsyncAiCoreJob`) is non-functional**; it should not be the default until 1–6 exist.
8. Minor: `LocalOllamaDirect` mode is defined but unhandled by the worker dispatcher.

**General gaps:** AiCore has no file **upload** HTTP endpoint (jobs reference file IDs; `/api/v1/files` only lists), and no RAG/document-retrieval endpoint (only `embeddings` + `similarity` primitives exist).

## 9. Recommended integration approach

Because AiCore lacks the async-text-job and callback features, follow the task’s guidance: **PetroProcure keeps its adapter interface (`IAiCoreJobClient`) and integration modes; we make the working path the default now and add the async contract to AiCore as an additive, non-breaking change.**

**Step 1 — Make it work today (PetroProcure only, low risk):**
- Set default `AI:AiCore:Mode = SyncAiCoreDirect`. The worker’s `RunSyncFallbackAsync` already calls AiCore `/api/ai/text` and persists findings/recommendations. This delivers end-to-end AI analysis through PetroProcure’s own job queue **without any AiCore changes**.
- Keep `AsyncAiCoreJob` available behind config for when AiCore catches up.

**Step 2 — Define the shared async contract (the adapter is already the source of truth):**
- Adopt `AiCoreSubmitJobRequest`/`AiCoreSubmitJobResponse` and `AiCoreCallbackRequest` (Section 4) as the canonical async contract. Document the new AiCore endpoint `POST /api/ai/jobs` (analysis) + callback semantics + HMAC header.

**Step 3 — Implement AiCore side (separate phase, additive):**
- Add `AiJobType.TextAnalysis`; add `POST /api/ai/jobs` that creates such a job storing `messages`, `context`, `model`, `correlationId`, `callbackUrl`; extend the worker to run text analysis via `IChatService` and to POST the signed callback on completion/failure. Preserve existing media job behavior.

**Step 4 — Complete PetroProcure callback receiver:**
- Map `POST /api/ai/callbacks/aicore`, verify HMAC (`CallbackSecret`) + timestamp tolerance, then complete/fail the matching `AiEvaluationJob` by `CorrelationId` and store the `AiEvaluationResult`.

**Step 5 — Switch default to `AsyncAiCoreJob`** only after Steps 3–4 are verified end-to-end (long-running models then won’t hold HTTP connections open).

### Interim adapter note
No new adapter interface is required — PetroProcure already exposes the right seam (`IAiCoreClient` for sync, `IAiCoreJobClient` for async, selected by `AiCoreIntegrationMode`). The immediate, safe action is the **config default flip to `SyncAiCoreDirect`** plus implementing the **PetroProcure callback receiver** so the async path is ready the moment AiCore ships Step 3.

---

## 10. Quick reference — endpoint/contract alignment

| Concern | PetroProcure expects | AiCore provides | Gap |
|---|---|---|---|
| Sync analysis | `POST /api/ai/text` (`AiCoreTextRequest`) | `POST /api/ai/text` (`ChatRequestDto`) | **Aligned** |
| Health | `GET /health/ready` | `GET /health/ready` | **Aligned** |
| Auth | `X-AI-API-KEY` (+ redundant Bearer) | `X-AI-API-KEY` | Aligned (drop Bearer) |
| Async submit | `POST /api/ai/jobs` (`AiCoreSubmitJobRequest`) | none (media-only, fixed types) | **Missing on AiCore** |
| Text job type | `TextAnalysis`-style | media types only; worker = EmbeddingBatch only | **Missing on AiCore** |
| Result delivery | callback → `/api/ai/callbacks/aicore` | poll `GET /api/ai/jobs/{id}` only | **Missing both sides** |
| Callback receiver | should exist | n/a | **Missing on PetroProcure** |
| File upload | (not used by analysis) | no upload endpoint | Out of scope for now |
