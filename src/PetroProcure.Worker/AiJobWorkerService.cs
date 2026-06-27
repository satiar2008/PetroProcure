using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using PetroProcure.AI;

namespace PetroProcure.Worker;

public sealed class AiJobWorkerService(
    IServiceScopeFactory scopeFactory,
    IOptions<AiJobWorkerOptions> workerOptions,
    IOptions<AiCoreIntegrationOptions> aiCoreOptions,
    ILogger<AiJobWorkerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerId = string.IsNullOrWhiteSpace(workerOptions.Value.WorkerId)
            ? $"{Environment.MachineName}-{Guid.NewGuid():N}"
            : workerOptions.Value.WorkerId.Trim();
        logger.LogInformation("AI job worker {WorkerId} started.", workerId);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<AiJobProcessor>();
                await processor.RunMaintenanceAsync(stoppingToken);
                var batchSize = workerOptions.Value.BatchSize > 0
                    ? workerOptions.Value.BatchSize
                    : aiCoreOptions.Value.WorkerBatchSize;
                var processed = await processor.ProcessBatchAsync(workerId, Math.Max(1, batchSize), stoppingToken);
                if (processed == 0)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, workerOptions.Value.PollIntervalSeconds)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AI job worker loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, workerOptions.Value.PollIntervalSeconds)), stoppingToken);
            }
        }

        logger.LogInformation("AI job worker {WorkerId} stopped.", workerId);
    }
}
