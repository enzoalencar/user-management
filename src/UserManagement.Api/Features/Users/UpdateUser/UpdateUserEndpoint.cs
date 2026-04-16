using Microsoft.AspNetCore.Mvc;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.UpdateUser;

public static class UpdateUserEndpoint
{
    public static IEndpointRouteBuilder MapUpdateUser(this IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{id:guid}", (
                [FromRoute] Guid id,
                [FromBody] UpdateUserRequest request,
                IUserRepository userRepository,
                CancellationToken cancellationToken) =>
            UpdateUserHandler.Handle(
                new UpdateUserRequest
                {
                    Id = id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                    DocumentNumber = request.DocumentNumber,
                    PhoneNumber = request.PhoneNumber
                },
                userRepository,
                cancellationToken))
            .WithName("UpdateUser")
            .WithSummary("Updates a user")
            .Produces<UpdateUserResponse>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
}
