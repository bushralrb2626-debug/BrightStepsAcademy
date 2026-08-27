using System.Text.Json;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Authorize(Roles = AppRoleNames.SuperAdmin)]
[Route("Manage/SuperAdmin")]
[Route("superadmin")]
public abstract class SuperAdminControllerBase : Controller
{
    protected readonly AppDbContext Db;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly SignInManager<ApplicationUser> SignInManager;
    protected readonly IFileStorageService Files;

    protected SuperAdminControllerBase(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFileStorageService files)
    {
        Db = db;
        UserManager = userManager;
        SignInManager = signInManager;
        Files = files;
    }

    protected IActionResult ManageView(string name, object? model = null)
        => View($"~/Views/Manage/SuperAdmin/{name}.cshtml", model);

    protected async Task<int> GetWarningDaysAsync(CancellationToken ct)
    {
        var settings = await Db.PlatformSettings.AsNoTracking().OrderBy(s => s.UpdatedAt).FirstOrDefaultAsync(ct);
        return settings?.ExpiryWarningDays ?? 30;
    }

    protected async Task RefreshAllSubscriptionsAsync(CancellationToken ct)
    {
        var warningDays = await GetWarningDaysAsync(ct);
        var subs = await Db.SchoolSubscriptions.ToListAsync(ct);
        foreach (var sub in subs)
            SubscriptionStatusHelper.Refresh(sub, warningDays);
        await Db.SaveChangesAsync(ct);
    }

    protected async Task WriteAuditAsync(
        Guid? schoolId,
        string action,
        string module,
        string? recordType,
        string? recordId,
        string details,
        CancellationToken ct)
    {
        var userId = UserManager.GetUserId(User) ?? string.Empty;
        var userName = User.Identity?.Name;

        Db.AuditLogs.Add(new AuditLog
        {
            SchoolId = schoolId,
            UserId = userId,
            UserName = userName,
            Action = action,
            Module = module,
            RecordType = recordType,
            RecordId = recordId,
            Details = details,
            Timestamp = DateTimeOffset.UtcNow
        });

        await Task.CompletedTask;
    }

    protected static string FriendlyIdentityError(string description)
    {
        if (description.Contains("Password", StringComparison.OrdinalIgnoreCase))
            return "Password does not meet requirements. Use at least 8 characters with mixed case, a number, and a symbol.";
        if (description.Contains("Email", StringComparison.OrdinalIgnoreCase))
            return "This email cannot be used. It may already be registered.";
        if (description.Contains("Username", StringComparison.OrdinalIgnoreCase) ||
            description.Contains("User name", StringComparison.OrdinalIgnoreCase))
            return "This login ID cannot be used. It may already be taken.";
        return description;
    }

    protected static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected static readonly JsonSerializerOptions WizardJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}
