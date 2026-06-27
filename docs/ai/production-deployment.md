# PetroProcure AI Production Deployment

## Architecture

Production AI analysis must run through the async job pipeline:

1. PetroProcure.Web calls PetroProcure.Api.
2. PetroProcure.Api creates an `AiEvaluationJob` and returns `202 Accepted`.
3. PetroProcure.Worker claims queued jobs from the database.
4. PetroProcure.Worker submits the job to AiCore.
5. AiCore processes the analysis asynchronously.
6. AiCore calls `POST /api/ai/callbacks/aicore` on PetroProcure.Api.
7. PetroProcure.Api validates the callback, stores the advisory result, and updates job status.
8. PetroProcure.Web shows status through polling and SignalR notifications.

The callback endpoint must target PetroProcure.Api, never PetroProcure.Web.

## Required Configuration

Configure these values from environment variables, user-secrets, or the production secret store. Do not put production secrets in `appsettings.json`.

- `PetroProcure:AI:AiCore:Mode=AsyncAiCoreJob`
- `PetroProcure:AI:AiCore:BaseUrl=https://aicore.example`
- `PetroProcure:AI:AiCore:SubmitJobPath=/api/ai/jobs`
- `PetroProcure:AI:AiCore:HealthPath=/health/ready`
- `PetroProcure:AI:AiCore:CallbackPublicUrl=https://petroprocure.example/api/ai/callbacks/aicore`
- `PetroProcure:AI:AiCore:DefaultModel=<approved model>`
- `PetroProcure:AI:AiCore:ApiKey=<secret>`
- `PetroProcure:AI:AiCore:CallbackSecret=<secret>`
- `PetroProcure:AI:AiCore:RequestTimeoutSeconds=120`
- `PetroProcure:AI:AiCore:WorkerBatchSize=5`
- `PetroProcure:AI:AiCore:MaxRetryCount=3`
- `PetroProcure:AI:AiCore:RetryDelaySeconds=30`
- `PetroProcure:AI:AiCore:CallbackTimestampToleranceSeconds=300`
- `PetroProcure:AI:AiCore:StuckJobTimeoutMinutes=15`
- `PetroProcure:AI:AiCore:RunningJobTimeoutMinutes=120`
- `PetroProcure:AI:AiCore:CompletedJobRetentionDays=180`
- `PetroProcure:AI:AiCore:CallbackAllowedIpAddresses:0=<optional AiCore egress IP>`

## Modes

- `AsyncAiCoreJob`: production default. Worker submits to AiCore and waits for callback.
- `LocalOllamaWorker`: development fallback. Worker processes through local Ollama and stores results in the same AI tables.
- `SyncAiCoreDirect`: debug only. Do not use for production workflows.

## Security

- PetroProcure to AiCore authentication uses the configured API key or bearer token.
- AiCore to PetroProcure callback authentication uses the same callback API key plus optional HMAC.
- HMAC signatures cover `{timestamp}.{rawBody}` when `X-AiCore-Timestamp` is present.
- Timestamp tolerance prevents stale callbacks.
- Replay protection rejects duplicate signatures within the tolerance window.
- Optional IP allowlist can restrict callbacks to known AiCore egress addresses.
- Logs must not include API keys, callback secrets, bearer tokens, or raw AI result JSON.

## Operations

- Run PetroProcure.Api and PetroProcure.Worker as separate production services.
- Scale Worker instances horizontally; database claiming prevents duplicate processing.
- Keep AiCore as an external service, even if development environments run it on the same machine.
- Monitor `/health` for the `aicore` health check.
- Use the admin AI dashboard for queue counts and AiCore connectivity status.
- Keep database migrations applied before starting Worker instances.

## Audit Events

The AI integration writes structured audit logs for:

- Job created
- Job submitted to AiCore
- Callback received
- Job completed
- Job failed
- Job cancelled
- Retry scheduled
- Stuck job requeued
- Job expired

AI output remains advisory only. Human users and commissions remain responsible for final approval.
