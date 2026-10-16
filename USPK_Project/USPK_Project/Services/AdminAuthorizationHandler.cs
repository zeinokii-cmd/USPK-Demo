using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using USPK_Project.IServices;

namespace USPK_Project.Services;

public sealed class AdminRoleRequirement : IAuthorizationRequirement;

public sealed class AdminAuthorizationHandler(IServiceProvider services, IConfiguration configuration)
    : AuthorizationHandler<AdminRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRoleRequirement requirement)
    {
        var adminRole = configuration["Authorization:AdministratorRole"];
        if (string.IsNullOrWhiteSpace(adminRole) ||
            !context.User.IsInRole(adminRole) ||
            !int.TryParse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId)) return;

        var users = services.GetService<IUserStore>();
        if (users is null) return;
        var user = await users.FindByIdAsync(userId, CancellationToken.None);
        if (string.Equals(user?.Role, adminRole, StringComparison.Ordinal)) context.Succeed(requirement);
    }
}
