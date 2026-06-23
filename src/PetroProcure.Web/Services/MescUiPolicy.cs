using PetroProcure.Contracts.V1.Mesc;

namespace PetroProcure.Web.Services;

public static class MescUiPolicy
{
    public static bool CanView(Func<string, bool> hasPermission) => hasPermission("Item.View");
    public static bool CanCreate(Func<string, bool> hasPermission) => hasPermission("Item.Create");
    public static bool CanEdit(Func<string, bool> hasPermission) => hasPermission("Item.Edit");
    public static bool CanActivateDeactivate(Func<string, bool> hasPermission) => hasPermission("Item.ActivateDeactivate");

    public static string GetItemDisplayText(MescItemDto item) =>
        $"{item.Code} — {item.SpecificDescription} | {item.GeneralGroupCode} — {item.GeneralDescription}";

    public static IReadOnlyList<MescItemGroupedDto> GroupItems(IEnumerable<MescItemDto> items) =>
        items.GroupBy(item => new { item.GeneralGroupCode, item.GeneralDescription })
            .OrderBy(group => group.Key.GeneralGroupCode)
            .Select(group => new MescItemGroupedDto(
                group.Key.GeneralGroupCode,
                group.Key.GeneralDescription,
                group.OrderBy(item => item.Code).ToArray()))
            .ToArray();
}
