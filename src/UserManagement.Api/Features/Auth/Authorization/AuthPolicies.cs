namespace UserManagement.Api.Features.Auth.Authorization;

public static class AuthPolicies
{
    public const string AuthenticatedUser = "AuthenticatedUser";
    public const string ActiveUser = "ActiveUser";
    public const string Administrator = "Administrator";
    public const string OwnerOrAdministrator = "OwnerOrAdministrator";
}
