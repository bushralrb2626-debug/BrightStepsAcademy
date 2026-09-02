using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public sealed class GuardianAssignmentRequest
{
    public Guid SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public string? CreatedByUserId { get; init; }
    public bool UseExistingGuardian { get; init; }
    public Guid? ExistingGuardianId { get; init; }
    public string GuardianName { get; init; } = "";
    public string Relationship { get; init; } = "";
    public string? GuardianEmail { get; init; }
    public string? GuardianPhone { get; init; }
    public bool EnablePortal { get; init; }
    public string? LoginId { get; init; }
    public string? Password { get; init; }
}

public sealed class GuardianUpdateRequest
{
    public Guid SchoolId { get; init; }
    public Guid StudentId { get; init; }
    public string? UpdatedByUserId { get; init; }
    public bool ChangeGuardian { get; init; }
    public bool UseExistingGuardian { get; init; }
    public Guid? ExistingGuardianId { get; init; }
    public string GuardianName { get; init; } = "";
    public string Relationship { get; init; } = "";
    public string? GuardianEmail { get; init; }
    public string? GuardianPhone { get; init; }
    public bool EnablePortal { get; init; }
    public string? LoginId { get; init; }
    public string? Password { get; init; }
    public bool ResetPassword { get; init; }
    public string? NewPassword { get; init; }
}

public sealed class GuardianOperationResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    public static GuardianOperationResult Ok() => new() { Success = true };
    public static GuardianOperationResult Fail(string error) => new() { Success = false, Error = error };
}

public interface IGuardianService
{
    Task<IReadOnlyList<GuardianProfile>> ListGuardiansAsync(Guid schoolId, CancellationToken ct = default);
    Task<StudentGuardianLink?> GetLinkForStudentAsync(Guid studentId, Guid schoolId, CancellationToken ct = default);
    Task<GuardianProfile?> GetProfileForUserAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentRecord>> GetLinkedStudentsAsync(string userId, CancellationToken ct = default);
    Task<GuardianOperationResult> AssignGuardianAsync(GuardianAssignmentRequest request, CancellationToken ct = default);
    Task<GuardianOperationResult> UpdateGuardianAsync(GuardianUpdateRequest request, CancellationToken ct = default);
}

public class GuardianService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IAccountEmailNotificationService accountEmails) : IGuardianService
{
    public async Task<IReadOnlyList<GuardianProfile>> ListGuardiansAsync(Guid schoolId, CancellationToken ct = default)
        => await db.GuardianProfiles.AsNoTracking()
            .Where(g => g.SchoolId == schoolId && g.IsActive)
            .OrderBy(g => g.FullName)
            .ToListAsync(ct);

    public Task<StudentGuardianLink?> GetLinkForStudentAsync(Guid studentId, Guid schoolId, CancellationToken ct = default)
        => db.StudentGuardianLinks
            .Include(l => l.Guardian)
            .FirstOrDefaultAsync(l => l.StudentId == studentId && l.SchoolId == schoolId, ct);

