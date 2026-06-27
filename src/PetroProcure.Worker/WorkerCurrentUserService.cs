using PetroProcure.Application.Security;
using PetroProcure.Infrastructure.Persistence.Seeding;

namespace PetroProcure.Worker;

/// <summary>
/// <see cref="ICurrentUserService"/> for the background worker, which has no HTTP context.
/// The worker runs as a trusted system principal (the seeded system administrator). Its job-
/// processing path attributes results to each job's own creator; this implementation exists so
/// that application/infrastructure services depending on a current user can be constructed and
/// validated inside the worker host.
/// </summary>
internal sealed class WorkerCurrentUserService : ICurrentUserService
{
    public Guid UserId => IdentitySeedData.DefaultAdminUserId;
    public string? UserName => "system-worker";
    public string? Email => null;
    public IReadOnlyCollection<string> Roles { get; } = [ApplicationRoles.SystemAdmin];
    public IReadOnlyCollection<string> Permissions { get; } = [];
    public IReadOnlyCollection<Guid> DepartmentIds { get; } = [];
    public bool IsAuthenticated => true;
    public bool IsSystemAdmin => true;
}
