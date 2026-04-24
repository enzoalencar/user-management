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

        var existingUser = await repository.FindOneAsync(request.Id, cancellationToken);
        if (existingUser == null)
            throw new KeyNotFoundException("User not found.");

        // TODO: use immutable variables
        existingUser.FirstName = request.FirstName.Trim();
        existingUser.LastName = request.LastName.Trim();
        existingUser.DateOfBirth = request.DateOfBirth.ToUniversalTime();
        existingUser.Email = request.Email;
        existingUser.DocumentNumber = request.DocumentNumber;
        existingUser.PhoneNumber = request.PhoneNumber ?? [];
        existingUser.IsActive = true;

        if (!string.IsNullOrWhiteSpace(request.Password))
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var updated = await repository.UpdateAsync(existingUser, cancellationToken);
        if (!updated) throw new Exception("Error updating user");

        var result = new UpdateUserResult
        {
            Id = existingUser.Id,
            FirstName = existingUser.FirstName,
            LastName = existingUser.LastName,
            DateOfBirth = existingUser.DateOfBirth,
            Email = existingUser.Email,
            DocumentNumber = existingUser.DocumentNumber,
            PhoneNumber = existingUser.PhoneNumber,
            IsActive = existingUser.IsActive
        };
        
        return result;
    }
}