    public Task<GuardianProfile?> GetProfileForUserAsync(string userId, CancellationToken ct = default)
        => db.GuardianProfiles.AsNoTracking()
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive && g.PortalEnabled, ct);

    public async Task<IReadOnlyList<StudentRecord>> GetLinkedStudentsAsync(string userId, CancellationToken ct = default)
    {
        var profile = await db.GuardianProfiles.AsNoTracking()
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive && g.PortalEnabled, ct);
        if (profile is null)
            return Array.Empty<StudentRecord>();

        return await db.StudentGuardianLinks.AsNoTracking()
            .Where(l => l.GuardianProfileId == profile.Id && l.IsActive)
            .Join(db.StudentRecords.AsNoTracking(),
                l => l.StudentId,
                s => s.Id,
                (_, s) => s)
            .Where(s => s.IsActive)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);
    }

    public async Task<GuardianOperationResult> AssignGuardianAsync(GuardianAssignmentRequest request, CancellationToken ct = default)
    {
        if (await db.StudentGuardianLinks.AnyAsync(l => l.StudentId == request.StudentId, ct))
            return GuardianOperationResult.Fail("This student already has a guardian portal assigned.");

        var student = await db.StudentRecords.FirstOrDefaultAsync(
            s => s.Id == request.StudentId && s.SchoolId == request.SchoolId, ct);
        if (student is null)
            return GuardianOperationResult.Fail("Student not found.");

        var guardianResult = request.UseExistingGuardian
            ? await ResolveExistingGuardianAsync(request, ct)
            : await CreateNewGuardianAsync(request, ct);

        if (!guardianResult.Success || guardianResult.Guardian is null)
            return GuardianOperationResult.Fail(guardianResult.Error ?? "Could not assign guardian.");

        db.StudentGuardianLinks.Add(new StudentGuardianLink
        {
            SchoolId = request.SchoolId,
            StudentId = request.StudentId,
            GuardianProfileId = guardianResult.Guardian.Id,
            Relationship = request.Relationship.Trim(),
            CreatedByUserId = request.CreatedByUserId,
            IsActive = true
        });

        SyncStudentParentFields(student, guardianResult.Guardian, request.Relationship);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return GuardianOperationResult.Fail("Could not assign guardian. Each student may only have one guardian portal.");
        }

        return GuardianOperationResult.Ok();
    }

    public async Task<GuardianOperationResult> UpdateGuardianAsync(GuardianUpdateRequest request, CancellationToken ct = default)
    {
        var link = await db.StudentGuardianLinks
            .Include(l => l.Guardian)
            .Include(l => l.Student)
            .FirstOrDefaultAsync(l => l.StudentId == request.StudentId && l.SchoolId == request.SchoolId, ct);

        if (request.ChangeGuardian)
        {
            if (link is not null)
            {
                db.StudentGuardianLinks.Remove(link);
                await db.SaveChangesAsync(ct);
            }

            return await AssignGuardianAsync(new GuardianAssignmentRequest
            {
                SchoolId = request.SchoolId,
                StudentId = request.StudentId,
                CreatedByUserId = request.UpdatedByUserId,
                UseExistingGuardian = request.UseExistingGuardian,
                ExistingGuardianId = request.ExistingGuardianId,
                GuardianName = request.GuardianName,
                Relationship = request.Relationship,
                GuardianEmail = request.GuardianEmail,
                GuardianPhone = request.GuardianPhone,
                EnablePortal = request.EnablePortal,
                LoginId = request.LoginId,
                Password = request.Password
            }, ct);
        }

        if (link is null)
        {
            return await AssignGuardianAsync(new GuardianAssignmentRequest
            {
                SchoolId = request.SchoolId,
                StudentId = request.StudentId,
                CreatedByUserId = request.UpdatedByUserId,
                UseExistingGuardian = request.UseExistingGuardian,
                ExistingGuardianId = request.ExistingGuardianId,
                GuardianName = request.GuardianName,
                Relationship = request.Relationship,
                GuardianEmail = request.GuardianEmail,
                GuardianPhone = request.GuardianPhone,
                EnablePortal = request.EnablePortal,
                LoginId = request.LoginId,
                Password = request.Password
            }, ct);
        }

        var guardian = link.Guardian;
        guardian.FullName = request.GuardianName.Trim();
        guardian.Email = request.GuardianEmail?.Trim() ?? guardian.Email;
        guardian.Phone = request.GuardianPhone?.Trim();
        guardian.LoginId = string.IsNullOrWhiteSpace(request.LoginId) ? guardian.LoginId : request.LoginId.Trim();
        guardian.UpdatedAt = DateTimeOffset.UtcNow;
        guardian.UpdatedByUserId = request.UpdatedByUserId;
        link.Relationship = request.Relationship.Trim();
        link.UpdatedAt = DateTimeOffset.UtcNow;
        link.UpdatedByUserId = request.UpdatedByUserId;

        SyncStudentParentFields(link.Student, guardian, request.Relationship);

        if (request.EnablePortal && !guardian.PortalEnabled)
        {
            var password = request.Password ?? request.NewPassword;
            if (string.IsNullOrWhiteSpace(password))
                return GuardianOperationResult.Fail("Initial password is required when enabling the guardian portal.");

            var enableResult = await EnablePortalAsync(guardian, request.SchoolId, request.LoginId, password, ct);
            if (!enableResult.Success)
                return enableResult;
        }
        else if (!request.EnablePortal && guardian.PortalEnabled)
        {
            await DisablePortalAsync(guardian, ct);
        }
        else if (request.EnablePortal && guardian.PortalEnabled && guardian.UserId is not null)
        {
            var user = await userManager.FindByIdAsync(guardian.UserId);
            if (user is not null)
            {
                user.FullName = guardian.FullName;
                user.LoginId = guardian.LoginId;
                user.PhoneNumber = guardian.Phone;
                if (!string.IsNullOrWhiteSpace(guardian.Email) && user.Email != guardian.Email)
                {
                    user.Email = guardian.Email;
                    user.UserName = guardian.Email;
                    user.NormalizedEmail = guardian.Email.ToUpperInvariant();
                    user.NormalizedUserName = guardian.Email.ToUpperInvariant();
                }
                await userManager.UpdateAsync(user);
            }
        }

        if (request.ResetPassword && request.EnablePortal)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return GuardianOperationResult.Fail("Enter a new temporary password to reset the guardian password.");

            if (guardian.UserId is null)
                return GuardianOperationResult.Fail("Guardian portal login does not exist yet.");

            var user = await userManager.FindByIdAsync(guardian.UserId);
            if (user is null)
                return GuardianOperationResult.Fail("Guardian login account not found.");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!reset.Succeeded)
                return GuardianOperationResult.Fail(string.Join(" ", reset.Errors.Select(e => e.Description)));

            user.MustChangePassword = true;
            await userManager.UpdateAsync(user);

            await TrySendGuardianPasswordResetEmailAsync(user, guardian, request.SchoolId, request.NewPassword, ct);
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return GuardianOperationResult.Fail("Could not update guardian assignment.");
        }

        return GuardianOperationResult.Ok();
    }

    private sealed class GuardianResolveResult
    {
        public bool Success { get; init; }
        public GuardianProfile? Guardian { get; init; }
        public string? Error { get; init; }
    }

    private async Task<GuardianResolveResult> ResolveExistingGuardianAsync(GuardianAssignmentRequest request, CancellationToken ct)
    {
        if (!request.ExistingGuardianId.HasValue)
            return new GuardianResolveResult { Success = false, Error = "Select an existing guardian to link." };

        var guardian = await db.GuardianProfiles.FirstOrDefaultAsync(
            g => g.Id == request.ExistingGuardianId.Value && g.SchoolId == request.SchoolId && g.IsActive, ct);

        if (guardian is null)
            return new GuardianResolveResult { Success = false, Error = "Selected guardian was not found." };

        if (request.EnablePortal && !guardian.PortalEnabled)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return new GuardianResolveResult { Success = false, Error = "Initial password is required to enable portal access for this guardian." };

            var enableResult = await EnablePortalAsync(guardian, request.SchoolId, request.LoginId ?? guardian.LoginId, request.Password, ct);
            if (!enableResult.Success)
                return new GuardianResolveResult { Success = false, Error = enableResult.Error };
        }

        return new GuardianResolveResult { Success = true, Guardian = guardian };
    }

    private async Task<GuardianResolveResult> CreateNewGuardianAsync(GuardianAssignmentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.GuardianName))
            return new GuardianResolveResult { Success = false, Error = "Guardian name is required." };

        if (string.IsNullOrWhiteSpace(request.Relationship))
            return new GuardianResolveResult { Success = false, Error = "Guardian relationship is required." };

        if (string.IsNullOrWhiteSpace(request.GuardianEmail))
            return new GuardianResolveResult { Success = false, Error = "Guardian email is required." };

        var guardian = new GuardianProfile
        {
            SchoolId = request.SchoolId,
            FullName = request.GuardianName.Trim(),
            Email = request.GuardianEmail.Trim(),
            Phone = request.GuardianPhone?.Trim(),
            LoginId = string.IsNullOrWhiteSpace(request.LoginId) ? null : request.LoginId.Trim(),
            PortalEnabled = request.EnablePortal,
            CreatedByUserId = request.CreatedByUserId,
            IsActive = true
        };

        db.GuardianProfiles.Add(guardian);

        if (request.EnablePortal)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return new GuardianResolveResult { Success = false, Error = "Initial password is required when guardian portal access is enabled." };

            var userResult = await CreateGuardianUserAsync(guardian, request.SchoolId, request.Password, ct);
            if (!userResult.Success)
                return new GuardianResolveResult { Success = false, Error = userResult.Error };
        }

        return new GuardianResolveResult { Success = true, Guardian = guardian };
    }

    private async Task<GuardianOperationResult> EnablePortalAsync(
        GuardianProfile guardian,
        Guid schoolId,
        string? loginId,
        string password,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(loginId))
            guardian.LoginId = loginId.Trim();

        if (guardian.UserId is not null)
        {
            var existing = await userManager.FindByIdAsync(guardian.UserId);
            if (existing is not null)
            {
                existing.IsActive = true;
                await userManager.UpdateAsync(existing);
                guardian.PortalEnabled = true;
                return GuardianOperationResult.Ok();
            }
        }

        var userResult = await CreateGuardianUserAsync(guardian, schoolId, password, ct);
        if (!userResult.Success)
            return GuardianOperationResult.Fail(userResult.Error ?? "Could not create guardian login.");

        guardian.PortalEnabled = true;
        return GuardianOperationResult.Ok();
    }

    private async Task DisablePortalAsync(GuardianProfile guardian, CancellationToken ct)
    {
        guardian.PortalEnabled = false;
        if (guardian.UserId is null)
            return;

        var user = await userManager.FindByIdAsync(guardian.UserId);
        if (user is not null)
        {
            user.IsActive = false;
            await userManager.UpdateAsync(user);
        }
    }

    private async Task<(bool Success, string? Error)> CreateGuardianUserAsync(
        GuardianProfile guardian,
        Guid schoolId,
        string password,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(guardian.Email))
            return (false, "Guardian email is required for portal access.");

        var email = guardian.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
            return (false, "An account with this guardian email already exists.");

        if (!string.IsNullOrWhiteSpace(guardian.LoginId))
        {
            var loginTaken = await userManager.Users.AnyAsync(
                u => u.LoginId != null && u.LoginId.ToLower() == guardian.LoginId.ToLower(), ct);
            if (loginTaken)
                return (false, "This login ID is already in use.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = guardian.FullName,
            LoginId = guardian.LoginId,
            PhoneNumber = guardian.Phone,
            SchoolId = schoolId,
            IsActive = true,
            MustChangePassword = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, AppRoleNames.Guardian);
        guardian.UserId = user.Id;
        guardian.PortalEnabled = true;

        await TrySendGuardianNewAccountEmailAsync(user, guardian, schoolId, password, ct);
        return (true, null);
    }

    private async Task TrySendGuardianNewAccountEmailAsync(
        ApplicationUser user,
        GuardianProfile guardian,
        Guid schoolId,
        string password,
        CancellationToken ct)
    {
        var recipient = AccountEmailNotificationService.ResolveRecipientEmail(guardian.Email)
                        ?? AccountEmailNotificationService.ResolveRecipientEmail(user.Email);
        if (recipient is null)
            return;

        await accountEmails.SendNewAccountEmailAsync(new AccountEmailRequest
        {
            SchoolId = schoolId,
            UserId = user.Id,
            RecipientEmail = recipient,
            UserName = user.FullName,
            LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
            TemporaryPassword = password,
            AccountType = PortalAccountType.Guardian
        }, ct);
    }

    private async Task TrySendGuardianPasswordResetEmailAsync(
        ApplicationUser user,
        GuardianProfile guardian,
        Guid schoolId,
        string? newPassword,
        CancellationToken ct)
    {
        var recipient = AccountEmailNotificationService.ResolveRecipientEmail(guardian.Email)
                        ?? AccountEmailNotificationService.ResolveRecipientEmail(user.Email);
        if (recipient is null || string.IsNullOrWhiteSpace(newPassword))
            return;

        await accountEmails.SendPasswordResetEmailAsync(new AccountEmailRequest
        {
            SchoolId = schoolId,
            UserId = user.Id,
            RecipientEmail = recipient,
            UserName = user.FullName,
            LoginId = user.LoginId ?? user.Email ?? user.UserName ?? user.Id,
            TemporaryPassword = newPassword,
            AccountType = PortalAccountType.Guardian
        }, ct);
    }

    private static void SyncStudentParentFields(StudentRecord student, GuardianProfile guardian, string relationship)
    {
        student.ParentName = string.IsNullOrWhiteSpace(relationship)
            ? guardian.FullName
            : $"{guardian.FullName} ({relationship.Trim()})";
        student.ParentEmail = guardian.Email;
        student.ParentPhone = guardian.Phone;
    }
}
