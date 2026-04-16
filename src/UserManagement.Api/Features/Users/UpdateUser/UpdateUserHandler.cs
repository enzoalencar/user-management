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
            return Results.BadRequest(new { message = "User Id are required." });
        
        var userToUpdate = new User
        {
            Id = request.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth.ToUniversalTime(),
            Email = request.Email,
            DocumentNumber = request.DocumentNumber,
            PhoneNumber = request.PhoneNumber ?? [],
            IsActive = true
        };
        
        var  result = await userRepository.UpdateAsync(userToUpdate, cancellationToken);
        
        return Results.Ok(result);
    }
}