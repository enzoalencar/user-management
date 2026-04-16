namespace UserManagement.Domain.Users;

public interface IUserRepository
{
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> FindOneAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<User>?> FindAllAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
