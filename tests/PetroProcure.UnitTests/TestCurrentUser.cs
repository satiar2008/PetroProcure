using PetroProcure.Application.Security;

namespace PetroProcure.UnitTests;

internal sealed class TestCurrentUser : ICurrentUserService
{
    public TestCurrentUser(Guid userId, IReadOnlyCollection<Guid>? departmentIds = null, bool isSystemAdmin = false)
    {
        UserId = userId;
        DepartmentIds = departmentIds ?? [];
        IsSystemAdmin = isSystemAdmin;
        Roles = isSystemAdmin ? [ApplicationRoles.SystemAdmin] : [];
    }

    public Guid UserId { get; }
    public string? UserName => "test-user";
    public string? Email => "test@petroprocure.local";
    public IReadOnlyCollection<string> Roles { get; }
    public IReadOnlyCollection<string> Permissions => [];
    public IReadOnlyCollection<Guid> DepartmentIds { get; }
    public bool IsAuthenticated => true;
    public bool IsSystemAdmin { get; }
}
