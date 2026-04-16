using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.FindOneUser;

public static class FindOneUserEnpoint
{
    public static IEndpointRouteBuilder MapFindOneUser(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}", (
                [FromRoute] Guid id,
                IUserRepository userRepository,
                CancellationToken cancellationToken) =>
            FindOneUserHandler.Handle(
                new FindOneUserRequest { Id = id },
                userRepository,
                cancellationToken))
            .WithName("FindOneUser")
            .WithSummary("Find one user")
            .Produces<FindOneUserResult>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
}
