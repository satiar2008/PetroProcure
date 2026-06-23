using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using PetroProcure.Api.Security;
using PetroProcure.Application.Security;
using PetroProcure.Contracts.V1.Identity;
using PetroProcure.Contracts.V1.Organization;
using PetroProcure.Infrastructure.Identity;
using PetroProcure.Infrastructure.Persistence;

namespace PetroProcure.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Authentication");

        auth.MapPost("/login", async (
            LoginRequest request, UserManager<ApplicationUser> userManager,
            PetroProcureDbContext db, JwtTokenService tokens, IConfiguration configuration, CancellationToken ct) =>
        {
            var user = request.UserNameOrEmail.Contains('@')
                ? await userManager.FindByEmailAsync(request.UserNameOrEmail)
                : await userManager.FindByNameAsync(request.UserNameOrEmail);
            if (user is null || await userManager.IsLockedOutAsync(user) || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                if (user is not null) await userManager.AccessFailedAsync(user);
                return Results.Unauthorized();
            }

            await userManager.ResetAccessFailedCountAsync(user);

            var current = await BuildCurrentUser(user, userManager, db, ct);
            var refreshToken = CreateRefreshToken();
            db.AuthRefreshTokens.Add(new AuthRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = HashToken(refreshToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(request.RememberMe
                    ? configuration.GetValue("Authentication:Jwt:RememberMeRefreshTokenDays", 30)
                    : configuration.GetValue("Authentication:Jwt:RefreshTokenDays", 1))
            });
            await db.SaveChangesAsync(ct);
            return Results.Ok(tokens.Create(current, request.RememberMe, refreshToken));
        }).AllowAnonymous();

        auth.MapPost("/refresh", async (
            RefreshTokenRequest request, UserManager<ApplicationUser> userManager,
            PetroProcureDbContext db, JwtTokenService tokens, IConfiguration configuration, CancellationToken ct) =>
        {
            var hash = HashToken(request.RefreshToken);
            var stored = await db.AuthRefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
            if (stored is null || stored.RevokedAt.HasValue || stored.ExpiresAt <= DateTime.UtcNow)
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(stored.UserId.ToString());
            if (user is null || await userManager.IsLockedOutAsync(user)) return Results.Unauthorized();

            var newRefreshToken = CreateRefreshToken();
            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByTokenHash = HashToken(newRefreshToken);
            db.AuthRefreshTokens.Add(new AuthRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = stored.ReplacedByTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(configuration.GetValue("Authentication:Jwt:RememberMeRefreshTokenDays", 30))
            });
            await db.SaveChangesAsync(ct);
            var current = await BuildCurrentUser(user, userManager, db, ct);
            return Results.Ok(tokens.Create(current, rememberMe: true, newRefreshToken));
        }).AllowAnonymous();

        auth.MapPost("/change-password", async (
            ChangePasswordRequest request,
            ICurrentUserService currentUser,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());
            if (user is null) return Results.Unauthorized();
            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            return result.Succeeded
                ? Results.NoContent()
                : Results.ValidationProblem(result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));
        }).RequireAuthorization();

        auth.MapGet("/sessions", async (ICurrentUserService currentUser, PetroProcureDbContext db, CancellationToken ct) =>
            await db.AuthRefreshTokens.AsNoTracking()
                .Where(x => x.UserId == currentUser.UserId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new UserSessionDto(x.Id, x.UserId, x.CreatedAt, x.ExpiresAt, x.RevokedAt,
                    x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow))
                .ToListAsync(ct))
            .RequireAuthorization();

        auth.MapPost("/revoke-refresh-token", async (
            RevokeRefreshTokenRequest request,
            PetroProcureDbContext db,
            CancellationToken ct) =>
        {
            var hash = HashToken(request.RefreshToken);
            var token = await db.AuthRefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
            if (token is null) return Results.NoContent();
            token.RevokedAt ??= DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization();

        auth.MapGet("/me", async (
            ICurrentUserService currentUser, UserManager<ApplicationUser> userManager,
            PetroProcureDbContext db, CancellationToken ct) =>
        {
            var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());
            return user is null ? Results.Unauthorized() : Results.Ok(await BuildCurrentUser(user, userManager, db, ct));
        }).RequireAuthorization();

        return app;
    }

    private static string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private static async Task<CurrentUserDto> BuildCurrentUser(
        ApplicationUser user, UserManager<ApplicationUser> userManager,
        PetroProcureDbContext db, CancellationToken ct)
    {
        var roles = (await userManager.GetRolesAsync(user)).ToArray();
        var normalized = roles.Select(role => role.ToUpperInvariant()).ToArray();
        var permissions = await (
            from role in db.Roles
            join rolePermission in db.RolePermissions on role.Id equals rolePermission.RoleId
            join permission in db.Permissions on rolePermission.PermissionId equals permission.Id
            where normalized.Contains(role.NormalizedName!) && permission.IsActive
            select permission.Name).Distinct().OrderBy(x => x).ToArrayAsync(ct);

        var profile = user.UserProfileId.HasValue
            ? await db.ApplicationUserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == user.UserProfileId, ct)
            : null;
        var departments = user.UserProfileId.HasValue
            ? await (
                from assignment in db.UserDepartments
                join department in db.Departments on assignment.DepartmentId equals department.Id
                where assignment.UserProfileId == user.UserProfileId && department.IsActive
                orderby assignment.IsPrimary descending, department.Name
                select new DepartmentDto(department.Id, department.Name, department.Type.ToString(), department.IsActive))
                .ToArrayAsync(ct)
            : [];

        return new CurrentUserDto(
            user.Id, user.UserName ?? string.Empty, user.Email, profile?.DisplayName,
            roles, permissions, departments.Select(x => x.Id).ToArray(), departments,
            roles.Contains(ApplicationRoles.SystemAdmin, StringComparer.OrdinalIgnoreCase));
    }
}
