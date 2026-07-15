using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Domain.Users;

namespace UserManagement.Api.Features.Auth.Authorization;

public sealed class OwnerOrAdministratorRequirement : IAuthorizationRequirement;

public sealed class OwnerOrAdministratorHandler
    : AuthorizationHandler<OwnerOrAdministratorRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdministratorRequirement requirement)
    {
        if (context.User.HasClaim(AuthClaimTypes.Role, UserRole.Administrator.ToString()))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext ||
            !Guid.TryParse(httpContext.Request.RouteValues["id"]?.ToString(), out var resourceOwnerId) ||
            !Guid.TryParse(context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var authenticatedUserId))
            return Task.CompletedTask;

        if (authenticatedUserId == resourceOwnerId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
