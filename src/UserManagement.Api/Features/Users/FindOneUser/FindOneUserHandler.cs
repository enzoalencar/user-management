using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.FindOneUser;

public static class FindOneUserHandler
{
    public static async Task<IResult> Handle(
        FindOneUserRequest request,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
            throw new ArgumentException("User Id is required.");
        
        var user = await userRepository.FindOneAsync(request.Id, cancellationToken);
        
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var result = new FindOneUserResult
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
        
        return Results.Ok(result);
    }
}