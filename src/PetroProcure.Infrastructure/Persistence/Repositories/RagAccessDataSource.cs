using Microsoft.EntityFrameworkCore;
using PetroProcure.Application.Rag;
using PetroProcure.Application.Security;

namespace PetroProcure.Infrastructure.Persistence.Repositories;

internal sealed class RagAccessDataSource(PetroProcureDbContext db) : IRagAccessDataSource
{
    public Task<bool> CanAccessPurchaseFileAsync(
        Guid purchaseFileId, RagUserContext userContext, CancellationToken ct = default)
    {
        if (!HasPurchaseFilePermission(userContext)) return Task.FromResult(false);
        if (userContext.IsSystemAdmin || userContext.DepartmentIds.Count == 0)
            return db.PurchaseFiles.AsNoTracking().AnyAsync(x => x.Id == purchaseFileId, ct);

        return db.PurchaseFiles.AsNoTracking().AnyAsync(x => x.Id == purchaseFileId
            && (userContext.DepartmentIds.Contains(x.CurrentDepartmentId)
                || userContext.DepartmentIds.Contains(x.PurchaseDepartmentId)
                || x.ResponsibleUserId == userContext.UserId
                || x.CreatedByUserId == userContext.UserId), ct);
    }

    public async Task<IReadOnlySet<Guid>> GetAllowedPurchaseFileIdsAsync(
        RagUserContext userContext, CancellationToken ct = default)
    {
        if (!HasPurchaseFilePermission(userContext)) return new HashSet<Guid>();

        var query = db.PurchaseFiles.AsNoTracking();
        if (!userContext.IsSystemAdmin && userContext.DepartmentIds.Count > 0)
        {
            query = query.Where(x => userContext.DepartmentIds.Contains(x.CurrentDepartmentId)
                || userContext.DepartmentIds.Contains(x.PurchaseDepartmentId)
                || x.ResponsibleUserId == userContext.UserId
                || x.CreatedByUserId == userContext.UserId);
        }

        return (await query.Select(x => x.Id).ToListAsync(ct)).ToHashSet();
    }

    public Task<Guid?> GetTenderPurchaseFileIdAsync(Guid tenderId, CancellationToken ct = default) =>
        db.Tenders.AsNoTracking()
            .Where(x => x.Id == tenderId)
            .Select(x => (Guid?)x.PurchaseFileId)
            .SingleOrDefaultAsync(ct);

    private static bool HasPurchaseFilePermission(RagUserContext userContext) =>
        userContext.IsSystemAdmin
        || userContext.Permissions.Contains(ApplicationPermissions.AiAgentUse)
        || userContext.Permissions.Contains(ApplicationPermissions.PurchaseFileView);
}
