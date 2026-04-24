using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Users.DeleteUser;

public sealed class DeleteUserHandler(IUserRepository repository)
{
    public async Task<DeleteUserResult> Handle(
        DeleteUserRequest request,
        CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(request.Id, cancellationToken);

        var result = new DeleteUserResult
        {
            Deleted = deleted
        };

        return result;
    }
}