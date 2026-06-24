using PetroProcure.Web.Components;
using System.Globalization;
using MudBlazor.Services;
using PetroProcure.Web.Services;
using PetroProcure.Web.Services.Api;
using PetroProcure.Web.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;

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
builder.Services.AddHttpClient<IPetroProcureApiClient, PetroProcureApiClient>(
    client => client.BaseAddress = new Uri(apiBaseUrl));
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
app.MapGet("/document-download/{id:guid}", async (Guid id, IPetroProcureApiClient api, CancellationToken ct) =>
{
    var file = await api.DownloadDocumentAsync(id, ct);
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

