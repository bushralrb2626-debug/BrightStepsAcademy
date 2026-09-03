using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School")]
public class SchoolDashboardController : SchoolManageControllerBase
{
    private readonly IAccountEmailNotificationService _accountEmails;

    public SchoolDashboardController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager,
        IAccountEmailNotificationService accountEmails)
        : base(db, tenant, permissions, audit, userManager)
    {
        _accountEmails = accountEmails;
    }

    [HttpGet("")]
    [HttpGet("Dashboard")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var school = await Db.Schools.AsNoTracking()
            .Where(s => s.Id == SchoolId)
            .Select(s => new { s.Name, s.LogoPath })
            .FirstOrDefaultAsync(ct);

        var teacherCatIds = await Db.StaffCategories.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive && c.Name.ToLower().Contains("teacher"))
            .Select(c => c.Id)
            .ToListAsync(ct);

        var staffByCategory = await Db.StaffMembers.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .GroupBy(s => s.StaffCategory.Name)
            .Select(g => new ChartSliceVm { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(ct);

        var roomsByType = await Db.Rooms.AsNoTracking()
            .Where(r => r.SchoolId == SchoolId && r.IsActive)
            .GroupBy(r => string.IsNullOrWhiteSpace(r.RoomType) ? "Other" : r.RoomType)
            .Select(g => new ChartSliceVm { Label = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(ct);

        var furnitureRaw = await Db.FurnitureItems.AsNoTracking()
            .Where(f => f.SchoolId == SchoolId && f.IsActive)
            .GroupBy(f => f.Condition)
            .Select(g => new { Condition = g.Key, Count = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);
        var furnitureByCondition = furnitureRaw
            .Select(g => new ChartSliceVm { Label = g.Condition.ToString(), Count = g.Count })
            .OrderByDescending(x => x.Count)
            .ToList();

        var studentsByClass = await Db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .GroupBy(s => string.IsNullOrWhiteSpace(s.ClassName)
                ? "Unassigned"
                : (string.IsNullOrWhiteSpace(s.Section) ? s.ClassName! : s.ClassName + " · " + s.Section))
            .Select(g => new ChartSliceVm { Label = g.Key, Count = g.Count() })
            .OrderBy(x => x.Label)
            .Take(12)
            .ToListAsync(ct);

        var recent = await Db.AuditLogs.AsNoTracking()
            .Where(a => a.SchoolId == SchoolId)
            .ToListOrderedByDescendingAsync(a => a.Timestamp, take: 8, ct);

        var vm = new SchoolDashboardVm
        {
            SchoolName = school?.Name ?? "School",
            LogoPath = school?.LogoPath,
            Buildings = await Db.Buildings.CountAsync(b => b.SchoolId == SchoolId && b.IsActive, ct),
            Floors = await Db.Floors.CountAsync(f => f.SchoolId == SchoolId && f.IsActive, ct),
            Rooms = await Db.Rooms.CountAsync(r => r.SchoolId == SchoolId && r.IsActive, ct),
            Furniture = await Db.FurnitureItems.CountAsync(f => f.SchoolId == SchoolId && f.IsActive, ct),
            Staff = await Db.StaffMembers.CountAsync(s => s.SchoolId == SchoolId && s.IsActive, ct),
            Teachers = await Db.StaffMembers.CountAsync(
                s => s.SchoolId == SchoolId && s.IsActive && teacherCatIds.Contains(s.StaffCategoryId), ct),
            Students = await Db.StudentRecords.CountAsync(s => s.SchoolId == SchoolId && s.IsActive, ct),
            Admins = await Db.SchoolAdminProfiles.CountAsync(a => a.SchoolId == SchoolId && a.IsActive, ct),
            Facilities = await Db.FacilityItems.CountAsync(f => f.SchoolId == SchoolId && f.IsActive, ct),
            StaffByCategory = staffByCategory,
            RoomsByType = roomsByType,
            FurnitureByCondition = furnitureByCondition,
            StudentsByClass = studentsByClass,
            RecentActivity = recent
        };

        return SchoolView("Dashboard/Index", vm);
    }

    [HttpGet("Visits")]
    public async Task<IActionResult> Visits(CancellationToken ct)
    {
        var visits = await Db.CampusVisits.AsNoTracking()
            .Where(v => v.SchoolId == SchoolId)
            .OrderByDescending(v => v.CreatedAt)
            .Take(200)
            .ToListAsync(ct);
        ViewData["Title"] = "Campus visits";
        return SchoolView("Dashboard/Visits", visits);
    }

    [HttpGet("Explorer")]
    public async Task<IActionResult> Explorer(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsView) is { } deny)
            return deny;

        var buildings = await Db.Buildings.AsNoTracking()
            .Include(b => b.Floors).ThenInclude(f => f.Rooms).ThenInclude(r => r.FurnitureItems)
            .Where(b => b.SchoolId == SchoolId)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);

        var tree = buildings.Select(b => new ExplorerBuildingVm
        {
            Id = b.Id,
            Name = b.Name,
            IsActive = b.IsActive,
            Floors = b.Floors.OrderBy(f => f.FloorNumber).Select(f => new ExplorerFloorVm
            {
                Id = f.Id,
                FloorNumber = f.FloorNumber,
                FloorName = f.FloorName,
                IsActive = f.IsActive,
                Rooms = f.Rooms.OrderBy(r => r.RoomNumber).Select(r => new ExplorerRoomVm
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomName = r.RoomName,
                    IsActive = r.IsActive,
                    FurnitureCount = r.FurnitureItems.Count(fi => fi.IsActive)
                }).ToList()
            }).ToList()
        }).ToList();

        return SchoolView("Explorer/Index", tree);
    }

    [HttpGet("Account")]
    public IActionResult Account() => SchoolView("Account/Index");

    [HttpGet("Security")]
    public async Task<IActionResult> Security()
    {
        var user = await UserManager.GetUserAsync(User);
        ViewBag.MustChangePassword = user?.MustChangePassword == true;
        return SchoolView("Security/Index", new ChangePasswordVm());
    }

    [HttpPost("Security")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Security(ChangePasswordVm model, CancellationToken ct = default)
    {
        var user = await UserManager.GetUserAsync(User);
        ViewBag.MustChangePassword = user?.MustChangePassword == true;
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
            return SchoolView("Security/Index", model);

        var result = await UserManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return SchoolView("Security/Index", model);
        }

        user.MustChangePassword = false;
        await UserManager.UpdateAsync(user);
        await _accountEmails.SendPasswordChangedEmailAsync(user.Id, ct);
        SetFlash("Password updated successfully.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        var items = await Db.AppNotifications.AsNoTracking()
            .Where(n => n.UserId == CurrentUserId && (n.SchoolId == null || n.SchoolId == SchoolId))
            .ToListOrderedByDescendingAsync(n => n.CreatedAt, take: 100, ct);
        return SchoolView("Notifications/Index", items);
    }

    [HttpGet("Activity")]
    public async Task<IActionResult> Activity(CancellationToken ct)
    {
        var logs = await Db.AuditLogs.AsNoTracking()
            .Where(a => a.SchoolId == SchoolId)
            .ToListOrderedByDescendingAsync(a => a.Timestamp, take: 200, ct);
        return SchoolView("Activity/Index", logs);
    }

    [HttpGet("Permissions")]
    public async Task<IActionResult> Permissions(string? userId, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.PermissionsManage) is { } deny)
            return deny;

        var admins = await (
            from p in Db.SchoolAdminProfiles.AsNoTracking()
            join u in Db.Users.AsNoTracking() on p.UserId equals u.Id
            where p.SchoolId == SchoolId && p.IsActive
            orderby p.IsPrimary descending, u.FullName
            select new AdminListItemVm
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                AdminType = p.AdminType,
                IsPrimary = p.IsPrimary,
                IsActive = u.IsActive
            }).ToListAsync(ct);

        var allPerms = await Db.AppPermissions.AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .ToListAsync(ct);

        var selectedUserId = userId ?? admins.FirstOrDefault(a => !a.IsPrimary)?.UserId ?? admins.FirstOrDefault()?.UserId;
        var granted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(selectedUserId))
        {
            granted = (await Db.UserPermissionGrants.AsNoTracking()
                .Where(g => g.SchoolId == SchoolId && g.UserId == selectedUserId && g.Granted && g.IsActive)
                .Select(g => g.PermissionCode)
                .ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        var vm = new PermissionsPageVm
        {
            Admins = admins,
            Permissions = allPerms,
            SelectedUserId = selectedUserId,
            GrantedCodes = granted
        };
        return SchoolView("Permissions/Index", vm);
    }

    [HttpPost("Permissions")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(string userId, string[]? permissionCodes, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.PermissionsManage) is { } deny)
            return deny;

        var profile = await Db.SchoolAdminProfiles
            .FirstOrDefaultAsync(p => p.SchoolId == SchoolId && p.UserId == userId && p.IsActive, ct);
        if (profile is null)
        {
            SetFlash("Admin not found.", "error");
            return RedirectToAction(nameof(Permissions));
        }

        if (profile.IsPrimary)
        {
            SetFlash("Primary school admin already has all permissions.", "error");
            return RedirectToAction(nameof(Permissions), new { userId });
        }

        var selected = new HashSet<string>(permissionCodes ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        var validCodes = PermissionCatalog.All.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        selected.IntersectWith(validCodes);
        var existing = await Db.UserPermissionGrants
            .Where(g => g.SchoolId == SchoolId && g.UserId == userId)
            .ToListAsync(ct);

        foreach (var grant in existing)
        {
            grant.Granted = selected.Contains(grant.PermissionCode);
            grant.IsActive = grant.Granted;
            grant.UpdatedAt = DateTimeOffset.UtcNow;
            grant.UpdatedByUserId = CurrentUserId;
            selected.Remove(grant.PermissionCode);
        }

        foreach (var code in selected)
        {
            Db.UserPermissionGrants.Add(new UserPermissionGrant
            {
                SchoolId = SchoolId,
                UserId = userId,
                PermissionCode = code,
                Granted = true,
                IsActive = true,
                CreatedByUserId = CurrentUserId
            });
        }

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Update", "Permissions", SchoolId, "User", userId, "Updated permission grants", ct);
        SetFlash("Permissions updated.");
        return RedirectToAction(nameof(Permissions), new { userId });
    }
}
