using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Authorize(Roles = $"{AppRoleNames.SchoolAdmin},{AppRoleNames.CustomAdmin}")]
public abstract class SchoolManageControllerBase : Controller
{
    protected readonly AppDbContext Db;
    protected readonly ITenantContext Tenant;
    protected readonly IPermissionService PermissionService;
    protected readonly IAuditService Audit;
    protected readonly UserManager<ApplicationUser> UserManager;

    protected SchoolManageControllerBase(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
    {
        Db = db;
        Tenant = tenant;
        PermissionService = permissions;
        Audit = audit;
        UserManager = userManager;
    }

    protected Guid SchoolId { get; private set; }
    protected string CurrentUserId => Tenant.UserId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (User.IsInRole(AppRoleNames.SuperAdmin))
        {
            context.Result = RedirectToAction("Index", "Home");
            return;
        }

        if (!Tenant.SchoolId.HasValue)
        {
            context.Result = new ForbidResult();
            return;
        }

        SchoolId = Tenant.SchoolId.Value;
        ViewBag.SchoolId = SchoolId;
        ViewBag.CurrentUserId = CurrentUserId;
        ViewBag.NavPermissions = await BuildNavPermissionsAsync();

        var brand = await Db.Schools.AsNoTracking()
            .Where(s => s.Id == SchoolId)
            .Select(s => new { s.Name, s.ShortName, s.LogoPath, s.FaviconPath, s.Tagline })
            .FirstOrDefaultAsync();
        ViewBag.SchoolDisplayName = brand?.ShortName ?? brand?.Name ?? "School";
        ViewBag.SchoolFullName = brand?.Name ?? "School";
        ViewBag.SchoolLogoPath = brand?.LogoPath;
        ViewBag.SchoolFaviconPath = brand?.FaviconPath;
        ViewBag.SchoolTagline = brand?.Tagline;

        await next();
    }

    protected ViewResult SchoolView(string relativePath, object? model = null)
        => View($"~/Views/Manage/School/{relativePath}.cshtml", model);

    protected async Task<bool> CanAsync(string permissionCode)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
            return false;
        return await PermissionService.HasAsync(CurrentUserId, SchoolId, permissionCode);
    }

    protected async Task<IActionResult?> ForbidUnlessAsync(string permissionCode)
    {
        if (await CanAsync(permissionCode))
            return null;
        return Forbid();
    }

    protected void SetFlash(string message, string type = "success")
    {
        TempData["Flash"] = message;
        TempData["FlashType"] = type;
    }

    private async Task<Dictionary<string, bool>> BuildNavPermissionsAsync()
    {
        var codes = new[]
        {
            PermissionCodes.SchoolProfile,
            PermissionCodes.WebsiteManage,
            PermissionCodes.BuildingsView,
            PermissionCodes.BuildingsManage,
            PermissionCodes.FloorsManage,
            PermissionCodes.RoomsView,
            PermissionCodes.RoomsManage,
            PermissionCodes.FurnitureManage,
            PermissionCodes.StaffView,
            PermissionCodes.StaffManage,
            PermissionCodes.StudentsView,
            PermissionCodes.StudentsManage,
            PermissionCodes.AdminsManage,
            PermissionCodes.PermissionsManage,
            PermissionCodes.ReportsView
        };

        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var code in codes)
            map[code] = await CanAsync(code);
        return map;
    }
}
