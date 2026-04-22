using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Login;

public sealed class LoginHandler(
    IUserRepository userRepository,
    IOptions<JwtSettings> jwtSettingsOptions)
{
    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required.");

        var user = await userRepository.FindOneByEmailAsync(request.Email.Trim(), cancellationToken);
        
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new SecurityTokenException("Invalid credentials.");

        if (!user.IsActive)
            throw new SecurityTokenException("Inactive user.");

        var jwtSettings = jwtSettingsOptions.Value;
        ValidateJwtSettings(jwtSettings);

        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(jwtSettings.ExpirationInMinutes),
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new LoginResponse(token);
    }

    private static void ValidateJwtSettings(JwtSettings jwtSettings)
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            throw new InvalidOperationException($"'{JwtSettings.SectionName}:Issuer' not configured.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            throw new InvalidOperationException($"'{JwtSettings.SectionName}:Audience' not configured.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
            throw new InvalidOperationException($"'{JwtSettings.SectionName}:SecretKey' not configured.");

        if (jwtSettings.SecretKey.Length < 32)
            throw new InvalidOperationException($"'{JwtSettings.SectionName}:SecretKey' must have at least 32 characters.");

        if (jwtSettings.ExpirationInMinutes <= 0)
            throw new InvalidOperationException($"'{JwtSettings.SectionName}:ExpirationInMinutes' must be greater than zero.");
    }
}
