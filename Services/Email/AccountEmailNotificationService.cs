using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BrightStepsAcademy.Services.Email;

public sealed class AccountEmailRequest
{
    public required Guid SchoolId { get; init; }
    public required string UserId { get; init; }
    public required string RecipientEmail { get; init; }
    public required string UserName { get; init; }
    public required string LoginId { get; init; }
    public required string TemporaryPassword { get; init; }
    public required PortalAccountType AccountType { get; init; }
}

public interface IAccountEmailNotificationService
{
    Task<AccountEmailLog> SendNewAccountEmailAsync(AccountEmailRequest request, CancellationToken ct = default);
    Task<AccountEmailLog> SendPasswordResetEmailAsync(AccountEmailRequest request, CancellationToken ct = default);
    Task<AccountEmailLog> SendPasswordChangedEmailAsync(string userId, CancellationToken ct = default);
    Task<AccountEmailLog> ResendCredentialsEmailAsync(string userId, Guid schoolId, PortalAccountType accountType, CancellationToken ct = default);
    Task<AccountEmailLog?> GetLatestStatusAsync(string userId, AccountEmailType emailType, CancellationToken ct = default);
}

public class AccountEmailNotificationService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IEmailTemplateRenderer templates,
    IOptions<EmailOptions> emailOptions,
    ILogger<AccountEmailNotificationService> logger) : IAccountEmailNotificationService
{
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    public Task<AccountEmailLog> SendNewAccountEmailAsync(AccountEmailRequest request, CancellationToken ct = default)
        => SendTemplatedAsync(
            request,
            AccountEmailType.NewAccountCreated,
            "NewAccountCreated",
            AccountEmailNotificationService.BuildNewAccountSubject(GetSchoolNameSync(request.SchoolId), request.AccountType),
            includePassword: true,
            ct);

    public async Task<AccountEmailLog> SendPasswordResetEmailAsync(AccountEmailRequest request, CancellationToken ct = default)
        => await SendTemplatedAsync(
            request,
            AccountEmailType.PasswordReset,
            "PasswordReset",
            $"{await GetSchoolNameAsync(request.SchoolId, ct)} - Password Reset",
            includePassword: true,
            ct);

    public async Task<AccountEmailLog> SendPasswordChangedEmailAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new InvalidOperationException("User not found.");

        var recipient = ResolveRecipientEmail(user.Email);
        var schoolId = user.SchoolId ?? Guid.Empty;
        var schoolName = schoolId == Guid.Empty ? "BrightSteps Academy" : await GetSchoolNameAsync(schoolId, ct);
        var accountType = await ResolveAccountTypeAsync(user, ct);

        var log = new AccountEmailLog
        {
            SchoolId = schoolId == Guid.Empty ? null : schoolId,
            UserId = user.Id,
            RecipientEmail = recipient ?? string.Empty,
            EmailType = AccountEmailType.PasswordChanged,
            AccountType = accountType,
            Status = AccountEmailDeliveryStatus.Pending
        };
        db.AccountEmailLogs.Add(log);
        await db.SaveChangesAsync(ct);

        if (string.IsNullOrWhiteSpace(recipient))
        {
            await MarkFailedAsync(log, "No deliverable email address on file.", ct);
            return log;
        }

        try
        {
            var placeholders = await BuildBasePlaceholdersAsync(schoolId, user.FullName, schoolName, accountType, ct);
            placeholders["Date"] = DateTime.Now.ToString("dd MMMM yyyy");
            placeholders["Heading"] = "Password Changed Successfully";

            var html = await templates.RenderAsync("PasswordChanged", placeholders, ct);
            await emailSender.SendAsync(new EmailMessage
            {
                ToEmail = recipient,
                ToName = user.FullName,
                Subject = $"{schoolName} - Password Changed Successfully",
                HtmlBody = html
            }, ct);

            await MarkSentAsync(log, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send password changed email to user {UserId}", userId);
            await MarkFailedAsync(log, ex.Message, ct);
        }

        return log;
    }

    public async Task<AccountEmailLog> ResendCredentialsEmailAsync(
        string userId,
        Guid schoolId,
        PortalAccountType accountType,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId)
                   ?? throw new InvalidOperationException("User not found.");

        var recipient = ResolveRecipientEmail(user.Email);
        if (string.IsNullOrWhiteSpace(recipient))
        {
            var failed = await CreateLogAsync(user, schoolId, accountType, AccountEmailType.NewAccountCreated, string.Empty, ct);
            await MarkFailedAsync(failed, "No deliverable email address on file.", ct);
            return failed;
        }

        var tempPassword = GenerateTemporaryPassword();
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await userManager.ResetPasswordAsync(user, token, tempPassword);
        if (!reset.Succeeded)
            throw new InvalidOperationException(string.Join(" ", reset.Errors.Select(e => e.Description)));

        user.MustChangePassword = true;
        await userManager.UpdateAsync(user);

        return await SendNewAccountEmailAsync(new AccountEmailRequest
        {
            SchoolId = schoolId,
            UserId = user.Id,
            RecipientEmail = recipient,
            UserName = user.FullName,
            LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
            TemporaryPassword = tempPassword,
            AccountType = accountType
        }, ct);
    }

    public Task<AccountEmailLog?> GetLatestStatusAsync(string userId, AccountEmailType emailType, CancellationToken ct = default)
        => db.AccountEmailLogs.AsNoTracking()
            .Where(l => l.UserId == userId && l.EmailType == emailType)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(ct)!;

    private async Task<AccountEmailLog> SendTemplatedAsync(
        AccountEmailRequest request,
        AccountEmailType emailType,
        string templateName,
        string subject,
        bool includePassword,
        CancellationToken ct)
    {
        var log = await CreateLogAsync(
            new ApplicationUser { Id = request.UserId, FullName = request.UserName, Email = request.RecipientEmail },
            request.SchoolId,
            request.AccountType,
            emailType,
            request.RecipientEmail,
            ct);

        if (!IsDeliverableEmail(request.RecipientEmail))
        {
            await MarkFailedAsync(log, "No deliverable email address was provided.", ct);
            return log;
        }

        try
        {
            var schoolName = await GetSchoolNameAsync(request.SchoolId, ct);
            var placeholders = await BuildBasePlaceholdersAsync(
                request.SchoolId, request.UserName, schoolName, request.AccountType, ct);
            placeholders["LoginId"] = request.LoginId;
            placeholders["Heading"] = emailType switch
            {
                AccountEmailType.PasswordReset => "Password Reset",
                _ => "New Account Created"
            };

            if (includePassword)
                placeholders["TemporaryPassword"] = request.TemporaryPassword;
            else
                placeholders["TemporaryPassword"] = string.Empty;

            var html = await templates.RenderAsync(templateName, placeholders, ct);
            await emailSender.SendAsync(new EmailMessage
            {
                ToEmail = request.RecipientEmail,
                ToName = request.UserName,
                Subject = subject,
                HtmlBody = html
            }, ct);

            await MarkSentAsync(log, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send {EmailType} email to user {UserId}", emailType, request.UserId);
            await MarkFailedAsync(log, ex.Message, ct);
        }

        return log;
    }

    private async Task<AccountEmailLog> CreateLogAsync(
        ApplicationUser user,
        Guid schoolId,
        PortalAccountType accountType,
        AccountEmailType emailType,
        string recipientEmail,
        CancellationToken ct)
    {
        var log = new AccountEmailLog
        {
            SchoolId = schoolId,
            UserId = user.Id,
            RecipientEmail = recipientEmail,
            EmailType = emailType,
            AccountType = accountType,
            Status = AccountEmailDeliveryStatus.Pending
        };
        db.AccountEmailLogs.Add(log);
        await db.SaveChangesAsync(ct);
        return log;
    }

    private async Task MarkSentAsync(AccountEmailLog log, CancellationToken ct)
    {
        log.Status = AccountEmailDeliveryStatus.Sent;
        log.SentAt = DateTimeOffset.UtcNow;
        log.FailureReason = null;
        await db.SaveChangesAsync(ct);
    }

    private async Task MarkFailedAsync(AccountEmailLog log, string reason, CancellationToken ct)
    {
        log.Status = AccountEmailDeliveryStatus.Failed;
        log.FailureReason = reason.Length > 500 ? reason[..500] : reason;
        await db.SaveChangesAsync(ct);
    }

    private async Task<Dictionary<string, string>> BuildBasePlaceholdersAsync(
        Guid schoolId,
        string userName,
        string schoolName,
        PortalAccountType accountType,
        CancellationToken ct)
    {
        var school = await db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == schoolId, ct);
        var logoBlock = BuildLogoBlock(school);

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["SchoolName"] = schoolName,
            ["UserName"] = userName,
            ["AccountType"] = accountType.ToString(),
            ["AccountTypeLabel"] = GetAccountTypeLabel(accountType),
            ["Date"] = DateTime.Now.ToString("dd MMMM yyyy"),
            ["LogoBlock"] = logoBlock
        };
    }

    private string BuildLogoBlock(School? school)
    {
        if (school is null || string.IsNullOrWhiteSpace(school.LogoPath))
            return string.Empty;

        var logoUrl = school.LogoPath.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? school.LogoPath
            : $"{_emailOptions.BaseUrl.TrimEnd('/')}/{school.LogoPath.TrimStart('/')}";

        return $"""<img class="logo" src="{logoUrl}" alt="{EmailTemplateRenderer.Escape(school.Name)} logo" />""";
    }

    private async Task<string> GetSchoolNameAsync(Guid schoolId, CancellationToken ct)
    {
        var name = await db.Schools.AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(ct);
        return string.IsNullOrWhiteSpace(name) ? "BrightSteps Academy" : name;
    }

    private string GetSchoolNameSync(Guid schoolId)
    {
        var name = db.Schools.AsNoTracking().Where(s => s.Id == schoolId).Select(s => s.Name).FirstOrDefault();
        return string.IsNullOrWhiteSpace(name) ? "BrightSteps Academy" : name;
    }

    private async Task<PortalAccountType> ResolveAccountTypeAsync(ApplicationUser user, CancellationToken ct)
    {
        if (await userManager.IsInRoleAsync(user, AppRoleNames.Student)) return PortalAccountType.Student;
        if (await userManager.IsInRoleAsync(user, AppRoleNames.Teacher)) return PortalAccountType.Teacher;
        if (await userManager.IsInRoleAsync(user, AppRoleNames.Guardian)) return PortalAccountType.Guardian;
        if (await userManager.IsInRoleAsync(user, AppRoleNames.SchoolAdmin)
            || await userManager.IsInRoleAsync(user, AppRoleNames.CustomAdmin)) return PortalAccountType.Admin;
        return PortalAccountType.Staff;
    }

    public static string? ResolveRecipientEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var trimmed = email.Trim();
        if (trimmed.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
            return null;

        return trimmed;
    }

    public static bool IsDeliverableEmail(string? email)
        => !string.IsNullOrWhiteSpace(ResolveRecipientEmail(email));

    public static string GetAccountTypeLabel(PortalAccountType accountType) => accountType switch
    {
        PortalAccountType.Student => "student",
        PortalAccountType.Teacher => "teacher",
        PortalAccountType.Guardian => "guardian",
        PortalAccountType.Admin => "admin",
        _ => "staff"
    };

    public static string BuildNewAccountSubject(string schoolName, PortalAccountType accountType)
    {
        var label = accountType switch
        {
            PortalAccountType.Student => "Student Account Created",
            PortalAccountType.Teacher => "Teacher Account Created",
            PortalAccountType.Guardian => "Guardian Account Created",
            PortalAccountType.Admin => "Admin Account Created",
            _ => "New Account Created"
        };
        return $"{schoolName} - {label}";
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var random = new char[8];
        for (var i = 0; i < random.Length; i++)
            random[i] = chars[Random.Shared.Next(chars.Length)];
        return $"Temp@{new string(random)}1";
    }
}
