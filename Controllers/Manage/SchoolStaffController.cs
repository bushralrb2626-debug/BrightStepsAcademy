using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Staff")]
public class SchoolStaffController : SchoolManageControllerBase
{
    private readonly IAccountEmailNotificationService _accountEmails;

    public SchoolStaffController(
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
        if (await ForbidUnlessAsync(PermissionCodes.StaffView) is { } deny)
            return deny;
        var items = await Db.StaffMembers.AsNoTracking()
            .Include(s => s.StaffCategory)
            .Where(s => s.SchoolId == SchoolId)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);
        return SchoolView("Staff/Index", items);
    }

    [HttpGet("Teachers")]
    public async Task<IActionResult> Teachers(CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TeachersView, PermissionCodes.StaffView) is { } deny)
            return deny;

        var items = await Db.StaffMembers.AsNoTracking()
            .Include(s => s.StaffCategory)
            .Where(s => s.SchoolId == SchoolId
                        && s.StaffCategory.Name.ToLower().Contains("teacher"))
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);

        ViewBag.TeachersOnly = true;
        return SchoolView("Staff/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;
        await LoadCategoriesAsync(ct);
        return SchoolView("Staff/Create", new StaffFormVm());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;

        if (!await Db.StaffCategories.AnyAsync(c => c.Id == model.StaffCategoryId && c.SchoolId == SchoolId, ct))
        {
            ModelState.AddModelError(nameof(model.StaffCategoryId), "Select a valid category.");
            await LoadCategoriesAsync(ct);
            return SchoolView("Staff/Create", model);
        }

        var staff = new StaffMember
        {
            SchoolId = SchoolId,
            StaffCategoryId = model.StaffCategoryId,
            StaffCode = model.StaffCode?.Trim() ?? "",
            FullName = model.FullName?.Trim() ?? "",
            Email = model.Email?.Trim(),
            Phone = model.Phone?.Trim(),
            EmployeeId = model.EmployeeId?.Trim(),
            Designation = model.Designation?.Trim(),
            Qualification = model.Qualification?.Trim(),
            Department = model.Department?.Trim(),
            DateOfJoining = model.DateOfJoining,
            Address = model.Address?.Trim(),
            HasLoginAccess = model.HasLoginAccess,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };

        if (model.HasLoginAccess)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.LoginPassword))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required when login access is enabled.");
                await LoadCategoriesAsync(ct);
                return SchoolView("Staff/Create", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true,
                FullName = staff.FullName,
                LoginId = string.IsNullOrWhiteSpace(model.LoginId) ? null : model.LoginId.Trim(),
                SchoolId = SchoolId,
                IsActive = true,
                MustChangePassword = true
            };
            var result = await UserManager.CreateAsync(user, model.LoginPassword);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                await LoadCategoriesAsync(ct);
                return SchoolView("Staff/Create", model);
            }
            await AssignStaffPortalRolesAsync(user, model.StaffCategoryId, ct);
            await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("school_id", SchoolId.ToString()));
            staff.UserId = user.Id;
        }

        Db.StaffMembers.Add(staff);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Staff code must be unique within the school.");
            await LoadCategoriesAsync(ct);
            return SchoolView("Staff/Create", model);
        }

        await Audit.LogAsync("Create", "Staff", SchoolId, "StaffMember", staff.Id.ToString(), staff.FullName, ct);

        if (model.HasLoginAccess && staff.UserId is not null)
        {
            var createdUser = await UserManager.FindByIdAsync(staff.UserId);
            if (createdUser is not null)
                await SendStaffAccountEmailAsync(createdUser, staff.Email, model.StaffCategoryId, model.LoginPassword!, ct);
        }

        SetFlash("Staff member created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;
        var item = await Db.StaffMembers.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        await LoadCategoriesAsync(ct, item.StaffCategoryId);

        var model = new StaffFormVm
        {
            Id = item.Id,
            StaffCategoryId = item.StaffCategoryId,
            StaffCode = item.StaffCode,
            FullName = item.FullName,
            Email = item.Email,
            Phone = item.Phone,
            EmployeeId = item.EmployeeId,
            Designation = item.Designation,
            Qualification = item.Qualification,
            Department = item.Department,
            DateOfJoining = item.DateOfJoining,
            Address = item.Address,
            HasLoginAccess = item.HasLoginAccess,
            LoginId = item.UserId is null
                ? null
                : (await UserManager.FindByIdAsync(item.UserId))?.LoginId
        };

        if (!string.IsNullOrEmpty(item.UserId))
        {
            var log = await _accountEmails.GetLatestStatusAsync(item.UserId, AccountEmailType.NewAccountCreated, ct);
            if (log is not null)
            {
                model.CredentialsEmailStatus = log.Status.ToString();
                model.CredentialsEmailFailureReason = log.FailureReason;
            }
            model.CanResendCredentialsEmail = log is null || log.Status != AccountEmailDeliveryStatus.Sent;
        }

        return SchoolView("Staff/Edit", model);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, StaffFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;

        var item = await Db.StaffMembers.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();

        model.Id = id;

        if (string.IsNullOrWhiteSpace(model.StaffCode))
            ModelState.AddModelError(nameof(model.StaffCode), "Staff code is required.");
        if (string.IsNullOrWhiteSpace(model.FullName))
            ModelState.AddModelError(nameof(model.FullName), "Full name is required.");

        if (!await Db.StaffCategories.AnyAsync(c => c.Id == model.StaffCategoryId && c.SchoolId == SchoolId, ct))
            ModelState.AddModelError(nameof(model.StaffCategoryId), "Select a valid category.");

        var enablingLogin = model.HasLoginAccess && string.IsNullOrEmpty(item.UserId);
        if (enablingLogin)
        {
            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email is required to enable login.");
            if (string.IsNullOrWhiteSpace(model.LoginPassword))
                ModelState.AddModelError(nameof(model.LoginPassword), "Temporary password is required to enable login.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(ct, model.StaffCategoryId);
            return SchoolView("Staff/Edit", model);
        }

        item.StaffCategoryId = model.StaffCategoryId;
        item.StaffCode = model.StaffCode.Trim();
        item.FullName = model.FullName.Trim();
        item.Email = model.Email?.Trim();
        item.Phone = model.Phone?.Trim();
        item.EmployeeId = model.EmployeeId?.Trim();
        item.Designation = model.Designation?.Trim();
        item.Qualification = model.Qualification?.Trim();
        item.Department = model.Department?.Trim();
        item.DateOfJoining = model.DateOfJoining;
        item.Address = model.Address?.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.UpdatedByUserId = CurrentUserId;

        if (enablingLogin)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email!.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true,
                FullName = item.FullName,
                LoginId = string.IsNullOrWhiteSpace(model.LoginId) ? null : model.LoginId.Trim(),
                SchoolId = SchoolId,
                IsActive = true,
                MustChangePassword = true
            };
            var result = await UserManager.CreateAsync(user, model.LoginPassword!);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                await LoadCategoriesAsync(ct, model.StaffCategoryId);
                return SchoolView("Staff/Edit", model);
            }
            await AssignStaffPortalRolesAsync(user, model.StaffCategoryId, ct);
            await UserManager.AddClaimAsync(user, new System.Security.Claims.Claim("school_id", SchoolId.ToString()));
            item.UserId = user.Id;
            item.HasLoginAccess = true;

            await SendStaffAccountEmailAsync(user, item.Email, model.StaffCategoryId, model.LoginPassword!, ct);
        }
        else if (!model.HasLoginAccess)
        {
            item.HasLoginAccess = false;
            if (!string.IsNullOrEmpty(item.UserId))
            {
                var linked = await UserManager.FindByIdAsync(item.UserId);
                if (linked is not null)
                {
                    linked.IsActive = false;
                    await UserManager.UpdateAsync(linked);
                }
            }
        }
        else if (!string.IsNullOrEmpty(item.UserId))
        {
            item.HasLoginAccess = true;
            var linked = await UserManager.FindByIdAsync(item.UserId);
            if (linked is not null)
            {
                linked.FullName = item.FullName;
                if (!string.IsNullOrWhiteSpace(item.Email))
                {
                    linked.Email = item.Email;
                    linked.UserName = item.Email;
                }
                if (!string.IsNullOrWhiteSpace(model.LoginId))
                    linked.LoginId = model.LoginId.Trim();
                linked.IsActive = true;
                await UserManager.UpdateAsync(linked);
            }
        }

        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Staff code must be unique within the school.");
            await LoadCategoriesAsync(ct, model.StaffCategoryId);
            return SchoolView("Staff/Edit", model);
        }

        await Audit.LogAsync("Update", "Staff", SchoolId, "StaffMember", item.Id.ToString(), item.FullName, ct);
        SetFlash("Staff member updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;
        var item = await Db.StaffMembers.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Staff member deactivated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories(CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.StaffCategoriesView, PermissionCodes.StaffView, PermissionCodes.StaffManage) is { } deny)
            return deny;
        await SchoolBootstrap.EnsureStaffCategoriesAsync(Db, SchoolId, ct);
        var items = await Db.StaffCategories.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
        return SchoolView("Categories/Index", items);
    }

    [HttpPost("Categories/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(StaffCategory model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;
        model.Id = Guid.NewGuid();
        model.SchoolId = SchoolId;
        model.Name = model.Name?.Trim() ?? "";
        model.CreatedByUserId = CurrentUserId;
        model.IsActive = true;
        Db.StaffCategories.Add(model);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            SetFlash("Category name must be unique.", "error");
            return Redirect("/Manage/School/Staff/Categories");
        }
        SetFlash("Category created.");
        return Redirect("/Manage/School/Staff/Categories");
    }

    [HttpPost("Categories/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(Guid id, string Name, string? Description, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;
        var item = await Db.StaffCategories.FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        if (string.IsNullOrWhiteSpace(Name))
        {
            SetFlash("Category name is required.", "error");
            return Redirect("/Manage/School/Staff/Categories");
        }
        item.Name = Name.Trim();
        item.Description = Description?.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            SetFlash("Category name must be unique.", "error");
            return Redirect("/Manage/School/Staff/Categories");
        }
        SetFlash("Category updated.");
        return Redirect("/Manage/School/Staff/Categories");
    }

    [HttpPost("Categories/Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateCategory(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;
        var item = await Db.StaffCategories.FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Category deactivated.");
        return Redirect("/Manage/School/Staff/Categories");
    }

    private async Task LoadCategoriesAsync(CancellationToken ct, Guid? selectedId = null)
    {
        await SchoolBootstrap.EnsureStaffCategoriesAsync(Db, SchoolId, ct);

        ViewBag.Categories = new SelectList(
            await Db.StaffCategories.AsNoTracking()
                .Where(c => c.SchoolId == SchoolId && (c.IsActive || (selectedId.HasValue && c.Id == selectedId)))
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(ct),
            "Id", "Name", selectedId);
    }

    [HttpPost("ResendCredentials/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCredentials(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StaffManage) is { } deny)
            return deny;

        var item = await Db.StaffMembers.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null || string.IsNullOrEmpty(item.UserId))
        {
            SetFlash("This staff member does not have portal login access.", "error");
            return RedirectToAction(nameof(Edit), new { id });
        }

        var accountType = await ResolveStaffAccountTypeAsync(item.StaffCategoryId, ct);
        var log = await _accountEmails.ResendCredentialsEmailAsync(item.UserId, SchoolId, accountType, ct);
        SetFlash(log.Status == AccountEmailDeliveryStatus.Sent
            ? "Credentials email sent with a new temporary password."
            : $"Could not send credentials email: {log.FailureReason ?? "Unknown error"}",
            log.Status == AccountEmailDeliveryStatus.Sent ? "success" : "error");
        return RedirectToAction(nameof(Edit), new { id });
    }

    private async Task SendStaffAccountEmailAsync(
        ApplicationUser user,
        string? staffEmail,
        Guid staffCategoryId,
        string password,
        CancellationToken ct)
    {
        var recipient = AccountEmailNotificationService.ResolveRecipientEmail(staffEmail)
                        ?? AccountEmailNotificationService.ResolveRecipientEmail(user.Email);
        if (recipient is null)
            return;

        var accountType = await ResolveStaffAccountTypeAsync(staffCategoryId, ct);
        await _accountEmails.SendNewAccountEmailAsync(new AccountEmailRequest
        {
            SchoolId = SchoolId,
            UserId = user.Id,
            RecipientEmail = recipient,
            UserName = user.FullName,
            LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
            TemporaryPassword = password,
            AccountType = accountType
        }, ct);
    }

    private async Task<PortalAccountType> ResolveStaffAccountTypeAsync(Guid staffCategoryId, CancellationToken ct)
    {
        var category = await Db.StaffCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == staffCategoryId && c.SchoolId == SchoolId, ct);
        return category?.Name.Contains("teacher", StringComparison.OrdinalIgnoreCase) == true
            ? PortalAccountType.Teacher
            : PortalAccountType.Staff;
    }

    private async Task AssignStaffPortalRolesAsync(ApplicationUser user, Guid staffCategoryId, CancellationToken ct)
    {
        var category = await Db.StaffCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == staffCategoryId && c.SchoolId == SchoolId, ct);
        var isTeacher = category?.Name.Contains("teacher", StringComparison.OrdinalIgnoreCase) == true;
        if (isTeacher)
            await UserManager.AddToRoleAsync(user, AppRoleNames.Teacher);
        else
            await UserManager.AddToRoleAsync(user, AppRoleNames.Staff);
    }
}
