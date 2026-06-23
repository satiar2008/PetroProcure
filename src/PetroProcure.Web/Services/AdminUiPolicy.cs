using PetroProcure.Contracts.V1.Identity;

namespace PetroProcure.Web.Services;

public static class AdminUiPolicy
{
    public const string ManageUsers = "Admin.ManageUsers";
    public const string ManageRoles = "Admin.ManageRoles";
    public const string ManageDepartments = "Admin.ManageDepartments";
    public const string ManageSettings = "Admin.ManageSettings";

    public static bool CanOpenAdmin(Func<string, bool> hasPermission) =>
        hasPermission(ManageUsers)
        || hasPermission(ManageRoles)
        || hasPermission(ManageDepartments)
        || hasPermission(ManageSettings);

    public static bool CanManageUsers(Func<string, bool> hasPermission) => hasPermission(ManageUsers);
    public static bool CanManageRoles(Func<string, bool> hasPermission) => hasPermission(ManageRoles);
    public static bool CanManageDepartments(Func<string, bool> hasPermission) => hasPermission(ManageDepartments);
    public static bool CanManageSettings(Func<string, bool> hasPermission) => hasPermission(ManageSettings);

    public static IReadOnlyDictionary<string, List<PermissionDto>> GroupPermissions(IEnumerable<PermissionDto> permissions) =>
        permissions
            .OrderBy(permission => permission.Name)
            .GroupBy(permission => permission.Name.Split('.', 2)[0])
            .ToDictionary(group => group.Key, group => group.ToList());
}
