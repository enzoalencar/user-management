namespace UserManagement.Api.Features.Users.UpdateUserStatus;

public sealed class UpdateUserStatusRequest
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
