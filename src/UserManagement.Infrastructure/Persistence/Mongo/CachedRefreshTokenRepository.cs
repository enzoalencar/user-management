using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UserManagement.Domain.Auth;

namespace UserManagement.Infrastructure.Persistence.Mongo;

public sealed class CachedRefreshTokenRepository(
    RefreshTokenRepository mongoRefreshTokenRepository,
    IDistributedCache distributedCache,
    ILogger<CachedRefreshTokenRepository> logger) : IRefreshTokenRepository
{
    private const string KeyPrefix = "auth:refresh-token:";

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        var createdToken = await mongoRefreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
        await TryWriteCacheAsync(createdToken, cancellationToken);
        return createdToken;
    }

    public async Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(tokenHash);
        var cachedToken = await TryReadCacheAsync(cacheKey, cancellationToken);

        if (cachedToken is not null)
            return cachedToken;

        var mongoToken = await mongoRefreshTokenRepository.FindByTokenHashAsync(tokenHash, cancellationToken);
        if (mongoToken is not null)
            await TryWriteCacheAsync(mongoToken, cancellationToken);

        return mongoToken;
    }

    public async Task<bool> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        var updated = await mongoRefreshTokenRepository.UpdateAsync(refreshToken, cancellationToken);
        if (!updated)
            return false;

        if (refreshToken.RevokedAtUtc is not null || refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await TryRemoveCacheAsync(BuildCacheKey(refreshToken.TokenHash), cancellationToken);
            return true;
        }

        await TryWriteCacheAsync(refreshToken, cancellationToken);
        return true;
    }

    private async Task<RefreshToken?> TryReadCacheAsync(string cacheKey, CancellationToken cancellationToken)
    {
        try
        {
            var cachedPayload = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (string.IsNullOrWhiteSpace(cachedPayload))
                return null;

            var cachedToken = JsonSerializer.Deserialize<RefreshToken>(cachedPayload);
            if (cachedToken is not null)
                return cachedToken;

            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
            return null;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis cache read failed for refresh token key '{CacheKey}'. Falling back to MongoDB.",
                cacheKey);
            return null;
        }
    }

    private async Task TryWriteCacheAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        var absoluteExpiration = refreshToken.ExpiresAtUtc - DateTime.UtcNow;
        if (absoluteExpiration <= TimeSpan.Zero)
        {
            await TryRemoveCacheAsync(BuildCacheKey(refreshToken.TokenHash), cancellationToken);
            return;
        }

        var entryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration
        };

        try
        {
            var serializedToken = JsonSerializer.Serialize(refreshToken);
            await distributedCache.SetStringAsync(
                BuildCacheKey(refreshToken.TokenHash),
                serializedToken,
                entryOptions,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis cache write failed for refresh token hash '{RefreshTokenHash}'. Continuing with MongoDB only.",
                refreshToken.TokenHash);
        }
    }

    private async Task TryRemoveCacheAsync(string cacheKey, CancellationToken cancellationToken)
    {
        try
        {
            await distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Redis cache remove failed for refresh token key '{CacheKey}'. Continuing with MongoDB only.",
                cacheKey);
        }
    }

    private static string BuildCacheKey(string tokenHash) => $"{KeyPrefix}{tokenHash}";
}
