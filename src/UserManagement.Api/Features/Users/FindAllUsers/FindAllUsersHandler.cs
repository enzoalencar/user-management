using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.FindAllUsers;

public sealed class FindAllUsersHandler(IUserRepository repository)
{
    public async Task<List<FindAllUsersResult>> Handle(
        FindAllUsersRequest request,
        CancellationToken cancellationToken)
    {
        var users = await repository.FindAllAsync(cancellationToken);
        
        if (users == null || users.Count == 0)
            throw new KeyNotFoundException("Users not found.");

        var result = new List<FindAllUsersResult>();
        foreach (var user in users)
        {
            var mapUser = new FindAllUsersResult()
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
            
            result.Add(mapUser);
        }
        
        return result;
    }
}