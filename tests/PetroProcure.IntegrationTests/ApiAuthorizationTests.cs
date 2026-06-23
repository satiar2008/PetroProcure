using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Indents;
using PetroProcure.Contracts.V1.Mesc;
using PetroProcure.Contracts.Security;
using PetroProcure.Infrastructure.Persistence;
using PetroProcure.Infrastructure.Persistence.Seeding;
using PetroProcure.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using PetroProcure.Contracts.V1.Identity;

namespace PetroProcure.IntegrationTests;

public sealed class ApiAuthorizationTests(ApiAuthorizationFactory factory)
    : IClassFixture<ApiAuthorizationFactory>
{
    [Fact]
    public async Task LoginSuccessReturnsTokenAndCurrentUser()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(IdentitySeedData.DefaultAdminUserName, ApiAuthorizationFactory.AdminPassword, false));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Equal(IdentitySeedData.DefaultAdminUserId, login.User.Id);
        Assert.Contains(ApplicationRoles.SystemAdmin, login.User.Roles);
        Assert.NotEmpty(login.User.Departments);
    }

    [Fact]
    public async Task LoginFailureReturnsUnauthorized()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest(IdentitySeedData.DefaultAdminUserName, "wrong-password", false));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AnonymousRequestReturns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/mesc/groups?includeInactive=false");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUserWithoutPermissionReturns403()
    {
        var client = factory.CreateAuthenticatedClient();
        var response = await client.GetAsync("/api/mesc/groups?includeInactive=false");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithPermissionCanCallProtectedEndpoint()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.ItemView);
        var response = await client.GetAsync("/api/mesc/groups?includeInactive=false");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var groups = await response.Content.ReadFromJsonAsync<List<MescGeneralGroupDto>>();
        Assert.NotNull(groups);
        Assert.All(groups, group =>
        {
            Assert.False(string.IsNullOrWhiteSpace(group.Code));
            Assert.False(string.IsNullOrWhiteSpace(group.GeneralDescription));
        });
    }

    [Fact]
    public async Task CreatedByUserIdComesFromClaimsAndClientValueIsIgnored()
    {
        var client = factory.CreateAuthenticatedClient(
            ApplicationPermissions.IndentCreate,
            departmentId: SeedDataIds.OrdersAndInventoryControlId,
            userId: IdentitySeedData.DefaultAdminUserId);
        var maliciousUserId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync("/api/indents", new
        {
            YearPart = 99,
            TypeDigit = 2,
            Title = "Security integration indent",
            RequestingDepartmentId = SeedDataIds.OrdersAndInventoryControlId,
            ApplicantDepartmentId = (Guid?)null,
            Description = "Claims test",
            CreatedByUserId = maliciousUserId,
            IsAdmin = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var indent = await response.Content.ReadFromJsonAsync<IndentCreatedResponse>();
        Assert.Equal(IdentitySeedData.DefaultAdminUserId, indent!.CreatedByUserId);
        Assert.NotEqual(maliciousUserId, indent.CreatedByUserId);
    }

    [Fact]
    public async Task DepartmentAccessIsEnforced()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.IndentCreate);
        var response = await client.PostAsJsonAsync("/api/indents", new CreateIndentRequest(
            98, 2, "Forbidden department", SeedDataIds.OrdersAndInventoryControlId, null, null));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpointBlocksNonAdminUser()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.ItemView);
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record IndentCreatedResponse(Guid Id, Guid CreatedByUserId);
}

public sealed class ApiAuthorizationFactory : WebApplicationFactory<Program>
{
    public const string AdminPassword = "Integration-Admin-Password-2026!";
    private readonly string _connectionString =
        $"Server=localhost;Database=PetroProcureApiSecurity_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Authentication:Jwt:Issuer"] = "PetroProcure.Tests",
                ["Authentication:Jwt:Audience"] = "PetroProcure.Api.Tests",
                //["Authentication:Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-chars",
                ["ConnectionStrings:PetroProcureDb"] = _connectionString,
                ["PetroProcure:FileStorage:RootPath"] = Path.Combine(Path.GetTempPath(), "PetroProcureApiSecurity"),
                ["Security:BootstrapAdmin:Enabled"] = "false"
            }));

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
                options.DefaultForbidScheme = TestAuthenticationHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                TestAuthenticationHandler.SchemeName, _ => { });

        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>().Database.Migrate();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = userManager.FindByIdAsync(IdentitySeedData.DefaultAdminUserId.ToString()).GetAwaiter().GetResult();
        if (admin is not null)
        {
            admin.PasswordHash = new PasswordHasher<ApplicationUser>().HashPassword(admin, AdminPassword);
            var result = userManager.UpdateAsync(admin).GetAwaiter().GetResult();
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }
        return host;
    }

    public HttpClient CreateAuthenticatedClient(
        string? permission = null,
        Guid? departmentId = null,
        Guid? userId = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-User", (userId ?? Guid.NewGuid()).ToString());
        if (permission is not null)
            client.DefaultRequestHeaders.Add("X-Test-Permission", permission);
        if (departmentId.HasValue)
            client.DefaultRequestHeaders.Add("X-Test-Department", departmentId.Value.ToString());
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var options = new DbContextOptionsBuilder<PetroProcureDbContext>()
                .UseSqlServer(_connectionString).Options;
            using var db = new PetroProcureDbContext(options);
            db.Database.EnsureDeleted();
        }
        base.Dispose(disposing);
    }
}

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "SecurityTests";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var userId))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "security-test-user")
        };
        if (Request.Headers.TryGetValue("X-Test-Permission", out var permission))
            claims.AddRange(permission.Select(value => new Claim(PetroProcureClaimTypes.Permission, value!)));
        if (Request.Headers.TryGetValue("X-Test-Department", out var department))
            claims.AddRange(department.Select(value => new Claim(PetroProcureClaimTypes.DepartmentId, value!)));

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
