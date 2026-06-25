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
using PetroProcure.Contracts.V1.Suppliers;
using PetroProcure.Contracts.V1.Inquiry;
using PetroProcure.Contracts.V1.Orders;
using PetroProcure.Contracts.V1.Commission;
using PetroProcure.Contracts.V1.Common;
using PetroProcure.Contracts.V1.Tenders;
using PetroProcure.Domain.Enums;
using PetroProcure.Domain.Modules.Orders;

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

    [Fact]
    public async Task SupplierListRequiresSupplierView()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.ItemView);
        var response = await client.GetAsync("/api/suppliers");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ContractListRequiresContractView()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.SupplierView);
        var response = await client.GetAsync("/api/contracts");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithContractViewCanListContracts()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.ContractView);
        var response = await client.GetAsync("/api/contracts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateSupplierThroughApi()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.SupplierCreate, userId: IdentitySeedData.DefaultAdminUserId);
        var response = await client.PostAsJsonAsync("/api/suppliers", new CreateSupplierRequest(
            $"SUP-T-{Guid.NewGuid():N}"[..16], "تأمین‌کننده تست", null, null, null, null, null, null, null,
            "بوشهر", "ایران", null, SupplierType.Distributor, null, [], null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var supplier = await response.Content.ReadFromJsonAsync<SupplierDetailDto>();
        Assert.Equal("تأمین‌کننده تست", supplier!.Supplier.Name);
        Assert.Equal(IdentitySeedData.DefaultAdminUserId, supplier.Supplier.CreatedByUserId);
    }

    [Fact]
    public async Task SupplierLookupExcludesBlacklistedByDefault()
    {
        var creator = factory.CreateAuthenticatedClient(ApplicationPermissions.SupplierCreate, userId: IdentitySeedData.DefaultAdminUserId);
        var code = $"SUP-B-{Guid.NewGuid():N}"[..16];
        var created = await creator.PostAsJsonAsync("/api/suppliers", new CreateSupplierRequest(
            code, "تأمین‌کننده مسدود", null, null, null, null, null, null, null,
            "تهران", "ایران", null, SupplierType.Distributor, null, [], null));
        var detail = await created.Content.ReadFromJsonAsync<SupplierDetailDto>();

        var blacklister = factory.CreateAuthenticatedClient(ApplicationPermissions.SupplierBlacklist, userId: IdentitySeedData.DefaultAdminUserId);
        var blacklist = await blacklister.PostAsJsonAsync($"/api/suppliers/{detail!.Supplier.Id}/blacklist", new ChangeSupplierStatusRequest("ریسک"));
        Assert.Equal(HttpStatusCode.NoContent, blacklist.StatusCode);

        var viewer = factory.CreateAuthenticatedClient(ApplicationPermissions.SupplierView);
        var lookup = await viewer.GetFromJsonAsync<List<SupplierLookupDto>>($"/api/suppliers/lookup?term={code}");

        Assert.DoesNotContain(lookup!, x => x.SupplierCode == code);
    }

    [Fact]
    public async Task UserWithoutInquiryViewCannotListInquiries()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.SupplierView);
        var response = await client.GetAsync("/api/inquiries");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateInquiryFromPurchaseFileAndSend()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.InquiryCreate, userId: IdentitySeedData.DefaultAdminUserId);
        var response = await client.PostAsJsonAsync($"/api/inquiries/from-purchase-file/{SeedDataIds.SamplePurchaseFileId}",
            new CreateInquiryFromPurchaseFileRequest("استعلام تست", InquiryType.PriceInquiry, DateTime.UtcNow.AddDays(7), null, [], [Guid.Parse("a2000000-0000-0000-0000-000000000001")]));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var inquiry = await response.Content.ReadFromJsonAsync<InquiryDetailDto>();
        Assert.NotEmpty(inquiry!.Items);
        Assert.NotEmpty(inquiry.Suppliers);

        var sender = factory.CreateAuthenticatedClient(ApplicationPermissions.InquirySend, userId: IdentitySeedData.DefaultAdminUserId);
        var send = await sender.PostAsJsonAsync($"/api/inquiries/{inquiry.Inquiry.Id}/send", new SendInquiryRequest());
        Assert.Equal(HttpStatusCode.NoContent, send.StatusCode);
    }

    [Fact]
    public async Task AddSupplierQuoteAndGetComparison()
    {
        var creator = factory.CreateAuthenticatedClient(ApplicationPermissions.InquiryCreate, userId: IdentitySeedData.DefaultAdminUserId);
        var created = await creator.PostAsJsonAsync($"/api/inquiries/from-purchase-file/{SeedDataIds.SamplePurchaseFileId}",
            new CreateInquiryFromPurchaseFileRequest("استعلام مقایسه", InquiryType.PriceInquiry, DateTime.UtcNow.AddDays(7), null, [], [Guid.Parse("a2000000-0000-0000-0000-000000000002")]));
        var inquiry = await created.Content.ReadFromJsonAsync<InquiryDetailDto>();

        var quoteClient = factory.CreateAuthenticatedClient(ApplicationPermissions.InquiryReceiveQuote, userId: IdentitySeedData.DefaultAdminUserId);
        var quoteResponse = await quoteClient.PostAsJsonAsync($"/api/inquiries/{inquiry!.Inquiry.Id}/quotes",
            new AddSupplierQuoteRequest(inquiry.Suppliers[0].Id, "Q-1", DateTime.UtcNow, DateTime.UtcNow.AddDays(30), "IRR", "تحویل انبار", "نقدی", null, 1000, 90, 10, null, null));
        Assert.Equal(HttpStatusCode.Created, quoteResponse.StatusCode);

        var compareClient = factory.CreateAuthenticatedClient(ApplicationPermissions.InquiryCompareQuotes);
        var comparison = await compareClient.GetFromJsonAsync<InquiryComparisonDto>($"/api/inquiries/{inquiry.Inquiry.Id}/comparison");
        Assert.NotNull(comparison);
        Assert.NotEmpty(comparison!.Suppliers);
    }

    [Fact]
    public async Task UserWithoutOrdersViewInventoryCannotViewInventoryControl()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.ItemView);
        var response = await client.GetAsync("/api/orders/inventory-control");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateSubmitApproveAndConvertMaterialNeedToIndent()
    {
        var creator = factory.CreateAuthenticatedClient(ApplicationPermissions.OrdersCreateMaterialNeed,
            departmentId: SeedDataIds.OrdersAndInventoryControlId,
            userId: IdentitySeedData.DefaultAdminUserId);
        var created = await creator.PostAsJsonAsync("/api/orders/material-needs", new CreateMaterialNeedRequest(
            SeedDataIds.PipeItemId, 3, null, SeedDataIds.OrdersAndInventoryControlId, null,
            MaterialNeedPriority.Normal, "Integration need"));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var need = await created.Content.ReadFromJsonAsync<MaterialNeedDto>();
        Assert.NotNull(need);

        var submit = await creator.PostAsJsonAsync($"/api/orders/material-needs/{need!.Id}/submit", new SubmitMaterialNeedRequest());
        Assert.Equal(HttpStatusCode.NoContent, submit.StatusCode);

        var approver = factory.CreateAuthenticatedClient(ApplicationPermissions.OrdersApproveMaterialNeed,
            userId: IdentitySeedData.DefaultAdminUserId);
        var approve = await approver.PostAsJsonAsync($"/api/orders/material-needs/{need.Id}/approve", new ApproveMaterialNeedRequest());
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        var converter = factory.CreateAuthenticatedClient(ApplicationPermissions.OrdersConvertNeedToIndent,
            userId: IdentitySeedData.DefaultAdminUserId);
        var convert = await converter.PostAsJsonAsync($"/api/orders/material-needs/{need.Id}/convert-to-indent",
            new ConvertMaterialNeedToIndentRequest(97, 3, "Converted need"));
        Assert.Equal(HttpStatusCode.OK, convert.StatusCode);
        var reference = await convert.Content.ReadFromJsonAsync<IndentReferenceResponse>();
        var viewer = factory.CreateAuthenticatedClient(ApplicationPermissions.IndentView);
        var detail = await viewer.GetFromJsonAsync<IndentDto>($"/api/indents/{reference!.IndentId}");
        Assert.Equal(IndentSourceType.MaterialNeed, detail!.SourceType);
        Assert.Equal(need.Id, detail.SourceReferenceId);
        Assert.Contains("نیاز کالا", detail.SourceDisplayText);
    }

    [Fact]
    public async Task DetectShortageAlertAndConvertToIndent()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PetroProcureDbContext>();
            var exists = db.InventoryControlItems.Any(x => x.MescItemId == SeedDataIds.GateValveItemId);
            if (!exists)
            {
                db.InventoryControlItems.Add(new InventoryControlItem(Guid.NewGuid(), SeedDataIds.GateValveItemId,
                    "2001000001", "200100", "Valves", "Gate valve", SeedDataIds.EachUnitId, 1, 10, null, null, true));
                db.StockBalances.Add(new StockBalance(Guid.NewGuid(), SeedDataIds.GateValveItemId, null, 2));
                await db.SaveChangesAsync();
            }
        }

        var detector = factory.CreateAuthenticatedClient(ApplicationPermissions.OrdersManageShortageAlerts,
            userId: IdentitySeedData.DefaultAdminUserId);
        var detected = await detector.PostAsJsonAsync("/api/orders/shortage-alerts/detect", new DetectShortageAlertsRequest(true));
        Assert.Equal(HttpStatusCode.OK, detected.StatusCode);
        var alerts = await detected.Content.ReadFromJsonAsync<List<ShortageAlertDto>>();
        Assert.NotEmpty(alerts!);

        var converter = factory.CreateAuthenticatedClient(ApplicationPermissions.OrdersConvertShortageToIndent,
            departmentId: SeedDataIds.OrdersAndInventoryControlId,
            userId: IdentitySeedData.DefaultAdminUserId);
        var convert = await converter.PostAsJsonAsync($"/api/orders/shortage-alerts/{alerts![0].Id}/convert-to-indent",
            new ConvertShortageToIndentRequest(96, 3, SeedDataIds.OrdersAndInventoryControlId, "Shortage indent"));
        Assert.Equal(HttpStatusCode.OK, convert.StatusCode);
        var reference = await convert.Content.ReadFromJsonAsync<IndentReferenceResponse>();
        var viewer = factory.CreateAuthenticatedClient(ApplicationPermissions.IndentView);
        var detail = await viewer.GetFromJsonAsync<IndentDto>($"/api/indents/{reference!.IndentId}");
        Assert.Equal(IndentSourceType.ShortageAlert, detail!.SourceType);
        Assert.Equal(alerts[0].Id, detail.SourceReferenceId);
    }

    [Fact]
    public async Task SeededIndentHasSourceTypeAndDisplayText()
    {
        var viewer = factory.CreateAuthenticatedClient(ApplicationPermissions.IndentView);
        var detail = await viewer.GetFromJsonAsync<IndentDto>($"/api/indents/{SeedDataIds.SampleIndentId}");
        Assert.Equal(IndentSourceType.Manual, detail!.SourceType);
        Assert.False(string.IsNullOrWhiteSpace(detail.SourceDisplayText));
    }

    [Fact]
    public async Task UserWithoutCommissionViewCannotListSessions()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.TenderView);
        var response = await client.GetAsync("/api/commission/sessions");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithCommissionViewCanListSessions()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.CommissionView);
        var response = await client.GetAsync("/api/commission/sessions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TenderCommissionSessionSummaryDto>>();
        Assert.NotNull(page);
    }

    [Fact]
    public async Task UserWithoutCommissionCreateCannotCreateSession()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.CommissionView);
        var response = await client.PostAsJsonAsync("/api/commission/sessions",
            new CreateCommissionSessionRequest(Guid.NewGuid(), "Forbidden session", DateTime.UtcNow, null, null));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateCommissionSessionFromTender()
    {
        var tenderCreator = factory.CreateAuthenticatedClient(ApplicationPermissions.TenderCreate,
            userId: IdentitySeedData.DefaultAdminUserId);
        var tenderResponse = await tenderCreator.PostAsJsonAsync("/api/tenders",
            new CreateTenderRequest(SeedDataIds.SamplePurchaseFileId, "Tender for commission",
                TenderType.LimitedTender, DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(8), null));
        Assert.Equal(HttpStatusCode.Created, tenderResponse.StatusCode);
        var tender = await tenderResponse.Content.ReadFromJsonAsync<TenderDetailDto>();

        var commissionCreator = factory.CreateAuthenticatedClient(ApplicationPermissions.CommissionCreate,
            userId: IdentitySeedData.DefaultAdminUserId);
        var response = await commissionCreator.PostAsJsonAsync($"/api/commission/sessions/from-tender/{tender!.Tender.Id}",
            new CreateCommissionSessionFromTenderRequest("Commission session", DateTime.UtcNow.AddDays(1),
                "Meeting room", null, [], [new AddAgendaItemRequest(1, "Review bids", null, null, null)]));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var session = await response.Content.ReadFromJsonAsync<TenderCommissionSessionDetailDto>();
        Assert.Equal(tender.Tender.Id, session!.Session.TenderId);
        Assert.Equal(SeedDataIds.SamplePurchaseFileId, session.Session.PurchaseFileId);
        Assert.Equal(IdentitySeedData.DefaultAdminUserId, session.Session.CreatedByUserId);
        Assert.Single(session.AgendaItems);
    }

    [Fact]
    public async Task UserWithoutTenderReportExportCannotExportTenderReport()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.TenderView);
        var response = await client.GetAsync($"/api/tenders/{Guid.NewGuid()}/reports/summary/pdf");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithTenderReportExportCanGenerateTenderSummaryPdf()
    {
        var tenderCreator = factory.CreateAuthenticatedClient(ApplicationPermissions.TenderCreate,
            userId: IdentitySeedData.DefaultAdminUserId);
        var tenderResponse = await tenderCreator.PostAsJsonAsync("/api/tenders",
            new CreateTenderRequest(SeedDataIds.SamplePurchaseFileId, "Tender report integration",
                TenderType.PublicTender, DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(8), null));
        Assert.Equal(HttpStatusCode.Created, tenderResponse.StatusCode);
        var tender = await tenderResponse.Content.ReadFromJsonAsync<TenderDetailDto>();

        var reporter = factory.CreateAuthenticatedClient(ApplicationPermissions.TenderReportExport,
            userId: IdentitySeedData.DefaultAdminUserId);
        var response = await reporter.GetAsync($"/api/tenders/{tender!.Tender.Id}/reports/summary/pdf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task UserWithoutTenderManageDocumentsCannotUploadTenderDocument()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.TenderView);
        using var form = new MultipartFormDataContent();
        var response = await client.PostAsync($"/api/tenders/{Guid.NewGuid()}/documents/upload", form);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UserWithoutCommissionReportExportCannotExportCommissionReport()
    {
        var client = factory.CreateAuthenticatedClient(ApplicationPermissions.CommissionView);
        var response = await client.GetAsync($"/api/commission/sessions/{Guid.NewGuid()}/reports/minutes/pdf");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record IndentCreatedResponse(Guid Id, Guid CreatedByUserId);
    private sealed record IndentReferenceResponse(Guid IndentId);
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
                ["Authentication:Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-chars",
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
