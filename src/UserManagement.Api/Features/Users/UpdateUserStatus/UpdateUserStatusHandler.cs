using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.UpdateUserStatus;

public sealed class UpdateUserStatusHandler(IUserRepository repository)
{
    public async Task<UpdateUserStatusResult> Handle(
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("User Id is required.");

        var user = await repository.FindOneAsync(request.Id, cancellationToken);
        if (user is null)
            throw new KeyNotFoundException("User not found.");

        user.IsActive = request.IsActive;

        if (!await repository.UpdateAsync(user, cancellationToken))
            throw new InvalidOperationException("Error updating user status.");

        return new UpdateUserStatusResult
        {
            Id = user.Id,
            IsActive = user.IsActive
        };
    }
}
