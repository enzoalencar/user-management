using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Users.FindOneUser;

public class FindOneUserRequest
{
    [Required(ErrorMessage = "User Id is required")]
    public Guid Id { get; set; }
}