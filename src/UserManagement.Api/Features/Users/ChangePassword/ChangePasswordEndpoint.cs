using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Features.Auth.Authorization;

namespace UserManagement.Api.Features.Users.ChangePassword;

public static class ChangePasswordEndpoint
{
    public static IEndpointRouteBuilder MapChangePassword(this IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{id:guid}/password", async (
                [FromRoute] Guid id,
                [FromBody] ChangePasswordRequest request,
                ChangePasswordHandler handler,
                CancellationToken cancellationToken) =>
            {
                request.Id = id;
                await handler.Handle(request, cancellationToken);
                return Results.NoContent();
            })
            .WithName("ChangePassword")
            .WithSummary("Changes a user's password")
            .RequireAuthorization(AuthPolicies.OwnerOrAdministrator)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
