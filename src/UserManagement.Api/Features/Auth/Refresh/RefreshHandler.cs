using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Domain.Auth;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Refresh;

public sealed class RefreshHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    JwtTokenService jwtTokenService)
{
    public async Task<IssuedTokens> Handle(string rawRefreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
            throw new SecurityTokenException("Refresh token is required.");

        var refreshTokenHash = JwtTokenService.ComputeTokenHash(rawRefreshToken);
        var storedRefreshToken = await refreshTokenRepository.FindByTokenHashAsync(refreshTokenHash, cancellationToken);

        if (storedRefreshToken is null)
            throw new SecurityTokenException("Invalid refresh token.");

        var now = DateTime.UtcNow;
        if (storedRefreshToken.RevokedAtUtc is not null || storedRefreshToken.ExpiresAtUtc <= now)
            throw new SecurityTokenException("Invalid refresh token.");

        var user = await userRepository.FindOneAsync(storedRefreshToken.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            throw new SecurityTokenException("User not authorized for refresh.");

        var newAccessToken = jwtTokenService.GenerateAccessToken(user);
        var newRefreshTokenResult = jwtTokenService.GenerateRefreshToken(user.Id);

        storedRefreshToken.RevokedAtUtc = now;
        storedRefreshToken.ReplacedByTokenHash = newRefreshTokenResult.RefreshToken.TokenHash;

        var updated = await refreshTokenRepository.UpdateAsync(storedRefreshToken, cancellationToken);
        if (!updated)
            throw new SecurityTokenException("Refresh token rotation failed.");

        await refreshTokenRepository.CreateAsync(newRefreshTokenResult.RefreshToken, cancellationToken);

        return new IssuedTokens(newAccessToken, newRefreshTokenResult.RawToken);
    }
}
