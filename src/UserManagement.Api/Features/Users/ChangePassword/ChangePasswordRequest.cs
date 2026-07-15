using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Features.Users.ChangePassword;

public sealed class ChangePasswordRequest
{
    public Guid Id { get; set; }

    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}
