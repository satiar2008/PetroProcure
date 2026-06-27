using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PetroProcure.Contracts.V1.Ai;
using PetroProcure.Web.Services.Auth;

namespace PetroProcure.Web.Services.Api;

public sealed class AiJobSignalROptions
{
    public string HubBaseUrl { get; set; } = string.Empty;
}

public interface IAiJobSignalRClient : IAsyncDisposable
{
    event Func<AiJobNotificationDto, Task>? NotificationReceived;
    Task SubscribeToJobAsync(Guid jobId, CancellationToken ct = default);
    Task SubscribeToEntityAsync(string entityType, Guid entityId, CancellationToken ct = default);
}

public sealed class AiJobSignalRClient(
    IOptions<AiJobSignalROptions> options,
    AuthSession session,
    ILogger<AiJobSignalRClient> logger) : IAiJobSignalRClient
{
    private HubConnection? _connection;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public event Func<AiJobNotificationDto, Task>? NotificationReceived;

    public async Task SubscribeToJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var connection = await EnsureConnectedAsync(ct);
        await connection.InvokeAsync(AiJobHubEvents.SubscribeToJob, jobId.ToString(), ct);
    }

    public async Task SubscribeToEntityAsync(string entityType, Guid entityId, CancellationToken ct = default)
    {
        var connection = await EnsureConnectedAsync(ct);
        await connection.InvokeAsync(AiJobHubEvents.SubscribeToEntity, entityType, entityId.ToString(), ct);
    }

    private async Task<HubConnection> EnsureConnectedAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            if (_connection is null)
            {
                var hubUrl = $"{options.Value.HubBaseUrl.TrimEnd('/')}{AiJobHubEvents.HubPath}";
                _connection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, configure =>
                    {
                        configure.AccessTokenProvider = () => Task.FromResult(session.AccessToken);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                RegisterHandlers(_connection);
            }

            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync(ct);

            return _connection;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI job SignalR connection is unavailable; polling fallback remains active.");
            throw;
        }
        finally
        {
            _gate.Release();
        }
    }

    private void RegisterHandlers(HubConnection connection)
    {
        foreach (var eventName in new[]
        {
            AiJobHubEvents.Created,
            AiJobHubEvents.StatusChanged,
            AiJobHubEvents.Completed,
            AiJobHubEvents.Failed,
            AiJobHubEvents.Cancelled
        })
        {
            connection.On<AiJobNotificationDto>(eventName, async notification =>
            {
                if (NotificationReceived is { } handler)
                    await handler(notification);
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        _gate.Dispose();
    }
}
