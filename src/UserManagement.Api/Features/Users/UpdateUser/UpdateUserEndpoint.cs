using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Features.Auth.Authorization;

namespace UserManagement.Api.Features.Users.UpdateUser;

public static class UpdateUserEndpoint
{
    public static IEndpointRouteBuilder MapUpdateUser(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/users/{id:guid}", (
                [FromRoute] Guid id,
                [FromBody] UpdateUserRequest request,
                UpdateUserHandler handler,
                CancellationToken cancellationToken) =>
            Handle(
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
                handler,
                cancellationToken))
            .WithName("UpdateUser")
            .WithSummary("Updates a user")
            .RequireAuthorization(AuthPolicies.OwnerOrAdministrator)
            .Produces<UpdateUserResult>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
    
    private static async Task<IResult> Handle(
        UpdateUserRequest request,
        UpdateUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);
        return Results.Ok(result);
    }
}
