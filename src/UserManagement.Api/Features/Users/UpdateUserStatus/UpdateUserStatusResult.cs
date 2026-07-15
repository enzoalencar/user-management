namespace UserManagement.Api.Features.Users.UpdateUserStatus;

public sealed class UpdateUserStatusResult
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
