using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Users.DeleteUser;

public class DeleteUserRequest
{
    [Required(ErrorMessage = "User Id is required")]
    public Guid Id { get; set; }
}