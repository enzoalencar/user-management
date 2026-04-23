namespace UserManagement.Infrastructure.Persistence.Mongo;

public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = string.Empty;
    public string DatabaseName { get; init; } = string.Empty;
    public string UsersCollectionName { get; init; } = "users";
    public string RefreshTokensCollectionName { get; init; } = "refresh_tokens";
}
