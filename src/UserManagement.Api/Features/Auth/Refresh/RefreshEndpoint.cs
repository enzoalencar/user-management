using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserManagement.Api.Features.Auth.Jwt;

namespace UserManagement.Api.Features.Auth.Refresh;

public static class RefreshEndpoint
{
    public static IEndpointRouteBuilder MapRefresh(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", async (
                HttpRequest request,
                HttpResponse response,
                IOptions<JwtSettings> jwtSettingsOptions,
                RefreshHandler refreshHandler,
                CancellationToken cancellationToken) =>
            {
                var cookieName = jwtSettingsOptions.Value.RefreshTokenCookieName;
                if (!request.Cookies.TryGetValue(cookieName, out var rawRefreshToken))
                    throw new SecurityTokenException("Refresh token cookie is missing.");

                var issuedTokens = await refreshHandler.Handle(rawRefreshToken ?? string.Empty, cancellationToken);
                RefreshTokenCookieHelper.WriteCookie(response, issuedTokens.RefreshToken, jwtSettingsOptions);

                return Results.Ok(new Login.LoginResponse(issuedTokens.AccessToken));
            })
            .WithName("RefreshToken")
            .WithSummary("Rotates refresh token and issues a new access token")
            .AllowAnonymous()
            .Produces<Login.LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
