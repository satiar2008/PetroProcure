using PetroProcure.Contracts.V1.Organization;

namespace PetroProcure.Contracts.V1.Identity;

public sealed record LoginRequest(string UserNameOrEmail, string Password, bool RememberMe = false);
public sealed record LoginResponse(
    string AccessToken, string RefreshToken, DateTime ExpiresAt, CurrentUserDto User);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record ResetPasswordRequest(string NewPassword);
public sealed record RevokeRefreshTokenRequest(string RefreshToken);
public sealed record UserSessionDto(Guid Id, Guid UserId, DateTime CreatedAt, DateTime ExpiresAt, DateTime? RevokedAt, bool IsActive);
public sealed record CurrentUserDto(
    Guid Id, string UserName, string? Email, string? DisplayName,
    IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions,
    IReadOnlyList<Guid> DepartmentIds, IReadOnlyList<DepartmentDto> Departments,
    bool IsSystemAdmin);
public sealed record PermissionDto(Guid Id, string Name, string Description, bool IsActive);
public sealed record RoleDto(Guid Id, string Name, IReadOnlyList<string> Permissions);
public sealed record CreateRoleRequest(string Name);
public sealed record UpdateRoleRequest(string Name);
public sealed record UserDto(
    Guid Id, string UserName, string? Email, string? DisplayName,
    Guid? UserProfileId, IReadOnlyList<string> Roles, IReadOnlyList<Guid> DepartmentIds);
public sealed record UpdateUserRequest(string? Email, string? DisplayName, bool? IsActive);
public sealed record CreateUserRequest(
    string UserName, string Email, string Password, string DisplayName,
    bool EmailConfirmed, string[] Roles);
public sealed record AssignUserDepartmentRequest(Guid UserProfileId, Guid DepartmentId, bool IsPrimary);
public sealed record AssignRolePermissionsRequest(Guid[] PermissionIds);
public sealed record AssignUserRolesRequest(string[] Roles);
public sealed record UserDepartmentDto(Guid Id, Guid UserProfileId, Guid DepartmentId, bool IsPrimary);
public sealed record AdminDashboardDto(
    int UsersCount, int RolesCount, int DepartmentsCount, int ActivePermissionsCount, int WorkflowActionsCount);
public sealed record AdminAuditLogDto(
    Guid Id, Guid? ActorUserId, string Action, string EntityType, string? EntityId,
    string? Summary, DateTime CreatedAt);
public sealed record SystemSettingDto(string Key, string? Value, string? Description, bool IsSecret);
public sealed record UpdateSystemSettingRequest(string? Value, string? Description, bool IsSecret);
