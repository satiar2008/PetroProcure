namespace PetroProcure.Web.Services;

public static class NavigationAccessPolicy
{
    public static IEnumerable<NavigationAccess> VisibleForDepartment(
        IEnumerable<NavigationAccess> items,
        string departmentKey,
        Func<string, bool> hasPermission) =>
        items.Where(item => item.DepartmentKey == departmentKey
            && (item.Permission is null || hasPermission(item.Permission)));

    public static IEnumerable<NavigationAccess> VisibleShared(
        IEnumerable<NavigationAccess> items,
        Func<string, bool> hasPermission) =>
        items.Where(item => item.DepartmentKey is null
            && (item.Permission is null || hasPermission(item.Permission)));
}
