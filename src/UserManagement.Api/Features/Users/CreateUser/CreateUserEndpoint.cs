using Microsoft.AspNetCore.Mvc;

namespace UserManagement.Api.Features.Users.CreateUser;

public static class CreateUserEndpoint
{
    public static IEndpointRouteBuilder MapCreateUser(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users", (
                [FromBody] CreateUserRequest request,
                CreateUserHandler handler,
                CancellationToken cancellationToken) =>
            Handle(request, handler, cancellationToken))
            .WithName("CreateUser")
            .WithSummary("Creates a new user")
            .AllowAnonymous()
            .Produces<CreateUserResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> Handle(
        CreateUserRequest request,
        CreateUserHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);
        return Results.Created($"/users/{result.Id}", result);
    }
}
