using System.Net;
using System.Net.Http.Json;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.PurchaseFiles;
using PetroProcure.Domain.Enums;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.IntegrationTests;

public sealed class ProcurementLifecycleEndpointTests(ApiAuthorizationFactory factory)
    : IClassFixture<ApiAuthorizationFactory>
{
    [Fact]
    public async Task PurchaseFileLifecycleRequiresViewPermissionAndReturnsSummary()
    {
        var departmentId = SeedDataIds.PurchaseDepartmentId;
        var creator = factory.CreateAuthenticatedClient(
            ApplicationPermissions.PurchaseFileCreate,
            departmentId: departmentId,
            userId: IdentitySeedData.DefaultAdminUserId);

        var createdResponse = await creator.PostAsJsonAsync("/api/purchase-files", new CreatePurchaseFileRequest(
            2026,
            $"Lifecycle regression {Guid.NewGuid():N}",
            "Phase 39.1 lifecycle endpoint regression",
            PurchaseFilePriority.Normal,
            departmentId,
            departmentId,
            null));
        createdResponse.EnsureSuccessStatusCode();

        var created = await createdResponse.Content.ReadFromJsonAsync<PurchaseFileDto>();
        Assert.NotNull(created);

        var forbidden = await factory.CreateAuthenticatedClient()
            .GetAsync($"/api/purchase-files/{created.Id}/lifecycle");
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var viewer = factory.CreateAuthenticatedClient(ApplicationPermissions.PurchaseFileView);
        var lifecycle = await viewer.GetFromJsonAsync<PurchaseFileLifecycleDto>(
            $"/api/purchase-files/{created.Id}/lifecycle");

        Assert.NotNull(lifecycle);
        Assert.Equal(created.Id, lifecycle.PurchaseFileId);
        Assert.Equal(created.FileNumber, lifecycle.FileNumber);
        Assert.Equal(created.Title, lifecycle.Title);
        Assert.Equal(nameof(PurchaseFileStatus.Draft), lifecycle.Status);
        Assert.Equal(0, lifecycle.DocumentsCount);
        Assert.Equal(0, lifecycle.ReportsCount);
        Assert.Contains(lifecycle.Steps, step => step.Stage == "Indent" && step.Count == 0);
        Assert.Contains(lifecycle.Steps, step => step.Stage == "Documents" && step.Count == 0);
        Assert.Contains(lifecycle.Steps, step => step.Stage == "AiEvaluations" && step.Count == 0);
    }
}
