using MongoDB.Driver;
using MongoDB.Driver.Linq;
using UserManagement.Domain.Users;

namespace UserManagement.Infrastructure.Persistence.Mongo;

public sealed class UserRepository(IMongoCollection<User> usersCollection) : IUserRepository
{
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        await usersCollection.InsertOneAsync(user, cancellationToken: cancellationToken);
        return user;
    }
    
    public async Task<User?> FindOneAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(o => o.Id, userId);
        return await usersCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<List<User>?> FindAllAsync(CancellationToken cancellationToken = default)
    {
        return await usersCollection.AsQueryable().ToListAsync(cancellationToken);
    }
    
    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(o => o.Id, user.Id);
        var result = await usersCollection.ReplaceOneAsync(filter, user, cancellationToken: cancellationToken);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(o => o.Id, userId);
        var result = await usersCollection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<User?> FindOneByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var filter = Builders<User>.Filter.Eq(o => o.Email, email);
        return await usersCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }
}
