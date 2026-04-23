using Microsoft.Extensions.Options;
using UserManagement.Api.Features.Auth.Jwt;

namespace UserManagement.Api.Features.Auth.Refresh;

public static class RefreshTokenCookieHelper
{
    public static void WriteCookie(HttpResponse response, string refreshToken, IOptions<JwtSettings> jwtSettingsOptions)
    {
        var jwtSettings = jwtSettingsOptions.Value;

        response.Cookies.Append(jwtSettings.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationInDays),
            IsEssential = true
        });
    }
}
