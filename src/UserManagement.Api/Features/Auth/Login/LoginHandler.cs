using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Domain.Auth;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Login;

public sealed class LoginHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    JwtTokenService jwtTokenService)
{
    public async Task<IssuedTokens> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required.");

        var user = await userRepository.FindOneByEmailAsync(request.Email.Trim(), cancellationToken);
        
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new SecurityTokenException("Invalid credentials.");

        if (!user.IsActive)
            throw new SecurityTokenException("Inactive user.");

        var accessToken = jwtTokenService.GenerateAccessToken(user);
        var refreshTokenIssueResult = jwtTokenService.GenerateRefreshToken(user.Id);
        await refreshTokenRepository.CreateAsync(refreshTokenIssueResult.RefreshToken, cancellationToken);

        return new IssuedTokens(accessToken, refreshTokenIssueResult.RawToken);
    }
}
