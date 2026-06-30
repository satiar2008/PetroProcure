using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PetroProcure.Infrastructure;
using PetroProcure.Api.Endpoints;
using PetroProcure.Application;
using PetroProcure.Reporting;
using PetroProcure.AI;
using PetroProcure.Api.Security;
using PetroProcure.Api.Hubs;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Security;
using PetroProcure.Api.Health;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
const string DevelopmentJwtSigningKey = "PetroProcure-Development-Jwt-Signing-Key-2026-Minimum-32-Characters";

builder.AddServiceDefaults();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPetroProcureReporting();
builder.Services.AddPetroProcureAi(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AdminAuditService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Authentication:Jwt:Issuer"]
            ?? throw new InvalidOperationException("Authentication:Jwt:Issuer is required.");
        var audience = builder.Configuration["Authentication:Jwt:Audience"]
            ?? throw new InvalidOperationException("Authentication:Jwt:Audience is required.");
        var signingKey = builder.Configuration["Authentication:Jwt:SigningKey"];
        if ((string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
            && builder.Environment.IsDevelopment())
        {
            signingKey = DevelopmentJwtSigningKey;
        }

        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("Authentication:Jwt:SigningKey must be supplied securely and contain at least 32 characters.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };

        // SignalR sends the access token in the query string for the WebSocket/SSE handshake.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken)
                    && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck<AiCoreConnectivityHealthCheck>("aicore");
builder.Services.AddSignalR();
// Override the no-op notifier registered by AddApplication with the SignalR transport.
builder.Services.AddScoped<IAiJobNotifier, SignalRAiJobNotifier>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply EF Core migrations (which also seed roles, permissions, MESC samples and the admin user via
// HasData) when running in Development, when explicitly requested through Database:MigrateOnStartup
// (used by the IIS installer), or when launched with the "--migrate" switch.
var migrateOnStartup = app.Environment.IsDevelopment()
    || builder.Configuration.GetValue<bool>("Database:MigrateOnStartup");
var migrateOnly = args.Contains("--migrate", StringComparer.OrdinalIgnoreCase);

if (migrateOnStartup || migrateOnly)
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider
        .GetRequiredService<PetroProcure.Infrastructure.Persistence.PetroProcureDbContext>()
        .Database.MigrateAsync();
}

// "--migrate" runs migrations + admin bootstrap seeding deterministically and then exits, so the
// installer can provision the database before the IIS site starts serving requests.
if (migrateOnly)
{
    foreach (var hostedService in app.Services.GetServices<IHostedService>())
    {
        if (hostedService is PetroProcure.Infrastructure.Identity.AdminBootstrapService bootstrap)
            await bootstrap.StartAsync(CancellationToken.None);
    }

    Console.WriteLine("PetroProcure database migration and seeding completed.");
    return;
}

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("PetroProcure.Api.ExceptionHandler");
    if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
    {
        logger.LogDebug("API request was canceled by the client for {Path}", context.Request.Path);
        context.Response.StatusCode = 499;
        return;
    }

    logger.LogError(exception, "Unhandled API exception for {Path}", context.Request.Path);
    var (statusCode, title) = exception switch
    {
        PetroProcure.Application.Mesc.MescCatalogValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.Mesc.MescCatalogNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Mesc.MescCatalogConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        PetroProcure.Application.Indents.IndentValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.Indents.IndentNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Indents.IndentConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        PetroProcure.Application.PurchaseFiles.PurchaseFileValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.PurchaseFiles.PurchaseFileNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.PurchaseFiles.PurchaseFileConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        PetroProcure.Infrastructure.Storage.FileStorageValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Infrastructure.Storage.FileStorageNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Workflow.WorkflowValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.Workflow.WorkflowNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Workflow.WorkflowAccessDeniedException => (StatusCodes.Status403Forbidden, "Forbidden"),
        PetroProcure.Application.Contracts.ContractValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.Contracts.ContractNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Contracts.ContractConflictException => (StatusCodes.Status409Conflict, "Conflict"),
        PetroProcure.Application.Warehouse.WarehouseReceiptValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.Warehouse.WarehouseReceiptNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Legal.LegalRuleValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
        PetroProcure.Application.Legal.LegalRuleNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        PetroProcure.Application.Rag.RagEmbeddingUnavailableException => (StatusCodes.Status503ServiceUnavailable, "RAG embedding unavailable"),
        PetroProcure.Application.Security.CurrentUserNotAuthenticatedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
        PetroProcure.Application.Security.CurrentUserForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
    };

    context.Response.StatusCode = statusCode;
    await Results.Problem(
        statusCode: statusCode,
        title: title,
        detail: statusCode == 500 && !app.Environment.IsDevelopment() ? null : exception?.Message).ExecuteAsync(context);
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// In Development we run over plain HTTP (e.g. http://localhost:5073) without AppHost. HTTPS
// redirection would 307 authenticated calls to https, and HttpClient drops the Authorization
// header across the scheme/port change -> 401 -> forced logout right after login.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    Name = "PetroProcure API",
    Status = "Ready"
}))
.WithName("GetApiStatus");

app.MapIdentityFoundationEndpoints();
app.MapAuthEndpoints();
app.MapMescCatalogEndpoints();
app.MapIndentEndpoints();
app.MapPurchaseFileEndpoints();
app.MapApplicantEndpoints();
app.MapDocumentEndpoints();
app.MapWorkflowEndpoints();
app.MapReportEndpoints();
app.MapAiEndpoints();
app.MapRagEndpoints();
app.MapSupplierEndpoints();
app.MapInquiryEndpoints();
app.MapOrdersEndpoints();
app.MapTenderEndpoints();
app.MapCommissionEndpoints();
app.MapTenderCommissionDocumentReportEndpoints();
app.MapContractEndpoints();
app.MapPurchaseOrderEndpoints();
app.MapWarehouseEndpoints();
app.MapLegalRuleEndpoints();

app.MapHub<AiJobHub>("/hubs/ai-jobs");

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
