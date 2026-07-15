using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Features.Auth.Authorization;

namespace UserManagement.Api.Features.Users.UpdateUserStatus;

public static class UpdateUserStatusEndpoint
{
    public static IEndpointRouteBuilder MapUpdateUserStatus(this IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{id:guid}/status", async (
                [FromRoute] Guid id,
                [FromBody] UpdateUserStatusRequest request,
                UpdateUserStatusHandler handler,
                CancellationToken cancellationToken) =>
            {
                request.Id = id;
                var result = await handler.Handle(request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("UpdateUserStatus")
            .WithSummary("Activates or deactivates a user")
            .RequireAuthorization(AuthPolicies.Administrator)
            .Produces<UpdateUserStatusResult>()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
