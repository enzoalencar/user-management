using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Auth.Login;

public class LoginRequest
{
    [Required]
    public required string Email { get; set; }
    
    [Required]
    public required string Password { get; set; }
}