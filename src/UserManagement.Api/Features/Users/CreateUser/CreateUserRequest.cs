using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Users.CreateUser;

public class CreateUserRequest
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must have at least 6 characters")]
    public string Password { get; set; } = string.Empty;
    
    public string DocumentNumber { get; set; } = string.Empty;
    
    public List<string> PhoneNumber { get; set; } = [];
}
