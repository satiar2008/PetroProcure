using PetroProcure.Worker;
using PetroProcure.Application;
using PetroProcure.Application.Security;
using PetroProcure.Infrastructure;
using PetroProcure.AI;
using PetroProcure.Reporting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPetroProcureReporting();
builder.Services.AddPetroProcureAi(builder.Configuration);
// The worker has no HTTP context; supply a system identity for services that require ICurrentUserService.
builder.Services.AddScoped<ICurrentUserService, WorkerCurrentUserService>();
builder.Services.Configure<AiJobWorkerOptions>(builder.Configuration.GetSection(AiJobWorkerOptions.SectionName));
builder.Services.AddScoped<AiJobProcessor>();
builder.Services.AddHostedService<AiJobWorkerService>();

var host = builder.Build();
ValidateAiCoreCallbackOptions(host.Services.GetRequiredService<IOptions<AiCoreIntegrationOptions>>().Value);
host.Run();

static void ValidateAiCoreCallbackOptions(AiCoreIntegrationOptions options)
{
    if (options.Mode != AiCoreIntegrationMode.AsyncAiCoreJob)
        return;

    if (string.IsNullOrWhiteSpace(options.CallbackPublicUrl))
        throw new InvalidOperationException("PetroProcure:AI:AiCore:CallbackPublicUrl is required when AI mode is AsyncAiCoreJob.");

    if (!Uri.TryCreate(options.CallbackPublicUrl, UriKind.Absolute, out var callbackUri) ||
        callbackUri.Scheme is not ("http" or "https"))
    {
        throw new InvalidOperationException("PetroProcure:AI:AiCore:CallbackPublicUrl must be an absolute http/https URL reachable by AiCore.");
    }

    if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var aiCoreUri) &&
        !IsLoopback(aiCoreUri) &&
        IsLoopback(callbackUri))
    {
        throw new InvalidOperationException(
            "PetroProcure:AI:AiCore:CallbackPublicUrl cannot use localhost/loopback when AiCore BaseUrl points to a separate host.");
    }
}

static bool IsLoopback(Uri uri) =>
    uri.IsLoopback ||
    string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
    string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase);
