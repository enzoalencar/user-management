using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Auth.Refresh;
using UserManagement.Api.Features.Users.CreateUser;
using UserManagement.Api.Features.Users.DeleteUser;
using UserManagement.Api.Features.Users.FindAllUsers;
using UserManagement.Api.Features.Users.FindOneUser;
using UserManagement.Api.Features.Users.UpdateUser;

namespace UserManagement.Api.Utils.Extensions;

public static class ApiEndpointExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
        app.MapLogin();
        app.MapRefresh();
        app.MapCreateUser();
        app.MapDeleteUser();
        app.MapUpdateUser();
        app.MapFindOneUser();
        app.MapFindAllUser();
        
        return app;
    }
}