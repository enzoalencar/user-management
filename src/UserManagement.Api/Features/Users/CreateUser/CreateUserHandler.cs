using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.CreateUser;

public static class CreateUserHandler
{
    public static async Task<IResult> Handle(
        CreateUserRequest request,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("First name and Email are required.");

        var userToCreate = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth.ToUniversalTime(),
            Email = request.Email,
            DocumentNumber = request.DocumentNumber,
            PhoneNumber = request.PhoneNumber ?? [],
            IsActive = true
        };

        var user = await userRepository.CreateAsync(userToCreate, cancellationToken);

        var response = new CreateUserResponse
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

        return Results.Created($"/users/{user.Id}", response);
    }
}
