using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.CreateUser;

public sealed class CreateUserHandler(IUserRepository userRepository)
{
    public async Task<CreateUserResult> Handle(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("First name, email and password are required.");

        var userToCreate = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth.ToUniversalTime(),
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DocumentNumber = request.DocumentNumber,
            PhoneNumber = request.PhoneNumber ?? [],
            IsActive = true
        };

        var user = await userRepository.CreateAsync(userToCreate, cancellationToken);

        var response = new CreateUserResult
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            Email = user.Email,
            DocumentNumber = user.DocumentNumber,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive
        };

        return response;
    }
}
