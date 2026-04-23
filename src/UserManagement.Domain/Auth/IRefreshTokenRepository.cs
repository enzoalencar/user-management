namespace UserManagement.Domain.Auth;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}
