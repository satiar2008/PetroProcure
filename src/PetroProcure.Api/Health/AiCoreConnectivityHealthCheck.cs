using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetroProcure.AI;

namespace PetroProcure.Api.Health;

public sealed class AiCoreConnectivityHealthCheck(IAiCoreJobClient client) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var health = await client.GetHealthAsync(cancellationToken);
        return health.IsHealthy
            ? HealthCheckResult.Healthy(health.Status)
            : HealthCheckResult.Unhealthy(health.ErrorMessage ?? health.Status);
    }
}
