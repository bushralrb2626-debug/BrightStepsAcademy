using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public sealed class StudentAccountRequest
{
    public Guid SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public string? UpdatedByUserId { get; init; }
    public bool EnablePortal { get; init; }
    public string? LoginId { get; init; }
    public string? Password { get; init; }
    public bool ResetPassword { get; init; }
    public string? NewPassword { get; init; }
}

public sealed class StudentAccountResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static StudentAccountResult Ok() => new() { Success = true };
    public static StudentAccountResult Fail(string error) => new() { Success = false, Error = error };
}

public interface IStudentAccountService
{
    Task<StudentAccountResult> ConfigureLoginAsync(StudentAccountRequest request, CancellationToken ct = default);
}

public class StudentAccountService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IAccountEmailNotificationService accountEmails) : IStudentAccountService
{
    public async Task<StudentAccountResult> ConfigureLoginAsync(StudentAccountRequest request, CancellationToken ct = default)
    {
        var student = await db.StudentRecords.FirstOrDefaultAsync(
            s => s.Id == request.StudentId && s.SchoolId == request.SchoolId, ct);
        if (student is null)
            return StudentAccountResult.Fail("Student not found.");

        if (!request.EnablePortal)
        {
            if (student.UserId is not null)
            {
                var user = await userManager.FindByIdAsync(student.UserId);
                if (user is not null)
                {
                    user.IsActive = false;
                    await userManager.UpdateAsync(user);
                }
            }

            student.UserId = null;
            student.UpdatedAt = DateTimeOffset.UtcNow;
            student.UpdatedByUserId = request.UpdatedByUserId;
            await db.SaveChangesAsync(ct);
            return StudentAccountResult.Ok();
        }

        if (string.IsNullOrWhiteSpace(request.Password) && student.UserId is null)
            return StudentAccountResult.Fail("Initial password is required when enabling the student portal.");

        if (request.ResetPassword)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return StudentAccountResult.Fail("Enter a new temporary password to reset the student password.");
            if (student.UserId is null)
                return StudentAccountResult.Fail("Student portal login does not exist yet.");

            var existing = await userManager.FindByIdAsync(student.UserId);
            if (existing is null)
                return StudentAccountResult.Fail("Student login account not found.");

            var token = await userManager.GeneratePasswordResetTokenAsync(existing);
            var reset = await userManager.ResetPasswordAsync(existing, token, request.NewPassword);
            if (!reset.Succeeded)
                return StudentAccountResult.Fail(string.Join(" ", reset.Errors.Select(e => e.Description)));

            existing.MustChangePassword = true;
            await userManager.UpdateAsync(existing);

            await TrySendPasswordResetEmailAsync(existing, student, request, ct);
            return StudentAccountResult.Ok();
        }

        if (student.UserId is not null)
        {
            var user = await userManager.FindByIdAsync(student.UserId);
            if (user is not null)
            {
                user.FullName = student.FullName;
                user.IsActive = true;
                if (!string.IsNullOrWhiteSpace(request.LoginId))
                    user.LoginId = request.LoginId.Trim();
                if (!string.IsNullOrWhiteSpace(student.Email))
                {
                    user.Email = student.Email;
                    user.UserName = student.Email;
                    user.NormalizedEmail = student.Email.ToUpperInvariant();
                    user.NormalizedUserName = student.Email.ToUpperInvariant();
                }

                await userManager.UpdateAsync(user);
                return StudentAccountResult.Ok();
            }
        }

        var email = await ResolveUniqueStudentEmailAsync(student, request, ct);

        if (!string.IsNullOrWhiteSpace(request.LoginId))
        {
            var loginTaken = await userManager.Users.AnyAsync(
                u => u.LoginId != null && u.LoginId.ToLower() == request.LoginId.Trim().ToLower(), ct);
            if (loginTaken)
                return StudentAccountResult.Fail("This login ID is already in use.");
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = student.FullName,
            LoginId = request.LoginId?.Trim(),
            SchoolId = request.SchoolId,
            IsActive = true,
            MustChangePassword = true
        };

        var create = await userManager.CreateAsync(newUser, request.Password!);
        if (!create.Succeeded)
            return StudentAccountResult.Fail(string.Join(" ", create.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(newUser, AppRoleNames.Student);
        student.UserId = newUser.Id;
        student.UpdatedAt = DateTimeOffset.UtcNow;
        student.UpdatedByUserId = request.UpdatedByUserId;
        await db.SaveChangesAsync(ct);

        await TrySendNewAccountEmailAsync(newUser, student, request, ct);
        return StudentAccountResult.Ok();
    }

    private async Task TrySendNewAccountEmailAsync(
        ApplicationUser user,
        StudentRecord student,
        StudentAccountRequest request,
        CancellationToken ct)
    {
        var recipient = AccountEmailNotificationService.ResolveRecipientEmail(student.Email)
                        ?? AccountEmailNotificationService.ResolveRecipientEmail(user.Email);
        if (recipient is null || string.IsNullOrWhiteSpace(request.Password))
            return;

        await accountEmails.SendNewAccountEmailAsync(new AccountEmailRequest
        {
            SchoolId = request.SchoolId,
            UserId = user.Id,
            RecipientEmail = recipient,
            UserName = user.FullName,
            LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
            TemporaryPassword = request.Password,
            AccountType = PortalAccountType.Student
        }, ct);
    }

    private async Task TrySendPasswordResetEmailAsync(
        ApplicationUser user,
        StudentRecord student,
        StudentAccountRequest request,
        CancellationToken ct)
    {
        var recipient = AccountEmailNotificationService.ResolveRecipientEmail(student.Email)
                        ?? AccountEmailNotificationService.ResolveRecipientEmail(user.Email);
        if (recipient is null || string.IsNullOrWhiteSpace(request.NewPassword))
            return;

        await accountEmails.SendPasswordResetEmailAsync(new AccountEmailRequest
        {
            SchoolId = request.SchoolId,
            UserId = user.Id,
            RecipientEmail = recipient,
            UserName = user.FullName,
            LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
            TemporaryPassword = request.NewPassword,
            AccountType = PortalAccountType.Student
        }, ct);
    }

    private async Task<string> ResolveUniqueStudentEmailAsync(
        StudentRecord student,
        StudentAccountRequest request,
        CancellationToken ct)
    {
        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(student.Email))
            candidates.Add(student.Email.Trim());

        if (!string.IsNullOrWhiteSpace(request.LoginId))
            candidates.Add($"{request.LoginId.Trim().ToLowerInvariant()}@student.local");

        candidates.Add($"student-{student.StudentCode.ToLowerInvariant()}@{request.SchoolId:N}.school.local");
        candidates.Add($"student-{student.Id:N}@{request.SchoolId:N}.school.local");

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (await userManager.FindByEmailAsync(candidate) is null)
                return candidate;
        }

        return $"student-{student.Id:N}@{request.SchoolId:N}.school.local";
    }
}
