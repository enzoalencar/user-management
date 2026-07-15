using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Users.UpdateUser;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "User Id is required")]
    public Guid Id { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }

    public string? DocumentNumber { get; set; }
    
    public List<string>? PhoneNumber { get; set; }
}
