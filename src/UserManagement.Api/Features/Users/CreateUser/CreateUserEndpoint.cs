using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.CreateUser;

public static class CreateUserEndpoint
{
    public static IEndpointRouteBuilder MapCreateUser(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users", (
                [FromBody] CreateUserRequest request,
                IUserRepository userRepository,
                CancellationToken cancellationToken) =>
            CreateUserHandler.Handle(request, userRepository, cancellationToken))
            .WithName("CreateUser")
            .WithSummary("Creates a new user")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }
}
