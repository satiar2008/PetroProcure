using PetroProcure.Application.Security;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.IntegrationTests;

internal sealed class TestCurrentUser : ICurrentUserService
{
    public Guid UserId => IdentitySeedData.DefaultAdminUserId;
    public string? UserName => IdentitySeedData.DefaultAdminUserName;
    public string? Email => IdentitySeedData.DefaultAdminEmail;
    public IReadOnlyCollection<string> Roles => [ApplicationRoles.SystemAdmin];
    public IReadOnlyCollection<string> Permissions => ApplicationPermissions.All;
    public IReadOnlyCollection<Guid> DepartmentIds =>
        [SeedDataIds.PurchaseDepartmentId, SeedDataIds.OrdersAndInventoryControlId, SeedDataIds.ApplicantId];
    public bool IsAuthenticated => true;
    public bool IsSystemAdmin => true;
}
