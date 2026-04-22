using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Users.UpdateUser;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "User Id is required")]
    public Guid Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
    
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
    
    public string DocumentNumber { get; set; } = string.Empty;
    
    public List<string> PhoneNumber { get; set; } = [];
}
