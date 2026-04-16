using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.DeleteUser;

public static class DeleteUserEndpoint
{
    public static IEndpointRouteBuilder MapDeleteUser(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{id:guid}", (
                [FromRoute] Guid id,
                IUserRepository userRepository,
                CancellationToken cancellationToken) =>
            DeleteUserHandler.Handle(new DeleteUserRequest { Id = id }, userRepository, cancellationToken))
            .WithName("DeleteUser")
            .WithSummary("Deletes a user")
            .Produces<DeleteUserResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
