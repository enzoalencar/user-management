using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Authorization;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Auth.Refresh;

namespace UserManagement.Api.Utils.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
                          ?? throw new InvalidOperationException($"'{JwtSettings.SectionName}' not configured.");
        
        ValidateJwtSettings(jwtSettings);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(JwtRegisteredClaimNames.Sub)
                    .RequireClaim(JwtRegisteredClaimNames.Email))
            .AddPolicy(AuthPolicies.ActiveUser, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(AuthClaimTypes.IsActive, "true"))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(JwtRegisteredClaimNames.Sub)
                .RequireClaim(JwtRegisteredClaimNames.Email)
                .Build());
        
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<JwtTokenService>();
        
        return services;
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

        if (jwtSettings.AccessTokenExpirationInMinutes <= 0)
            throw new InvalidOperationException(
                $"'{JwtSettings.SectionName}:AccessTokenExpirationInMinutes' must be greater than zero.");

        if (jwtSettings.RefreshTokenExpirationInDays <= 0)
            throw new InvalidOperationException(
                $"'{JwtSettings.SectionName}:RefreshTokenExpirationInDays' must be greater than zero.");

        if (string.IsNullOrWhiteSpace(jwtSettings.RefreshTokenCookieName))
            throw new InvalidOperationException($"'{JwtSettings.SectionName}:RefreshTokenCookieName' not configured.");
    }
}