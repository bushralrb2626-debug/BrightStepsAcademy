using System.Text.Json;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

public class SuperAdminSchoolsController : SuperAdminControllerBase
{
    private const int SchoolsPageSize = 10;
    private const string WizardTempKey = "SuperAdminSchoolWizard";

    public SuperAdminSchoolsController(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFileStorageService files)
        : base(db, userManager, signInManager, files)
    {
    }

    [HttpGet("Schools")]
    public async Task<IActionResult> Schools(
        string? search,
        string? status,
        string? subscription,
        string? date,
        int page = 1,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        await RefreshAllSubscriptionsAsync(ct);

        var query = Db.Schools.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s =>
                s.Name.Contains(term) ||
                s.SchoolCode.Contains(term) ||
                (s.Email != null && s.Email.Contains(term)) ||
                (s.City != null && s.City.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<SchoolStatus>(status, true, out var schoolStatus))
            query = query.Where(s => s.Status == schoolStatus);

        if (!string.IsNullOrWhiteSpace(subscription) && Enum.TryParse<SubscriptionStatus>(subscription, true, out var subStatus))
            query = query.Where(s => s.Subscription != null && s.Subscription.Status == subStatus);

        if (!string.IsNullOrWhiteSpace(date))
        {
            var now = DateTimeOffset.UtcNow;
            query = date.ToLowerInvariant() switch
            {
                "today" => query.Where(s => s.CreatedAt.Date == now.Date),
                "week" => query.Where(s => s.CreatedAt >= now.AddDays(-7)),
                "month" => query.Where(s => s.CreatedAt >= now.AddMonths(-1)),
                "year" => query.Where(s => s.CreatedAt >= now.AddYears(-1)),
                _ => query
            };
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * SchoolsPageSize)
            .Take(SchoolsPageSize)
            .Select(s => new SchoolListItemVm
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
                AdminName = s.AdminProfiles
                    .Where(p => p.IsPrimary)
                    .Select(p => Db.Users.Where(u => u.Id == p.UserId).Select(u => u.FullName).FirstOrDefault())
                    .FirstOrDefault(),
                AdminEmail = s.AdminProfiles
                    .Where(p => p.IsPrimary)
                    .Select(p => Db.Users.Where(u => u.Id == p.UserId).Select(u => u.Email).FirstOrDefault())
                    .FirstOrDefault(),
                PlanName = s.Subscription != null ? s.Subscription.PlanName : null,
                SubscriptionStatus = s.Subscription != null ? s.Subscription.Status : null,
                ExpiryDate = s.Subscription != null ? s.Subscription.ExpiryDate : null
            })
            .ToListAsync(ct);

