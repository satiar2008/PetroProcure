using PetroProcure.Web.Components;
using System.Globalization;
using MudBlazor.Services;
using PetroProcure.Web.Services;
using PetroProcure.Web.Services.Api;
using PetroProcure.Web.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMudServices();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthSession>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<PetroProcureAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<PetroProcureAuthenticationStateProvider>());
builder.Services.AddScoped<IUserAccessContext, UserAccessContext>();
builder.Services.AddScoped<ILookupCacheService, LookupCacheService>();
builder.Services.AddSingleton<IPersianDateService, PersianDateService>();
var apiBaseUrl = ResolveApiBaseUrl(builder.Configuration, builder.Environment);
builder.Services.AddHttpClient("PetroProcure.Auth", client => client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler();
    http.AddServiceDiscovery();
});
builder.Services.AddHttpClient<IPetroProcureApiClient, PetroProcureApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromMinutes(10);
    });

// Dedicated AI job client. AI calls (create job / poll status / cancel) are short; the UI polls
// for results instead of waiting on a long request.
builder.Services.AddHttpClient<IPetroProcureAiApiClient, PetroProcureAiApiClient>(
    client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
builder.Services.Configure<AiJobSignalROptions>(options => options.HubBaseUrl = apiBaseUrl);
builder.Services.AddScoped<IAiJobSignalRClient, AiJobSignalRClient>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

var persianCulture = new CultureInfo("fa-IR");
persianCulture.DateTimeFormat.Calendar = new PersianCalendar();
persianCulture.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
persianCulture.DateTimeFormat.LongDatePattern = "dddd، dd MMMM yyyy";
CultureInfo.DefaultThreadCurrentCulture = persianCulture;
CultureInfo.DefaultThreadCurrentUICulture = persianCulture;

var supportedCultures = new[]
{
    persianCulture,
    new CultureInfo("en-US")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("fa-IR"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAntiforgery();

app.MapGet("/report-preview/purchase-file/{id:guid}", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetPurchaseFileSummaryPdfAsync(id, ct), "application/pdf"));
app.MapGet("/report-preview/tender/{id:guid}/summary", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetTenderSummaryReportPdfAsync(id, ct), "application/pdf", $"tender-summary-{id}.pdf", enableRangeProcessing: true));
app.MapGet("/report-preview/tender/{id:guid}/comparison", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetTenderComparisonReportPdfAsync(id, ct), "application/pdf", $"tender-comparison-{id}.pdf", enableRangeProcessing: true));
app.MapGet("/report-preview/tender/{id:guid}/winner-decision", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetTenderWinnerDecisionReportPdfAsync(id, ct), "application/pdf", $"tender-winner-decision-{id}.pdf", enableRangeProcessing: true));
app.MapGet("/report-preview/commission/{id:guid}/minutes", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetCommissionMinutesReportPdfAsync(id, ct), "application/pdf", $"commission-minutes-{id}.pdf", enableRangeProcessing: true));
app.MapGet("/report-preview/commission/{id:guid}/decision/{decisionId:guid}", async (Guid id, Guid decisionId, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetCommissionDecisionReportPdfAsync(id, decisionId, ct), "application/pdf", $"commission-decision-{decisionId}.pdf", enableRangeProcessing: true));
app.MapGet("/report-preview/contract/{id:guid}", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
    Results.File(await api.GetContractPdfAsync(id, ct), "application/pdf", $"contract-{id}.pdf", enableRangeProcessing: true));
app.MapGet("/document-download/{id:guid}", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
{
    var file = await api.DownloadDocumentAsync(id, ct);
    return Results.File(file.Content, file.ContentType, file.FileName);
});
app.MapGet("/tender-document-download/{tenderId:guid}/{documentId:guid}", async (Guid tenderId, Guid documentId, IPetroProcureApiClient api, CancellationToken ct) =>
{
    var file = await api.DownloadTenderDocumentAsync(tenderId, documentId, ct);
    return Results.File(file.Content, file.ContentType, file.FileName);
});
app.MapGet("/commission-attachment-download/{sessionId:guid}/{attachmentId:guid}", async (Guid sessionId, Guid attachmentId, IPetroProcureApiClient api, CancellationToken ct) =>
{
    var file = await api.DownloadCommissionAttachmentAsync(sessionId, attachmentId, ct);
    return Results.File(file.Content, file.ContentType, file.FileName);
});
app.MapGet("/contract-document-download/{contractId:guid}/{documentId:guid}", async (Guid contractId, Guid documentId, IPetroProcureApiClient api, CancellationToken ct) =>
{
    var file = await api.DownloadContractDocumentAsync(contractId, documentId, ct);
    return Results.File(file.Content, file.ContentType, file.FileName);
});
app.MapGet("/report-preview/indent/{id:guid}", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
{
    var bytes = await api.GetIndentPdfAsync(id, ct);
    return Results.File(bytes, "application/pdf", $"indent-{id}.pdf", enableRangeProcessing: true);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();

static string ResolveApiBaseUrl(IConfiguration configuration, IHostEnvironment environment)
{
    var configured = configuration["ApiBaseUrl"];
    if (!string.IsNullOrWhiteSpace(configured))
    {
        return configured;
    }

    var aspireHttpsEndpoint = configuration["services:api:https:0"];
    var aspireHttpEndpoint = configuration["services:api:http:0"];
    if (!string.IsNullOrWhiteSpace(aspireHttpsEndpoint) || !string.IsNullOrWhiteSpace(aspireHttpEndpoint))
    {
        return "https+http://api";
    }

    return environment.IsDevelopment()
        ? "https://localhost:7008"
        : "https+http://api";
}

