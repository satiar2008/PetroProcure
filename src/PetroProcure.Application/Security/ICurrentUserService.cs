namespace PetroProcure.Application.Security;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
    IReadOnlyCollection<Guid> DepartmentIds { get; }
    bool IsAuthenticated { get; }
    bool IsSystemAdmin { get; }
}

public sealed class CurrentUserNotAuthenticatedException()
    : UnauthorizedAccessException("An authenticated user is required.");

public sealed class CurrentUserForbiddenException(string message)
    : Exception(message);
