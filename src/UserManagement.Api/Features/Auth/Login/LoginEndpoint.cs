using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Api.Features.Auth.Refresh;

namespace UserManagement.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
                [FromBody] LoginRequest request,
                HttpResponse response,
                IOptions<JwtSettings> jwtSettingsOptions,
                LoginHandler loginHandler,
                CancellationToken cancellationToken) =>
            {
                var issuedTokens = await loginHandler.Handle(request, cancellationToken);
                RefreshTokenCookieHelper.WriteCookie(response, issuedTokens.RefreshToken, jwtSettingsOptions);

                return Results.Ok(new LoginResponse(issuedTokens.AccessToken));
            })
            .WithName("Login")
            .WithSummary("Authenticates a user and returns a short-lived JWT token")
            .AllowAnonymous()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
