using PetroProcure.Application.Security;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.Api.Security;

public sealed class AdminAuditService(PetroProcureDbContext db, ICurrentUserService currentUser)
{
    public async Task LogAsync(string action, string entityType, string? entityId, string? summary, CancellationToken ct = default)
    {
        db.AdminAuditLogs.Add(new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = currentUser.IsAuthenticated ? currentUser.UserId : null,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
