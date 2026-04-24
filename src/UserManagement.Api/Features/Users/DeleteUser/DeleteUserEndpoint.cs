using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Features.Auth.Authorization;

namespace UserManagement.Api.Features.Users.DeleteUser;

public static class DeleteUserEndpoint
{
    public static IEndpointRouteBuilder MapDeleteUser(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{id:guid}", (
                    [FromRoute] Guid id,
                    DeleteUserHandler handler,
                    CancellationToken cancellationToken) =>
                Handle(new DeleteUserRequest { Id = id }, handler, cancellationToken))
            .WithName("DeleteUser")
            .WithSummary("Deletes a user")
            .RequireAuthorization(AuthPolicies.ActiveUser)
            .Produces<DeleteUserResult>()
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> Handle(
        DeleteUserRequest request,
        DeleteUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);
        return TypedResults.Ok(result);
    }
}
