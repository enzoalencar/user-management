using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.DeleteUser;

public static class DeleteUserHandler
{
    public static async Task<IResult> Handle(
        DeleteUserRequest request,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        await userRepository.DeleteAsync(request.Id, cancellationToken);

        return Results.Ok();
    }
}