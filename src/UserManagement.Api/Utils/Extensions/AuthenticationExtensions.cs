using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Authorization;
using UserManagement.Api.Features.Auth.Jwt;

namespace UserManagement.Api.Utils.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .Validate(jwtSettings => !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                $"'{JwtSettings.SectionName}:Issuer' not configured.")
            .Validate(jwtSettings => !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                $"'{JwtSettings.SectionName}:Audience' not configured.")
            .Validate(jwtSettings => !string.IsNullOrWhiteSpace(jwtSettings.SecretKey),
                $"'{JwtSettings.SectionName}:SecretKey' not configured.")
            .Validate(jwtSettings => jwtSettings.SecretKey.Length >= 32,
                $"'{JwtSettings.SectionName}:SecretKey' must have at least 32 characters.")
            .Validate(jwtSettings => jwtSettings.AccessTokenExpirationInMinutes > 0,
                $"'{JwtSettings.SectionName}:AccessTokenExpirationInMinutes' must be greater than zero.")
            .Validate(jwtSettings => jwtSettings.RefreshTokenExpirationInDays > 0,
                $"'{JwtSettings.SectionName}:RefreshTokenExpirationInDays' must be greater than zero.")
            .Validate(jwtSettings => !string.IsNullOrWhiteSpace(jwtSettings.RefreshTokenCookieName),
                $"'{JwtSettings.SectionName}:RefreshTokenCookieName' not configured.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtSettingsOptions) =>
            {
                var jwtSettings = jwtSettingsOptions.Value;
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

        return services;
    }
}
