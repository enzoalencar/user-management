using Microsoft.AspNetCore.Mvc;
using UserManagement.Api.Features.Auth.Authorization;

namespace UserManagement.Api.Features.Users.FindOneUser;

public static class FindOneUserEnpoint
{
    public static IEndpointRouteBuilder MapFindOneUser(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:guid}", (
                [FromRoute] Guid id,
                FindOneUserHandler handler,
                CancellationToken cancellationToken) =>
            Handle(
                new FindOneUserRequest { Id = id },
                handler,
                cancellationToken))
            .WithName("FindOneUser")
            .WithSummary("Find one user")
            .RequireAuthorization(AuthPolicies.ActiveUser)
            .Produces<FindOneUserResult>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
    
    private static async Task<IResult> Handle(
        FindOneUserRequest request,
        FindOneUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);
        return TypedResults.Ok(result);
    }
}
