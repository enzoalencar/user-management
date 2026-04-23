using MongoDB.Driver;
using UserManagement.Domain.Auth;

namespace UserManagement.Infrastructure.Persistence.Mongo;

public sealed class RefreshTokenRepository(IMongoCollection<RefreshToken> refreshTokensCollection) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await refreshTokensCollection.InsertOneAsync(refreshToken, cancellationToken: cancellationToken);
        return refreshToken;
    }

    public async Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(token => token.TokenHash, tokenHash);
        return await refreshTokensCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        var filter = Builders<RefreshToken>.Filter.Eq(token => token.Id, refreshToken.Id);
        var result = await refreshTokensCollection.ReplaceOneAsync(
            filter,
            refreshToken,
            cancellationToken: cancellationToken);

        return result.MatchedCount > 0;
    }
}
