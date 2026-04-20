using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", (
                    LoginHandler loginHandler,
                    [FromBody] LoginRequest request,
                    IUserRepository userRepository,
                    CancellationToken cancellationToken) =>
                loginHandler.Handle(request, userRepository, cancellationToken))
            .WithName("Login")
            .Produces<string>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
}
