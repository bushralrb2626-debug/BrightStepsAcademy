using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PermissionService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<bool> HasAsync(string userId, Guid? schoolId, string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(permissionCode))
            return false;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return false;

        if (await _userManager.IsInRoleAsync(user, AppRoleNames.SuperAdmin))
            return true;

        if (schoolId.HasValue)
        {
            var isPrimaryAdmin = await _db.SchoolAdminProfiles.AsNoTracking()
                .AnyAsync(p =>
                    p.UserId == userId
                    && p.SchoolId == schoolId.Value
                    && p.IsPrimary
                    && p.IsActive);

            if (isPrimaryAdmin)
                return true;
        }

        if (!schoolId.HasValue)
            return false;

        return await _db.UserPermissionGrants.AsNoTracking()
            .AnyAsync(g =>
                g.UserId == userId
                && g.SchoolId == schoolId.Value
                && g.PermissionCode == permissionCode
                && g.Granted
                && g.IsActive);
    }
}
