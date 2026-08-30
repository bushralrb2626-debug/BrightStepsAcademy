using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public static class StaffRoleHelper
{
    public static async Task EnsureTeacherRoleAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        Guid staffMemberId,
        CancellationToken ct = default)
    {
        var staff = await db.StaffMembers.AsNoTracking()
            .Include(s => s.StaffCategory)
            .FirstOrDefaultAsync(s => s.Id == staffMemberId, ct);
        if (staff?.UserId is null || staff.StaffCategory?.Name.Contains("teacher", StringComparison.OrdinalIgnoreCase) != true)
            return;

        var user = await userManager.FindByIdAsync(staff.UserId);
        if (user is null) return;

        if (!await userManager.IsInRoleAsync(user, AppRoleNames.Teacher))
            await userManager.AddToRoleAsync(user, AppRoleNames.Teacher);
    }
}
