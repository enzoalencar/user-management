namespace UserManagement.Infrastructure.Caching.Redis;

public sealed class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
}
