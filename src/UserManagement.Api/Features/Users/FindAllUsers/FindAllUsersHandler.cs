using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.FindAllUsers;

public class FindAllUsersHandler
{
    public static async Task<IResult> Handle(
        FindAllUsersRequest request,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.FindAllAsync(cancellationToken);
        
        if (users == null || users.Count == 0)
            return Results.BadRequest(new { message = "User not found." });

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
        
        return Results.Ok(result);
    }
}