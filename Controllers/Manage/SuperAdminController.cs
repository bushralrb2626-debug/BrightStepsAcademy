using System.Text.Json;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

public class SuperAdminController : SuperAdminControllerBase
{
    public SuperAdminController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFileStorageService files)
        : base(db, userManager, signInManager, files)
    {
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        await RefreshAllSubscriptionsAsync(ct);

        var schools = Db.Schools.AsNoTracking();
        var totalSchools = await schools.CountAsync(ct);
        var active = await schools.CountAsync(s => s.Status == SchoolStatus.Active, ct);
        var inactive = await schools.CountAsync(s => s.Status == SchoolStatus.Inactive, ct);
        var pending = await schools.CountAsync(s => s.Status == SchoolStatus.Pending, ct);
        var suspended = await schools.CountAsync(s => s.Status == SchoolStatus.Suspended, ct);

        var schoolAdminUsers = await UserManager.GetUsersInRoleAsync(AppRoleNames.SchoolAdmin);
        var totalAdmins = schoolAdminUsers.Count;

        var subs = Db.SchoolSubscriptions.AsNoTracking();
        var activeSubs = await subs.CountAsync(s =>
            s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial, ct);
        var expiredSubs = await subs.CountAsync(s => s.Status == SubscriptionStatus.Expired, ct);
        var expiringSoon = await subs.CountAsync(s => s.Status == SubscriptionStatus.ExpiringSoon, ct);

        var recentSchools = (await ProjectSchoolList(Db.Schools.AsNoTracking()).ToListAsync(ct))
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .ToList();

        var expiringSchools = await (
            from s in Db.Schools.AsNoTracking()
            join sub in Db.SchoolSubscriptions.AsNoTracking() on s.Id equals sub.SchoolId
            where sub.Status == SubscriptionStatus.ExpiringSoon
            orderby sub.ExpiryDate
            select new SchoolListItemVm
            {
                Id = s.Id,
                Name = s.Name,
                SchoolCode = s.SchoolCode,
                City = s.City,
                Email = s.Email,
                Phone = s.Phone,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
                HasAdmin = s.AdminProfiles.Any(),
                PlanName = sub.PlanName,
                SubscriptionStatus = sub.Status,
                ExpiryDate = sub.ExpiryDate
            }).Take(8).ToListAsync(ct);

        var recentLogs = await Db.AuditLogs.AsNoTracking()
            .ToListOrderedByDescendingAsync(a => a.Timestamp, take: 8, ct);

        var userId = UserManager.GetUserId(User) ?? string.Empty;
        var recentNotifications = await Db.AppNotifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .ToListOrderedByDescendingAsync(n => n.CreatedAt, take: 6, ct);

        var growth = await BuildGrowthPointsAsync(ct);

        var vm = new SuperAdminDashboardVm
        {
            TotalSchools = totalSchools,
            ActiveSchools = active,
            InactiveSchools = inactive,
            PendingSchools = pending,
            SuspendedSchools = suspended,
            TotalSchoolAdmins = totalAdmins,
            ActiveSubscriptions = activeSubs,
            ExpiredSubscriptions = expiredSubs,
            ExpiringSoon = expiringSoon,
            RecentSchools = recentSchools,
            ExpiringSchools = expiringSchools,
            RecentAuditLogs = recentLogs,
            RecentNotifications = recentNotifications,
            GrowthPoints = growth
        };

        return ManageView("Index", vm);
    }

    [HttpGet("Visits")]
    public async Task<IActionResult> Visits(CancellationToken ct)
    {
        var visits = await Db.CampusVisits.AsNoTracking()
            .Include(v => v.School)
            .OrderByDescending(v => v.CreatedAt)
            .Take(300)
            .ToListAsync(ct);
        return ManageView("Visits", visits);
    }

    [HttpGet("Analytics")]
    public async Task<IActionResult> Analytics(CancellationToken ct)
    {
        await RefreshAllSubscriptionsAsync(ct);
        var growth = await BuildGrowthPointsAsync(ct);

        var schools = Db.Schools.AsNoTracking();
        var active = await schools.CountAsync(s => s.Status == SchoolStatus.Active, ct);
        var pending = await schools.CountAsync(s => s.Status == SchoolStatus.Pending, ct);
        var suspended = await schools.CountAsync(s => s.Status == SchoolStatus.Suspended, ct);
        var inactive = await schools.CountAsync(s => s.Status == SchoolStatus.Inactive, ct);
        var expiredSchool = await schools.CountAsync(s => s.Status == SchoolStatus.Expired, ct);

        var subs = Db.SchoolSubscriptions.AsNoTracking();
        var vm = new AnalyticsPageVm
        {
            GrowthPoints = growth,
            TotalSchools = await schools.CountAsync(ct),
            ActiveSchools = active,
            PendingSchools = pending,
            SuspendedSchools = suspended,
            ActiveSubscriptions = await subs.CountAsync(s =>
                s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial, ct),
            ExpiringSoon = await subs.CountAsync(s => s.Status == SubscriptionStatus.ExpiringSoon, ct),
            ExpiredSubscriptions = await subs.CountAsync(s => s.Status == SubscriptionStatus.Expired, ct),
            GrowthJson = JsonSerializer.Serialize(growth.Select(g => new { label = g.Label, count = g.Count })),
            StatusJson = JsonSerializer.Serialize(new[]
            {
                new { label = "Active", count = active },
                new { label = "Pending", count = pending },
                new { label = "Suspended", count = suspended },
                new { label = "Inactive", count = inactive },
                new { label = "Expired", count = expiredSchool }
            })
        };

        return ManageView("Analytics/Index", vm);
    }

    [HttpGet("Activity")]
    [HttpGet("AuditLogs")]
    public async Task<IActionResult> Activity(string? search, string? module, int page = 1, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        const int pageSize = 25;

        var query = Db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                (a.Details != null && a.Details.Contains(term)) ||
                a.Action.Contains(term) ||
                (a.UserName != null && a.UserName.Contains(term)) ||
                a.Module.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(a => a.Module == module);

        var total = await query.CountAsync(ct);
        var logs = await query
            .ToListOrderedByDescendingAsync(a => a.Timestamp, skip: (page - 1) * pageSize, take: pageSize, ct);

        return ManageView("Activity/Index", new AuditLogListVm
        {
            Search = search,
            ModuleFilter = module,
            Logs = logs,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    [HttpGet("Notifications")]
    public async Task<IActionResult> Notifications(int page = 1, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        const int pageSize = 25;
        var userId = UserManager.GetUserId(User) ?? string.Empty;

        var query = Db.AppNotifications.AsNoTracking()
            .Where(n => n.UserId == userId);

        var total = await query.CountAsync(ct);
        var items = await query
            .ToListOrderedByDescendingAsync(n => n.CreatedAt, skip: (page - 1) * pageSize, take: pageSize, ct);

        // Mark visible as read
        var unreadIds = items.Where(i => !i.IsRead).Select(i => i.Id).ToList();
        if (unreadIds.Count > 0)
        {
            var toUpdate = await Db.AppNotifications.Where(n => unreadIds.Contains(n.Id)).ToListAsync(ct);
            foreach (var n in toUpdate)
                n.IsRead = true;
            await Db.SaveChangesAsync(ct);
        }

        return ManageView("Notifications/Index", new NotificationListVm
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    [HttpGet("Settings")]
    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        var settings = await EnsurePlatformSettingsAsync(ct);
        return ManageView("Settings/Index", MapSettings(settings));
    }

    [HttpPost("Settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(PlatformSettingsVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ManageView("Settings/Index", model);

        var settings = await Db.PlatformSettings.FirstOrDefaultAsync(s => s.Id == model.Id, ct)
                       ?? await EnsurePlatformSettingsAsync(ct);

        settings.PlatformName = model.PlatformName.Trim();
        settings.SupportEmail = NullIfWhiteSpace(model.SupportEmail);
        settings.SupportPhone = NullIfWhiteSpace(model.SupportPhone);
        settings.Website = NullIfWhiteSpace(model.Website);
        settings.DefaultSubscriptionMonths = model.DefaultSubscriptionMonths;
        settings.ExpiryWarningDays = model.ExpiryWarningDays;
        settings.AvailablePlansJson = string.IsNullOrWhiteSpace(model.AvailablePlans)
            ? JsonSerializer.Serialize(SubscriptionStatusHelper.DefaultPlans)
            : JsonSerializer.Serialize(model.AvailablePlans.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
        settings.UpdatedAt = DateTimeOffset.UtcNow;

        await WriteAuditAsync(null, "PlatformSettingsUpdated", "Settings", nameof(PlatformSettings),
            settings.Id.ToString(), "Updated platform settings.", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = "Platform settings saved.";
        return RedirectToAction(nameof(Settings));
    }

    [HttpGet("Profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return ManageView("Profile/Index", new SuperAdminProfileVm
        {
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.PhoneNumber,
            LoginId = user.LoginId
        });
    }

    [HttpPost("Profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(SuperAdminProfileVm model, CancellationToken ct)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user is null) return Challenge();

        if (!ModelState.IsValid)
        {
            model.Email = user.Email;
            model.LoginId = user.LoginId;
            return ManageView("Profile/Index", model);
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = NullIfWhiteSpace(model.Phone);
        var result = await UserManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, FriendlyIdentityError(err.Description));
            model.Email = user.Email;
            model.LoginId = user.LoginId;
            return ManageView("Profile/Index", model);
        }

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet("ChangePassword")]
    public IActionResult ChangePassword()
        => ManageView("Profile/ChangePassword", new ChangePasswordVm());

    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ManageView("Profile/ChangePassword", model);

        var user = await UserManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await UserManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, FriendlyIdentityError(err.Description));
            return ManageView("Profile/ChangePassword", model);
        }

        user.MustChangePassword = false;
        await UserManager.UpdateAsync(user);
        await SignInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await SignInManager.SignOutAsync();
        return RedirectToAction("Login", "Portal");
    }

    private async Task<List<GrowthPointVm>> BuildGrowthPointsAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var start = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-11);
        var schools = await Db.Schools.AsNoTracking()
            .Where(s => s.CreatedAt >= start)
            .Select(s => s.CreatedAt)
            .ToListAsync(ct);

        var points = new List<GrowthPointVm>();
        for (var i = 0; i < 12; i++)
        {
            var month = start.AddMonths(i);
            var label = month.ToString("MMM yyyy");
            var count = schools.Count(d => d.Year == month.Year && d.Month == month.Month);
            points.Add(new GrowthPointVm { Label = label, Count = count });
        }

        return points;
    }

    private static IQueryable<SchoolListItemVm> ProjectSchoolList(IQueryable<School> query)
    {
        return query.Select(s => new SchoolListItemVm
        {
            Id = s.Id,
            Name = s.Name,
            SchoolCode = s.SchoolCode,
            City = s.City,
            Email = s.Email,
            Phone = s.Phone,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            HasAdmin = s.AdminProfiles.Any(),
            PlanName = s.Subscription != null ? s.Subscription.PlanName : null,
            SubscriptionStatus = s.Subscription != null ? s.Subscription.Status : null,
            ExpiryDate = s.Subscription != null ? s.Subscription.ExpiryDate : null
        });
    }

    private async Task<PlatformSettings> EnsurePlatformSettingsAsync(CancellationToken ct)
    {
        var settings = await Db.PlatformSettings.FirstOrDefaultOrderedByAsync(s => s.UpdatedAt, ct);
        if (settings is not null)
            return settings;

        settings = new PlatformSettings
        {
            PlatformName = "BrightSteps Platform",
            SupportEmail = "support@brightsteps.academy",
            DefaultSubscriptionMonths = 12,
            ExpiryWarningDays = 30,
            AvailablePlansJson = JsonSerializer.Serialize(SubscriptionStatusHelper.DefaultPlans),
            UpdatedAt = DateTimeOffset.UtcNow
        };
        Db.PlatformSettings.Add(settings);
        await Db.SaveChangesAsync(ct);
        return settings;
    }

    private static PlatformSettingsVm MapSettings(PlatformSettings s)
    {
        string? plans = null;
        if (!string.IsNullOrWhiteSpace(s.AvailablePlansJson))
        {
            try
            {
                var arr = JsonSerializer.Deserialize<string[]>(s.AvailablePlansJson);
                plans = arr is null ? s.AvailablePlansJson : string.Join(", ", arr);
            }
            catch
            {
                plans = s.AvailablePlansJson;
            }
        }

        return new PlatformSettingsVm
        {
            Id = s.Id,
            PlatformName = s.PlatformName,
            SupportEmail = s.SupportEmail,
            SupportPhone = s.SupportPhone,
            Website = s.Website,
            DefaultSubscriptionMonths = s.DefaultSubscriptionMonths,
            ExpiryWarningDays = s.ExpiryWarningDays,
            AvailablePlans = plans,
            LogoPath = s.LogoPath
        };
    }
}
