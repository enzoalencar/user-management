using Microsoft.IdentityModel.Tokens;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.ChangePassword;

public sealed class ChangePasswordHandler(IUserRepository repository)
{
    public async Task Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("User Id is required.");

        var user = await repository.FindOneAsync(request.Id, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.Password))
            throw new SecurityTokenException("Current password is invalid.");

        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        if (!await repository.UpdateAsync(user, cancellationToken))
            throw new InvalidOperationException("Error updating password.");
    }
}
