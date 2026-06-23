namespace PetroProcure.Application.Security;

public static class PermissionPolicyNames
{
    public const string Prefix = "Permission:";

    public static string For(string permission)
    {
        return $"{Prefix}{permission}";
    }
}
