using PetroProcure.Domain.Enums;

namespace PetroProcure.Web.Services;

public static class PurchaseFileUiPolicy
{
    public static bool CanEdit(PurchaseFileStatus status, Func<string, bool> hasPermission) =>
        status is not PurchaseFileStatus.Completed and not PurchaseFileStatus.Archived
        && hasPermission("PurchaseFile.Edit");
}
