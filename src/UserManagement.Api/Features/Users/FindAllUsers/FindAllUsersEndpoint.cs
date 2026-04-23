using UserManagement.Api.Features.Auth.Authorization;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.FindAllUsers;

public static class FindAllUsersEndpoint
{
    public static IEndpointRouteBuilder MapFindAllUser(this IEndpointRouteBuilder app)
    {
        app.MapGet("/users", (
                IUserRepository userRepository,
                CancellationToken cancellationToken) =>
            FindAllUsersHandler.Handle(new FindAllUsersRequest(), userRepository, cancellationToken))
            .WithName("FindAllUsers")
            .WithSummary("Find all users")
            .RequireAuthorization(AuthPolicies.ActiveUser)
            .Produces<List<FindAllUsersResult>>()
            .Produces(StatusCodes.Status400BadRequest);
        
        return app;
    }
}
