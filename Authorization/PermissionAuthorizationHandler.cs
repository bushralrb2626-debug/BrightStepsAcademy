using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BrightStepsAcademy.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissions;
    private readonly ITenantContext _tenant;

    public PermissionAuthorizationHandler(IPermissionService permissions, ITenantContext tenant)
    {
        _permissions = permissions;
        _tenant = tenant;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return;

        if (await _permissions.HasAsync(userId, _tenant.SchoolId, requirement.Code))
            context.Succeed(requirement);
    }
}
