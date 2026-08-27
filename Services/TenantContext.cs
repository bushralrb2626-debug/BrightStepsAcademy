using System.Security.Claims;
using BrightStepsAcademy.Data;

namespace BrightStepsAcademy.Services;

public class TenantContext : ITenantContext
{
    public Guid? SchoolId { get; }
    public string? UserId { get; }
    public bool IsSuperAdmin { get; }

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return;

        UserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var schoolClaim = user.FindFirstValue("school_id");
        if (Guid.TryParse(schoolClaim, out var schoolId))
            SchoolId = schoolId;

        IsSuperAdmin = user.IsInRole(AppRoleNames.SuperAdmin)
            || user.Claims.Any(c =>
                c.Type == ClaimTypes.Role
                && string.Equals(c.Value, AppRoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
    }
}
