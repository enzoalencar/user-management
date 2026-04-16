using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserManagement.Domain.Users;
using UserManagement.Infrastructure.Persistence;
using UserManagement.Infrastructure.Persistence.Mongo;

namespace UserManagement.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        UserBsonClassMap.Register();

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));
    
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString)) 
                throw new InvalidOperationException($"'{MongoDbSettings.SectionName}:ConnectionString' not configured.");

            return new MongoClient(settings.ConnectionString);
        });

        services.AddScoped<IMongoDatabase>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var client = serviceProvider.GetRequiredService<IMongoClient>();

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new InvalidOperationException($"'{MongoDbSettings.SectionName}:DatabaseName' not configured.");

            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddScoped<IMongoCollection<User>>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            var database = serviceProvider.GetRequiredService<IMongoDatabase>();

            return database.GetCollection<User>(settings.UsersCollectionName);
        });

        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
