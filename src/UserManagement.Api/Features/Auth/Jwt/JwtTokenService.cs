using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Authorization;
using UserManagement.Domain.Auth;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Jwt;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtSettingsOptions)
{
    public string GenerateAccessToken(User user)
    {
        var jwtSettings = jwtSettingsOptions.Value;

        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(AuthClaimTypes.IsActive, user.IsActive.ToString().ToLowerInvariant()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(jwtSettings.AccessTokenExpirationInMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public RefreshTokenIssueResult GenerateRefreshToken(Guid userId)
    {
        var jwtSettings = jwtSettingsOptions.Value;

        var rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
        var tokenHash = ComputeTokenHash(rawToken);
        var now = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(jwtSettings.RefreshTokenExpirationInDays)
        };

        return new RefreshTokenIssueResult(rawToken, refreshToken);
    }

    public static string ComputeTokenHash(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}

public sealed record RefreshTokenIssueResult(string RawToken, RefreshToken RefreshToken);
