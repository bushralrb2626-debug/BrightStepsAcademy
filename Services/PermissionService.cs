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

    public Task<bool> HasAsync(string userId, Guid? schoolId, string permissionCode)
        => HasAnyAsync(userId, schoolId, permissionCode);

    public async Task<bool> HasAnyAsync(string userId, Guid? schoolId, params string[] permissionCodes)
    {
        if (string.IsNullOrWhiteSpace(userId) || permissionCodes.Length == 0)
            return false;

        var expanded = ExpandCodes(permissionCodes);
        var granted = await GetGrantedCodesAsync(userId, schoolId);
        return expanded.Any(granted.Contains);
    }

    public async Task<IReadOnlySet<string>> GetGrantedCodesAsync(string userId, Guid? schoolId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (await _userManager.IsInRoleAsync(user, AppRoleNames.SuperAdmin))
            return AllCodes();

        if (await _userManager.IsInRoleAsync(user, AppRoleNames.SchoolAdmin))
            return AllCodes();

        if (schoolId.HasValue)
        {
            var isPrimaryAdmin = await _db.SchoolAdminProfiles.AsNoTracking()
                .AnyAsync(p =>
                    p.UserId == userId
                    && p.SchoolId == schoolId.Value
                    && p.IsPrimary
                    && p.IsActive);

            if (isPrimaryAdmin)
                return AllCodes();
        }

        if (!schoolId.HasValue)
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var codes = await _db.UserPermissionGrants.AsNoTracking()
            .Where(g =>
                g.UserId == userId
                && g.SchoolId == schoolId.Value
                && g.Granted
                && g.IsActive)
            .Select(g => g.PermissionCode)
            .ToListAsync();

        return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> AllCodes()
        => PermissionCatalog.All.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static IEnumerable<string> ExpandCodes(IEnumerable<string> requested)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in requested)
        {
            if (string.IsNullOrWhiteSpace(code)) continue;
            set.Add(code);
            if (PermissionCatalog.ImpliedBy.TryGetValue(code, out var implied))
            {
                foreach (var alt in implied)
                    set.Add(alt);
            }
        }
        return set;
    }
}
