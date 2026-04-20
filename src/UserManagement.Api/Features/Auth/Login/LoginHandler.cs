using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Login;

public class LoginHandler(IConfiguration config)
{
    public async Task<IResult> Handle(
        LoginRequest request,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Email and password are required.");

        var user = await userRepository.FindOneByEmailAsync(email, cancellationToken);
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var checkPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        
        return checkPassword
            ? Results.Ok(GenerateJwt(user)) 
            : throw new SecurityTokenException("Invalid login attempt.");
    }

    private string GenerateJwt(User user)
    {
        var secret = config["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret is not configured.");
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("JWT audience is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FirstName!),
            new Claim(ClaimTypes.Email, user.Email!),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: userClaims,
            expires: DateTime.Now.AddDays(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
