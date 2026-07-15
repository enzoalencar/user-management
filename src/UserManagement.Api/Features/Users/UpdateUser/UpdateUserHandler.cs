using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.UpdateUser;

public sealed class UpdateUserHandler(IUserRepository repository)
{
    public async Task<UpdateUserResult> Handle(
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("User Id is required.");

        if (request.FirstName is null &&
            request.LastName is null &&
            request.DateOfBirth is null &&
            request.Email is null &&
            request.DocumentNumber is null &&
            request.PhoneNumber is null)
            throw new ArgumentException("At least one field must be provided.");

        var user = await repository.FindOneAsync(request.Id, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (request.FirstName is not null)
            user.FirstName = request.FirstName.Trim();

        if (request.LastName is not null)
            user.LastName = request.LastName.Trim();

        if (request.DateOfBirth.HasValue)
            user.DateOfBirth = request.DateOfBirth.Value.ToUniversalTime();

        if (request.Email is not null)
            user.Email = request.Email;

        if (request.DocumentNumber is not null)
            user.DocumentNumber = request.DocumentNumber;

        if (request.PhoneNumber is not null)
            user.PhoneNumber = request.PhoneNumber;

        var updated = await repository.UpdateAsync(user, cancellationToken);
        if (!updated) throw new Exception("Error updating user");

        var result = new UpdateUserResult
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive
        };
        
        return result;
    }
}
