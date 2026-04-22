using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.UpdateUser;

public static class UpdateUserHandler
{
    public static async Task<IResult> Handle(
        UpdateUserRequest request,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("User Id is required.");

        var existingUser = await userRepository.FindOneAsync(request.Id, cancellationToken);
        if (existingUser == null)
            throw new KeyNotFoundException("User not found.");

        existingUser.FirstName = request.FirstName.Trim();
        existingUser.LastName = request.LastName.Trim();
        existingUser.DateOfBirth = request.DateOfBirth.ToUniversalTime();
        existingUser.Email = request.Email;
        existingUser.DocumentNumber = request.DocumentNumber;
        existingUser.PhoneNumber = request.PhoneNumber ?? [];
        existingUser.IsActive = true;

        if (!string.IsNullOrWhiteSpace(request.Password))
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var result = await userRepository.UpdateAsync(existingUser, cancellationToken);
        
        return Results.Ok(result);
    }
}
