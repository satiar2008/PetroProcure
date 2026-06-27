# AiCore Contract

Date: 2026-06-26

## Purpose

This document defines the target PetroProcure-to-AiCore contract for asynchronous AI analysis. It is based on the discovery findings in `docs/ai/aicore-integration-discovery.md`.

Important finding from discovery: AiCore currently exposes synchronous text/chat through `POST /api/ai/text`, but does not yet expose async text analysis with callback. PetroProcure should introduce an adapter interface first and target the contract below for production integration.

## Current AiCore Text Contract

Current endpoint:

- `POST /api/ai/text`

Current request shape:

```json
{
  "model": "gemma3",
  "messages": [
    {
      "role": "system",
      "content": "You are an advisory procurement analysis assistant."
    },
    {
      "role": "user",
      "content": "Analyze this procurement context."
    }
  ],
  "temperature": null,
  "maxTokens": 2048,
  "stream": false,
  "jsonMode": true,
  "metadata": {
    "sourceSystem": "PetroProcure",
    "entityType": "PurchaseFile",
    "entityId": "guid",
    "analysisType": "Summary",
    "requestId": "correlation id"
  }
}
```

Current response shape:

```json
{
  "model": "gemma3",
  "content": "string",
  "finishReason": "stop",
  "inputTokens": 0,
  "outputTokens": 0,
  "totalTokens": 0,
  "durationMs": 0
}
```

This endpoint is synchronous and is not the production architecture for long-running PetroProcure analysis.

## Target Async Job Submission

Endpoint:

- `POST /api/ai/analysis-jobs`

Authentication:

- `X-AI-API-KEY: {api key}`
- Optional future bearer token may be supported, but `X-AI-API-KEY` is the known AiCore authentication style.

Request:

```json
{
  "externalJobId": "petroprocure-job-guid",
  "correlationId": "trace-or-request-correlation-id",
  "sourceSystem": "PetroProcure",
  "entityType": "PurchaseFile",
  "entityId": "entity-guid",
  "analysisType": "Summary",
  "model": "gemma3",
  "messages": [
    {
      "role": "system",
      "content": "You are an advisory procurement analysis assistant. Never make final business decisions."
    },
    {
      "role": "user",
      "content": "Analyze the supplied context and return advisory JSON."
    }
  ],
  "context": {
    "entity": {},
    "legalRules": []
  },
  "metadata": {
    "advisoryOnly": "true",
    "requestedByUserId": "user-guid",
    "tenant": "optional tenant"
  },
  "callback": {
    "url": "https://petroprocure-api.example.com/api/ai/aicore/callbacks",
    "authentication": {
      "scheme": "HMAC-SHA256",
      "keyId": "petroprocure-prod"
    }
  }
}
```

Response:

```json
{
  "jobId": "aicore-job-guid",
  "externalJobId": "petroprocure-job-guid",
  "correlationId": "trace-or-request-correlation-id",
  "status": "Queued",
  "acceptedAtUtc": "2026-06-26T00:00:00Z",
  "estimatedCompletionSeconds": null
}
```

Required response semantics:

- AiCore must return quickly after accepting the job.
- AiCore must not block until analysis is complete.
- `externalJobId` must echo the PetroProcure job id.
- `correlationId` must echo the request correlation id.
- `jobId` is the AiCore provider-side id stored by PetroProcure.

## Target Callback Contract

Callback endpoint in PetroProcure:

- `POST /api/ai/aicore/callbacks`

Callback request:

```json
{
  "jobId": "aicore-job-guid",
  "externalJobId": "petroprocure-job-guid",
  "correlationId": "trace-or-request-correlation-id",
  "status": "Completed",
  "completedAtUtc": "2026-06-26T00:00:00Z",
  "result": {
    "summary": "Advisory summary.",
    "riskLevel": "Info",
    "findings": [
      {
        "title": "Finding title",
        "description": "Finding description",
        "severity": "Medium",
        "code": "OPTIONAL_CODE",
        "evidence": "Optional evidence.",
        "recommendation": "Optional recommendation.",
        "relatedRuleClauseId": null,
        "legalReference": "Optional legal reference."
      }
    ],
    "recommendations": [
      {
        "title": "Recommendation title",
        "description": "Recommendation description",
        "severity": "Low",
        "relatedAction": "Optional workflow hint."
      }
    ],
    "usage": {
      "inputTokens": 0,
      "outputTokens": 0,
      "totalTokens": 0,
      "cost": null,
      "durationMs": 0
    },
    "rawResponseMetadata": {}
  },
  "error": null
}
```

Failed callback request:

```json
{
  "jobId": "aicore-job-guid",
  "externalJobId": "petroprocure-job-guid",
  "correlationId": "trace-or-request-correlation-id",
  "status": "Failed",
  "completedAtUtc": "2026-06-26T00:00:00Z",
  "result": null,
  "error": {
    "code": "ProviderError",
    "message": "Safe error message.",
    "retryable": false
  }
}
```

Required callback headers:

- `X-AiCore-Key-Id`
- `X-AiCore-Timestamp`
- `X-AiCore-Signature`
- `X-AiCore-Delivery-Id`

Signature payload:

```text
{timestamp}.{raw_request_body}
```

Signature algorithm:

- `HMACSHA256(sharedSecret, signaturePayload)`
- Encoded as lowercase hex or base64. The selected encoding must be configured consistently.

## Status Mapping

AiCore target statuses should map to PetroProcure job statuses.

| AiCore status | PetroProcure status |
| --- | --- |
| Queued | SubmittedToAiCore |
| Processing | Running |
| Completed | Completed |
| Failed | Failed |
| Cancelled | Cancelled |
| Expired | Expired |

PetroProcure has additional pre-submission statuses such as `Claimed`, `BuildingContext`, and `SendingToAiCore`.

## Advisory Result Rules

AiCore results must remain advisory. Result payloads may contain:

- `summary`
- `riskLevel`
- `findings`
- `recommendations`
- `usage`
- `rawResponseMetadata`

Result payloads must not contain instructions that imply final approval, final rejection, supplier award, or legally binding decision.

## Adapter Boundary

PetroProcure should depend on an adapter interface rather than hard-coding this contract across API, Worker, and UI code.

Suggested interface shape:

```csharp
public interface IAiCoreAnalysisJobClient
{
    Task<AiCoreSubmitJobResponse> SubmitAsync(
        AiCoreSubmitJobRequest request,
        CancellationToken cancellationToken);
}
```

The adapter implementation owns:

- AiCore URL building
- authentication headers
- serialization
- timeout policy
- transient retry policy
- mapping AiCore submit response to PetroProcure provider fields

## Versioning

The async contract should be versioned before production rollout.

Recommended options:

- URL version: `/api/v1/ai/analysis-jobs`
- Header version: `X-AiCore-Contract-Version: 1`

PetroProcure should store the contract version used for each submitted job for future audit and migration.

