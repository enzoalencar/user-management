using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
                [FromBody] LoginRequest request,
                LoginHandler loginHandler,
                CancellationToken cancellationToken) =>
            {
                var response = await loginHandler.Handle(request, cancellationToken);
                return Results.Ok(response);
            })
            .WithName("Login")
            .WithSummary("Authenticates a user and returns a JWT token")
            .AllowAnonymous()
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
