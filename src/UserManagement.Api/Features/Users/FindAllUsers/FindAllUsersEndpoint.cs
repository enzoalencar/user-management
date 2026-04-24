using UserManagement.Api.Features.Auth.Authorization;

namespace UserManagement.Api.Features.Users.FindAllUsers;

public static class FindAllUsersEndpoint
{
    public static IEndpointRouteBuilder MapFindAllUser(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", (
                FindAllUsersHandler handler,
                CancellationToken cancellationToken) =>
            Handle(new FindAllUsersRequest(), handler, cancellationToken))
            .WithName("FindAllUsers")
            .WithSummary("Find all users")
            .RequireAuthorization(AuthPolicies.ActiveUser)
            .Produces<List<FindAllUsersResult>>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
    
    private static async Task<IResult> Handle(
        FindAllUsersRequest request,
        FindAllUsersHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);
        return TypedResults.Ok(result);
    }
}
