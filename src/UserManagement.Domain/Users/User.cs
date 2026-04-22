namespace UserManagement.Domain.Users;

public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    // TODO: change primitive types to value objects
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public List<string> PhoneNumber { get; set; } = [];
    
    public bool IsActive { get; set; }
}