        return ManageView("Schools/Index", new SchoolListVm
        {
            Search = search,
            StatusFilter = status,
            SubscriptionFilter = subscription,
            DateFilter = date,
            Page = page,
            PageSize = SchoolsPageSize,
            TotalCount = total,
            Schools = items
        });
    }

    [HttpGet("Schools/Create")]
    public async Task<IActionResult> Create(int step = 1, CancellationToken ct = default)
    {
        var model = LoadWizard() ?? new SchoolFormVm();
        model.Step = Math.Clamp(step, 1, 6);
        if (model.SubscriptionStart == default)
            model.SubscriptionStart = DateTimeOffset.UtcNow.Date;
        if (model.SubscriptionExpiry == default)
        {
            var months = (await Db.PlatformSettings.AsNoTracking()
                .Select(s => (int?)s.DefaultSubscriptionMonths).FirstOrDefaultAsync(ct)) ?? 12;
            model.SubscriptionExpiry = DateTimeOffset.UtcNow.Date.AddMonths(months);
        }

        return ManageView("Schools/Create", model);
    }

    [HttpPost("Schools/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SchoolFormVm model, string? nav, CancellationToken ct = default)
    {
        var step = Math.Clamp(model.Step <= 0 ? 1 : model.Step, 1, 6);
        var wizard = LoadWizard() ?? new SchoolFormVm();
        ApplyStepOntoWizard(wizard, model, step);
        wizard.Step = step;
        SaveWizard(wizard);

        if (string.Equals(nav, "back", StringComparison.OrdinalIgnoreCase))
        {
            wizard.Step = Math.Max(1, step - 1);
            SaveWizard(wizard);
            return ManageView("Schools/Create", wizard);
        }

        ModelState.Clear();
        ValidateWizardStep(wizard);

        if (!ModelState.IsValid)
        {
            wizard.Step = step;
            return ManageView("Schools/Create", wizard);
        }

        if (step < 6 && !string.Equals(nav, "finish", StringComparison.OrdinalIgnoreCase))
        {
            if (step == 3 && model.LogoFile is { Length: > 0 })
            {
                try
                {
                    var tempId = wizard.Id ?? Guid.NewGuid();
                    wizard.Id ??= tempId;
                    wizard.LogoPath = await Files.SaveAsync(model.LogoFile, tempId, "branding", ct);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(model.LogoFile), ex.Message);
                    return ManageView("Schools/Create", wizard);
                }
            }

            wizard.Step = step + 1;
            SaveWizard(wizard);
            return ManageView("Schools/Create", wizard);
        }

        // Final create from full wizard state
        await ValidateSchoolUniquenessAsync(wizard, excludeId: null, ct);
        if (wizard.CreateAdmin)
            await ValidateWizardAdminAsync(wizard, ct);

        if (!ModelState.IsValid)
        {
            wizard.Step = 6;
            return ManageView("Schools/Create", wizard);
        }

        var schoolId = wizard.Id ?? Guid.NewGuid();
        var school = MapToEntity(wizard);
        school.Id = schoolId;
        school.CreatedAt = DateTimeOffset.UtcNow;
        if (wizard.Status is not (SchoolStatus.Active or SchoolStatus.Pending))
            school.Status = SchoolStatus.Active;

        Db.Schools.Add(school);

        var warningDays = await GetWarningDaysAsync(ct);
        var sub = new SchoolSubscription
        {
            SchoolId = school.Id,
            PlanCode = string.IsNullOrWhiteSpace(wizard.PlanCode) ? wizard.PlanName : wizard.PlanCode.Trim(),
            PlanName = string.IsNullOrWhiteSpace(wizard.PlanName) ? "Standard" : wizard.PlanName.Trim(),
            StartDate = wizard.SubscriptionStart,
            ExpiryDate = wizard.SubscriptionExpiry,
            BillingCycle = wizard.BillingCycle,
            Price = wizard.SubscriptionPrice,
            Notes = NullIfWhiteSpace(wizard.SubscriptionNotes),
            CreatedAt = DateTimeOffset.UtcNow
        };
        SubscriptionStatusHelper.Refresh(sub, warningDays);
        Db.SchoolSubscriptions.Add(sub);

        var actorId = UserManager.GetUserId(User) ?? string.Empty;
        Db.SubscriptionChangeLogs.Add(new SubscriptionChangeLog
        {
            SchoolSubscriptionId = sub.Id,
            SchoolId = school.Id,
            ChangedByUserId = actorId,
            ChangedByUserName = User.Identity?.Name,
            Summary = "Subscription created",
            Details = $"{sub.PlanName} · {sub.StartDate:d} → {sub.ExpiryDate:d} · {sub.Status}",
            Timestamp = DateTimeOffset.UtcNow
        });

        if (wizard.CreateAdmin
            && !string.IsNullOrWhiteSpace(wizard.AdminFullName)
            && !string.IsNullOrWhiteSpace(wizard.AdminEmail)
            && !string.IsNullOrWhiteSpace(wizard.AdminLoginId)
            && !string.IsNullOrWhiteSpace(wizard.AdminTemporaryPassword))
        {
            var adminUser = new ApplicationUser
            {
                UserName = wizard.AdminLoginId.Trim(),
                Email = wizard.AdminEmail.Trim(),
                EmailConfirmed = true,
                FullName = wizard.AdminFullName.Trim(),
                LoginId = wizard.AdminLoginId.Trim(),
                PhoneNumber = NullIfWhiteSpace(wizard.AdminPhone),
                SchoolId = school.Id,
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var createResult = await UserManager.CreateAsync(adminUser, wizard.AdminTemporaryPassword);
            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError(string.Empty, FriendlyIdentityError(err.Description));
                wizard.Step = 4;
                return ManageView("Schools/Create", wizard);
            }

            await UserManager.AddToRoleAsync(adminUser, AppRoleNames.SchoolAdmin);
            Db.SchoolAdminProfiles.Add(new SchoolAdminProfile
            {
                UserId = adminUser.Id,
                SchoolId = school.Id,
                AdminType = nameof(AppRoles.SchoolAdmin),
                IsPrimary = true,
                IsActive = true,
                CreatedByUserId = actorId
            });

            Db.AppNotifications.Add(new AppNotification
            {
                SchoolId = school.Id,
                UserId = adminUser.Id,
                Title = "Welcome to BrightSteps",
                Message = $"Your school admin account for {school.Name} is ready. Please sign in and change your temporary password.",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await WriteAuditAsync(school.Id, "SchoolCreated", "Schools", nameof(School), school.Id.ToString(),
            $"Created school '{school.Name}' ({school.SchoolCode}) as {school.Status}.", ct);

        if (!string.IsNullOrEmpty(actorId))
        {
            Db.AppNotifications.Add(new AppNotification
            {
                SchoolId = school.Id,
                UserId = actorId,
                Title = "School created",
                Message = $"School '{school.Name}' was created with status {school.Status}.",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await SchoolBootstrap.EnsureStaffCategoriesAsync(Db, school.Id, ct);
        await Db.SaveChangesAsync(ct);
        TempData.Remove(WizardTempKey);
        TempData["Success"] = $"School '{school.Name}' was created.";
        return RedirectToAction(nameof(Details), new { id = school.Id });
    }

    [HttpGet("Schools/Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var school = await Db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();
        return ManageView("Schools/Edit", MapToForm(school));
    }

    [HttpPost("Schools/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, SchoolFormVm model, CancellationToken ct)
    {
        model.Id = id;
        var school = await Db.Schools.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        await ValidateSchoolUniquenessAsync(model, excludeId: id, ct);
        if (!ModelState.IsValid)
            return ManageView("Schools/Edit", model);

        if (model.LogoFile is { Length: > 0 })
        {
            try
            {
                model.LogoPath = await Files.SaveAsync(model.LogoFile, id, "branding", ct);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(model.LogoFile), ex.Message);
                return ManageView("Schools/Edit", model);
            }
        }

        ApplyForm(school, model);
        school.UpdatedAt = DateTimeOffset.UtcNow;
        await WriteAuditAsync(school.Id, "SchoolUpdated", "Schools", nameof(School), school.Id.ToString(),
            $"Updated school '{school.Name}' ({school.SchoolCode}).", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = $"School '{school.Name}' was updated.";
        return RedirectToAction(nameof(Details), new { id = school.Id });
    }

    [HttpGet("Schools/Details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var school = await Db.Schools.AsNoTracking()
            .Include(s => s.Subscription)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        if (school.Subscription is not null)
        {
            var warningDays = await GetWarningDaysAsync(ct);
            // refresh tracked copy
            var tracked = await Db.SchoolSubscriptions.FirstAsync(s => s.Id == school.Subscription.Id, ct);
            SubscriptionStatusHelper.Refresh(tracked, warningDays);
            await Db.SaveChangesAsync(ct);
            school.Subscription = tracked;
        }

        var profile = await Db.SchoolAdminProfiles.AsNoTracking()
            .Where(p => p.SchoolId == id && p.IsPrimary)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        SchoolAdminSummaryVm? admin = null;
        if (profile is not null)
        {
            var user = await UserManager.FindByIdAsync(profile.UserId);
            if (user is not null)
            {
                admin = new SchoolAdminSummaryVm
                {
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    LoginId = user.LoginId,
                    Phone = user.PhoneNumber,
                    IsActive = user.IsActive
                };
            }
        }

        return ManageView("Schools/Details", new SchoolDetailsVm
        {
            School = school,
            PrimaryAdmin = admin,
            Subscription = school.Subscription,
            Buildings = await Db.Buildings.CountAsync(b => b.SchoolId == id, ct),
            Floors = await Db.Floors.CountAsync(f => f.SchoolId == id, ct),
            Rooms = await Db.Rooms.CountAsync(r => r.SchoolId == id, ct),
            Staff = await Db.StaffMembers.CountAsync(s => s.SchoolId == id, ct),
            Students = await Db.StudentRecords.CountAsync(s => s.SchoolId == id, ct),
            Administrators = await Db.SchoolAdminProfiles.CountAsync(p => p.SchoolId == id, ct),
            Furniture = await Db.FurnitureItems.CountAsync(f => f.SchoolId == id, ct)
        });
    }

    [HttpPost("Schools/Activate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
        => await SetSchoolStatusAsync(id, SchoolStatus.Active, "SchoolActivated", ct);

    [HttpPost("Schools/Suspend/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
        => await SetSchoolStatusAsync(id, SchoolStatus.Suspended, "SchoolSuspended", ct);

    [HttpPost("Schools/Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        => await SetSchoolStatusAsync(id, SchoolStatus.Inactive, "SchoolDeactivated", ct);

    [HttpPost("Schools/ToggleStatus/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken ct)
    {
        var school = await Db.Schools.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();
        var next = school.Status == SchoolStatus.Active ? SchoolStatus.Inactive : SchoolStatus.Active;
        return await SetSchoolStatusAsync(id, next,
            next == SchoolStatus.Active ? "SchoolActivated" : "SchoolDeactivated", ct);
    }

    [HttpGet("Schools/{id:guid}/Admin")]
    public async Task<IActionResult> Admin(Guid id, CancellationToken ct)
    {
        var school = await Db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        var profile = await Db.SchoolAdminProfiles.AsNoTracking()
            .Where(p => p.SchoolId == id && p.IsPrimary)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var vm = new SchoolAdminFormVm
        {
            SchoolId = school.Id,
            SchoolName = school.Name
        };

        if (profile is not null)
        {
            var user = await UserManager.FindByIdAsync(profile.UserId);
            if (user is not null)
            {
                vm.HasAdmin = true;
                vm.UserId = user.Id;
                vm.FullName = user.FullName;
                vm.Email = user.Email ?? string.Empty;
                vm.LoginId = user.LoginId ?? string.Empty;
                vm.Phone = user.PhoneNumber;
                vm.Status = user.IsActive ? RecordStatus.Active : RecordStatus.Inactive;
            }
        }

        return ManageView("Schools/Admin", vm);
    }

    [HttpPost("Schools/{id:guid}/Admin/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(Guid id, SchoolAdminFormVm model, CancellationToken ct)
    {
        var school = await Db.Schools.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        model.SchoolId = id;
        model.SchoolName = school.Name;
        model.HasAdmin = false;

        if (string.IsNullOrWhiteSpace(model.TemporaryPassword))
            ModelState.AddModelError(nameof(model.TemporaryPassword), "Temporary password is required when creating an admin.");

        await ValidateAdminUniquenessAsync(model, excludeUserId: null, ct);

        if (await Db.SchoolAdminProfiles.AnyAsync(p => p.SchoolId == id && p.IsPrimary, ct))
            ModelState.AddModelError(string.Empty, "This school already has a primary admin. Edit the existing admin instead.");

        if (!ModelState.IsValid)
            return ManageView("Schools/Admin", model);

        var user = new ApplicationUser
        {
            UserName = model.LoginId.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            FullName = model.FullName.Trim(),
            LoginId = model.LoginId.Trim(),
            PhoneNumber = NullIfWhiteSpace(model.Phone),
            SchoolId = school.Id,
            IsActive = model.Status == RecordStatus.Active,
            MustChangePassword = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createResult = await UserManager.CreateAsync(user, model.TemporaryPassword!);
        if (!createResult.Succeeded)
        {
            foreach (var err in createResult.Errors)
                ModelState.AddModelError(string.Empty, FriendlyIdentityError(err.Description));
            return ManageView("Schools/Admin", model);
        }

        await UserManager.AddToRoleAsync(user, AppRoleNames.SchoolAdmin);

        var actorId = UserManager.GetUserId(User) ?? string.Empty;
        Db.SchoolAdminProfiles.Add(new SchoolAdminProfile
        {
            UserId = user.Id,
            SchoolId = school.Id,
            AdminType = nameof(AppRoles.SchoolAdmin),
            IsPrimary = true,
            IsActive = true,
            CreatedByUserId = actorId
        });

        await WriteAuditAsync(school.Id, "SchoolAdminCreated", "SchoolAdmin", nameof(ApplicationUser), user.Id,
            $"Created primary school admin '{user.FullName}' for '{school.Name}'.", ct);

        Db.AppNotifications.Add(new AppNotification
        {
            SchoolId = school.Id,
            UserId = user.Id,
            Title = "Welcome to BrightSteps",
            Message = $"Your school admin account for {school.Name} is ready. Please sign in and change your temporary password.",
            CreatedAt = DateTimeOffset.UtcNow
        });

        if (!string.IsNullOrEmpty(actorId))
        {
            Db.AppNotifications.Add(new AppNotification
            {
                SchoolId = school.Id,
                UserId = actorId,
                Title = "School admin created",
                Message = $"Primary admin '{user.FullName}' was created for {school.Name}.",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await Db.SaveChangesAsync(ct);
        TempData["Success"] = $"School admin '{user.FullName}' was created.";
        return RedirectToAction(nameof(Admin), new { id });
    }

    [HttpPost("Schools/{id:guid}/Admin/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdmin(Guid id, SchoolAdminFormVm model, CancellationToken ct)
    {
        var school = await Db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        model.SchoolId = id;
        model.SchoolName = school.Name;
        model.HasAdmin = true;

        if (string.IsNullOrWhiteSpace(model.UserId))
        {
            ModelState.AddModelError(string.Empty, "Admin user was not found.");
            return ManageView("Schools/Admin", model);
        }

        var user = await UserManager.FindByIdAsync(model.UserId);
        if (user is null || user.SchoolId != id)
        {
            ModelState.AddModelError(string.Empty, "Admin user was not found for this school.");
            return ManageView("Schools/Admin", model);
        }

        ModelState.Remove(nameof(model.TemporaryPassword));
        await ValidateAdminUniquenessAsync(model, excludeUserId: user.Id, ct);
        if (!ModelState.IsValid)
            return ManageView("Schools/Admin", model);

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.UserName = model.LoginId.Trim();
        user.LoginId = model.LoginId.Trim();
        user.PhoneNumber = NullIfWhiteSpace(model.Phone);
        user.IsActive = model.Status == RecordStatus.Active;

        var updateResult = await UserManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var err in updateResult.Errors)
                ModelState.AddModelError(string.Empty, FriendlyIdentityError(err.Description));
            return ManageView("Schools/Admin", model);
        }

        await WriteAuditAsync(id, "SchoolAdminUpdated", "SchoolAdmin", nameof(ApplicationUser), user.Id,
            $"Updated school admin '{user.FullName}' for '{school.Name}'.", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = "School admin was updated.";
        return RedirectToAction(nameof(Admin), new { id });
    }

    [HttpPost("Schools/{id:guid}/Admin/ResetPassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetAdminPassword(Guid id, SchoolAdminResetPasswordVm model, CancellationToken ct)
    {
        var school = await Db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        model.SchoolId = id;
        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                ?? "Could not reset password.";
            return RedirectToAction(nameof(Admin), new { id });
        }

        var profile = await Db.SchoolAdminProfiles.AsNoTracking()
            .Where(p => p.SchoolId == id && p.IsPrimary)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (profile is null)
        {
            TempData["Error"] = "No school admin exists to reset.";
            return RedirectToAction(nameof(Admin), new { id });
        }

        var user = await UserManager.FindByIdAsync(profile.UserId);
        if (user is null)
        {
            TempData["Error"] = "Admin user was not found.";
            return RedirectToAction(nameof(Admin), new { id });
        }

        var token = await UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await UserManager.ResetPasswordAsync(user, token, model.TemporaryPassword);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => FriendlyIdentityError(e.Description)));
            return RedirectToAction(nameof(Admin), new { id });
        }

        user.MustChangePassword = true;
        await UserManager.UpdateAsync(user);

        await WriteAuditAsync(id, "SchoolAdminPasswordReset", "SchoolAdmin", nameof(ApplicationUser), user.Id,
            $"Password reset for school admin '{user.FullName}'.", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = "Temporary password was reset. The admin must change it on next login.";
        return RedirectToAction(nameof(Admin), new { id });
    }

    [HttpPost("Schools/{id:guid}/Admin/Toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdmin(Guid id, CancellationToken ct)
    {
        var profile = await Db.SchoolAdminProfiles
            .Where(p => p.SchoolId == id && p.IsPrimary)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (profile is null)
        {
            TempData["Error"] = "No school admin exists.";
            return RedirectToAction(nameof(Admin), new { id });
        }

        var user = await UserManager.FindByIdAsync(profile.UserId);
        if (user is null)
        {
            TempData["Error"] = "Admin user was not found.";
            return RedirectToAction(nameof(Admin), new { id });
        }

        user.IsActive = !user.IsActive;
        profile.IsActive = user.IsActive;
        profile.UpdatedAt = DateTimeOffset.UtcNow;
        profile.UpdatedByUserId = UserManager.GetUserId(User);

        var updateResult = await UserManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            TempData["Error"] = string.Join(" ", updateResult.Errors.Select(e => FriendlyIdentityError(e.Description)));
            return RedirectToAction(nameof(Admin), new { id });
        }

        await WriteAuditAsync(id, user.IsActive ? "SchoolAdminActivated" : "SchoolAdminDeactivated",
            "SchoolAdmin", nameof(ApplicationUser), user.Id,
            $"School admin '{user.FullName}' set to {(user.IsActive ? "Active" : "Inactive")}.", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = $"School admin is now {(user.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Admin), new { id });
    }

    [HttpGet("SchoolAdmins")]
    public async Task<IActionResult> SchoolAdmins(string? search, int page = 1, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        const int pageSize = 20;

        var query =
            from p in Db.SchoolAdminProfiles.AsNoTracking()
            join u in Db.Users.AsNoTracking() on p.UserId equals u.Id
            join s in Db.Schools.AsNoTracking() on p.SchoolId equals s.Id
            select new { p, u, s };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.u.FullName.Contains(term) ||
                (x.u.Email != null && x.u.Email.Contains(term)) ||
                x.s.Name.Contains(term) ||
                x.s.SchoolCode.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.s.Name).ThenBy(x => x.u.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PlatformSchoolAdminItemVm
            {
                UserId = x.u.Id,
                SchoolId = x.s.Id,
                SchoolName = x.s.Name,
                SchoolCode = x.s.SchoolCode,
                FullName = x.u.FullName,
                Email = x.u.Email ?? string.Empty,
                LoginId = x.u.LoginId,
                IsActive = x.u.IsActive,
                IsPrimary = x.p.IsPrimary
            })
            .ToListAsync(ct);

        return ManageView("SchoolAdmins/Index", new PlatformSchoolAdminListVm
        {
            Search = search,
            Admins = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        });
    }

    private async Task<IActionResult> SetSchoolStatusAsync(Guid id, SchoolStatus status, string action, CancellationToken ct)
    {
        var school = await Db.Schools.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (school is null) return NotFound();

        school.Status = status;
        school.UpdatedAt = DateTimeOffset.UtcNow;
        await WriteAuditAsync(school.Id, action, "Schools", nameof(School), school.Id.ToString(),
            $"School '{school.Name}' set to {status}.", ct);
        await Db.SaveChangesAsync(ct);

        TempData["Success"] = $"School '{school.Name}' is now {status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private SchoolFormVm? LoadWizard()
    {
        if (TempData[WizardTempKey] is not string json)
            return null;
        TempData.Keep(WizardTempKey);
        try
        {
            return JsonSerializer.Deserialize<SchoolFormVm>(json, WizardJsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private void ApplyStepOntoWizard(SchoolFormVm wizard, SchoolFormVm posted, int step)
    {
        switch (step)
        {
            case 1:
                wizard.Name = posted.Name;
                wizard.SchoolCode = posted.SchoolCode;
                wizard.ShortName = posted.ShortName;
                wizard.Tagline = posted.Tagline;
                wizard.RegistrationNumber = posted.RegistrationNumber;
                wizard.Email = posted.Email;
                wizard.Phone = posted.Phone;
                wizard.SchoolType = posted.SchoolType;
                wizard.PrincipalName = posted.PrincipalName;
                wizard.Website = posted.Website;
                wizard.EstablishedYear = posted.EstablishedYear;
                wizard.Description = posted.Description;
                wizard.Status = posted.Status;
                break;
            case 2:
                wizard.Address = posted.Address;
                wizard.City = posted.City;
                wizard.StateProvince = posted.StateProvince;
                wizard.Country = posted.Country;
                wizard.PostalCode = posted.PostalCode;
                wizard.EmergencyContact = posted.EmergencyContact;
                wizard.PrimaryContactName = posted.PrimaryContactName;
                wizard.PrimaryContactEmail = posted.PrimaryContactEmail;
                wizard.PrimaryContactPhone = posted.PrimaryContactPhone;
                break;
            case 3:
                if (!string.IsNullOrWhiteSpace(posted.LogoPath))
                    wizard.LogoPath = posted.LogoPath;
                break;
            case 4:
                wizard.CreateAdmin = posted.CreateAdmin;
                wizard.AdminFullName = posted.AdminFullName;
                wizard.AdminEmail = posted.AdminEmail;
                wizard.AdminLoginId = posted.AdminLoginId;
                wizard.AdminPhone = posted.AdminPhone;
                wizard.AdminTemporaryPassword = posted.AdminTemporaryPassword;
                break;
            case 5:
                wizard.PlanCode = posted.PlanCode;
                wizard.PlanName = posted.PlanName;
                wizard.BillingCycle = posted.BillingCycle;
                wizard.SubscriptionStart = posted.SubscriptionStart;
                wizard.SubscriptionExpiry = posted.SubscriptionExpiry;
                wizard.SubscriptionPrice = posted.SubscriptionPrice;
                wizard.SubscriptionNotes = posted.SubscriptionNotes;
                break;
        }

        if (posted.Id.HasValue)
            wizard.Id = posted.Id;
    }

    private void SaveWizard(SchoolFormVm model)
    {
        model.LogoFile = null;
        TempData[WizardTempKey] = JsonSerializer.Serialize(model, WizardJsonOptions);
    }

    private void ValidateWizardStep(SchoolFormVm model)
    {
        switch (model.Step)
        {
            case 1:
                if (string.IsNullOrWhiteSpace(model.Name))
                    ModelState.AddModelError(nameof(model.Name), "School name is required.");
                if (string.IsNullOrWhiteSpace(model.SchoolCode))
                    ModelState.AddModelError(nameof(model.SchoolCode), "School code is required.");
                if (string.IsNullOrWhiteSpace(model.Email))
                    ModelState.AddModelError(nameof(model.Email), "Email is required.");
                break;
            case 4 when model.CreateAdmin:
                if (string.IsNullOrWhiteSpace(model.AdminFullName))
                    ModelState.AddModelError(nameof(model.AdminFullName), "Admin name is required.");
                if (string.IsNullOrWhiteSpace(model.AdminEmail))
                    ModelState.AddModelError(nameof(model.AdminEmail), "Admin email is required.");
                if (string.IsNullOrWhiteSpace(model.AdminLoginId))
                    ModelState.AddModelError(nameof(model.AdminLoginId), "Admin login ID is required.");
                if (string.IsNullOrWhiteSpace(model.AdminTemporaryPassword) || model.AdminTemporaryPassword.Length < 8)
                    ModelState.AddModelError(nameof(model.AdminTemporaryPassword), "Temporary password must be at least 8 characters.");
                break;
            case 5:
                if (model.SubscriptionExpiry <= model.SubscriptionStart)
                    ModelState.AddModelError(nameof(model.SubscriptionExpiry), "Expiry must be after the start date.");
                if (string.IsNullOrWhiteSpace(model.PlanName))
                    ModelState.AddModelError(nameof(model.PlanName), "Plan name is required.");
                break;
        }
    }

    private async Task ValidateWizardAdminAsync(SchoolFormVm model, CancellationToken ct)
    {
        if (!model.CreateAdmin) return;
        var email = model.AdminEmail?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(email))
        {
            var existing = await UserManager.FindByEmailAsync(email);
            if (existing is not null)
                ModelState.AddModelError(nameof(model.AdminEmail), "This email is already registered.");
        }

        var loginId = model.AdminLoginId?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(loginId))
        {
            var loginTaken = await Db.Users.AnyAsync(u => u.LoginId == loginId, ct);
            if (loginTaken || await UserManager.FindByNameAsync(loginId) is not null)
                ModelState.AddModelError(nameof(model.AdminLoginId), "This login ID is already taken.");
        }
    }

    private async Task ValidateSchoolUniquenessAsync(SchoolFormVm model, Guid? excludeId, CancellationToken ct)
    {
        var code = model.SchoolCode?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(code))
        {
            var codeTaken = await Db.Schools.AnyAsync(s =>
                s.SchoolCode == code && (!excludeId.HasValue || s.Id != excludeId.Value), ct);
            if (codeTaken)
                ModelState.AddModelError(nameof(model.SchoolCode), "This school code is already in use. Choose a unique code.");
        }

        var email = model.Email?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(email))
        {
            var emailTaken = await Db.Schools.AnyAsync(s =>
                s.Email != null &&
                s.Email.ToLower() == email.ToLower() &&
                (!excludeId.HasValue || s.Id != excludeId.Value), ct);
            if (emailTaken)
                ModelState.AddModelError(nameof(model.Email), "A school with this email already exists.");
        }
    }

    private async Task ValidateAdminUniquenessAsync(SchoolAdminFormVm model, string? excludeUserId, CancellationToken ct)
    {
        var email = model.Email?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(email))
        {
            var existing = await UserManager.FindByEmailAsync(email);
            if (existing is not null && !string.Equals(existing.Id, excludeUserId, StringComparison.Ordinal))
                ModelState.AddModelError(nameof(model.Email), "This email is already registered to another user.");
        }

        var loginId = model.LoginId?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(loginId))
        {
            var loginTaken = await Db.Users.AnyAsync(u =>
                u.LoginId == loginId &&
                (excludeUserId == null || u.Id != excludeUserId), ct);
            if (loginTaken)
                ModelState.AddModelError(nameof(model.LoginId), "This login ID is already taken.");

            var userNameTaken = await UserManager.FindByNameAsync(loginId);
            if (userNameTaken is not null && !string.Equals(userNameTaken.Id, excludeUserId, StringComparison.Ordinal))
                ModelState.AddModelError(nameof(model.LoginId), "This login ID is already taken.");
        }
    }

    private static SchoolFormVm MapToForm(School school) => new()
    {
        Id = school.Id,
        Name = school.Name,
        SchoolCode = school.SchoolCode,
        ShortName = school.ShortName,
        Tagline = school.Tagline,
        RegistrationNumber = school.RegistrationNumber,
        Email = school.Email ?? string.Empty,
        Phone = school.Phone,
        Address = school.Address,
        City = school.City,
        StateProvince = school.StateProvince,
        Country = school.Country,
        PostalCode = school.PostalCode,
        SchoolType = school.SchoolType,
        PrincipalName = school.PrincipalName,
        EstablishedYear = school.EstablishedYear,
        Description = school.Description,
        EmergencyContact = school.EmergencyContact,
        PrimaryContactName = school.PrimaryContactName,
        PrimaryContactEmail = school.PrimaryContactEmail,
        PrimaryContactPhone = school.PrimaryContactPhone,
        Website = school.Website,
        Status = school.Status,
        LogoPath = school.LogoPath
    };

    private static School MapToEntity(SchoolFormVm model)
    {
        var school = new School();
        ApplyForm(school, model);
        return school;
    }

    private static void ApplyForm(School school, SchoolFormVm model)
    {
        school.Name = model.Name.Trim();
        school.SchoolCode = model.SchoolCode.Trim();
        school.ShortName = NullIfWhiteSpace(model.ShortName);
        school.Tagline = NullIfWhiteSpace(model.Tagline);
        school.RegistrationNumber = NullIfWhiteSpace(model.RegistrationNumber);
        school.Email = NullIfWhiteSpace(model.Email);
        school.Phone = NullIfWhiteSpace(model.Phone);
        school.Address = NullIfWhiteSpace(model.Address);
        school.City = NullIfWhiteSpace(model.City);
        school.StateProvince = NullIfWhiteSpace(model.StateProvince);
        school.Country = NullIfWhiteSpace(model.Country);
        school.PostalCode = NullIfWhiteSpace(model.PostalCode);
        school.SchoolType = NullIfWhiteSpace(model.SchoolType);
        school.PrincipalName = NullIfWhiteSpace(model.PrincipalName);
        school.EstablishedYear = model.EstablishedYear;
        school.Description = NullIfWhiteSpace(model.Description);
        school.EmergencyContact = NullIfWhiteSpace(model.EmergencyContact);
        school.PrimaryContactName = NullIfWhiteSpace(model.PrimaryContactName);
        school.PrimaryContactEmail = NullIfWhiteSpace(model.PrimaryContactEmail);
        school.PrimaryContactPhone = NullIfWhiteSpace(model.PrimaryContactPhone);
        school.Website = NullIfWhiteSpace(model.Website);
        school.Status = model.Status;
        if (!string.IsNullOrWhiteSpace(model.LogoPath))
            school.LogoPath = model.LogoPath;
    }
}
