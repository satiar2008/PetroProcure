using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using PetroProcure.AI;

namespace PetroProcure.UnitTests.AI;

public sealed class AiCoreClientTimeoutTests
{
    // Proves the configured timeout is actually enforced: a delayed AiCore call is cancelled
    // and surfaced as a friendly AiCoreClientException instead of hanging forever.
    [Fact]
    public async Task SendTextAsync_DelayedResponse_IsCancelledByTimeout()
    {
        // TimeoutSeconds is clamped to a 10s floor by the client, so the handler must outlast that.
        var handler = new HangingHandler(TimeSpan.FromSeconds(60));
        var http = new HttpClient(handler);
        var client = new AiCoreClient(http, new FakeSettings(timeoutSeconds: 10), NullLogger<AiCoreClient>.Instance);

        var stopwatch = Stopwatch.StartNew();
        var ex = await Assert.ThrowsAsync<AiCoreClientException>(() =>
            client.SendTextAsync(new AiCoreTextRequest("model",
                [new AiCoreTextMessage("user", "سلام")], JsonMode: false)));
        stopwatch.Stop();

        Assert.Contains("timed out", ex.Message, StringComparison.OrdinalIgnoreCase);
        // Cancelled around the 10s timeout, well before the 60s handler delay.
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(40), $"Call took {stopwatch.Elapsed} (timeout not enforced).");
        Assert.True(handler.ReceivedCancellation, "The HTTP send was not cancelled by the timeout token.");
    }

    private sealed class HangingHandler(TimeSpan delay) : HttpMessageHandler
    {
        public bool ReceivedCancellation { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }
            catch (OperationCanceledException)
            {
                ReceivedCancellation = true;
                throw;
            }
        }
    }

    private sealed class FakeSettings(int timeoutSeconds) : IAiCoreSettingsProvider
    {
        public Task<AiCoreSettings> GetAsync(CancellationToken ct = default) =>
            Task.FromResult(new AiCoreSettings("https://aicore.local", "secret", "PETROPROCURE_AICORE_API_KEY",
                "model", timeoutSeconds, 12000, 2000, true, false, null, null));
    }
}
