using MongoDB.Bson;
using MongoDB.Driver;
using UserManagement.Domain.Users;
using UserManagement.Infrastructure.Persistence.Mongo;
using Xunit;

namespace UserManagement.IntegrationTests;

public sealed class MongoFixture : IAsyncLifetime
{
    private const string DefaultConnectionString = "mongodb://localadmin:localadmin@localhost:27017/?authSource=admin";
    private readonly MongoClient _client;

    private readonly string _dbName = $"user_{Guid.NewGuid():N}";

    public IUserRepository Repository { get; private set; } = default!;

    public MongoFixture()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        settings.ConnectTimeout = TimeSpan.FromSeconds(5);

        _client = new MongoClient(settings);
    }

    public async Task InitializeAsync()
    {
        UserBsonClassMap.Register();

        try
        {
            await _client.GetDatabase("admin")
                .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Não foi possível conectar no MongoDB para os testes de integração. " +
                "Inicie o container (docker compose up -d mongodb) " +
                "ou ajuste a variável MONGO_CONNECTION_STRING.",
                ex);
        }

        var db = _client.GetDatabase(_dbName);
        var usersCollection = db.GetCollection<User>("users");
        Repository = new UserRepository(usersCollection);
    }
    
    public async Task DisposeAsync()
    {
        try
        {
            await _client.DropDatabaseAsync(_dbName);
        }
        catch
        {
            // Ignore cleanup errors when Mongo is unavailable.
        }
    }
}
