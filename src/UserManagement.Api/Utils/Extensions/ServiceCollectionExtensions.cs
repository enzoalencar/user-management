using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Auth.Refresh;
using UserManagement.Api.Features.Users.CreateUser;
using UserManagement.Api.Features.Users.DeleteUser;
using UserManagement.Api.Features.Users.FindAllUsers;
using UserManagement.Api.Features.Users.FindOneUser;
using UserManagement.Api.Features.Users.UpdateUser;

namespace UserManagement.Api.Utils.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<FindAllUsersHandler>();
        services.AddScoped<FindOneUserHandler>();
        services.AddScoped<UpdateUserHandler>();
        
        return services;
    }
}
