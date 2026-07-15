using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.FindOneUser;

public sealed class FindOneUserHandler(IUserRepository userRepository)
{
    public async Task<FindOneUserResult> Handle(
        FindOneUserRequest request,
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
            Email = user.Email,
            IsActive = user.IsActive
        };
        
        return result;
    }
}
