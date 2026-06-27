using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Ai;

namespace PetroProcure.IntegrationTests;

public sealed class AiJobSignalRTests(ApiAuthorizationFactory factory)
    : IClassFixture<ApiAuthorizationFactory>
{
    [Fact]
    public async Task HubRequiresAuthentication()
    {
        await using var connection = CreateConnection(factory, userId: null, permission: null);

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
    }

    [Fact]
    public async Task StatusUpdatePublishesNotification()
    {
        var userId = Guid.NewGuid();
        var created = await CreateJobAsync(userId);
        var received = new TaskCompletionSource<AiJobNotificationDto>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var connection = CreateConnection(factory, userId, ApplicationPermissions.AiAgentUse);
        connection.On<AiJobNotificationDto>(AiJobHubEvents.StatusChanged, notification => received.TrySetResult(notification));
        await connection.StartAsync();
        await connection.InvokeAsync(AiJobHubEvents.SubscribeToJob, created.JobId.ToString());

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
            await queue.MarkRunningAsync(created.JobId, 42, "در حال تحلیل", CancellationToken.None);
        }

        var notification = await WaitForAsync(received.Task);
        Assert.Equal(created.JobId, notification.JobId);
        Assert.Equal("Running", notification.Status);
        Assert.Equal(42, notification.ProgressPercent);
        Assert.False(notification.HasResult);
    }

    [Fact]
    public async Task CompletedNotificationAllowsClientToFetchResult()
    {
        var userId = Guid.NewGuid();
        var created = await CreateJobAsync(userId);
        string correlationId;

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
            var job = await queue.GetJobAsync(created.JobId, CancellationToken.None);
            correlationId = job!.CorrelationId;
        }

        var received = new TaskCompletionSource<AiJobNotificationDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var connection = CreateConnection(factory, userId, ApplicationPermissions.AiAgentUse);
        connection.On<AiJobNotificationDto>(AiJobHubEvents.Completed, notification => received.TrySetResult(notification));
        await connection.StartAsync();
        await connection.InvokeAsync(AiJobHubEvents.SubscribeToJob, created.JobId.ToString());

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var callback = scope.ServiceProvider.GetRequiredService<IAiCoreCallbackService>();
            var payload = new AiCoreCallbackRequest(
                correlationId,
                "ext-signalr-completed",
                "Completed",
                100,
                "done",
                new AiCoreCallbackResultDto("نتیجه آماده است", [], [], "{\"summary\":\"نتیجه آماده است\"}"),
                null,
                new AiUsageDto(10, 20, null, 30, 900, "test-model", "AiCore"),
                DateTime.UtcNow);

            var outcome = await callback.HandleAsync(payload, CancellationToken.None);
            Assert.Equal(AiCoreCallbackOutcome.Processed, outcome);
        }

        var notification = await WaitForAsync(received.Task);
        Assert.Equal(created.JobId, notification.JobId);
        Assert.Equal("Completed", notification.Status);
        Assert.True(notification.HasResult);

        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.AiAgentUse, userId: userId);
        var result = await client.GetFromJsonAsync<AiJobResultDto>($"/api/ai/jobs/{created.JobId}/result");

        Assert.NotNull(result);
        Assert.Equal("نتیجه آماده است", result!.Summary);
    }

    private async Task<CreateAiJobResponse> CreateJobAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var queue = scope.ServiceProvider.GetRequiredService<IAiJobQueueService>();
        return await queue.CreateJobAsync(new CreateAiJobRequest(
            "PurchaseFile",
            Guid.NewGuid(),
            "Summary"),
            userId,
            CancellationToken.None);
    }

    private static HubConnection CreateConnection(ApiAuthorizationFactory factory, Guid? userId, string? permission)
    {
        return new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress, AiJobHubEvents.HubPath), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
                if (userId.HasValue)
                    options.Headers.Add("X-Test-User", userId.Value.ToString());
                if (!string.IsNullOrWhiteSpace(permission))
                    options.Headers.Add("X-Test-Permission", permission);
            })
            .Build();
    }

    private static async Task<T> WaitForAsync<T>(Task<T> task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(10)));
        Assert.Same(task, completed);
        return await task;
    }
}
