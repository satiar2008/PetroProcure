# AI Security and Callback

Date: 2026-06-26

## Purpose

This document defines security requirements for PetroProcure and AiCore asynchronous AI integration.

Security goals:

- Authenticate PetroProcure requests to AiCore.
- Authenticate AiCore callbacks to PetroProcure.
- Prevent replay attacks.
- Preserve correlation and auditability.
- Keep AI output advisory and under human responsibility.

## PetroProcure to AiCore Authentication

Supported modes:

- API key in `X-AI-API-KEY`
- Bearer token, if AiCore adds or standardizes bearer support later

Known AiCore behavior from discovery:

- AiCore reads `X-AI-API-KEY`.
- The API key is SHA-256 hashed and matched against active AiCore API keys.
- The related AiCore API client must be active.

PetroProcure should store the secret reference, not the raw key, in normal database settings.

Recommended PetroProcure settings:

- `AiCore:BaseUrl`
- `AiCore:SubmitJobPath`
- `AiCore:HealthPath`
- `AiCore:ApiKeySecretName`
- `AiCore:ClientId`
- `AiCore:Tenant`
- `AiCore:TimeoutSeconds`
- `AiCore:CallbackUrl`
- `AiCore:CallbackKeyId`
- `AiCore:ContractVersion`

## AiCore to PetroProcure Callback Authentication

Callbacks must target `PetroProcure.Api`, never `PetroProcure.Web`.

Callback endpoint:

- `POST /api/ai/aicore/callbacks`

Required callback headers:

- `X-AiCore-Key-Id`
- `X-AiCore-Timestamp`
- `X-AiCore-Signature`
- `X-AiCore-Delivery-Id`

Recommended signature algorithm:

- HMAC-SHA256

Signature input:

```text
{timestamp}.{raw_request_body}
```

Signature validation:

1. Resolve shared secret by `X-AiCore-Key-Id`.
2. Parse `X-AiCore-Timestamp`.
3. Reject if timestamp is outside tolerance.
4. Recompute HMAC over timestamp and raw body.
5. Compare signatures with constant-time comparison.
6. Check replay cache or persisted delivery id.
7. Validate payload schema and job correlation.
8. Persist result idempotently.

## Timestamp Tolerance

Recommended default tolerance:

- 5 minutes

Rules:

- Reject callbacks with missing timestamp.
- Reject callbacks too far in the past.
- Reject callbacks too far in the future.
- Store rejected attempts in audit logs without storing sensitive payloads.

The tolerance should be configurable for clock-skew handling in development and production.

## Replay Protection

Replay protection should use:

- `X-AiCore-Delivery-Id`
- `externalJobId`
- payload hash
- timestamp

Rules:

- A delivery id can be processed only once.
- Repeated delivery id with identical payload can return success for idempotency.
- Repeated delivery id with different payload must be rejected and audited.
- A completed job must not accept a different result payload.

Replay records should expire after a configurable retention period, but not before the maximum callback retry window.

## Correlation

Every async AI request must carry:

- `correlationId`
- `externalJobId`
- `entityType`
- `entityId`
- `analysisType`

Definitions:

- `externalJobId`: PetroProcure `AiEvaluationJob.Id`, sent to AiCore and returned in callbacks.
- `correlationId`: trace id shared across logs, provider request logs, callbacks, and UI troubleshooting.
- `AiCoreJobId`: provider-side id returned by AiCore at submission.

Callbacks must be rejected if:

- `externalJobId` does not exist.
- `AiCoreJobId` does not match the submitted provider job id, when known.
- Entity fields conflict with the stored job.
- The job is terminal and payload is conflicting.

## Idempotent Callback Handling

Callback handling must be transactional.

Required behavior:

- Validate signature before deserializing sensitive payload into business objects.
- Load the job by `externalJobId`.
- Verify current job status allows callback.
- Save result, findings, recommendations, usage, and provider metadata.
- Update job status to `Completed` or `Failed`.
- Store callback delivery audit.
- Commit once.

Duplicate successful callback:

- Return success if the job is already completed and payload hash matches.
- Do not insert duplicate findings or recommendations.

Duplicate failed callback:

- Return success if the failure was already recorded with the same payload hash.
- Do not reset a completed job to failed.

## Audit Log

Audit logging should record:

- Job created
- Job claimed
- Context building started/completed
- Submission to AiCore started/completed/failed
- AiCore job id received
- Callback received
- Callback signature validation result
- Replay validation result
- Result persisted
- Job completed/failed/cancelled/expired

Audit entries should include:

- `jobId`
- `externalJobId`
- `correlationId`
- `AiCoreJobId`
- `deliveryId`
- `actor`
- `eventType`
- `statusBefore`
- `statusAfter`
- `errorCode`
- `safeErrorMessage`
- `createdAtUtc`

Do not store raw API keys, HMAC secrets, or full sensitive prompt payloads in audit logs.

## Authorization

User authorization must happen when the job is created.

The callback is service-to-service and should not run as a human user. It should be authorized through callback authentication and linked to the original requesting user for audit.

Workers should run as service principals or system actors in audit logs.

## Data Protection

Prompts and context may contain procurement-sensitive information.

Rules:

- Send only required context to AiCore.
- Avoid raw document content unless the specific analysis requires it.
- Respect file and entity access rules before creating a job.
- Store prompt summaries rather than full prompts where possible.
- Encrypt secrets outside normal application tables.
- Redact sensitive values from logs.

## Failure Handling

Rejected callbacks should return appropriate status codes:

- `401 Unauthorized`: unknown key id or invalid authentication token.
- `400 Bad Request`: malformed payload.
- `408 Request Timeout` or `400 Bad Request`: timestamp outside tolerance, depending on API convention.
- `409 Conflict`: conflicting terminal job state or payload mismatch.
- `202 Accepted` or `200 OK`: duplicate callback already processed successfully.

Callback failures must be auditable and should not expose secrets in error responses.

