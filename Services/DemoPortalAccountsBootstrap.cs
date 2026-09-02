using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

/// <summary>
/// Ensures one working student + parent demo login per school for testing.
/// </summary>
public static class DemoPortalAccountsBootstrap
{
    public const string DemoStudentCode = "DEMO001";
    public const string DemoStudentLoginId = "student_demo";
    public const string DemoParentLoginId = "parent_demo";
    public const string DemoPassword = "Demo@12345";

    public static async Task EnsureDemoAccountsAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<AppDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var studentAccounts = sp.GetRequiredService<IStudentAccountService>();
        var guardians = sp.GetRequiredService<IGuardianService>();

        var schoolId = await db.Schools.AsNoTracking()
            .Where(s => s.Status == SchoolStatus.Active)
            .OrderBy(s => s.SchoolCode == "BFA-001" ? 0 : 1)
            .ThenBy(s => s.Name)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);

        if (schoolId == Guid.Empty)
            return;

        await SchoolBootstrap.EnsureAcademicStructureAsync(db, schoolId, ct);

        var student = await EnsureDemoStudentAsync(db, schoolId, ct);
        await EnsureStudentLoginAsync(studentAccounts, userManager, student, ct);
        await EnsureDemoParentAsync(db, guardians, userManager, student, schoolId, ct);
    }

    private static async Task<StudentRecord> EnsureDemoStudentAsync(
        AppDbContext db, Guid schoolId, CancellationToken ct)
    {
        var student = await db.StudentRecords
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.StudentCode == DemoStudentCode, ct);

        var classId = await db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        var sectionId = classId == Guid.Empty
            ? Guid.Empty
            : await db.SchoolSections.AsNoTracking()
                .Where(s => s.SchoolId == schoolId && s.SchoolClassId == classId && s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

        var className = classId == Guid.Empty
            ? null
            : await db.SchoolClasses.AsNoTracking()
                .Where(c => c.Id == classId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(ct);

        var sectionName = sectionId == Guid.Empty
            ? null
            : await db.SchoolSections.AsNoTracking()
                .Where(s => s.Id == sectionId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(ct);

        if (student is null)
        {
            student = new StudentRecord
            {
                SchoolId = schoolId,
                StudentCode = DemoStudentCode,
                FullName = "Demo Student",
                Email = $"demo.student.{schoolId.ToString("N")[..8]}@school.local",
                Gender = "Male",
                AdmissionDate = DateOnly.FromDateTime(DateTime.Today),
                ClassName = className,
                Section = sectionName,
                SchoolClassId = classId == Guid.Empty ? null : classId,
                SchoolSectionId = sectionId == Guid.Empty ? null : sectionId,
                RollNumber = "1",
                IsActive = true
            };
            db.StudentRecords.Add(student);
            await db.SaveChangesAsync(ct);
            return student;
        }

        var changed = false;
        if (!student.IsActive) { student.IsActive = true; changed = true; }
        if (student.SchoolClassId is null && classId != Guid.Empty)
        {
            student.SchoolClassId = classId;
            student.ClassName = className;
            changed = true;
        }
        if (student.SchoolSectionId is null && sectionId != Guid.Empty)
        {
            student.SchoolSectionId = sectionId;
            student.Section = sectionName;
            changed = true;
        }

        if (changed)
        {
            student.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        return student;
    }

    private static async Task EnsureStudentLoginAsync(
        IStudentAccountService studentAccounts,
        UserManager<ApplicationUser> userManager,
        StudentRecord student,
        CancellationToken ct)
    {
        var result = await studentAccounts.ConfigureLoginAsync(new StudentAccountRequest
        {
            SchoolId = student.SchoolId,
            StudentId = student.Id,
            EnablePortal = true,
            LoginId = DemoStudentLoginId,
            Password = DemoPassword
        }, ct);

        if (!result.Success)
            return;

        await ResetDemoPasswordAsync(userManager, DemoStudentLoginId, DemoPassword, ct);

        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.LoginId != null && u.LoginId.ToLower() == DemoStudentLoginId.ToLower(), ct);
        if (user is not null)
        {
            user.MustChangePassword = false;
            await userManager.UpdateAsync(user);
        }
    }

    private static async Task EnsureDemoParentAsync(
        AppDbContext db,
        IGuardianService guardians,
        UserManager<ApplicationUser> userManager,
        StudentRecord student,
        Guid schoolId,
        CancellationToken ct)
    {
        var email = $"demo.parent.{schoolId.ToString("N")[..8]}@school.local";
        var link = await guardians.GetLinkForStudentAsync(student.Id, schoolId, ct);

        if (link?.Guardian is null)
        {
            var assign = await guardians.AssignGuardianAsync(new GuardianAssignmentRequest
            {
                SchoolId = schoolId,
                StudentId = student.Id,
                GuardianName = "Demo Parent",
                Relationship = "Father",
                GuardianEmail = email,
                GuardianPhone = "+92 300 0000000",
                EnablePortal = true,
                LoginId = DemoParentLoginId,
                Password = DemoPassword
            }, ct);

            if (!assign.Success)
                return;
        }
        else
        {
            var update = await guardians.UpdateGuardianAsync(new GuardianUpdateRequest
            {
                SchoolId = schoolId,
                StudentId = student.Id,
                GuardianName = link.Guardian.FullName,
                Relationship = link.Relationship,
                GuardianEmail = link.Guardian.Email ?? email,
                GuardianPhone = link.Guardian.Phone,
                EnablePortal = true,
                LoginId = DemoParentLoginId,
                ResetPassword = link.Guardian.UserId is not null,
                NewPassword = DemoPassword
            }, ct);

            if (!update.Success && link.Guardian.UserId is null)
            {
                await guardians.UpdateGuardianAsync(new GuardianUpdateRequest
                {
                    SchoolId = schoolId,
                    StudentId = student.Id,
                    GuardianName = "Demo Parent",
                    Relationship = link.Relationship,
                    GuardianEmail = email,
                    GuardianPhone = link.Guardian.Phone,
                    EnablePortal = true,
                    LoginId = DemoParentLoginId,
                    Password = DemoPassword
                }, ct);
            }
        }

        await ResetDemoPasswordAsync(userManager, DemoParentLoginId, DemoPassword, ct);

        var guardianUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.LoginId != null && u.LoginId.ToLower() == DemoParentLoginId.ToLower(), ct);
        if (guardianUser is not null && guardianUser.MustChangePassword)
        {
            guardianUser.MustChangePassword = false;
            await userManager.UpdateAsync(guardianUser);
        }
    }

    private static async Task ResetDemoPasswordAsync(
        UserManager<ApplicationUser> userManager,
        string loginId,
        string password,
        CancellationToken ct)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.LoginId != null && u.LoginId.ToLower() == loginId.ToLower(), ct);
        if (user is null)
            return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await userManager.ResetPasswordAsync(user, token, password);
        user.IsActive = true;
        user.MustChangePassword = false;
        await userManager.UpdateAsync(user);
    }
}
