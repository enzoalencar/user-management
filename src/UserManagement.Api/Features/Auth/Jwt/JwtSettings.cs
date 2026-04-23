namespace UserManagement.Api.Features.Auth.Jwt;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenExpirationInMinutes { get; init; } = 30;
    public int RefreshTokenExpirationInDays { get; init; } = 5;
    public string RefreshTokenCookieName { get; init; } = "um_refresh_token";
}
