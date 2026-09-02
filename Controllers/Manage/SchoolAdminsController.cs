using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Administrators")]
public class SchoolAdminsController : SchoolManageControllerBase
{
    private readonly IAccountEmailNotificationService _accountEmails;

    public SchoolAdminsController(
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
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.AdminsManage) is { } deny)
            return deny;

        var items = await (
            from p in Db.SchoolAdminProfiles.AsNoTracking()
            join u in Db.Users.AsNoTracking() on p.UserId equals u.Id
            where p.SchoolId == SchoolId
            orderby p.IsPrimary descending, u.FullName
            select new AdminListItemVm
            {
                UserId = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                AdminType = p.AdminType,
                IsPrimary = p.IsPrimary,
                IsActive = p.IsActive && u.IsActive
            }).ToListAsync(ct);

        return SchoolView("Administrators/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.AdminsManage) is { } deny)
            return deny;

        var vm = new CreateAdminVm
        {
            AllPermissions = await Db.AppPermissions.AsNoTracking()
                .OrderBy(p => p.Module).ThenBy(p => p.Name)
                .ToListAsync(ct)
        };
        return SchoolView("Administrators/Create", vm);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAdminVm model, string[]? permissionCodes, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.AdminsManage) is { } deny)
            return deny;

        model.AllPermissions = await Db.AppPermissions.AsNoTracking()
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .ToListAsync(ct);
        model.PermissionCodes = (permissionCodes ?? Array.Empty<string>()).ToList();

        if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email)
            || string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.LoginId))
        {
            ModelState.AddModelError(string.Empty, "Full name, email, login ID, and password are required.");
            return SchoolView("Administrators/Create", model);
        }

        var validCodes = PermissionCatalog.All.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        model.PermissionCodes = model.PermissionCodes
            .Where(c => validCodes.Contains(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            FullName = model.FullName.Trim(),
            LoginId = model.LoginId?.Trim(),
            SchoolId = SchoolId,
            IsActive = true,
            MustChangePassword = true
        };

        var result = await UserManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return SchoolView("Administrators/Create", model);
        }

        await UserManager.AddToRoleAsync(user, AppRoleNames.CustomAdmin);
        await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("school_id", SchoolId.ToString()));

        Db.SchoolAdminProfiles.Add(new SchoolAdminProfile
        {
            UserId = user.Id,
            SchoolId = SchoolId,
            AdminType = nameof(AppRoles.CustomAdmin),
            IsPrimary = false,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        });

        foreach (var code in model.PermissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            Db.UserPermissionGrants.Add(new UserPermissionGrant
            {
                UserId = user.Id,
                SchoolId = SchoolId,
                PermissionCode = code,
                Granted = true,
                IsActive = true,
                CreatedByUserId = CurrentUserId
            });
        }

        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Create", "Administrators", SchoolId, "User", user.Id, user.FullName, ct);

        var recipient = AccountEmailNotificationService.ResolveRecipientEmail(user.Email);
        if (recipient is not null)
        {
            await _accountEmails.SendNewAccountEmailAsync(new AccountEmailRequest
            {
                SchoolId = SchoolId,
                UserId = user.Id,
                RecipientEmail = recipient,
                UserName = user.FullName,
                LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
                TemporaryPassword = model.Password,
                AccountType = PortalAccountType.Admin
            }, ct);
        }

        SetFlash("Custom admin created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Deactivate/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string userId, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.AdminsManage) is { } deny)
            return deny;

        var profile = await Db.SchoolAdminProfiles
            .FirstOrDefaultAsync(p => p.SchoolId == SchoolId && p.UserId == userId, ct);
        if (profile is null) return NotFound();
        if (profile.IsPrimary)
        {
            SetFlash("Cannot deactivate the primary school admin.", "error");
            return RedirectToAction(nameof(Index));
        }

        profile.IsActive = false;
        profile.UpdatedAt = DateTimeOffset.UtcNow;
        var user = await UserManager.FindByIdAsync(userId);
        if (user is not null)
        {
            user.IsActive = false;
            await UserManager.UpdateAsync(user);
        }
        await Db.SaveChangesAsync(ct);
        SetFlash("Administrator deactivated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("ResendCredentials/{userId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCredentials(string userId, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.AdminsManage) is { } deny)
            return deny;

        var profile = await Db.SchoolAdminProfiles
            .FirstOrDefaultAsync(p => p.SchoolId == SchoolId && p.UserId == userId, ct);
        if (profile is null)
            return NotFound();

        var log = await _accountEmails.ResendCredentialsEmailAsync(userId, SchoolId, PortalAccountType.Admin, ct);
        SetFlash(log.Status == AccountEmailDeliveryStatus.Sent
            ? "Credentials email sent with a new temporary password."
            : $"Could not send credentials email: {log.FailureReason ?? "Unknown error"}",
            log.Status == AccountEmailDeliveryStatus.Sent ? "success" : "error");
        return RedirectToAction(nameof(Index));
    }
}
