# AiCore Provider Integration

AiCore is configured as an external AI analysis provider for PetroProcure and connects to the separate AI Server Core / Ai Core Platform.

AI output is advisory only. Findings, warnings, summaries, and recommendations never replace human approval, commission decisions, legal review, contract signature, purchase order issue, or warehouse receipt approval.

## Configuration keys

Non-secret provider settings can be maintained from `PetroProcure:AI:AiCore`, the `/admin/ai/providers` page, user-secrets, environment variables, or the production secret/configuration provider.

```json
{
  "PetroProcure": {
    "AI": {
      "AiCore": {
        "BaseUrl": "https://aicore.example",
        "ApiKeySecretName": "PETROPROCURE_AICORE_API_KEY",
        "AnalysisPath": "/api/ai/text",
        "HealthPath": "/health/ready",
        "DefaultModel": "aicore-default",
        "TimeoutSeconds": 60,
        "IsEnabled": true
      }
    }
  }
}
```

Important keys:

- `BaseUrl`: AI Server Core base URL.
- `AnalysisPath`: AI Server Core chat/analysis endpoint. Default: `/api/ai/text`.
- `HealthPath`: anonymous readiness endpoint. Default: `/health/ready`.
- `ApiKeySecretName`: name of the environment variable or secret reference that contains the real API key.
- `DefaultModel`: model name sent to AI Server Core.
- `TimeoutSeconds`, `MaxInputTokens`, `MaxOutputTokens`: provider request controls.
- `IsEnabled`: enables/disables live AiCore calls.

## Secret handling

Do not commit real API keys. Do not save raw AiCore keys in `SystemSettings`.

Store only the secret name, such as `PETROPROCURE_AICORE_API_KEY`, then provide the real secret through environment variables, `dotnet user-secrets`, or a production secret manager:

```powershell
$env:PETROPROCURE_AICORE_API_KEY = "<real-api-key>"
```

Development example:

```powershell
dotnet user-secrets set "PetroProcure:AI:AiCore:ApiKey" "<real-api-key>" --project src/PetroProcure.Api
```

Production example:

```powershell
$env:PetroProcure__AI__AiCore__ApiKeySecretName = "PETROPROCURE_AICORE_API_KEY"
$env:PETROPROCURE_AICORE_API_KEY = "<secret-from-vault>"
```

The API returns only `HasApiKey` and `ApiKeySecretName`; it never returns the raw API key. The Web UI must not display the raw key.

## Provider contract

PetroProcure sends structured procurement context to AI Server Core through `POST /api/ai/text`
using the AI Server Core `ChatRequestDto` contract and the `X-AI-API-KEY` header.

The prompt asks AI Server Core to return JSON that PetroProcure maps back into stored analysis results.

PetroProcure includes:

- entity lifecycle and status
- MESC-grouped items
- documents
- workflow timeline
- legal rule context and references

Expected analysis response:

- summary
- risk level
- findings
- recommendations
- optional token usage

## Supported analysis targets

PetroProcure API endpoints:

- `POST /api/ai/purchase-files/{purchaseFileId}/analyze`
- `POST /api/ai/tenders/{tenderId}/analyze`
- `POST /api/ai/contracts/{contractId}/analyze`
- `POST /api/ai/purchase-orders/{purchaseOrderId}/analyze`
- `POST /api/ai/warehouse-receipts/{receiptId}/analyze`
- `POST /api/ai/legal-compliance/analyze`
- `GET /api/ai/evaluations`

Provider management endpoints:

- `GET /api/ai/providers`
- `GET /api/ai/providers/aicore/settings`
- `PUT /api/ai/providers/aicore/settings`
- `POST /api/ai/providers/aicore/test`
- `GET /api/ai/providers/aicore/health`

AI Server Core endpoints used by PetroProcure:

- `POST /api/ai/text`
- `GET /health/ready`

## Advisory-only behavior

AI must not:

- approve or reject indents, tenders, contracts, purchase orders, or warehouse receipts;
- select a winning supplier;
- sign or finalize a contract;
- issue a purchase order;
- replace commission, legal, or expert decisions.

The required UI disclaimer is:

> تحلیل هوش مصنوعی صرفاً جنبه کمکی دارد و جایگزین تصمیم کارشناسی، حقوقی یا کمیسیون نیست.

## Security rules

- Do not log prompts, document contents, or API keys.
- Do not store AiCore API keys in `SystemSettings`.
- Store only the secret name, such as `PETROPROCURE_AICORE_API_KEY`.
- Send the key to AI Server Core with `X-AI-API-KEY`.
- AI Server Core stores API key hashes. The raw key must match an active `AiApiKey`
  whose `AiApiClient` is active. For chat calls, `POST /api/ai/text` requires a valid key.
  Admin-only endpoints such as `/api/management/providers/health` require the `admin`
  capability, so PetroProcure uses `/health/ready` as the default health check path.
- Keep evaluation history linked to the entity and provider/model used.
- Treat all AI output as advisory.

## Known limitations

- Deterministic rule evaluation exists, but full legal RAG/vector retrieval is not implemented yet.
- AiCore analysis is wired into Purchase File UI. Reusing `AiEvaluationPanel.razor` in Tender, Contract, Purchase Order, and Warehouse Receipt detail pages is intentionally left for Phase 38.2 if page-level UX needs adjustment.
- Provider request logs avoid raw sensitive content; deeper observability should be added with redaction.
- AiCore key rotation is handled outside PetroProcure by changing the referenced secret value.

## Future RAG/vector search plan

- Index legal documents, articles, clauses, tags, severity, and `AppliesTo` into a vector/search store.
- Retrieve the most relevant legal clauses per entity context before analysis.
- Keep every AI finding linked to the exact legal rule/version and clause used.
- Preserve old evaluations with the active rule version and source references used at evaluation time.
