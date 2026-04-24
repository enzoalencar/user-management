using UserManagement.Api.Features.Auth.Jwt;
using UserManagement.Api.Features.Auth.Login;
using UserManagement.Api.Features.Auth.Refresh;

namespace UserManagement.Api.Utils.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<JwtTokenService>();
        
        return services;
    }
}