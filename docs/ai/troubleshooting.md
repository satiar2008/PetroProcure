# PetroProcure AI Troubleshooting

## Job Stays Queued

Check:

- PetroProcure.Worker is running.
- `PetroProcure:AI:AiCore:Mode` is `AsyncAiCoreJob` or `LocalOllamaWorker`.
- The job `NextRetryAtUtc` is empty or in the past.
- Worker logs do not show database connection or permission errors.
- `WorkerBatchSize` is greater than zero.

Recovery:

- Restart PetroProcure.Worker.
- Confirm the maintenance pass requeues jobs stuck in `Claimed`, `BuildingContext`, or `SendingToAiCore`.

## Job Stuck in Claimed, BuildingContext, or SendingToAiCore

These statuses usually mean a worker stopped while holding a job lock.

Check:

- `LockedBy`
- `LockedAtUtc`
- Worker service uptime
- `StuckJobTimeoutMinutes`

Recovery:

- Let Worker maintenance run.
- If urgent, verify the job is not actively running and requeue through a controlled database operation.

## Job Submitted but No Result Arrives

Check:

- AiCore accepted the job and returned `externalJobId`.
- `CallbackPublicUrl` is reachable by AiCore.
- Firewalls and reverse proxies allow `POST /api/ai/callbacks/aicore`.
- AiCore has the correct callback API key and HMAC secret.
- PetroProcure.Api logs do not show callback authentication failures.

Recovery:

- Use AiCore job status endpoint with the stored `ExternalJobId`.
- Fix callback connectivity or credentials.
- Let the job expire after `RunningJobTimeoutMinutes` if no callback can be recovered.

## Callback Rejected

Common causes:

- Missing or incorrect `X-AI-API-KEY`.
- Incorrect bearer token.
- Invalid `X-AiCore-Signature`.
- Old `X-AiCore-Timestamp`.
- Replayed signature.
- Remote IP not present in `CallbackAllowedIpAddresses`.
- Unknown or missing `correlationId`.

Recovery:

- Compare AiCore and PetroProcure secret configuration without logging secret values.
- Confirm AiCore signs exactly `{timestamp}.{rawBody}`.
- Ensure clocks are synchronized with NTP.
- Update the optional callback IP allowlist if AiCore egress IP changed.

## AiCore Health Is Unhealthy

Check:

- Admin AI dashboard health panel.
- `BaseUrl` and `HealthPath`.
- DNS, TLS certificate, firewall, proxy, and service status.
- AiCore API key if health endpoint requires authentication.

Recovery:

- Correct settings in Admin AI settings.
- Restart AiCore if the service is down.
- Keep Web/API usable; users can still create jobs while Worker retries according to policy.

## Worker Retry Limit Reached

If submission keeps failing, the job moves to `Failed` after the configured retry limit.

Check:

- Worker logs around `RetryScheduled`.
- AiCore response body in sanitized error messages.
- `MaxRetryCount` and `RetryDelaySeconds`.

Recovery:

- Fix AiCore availability or request validation issue.
- Create a new job from the UI when the underlying cause is resolved.

## Local Ollama Development Mode

Use only for development:

- `PetroProcure:AI:AiCore:Mode=LocalOllamaWorker`
- `PetroProcure:AI:OllamaBaseUrl=http://localhost:11434`
- `PetroProcure:AI:OllamaModel=gemma3`

The UI and API still use the same job endpoints and result tables. No Web/API request should wait on local Ollama.

## Result History and Cleanup

Completed queue rows can be cleaned after the retention window, but purchase file result history is retained in `AiEvaluationResults`, `AiFindings`, and `AiRecommendations`.

Before changing retention:

- Confirm compliance and audit requirements.
- Confirm backup and restore procedures.
- Do not delete advisory result history required for procurement review.
