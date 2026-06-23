using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PetroProcure.Contracts.Security;
using PetroProcure.Contracts.V1.Identity;

namespace PetroProcure.Api.Security;

public sealed class JwtTokenService(IConfiguration configuration)
{
    private const string DevelopmentJwtSigningKey = "PetroProcure-Development-Jwt-Signing-Key-2026-Minimum-32-Characters";

    public LoginResponse Create(CurrentUserDto user, bool rememberMe, string? refreshToken = null)
    {
        var accessMinutes = configuration.GetValue("Authentication:Jwt:AccessTokenMinutes", 30);
        var refreshDays = rememberMe
            ? configuration.GetValue("Authentication:Jwt:RememberMeRefreshTokenDays", 30)
            : configuration.GetValue("Authentication:Jwt:RefreshTokenDays", 1);
        var accessExpiry = DateTime.UtcNow.AddMinutes(accessMinutes);
        var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

        return new LoginResponse(
            WriteToken(user, accessExpiry, "access"),
            refreshToken ?? WriteToken(user, refreshExpiry, "refresh"),
            accessExpiry,
            user);
    }

    public Guid ValidateRefreshToken(string token)
    {
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters(), out _);
        if (principal.FindFirstValue("token_type") != "refresh"
            || !Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new SecurityTokenException("Invalid refresh token.");
        return userId;
    }

    private string WriteToken(CurrentUserDto user, DateTime expires, string tokenType)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new("token_type", tokenType),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim(PetroProcureClaimTypes.Permission, permission)));
        claims.AddRange(user.DepartmentIds.Select(id => new Claim(PetroProcureClaimTypes.DepartmentId, id.ToString())));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey())),
            SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: configuration["Authentication:Jwt:Issuer"],
            audience: configuration["Authentication:Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials));
    }

    private TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = configuration["Authentication:Jwt:Issuer"],
        ValidAudience = configuration["Authentication:Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey())),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    private string SigningKey()
    {
        var signingKey = configuration["Authentication:Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey) || Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            var environment = configuration["ASPNETCORE_ENVIRONMENT"];
            if (string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase))
            {
                return DevelopmentJwtSigningKey;
            }
        }

        return !string.IsNullOrWhiteSpace(signingKey) && Encoding.UTF8.GetByteCount(signingKey) >= 32
            ? signingKey
            : throw new InvalidOperationException("JWT signing key is missing or too short.");
    }
}
