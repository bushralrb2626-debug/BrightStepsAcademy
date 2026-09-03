using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

/// <summary>
/// Ensures working demo portal logins for every role shown on the public portal page.
/// </summary>
public static class DemoPortalAccountsBootstrap
{
    public const string DemoStudentCode = "DEMO001";
    public const string DemoStudentLoginId = "student_demo";
    public const string DemoParentLoginId = "parent_demo";
    public const string DemoTeacherLoginId = "teacher_demo";
    public const string DemoHeadmasterLoginId = "headmaster_demo";
    public const string DemoSchoolAdminEmail = "admin@gmail.com";
    public const string DemoSuperAdminEmail = "superadmin@gmail.com";
    public const string DemoPassword = "Demo@12345";
    public const string DemoSchoolAdminPassword = "123456";
    public const string DemoSuperAdminPassword = "12345";

    public const string DemoTeacherStaffCode = "DEMO-TCH";
    public const string DemoHeadmasterStaffCode = "DEMO-HM";

    public static readonly IReadOnlyList<(string Login, string Role)> DemoAccounts =
    [
        (DemoTeacherLoginId, "Teacher"),
        (DemoParentLoginId, "Parent"),
        (DemoStudentLoginId, "Student")
    ];

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
        await SchoolBootstrap.EnsureStaffCategoriesAsync(db, schoolId, ct);

        var student = await EnsureDemoStudentAsync(db, schoolId, ct);
        await EnsureStudentLoginAsync(studentAccounts, userManager, student, ct);
        await EnsureDemoParentAsync(db, guardians, userManager, student, schoolId, ct);
        await EnsureDemoTeacherAsync(db, userManager, schoolId, ct);
        await EnsureDemoHeadmasterAsync(db, userManager, schoolId, ct);
        await EnsurePlatformAdminsAsync(userManager, db, schoolId, ct);
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

        await FinalizeDemoUserAsync(userManager, DemoStudentLoginId, ct);
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

        await FinalizeDemoUserAsync(userManager, DemoParentLoginId, ct);
    }

    private static async Task EnsureDemoTeacherAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        Guid schoolId,
        CancellationToken ct)
    {
        var teachersCategoryId = await db.StaffCategories.AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.Name == "Teachers")
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);
        if (teachersCategoryId == Guid.Empty)
            return;

        var email = $"demo.teacher.{schoolId.ToString("N")[..8]}@school.local";
        var staff = await EnsureDemoStaffMemberAsync(
            db, userManager, schoolId, teachersCategoryId, DemoTeacherStaffCode,
            DemoTeacherLoginId, "Demo Teacher", email, designation: "Teacher",
            grantTeacherRole: true, ct);

        if (staff is null)
            return;

        await EnsureDemoTeacherAssignmentAsync(db, schoolId, staff.Id, ct);
    }

    private static async Task EnsureDemoHeadmasterAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        Guid schoolId,
        CancellationToken ct)
    {
        var categoryId = await db.StaffCategories.AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.Name == "Reception")
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);
        if (categoryId == Guid.Empty)
            return;

        var email = $"demo.headmaster.{schoolId.ToString("N")[..8]}@school.local";
        await EnsureDemoStaffMemberAsync(
            db, userManager, schoolId, categoryId, DemoHeadmasterStaffCode,
            DemoHeadmasterLoginId, "Demo Headmaster", email, designation: "Headmaster",
            grantTeacherRole: false, ct);
    }

    private static async Task<StaffMember?> EnsureDemoStaffMemberAsync(
        AppDbContext db,
        UserManager<ApplicationUser> userManager,
        Guid schoolId,
        Guid staffCategoryId,
        string staffCode,
        string loginId,
        string fullName,
        string email,
        string designation,
        bool grantTeacherRole,
        CancellationToken ct)
    {
        var staff = await db.StaffMembers
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.StaffCode == staffCode, ct);

        if (staff is null)
        {
            staff = new StaffMember
            {
                SchoolId = schoolId,
                StaffCategoryId = staffCategoryId,
                StaffCode = staffCode,
                FullName = fullName,
                Email = email,
                Designation = designation,
                HasLoginAccess = true,
                IsActive = true
            };
            db.StaffMembers.Add(staff);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            staff.StaffCategoryId = staffCategoryId;
            staff.FullName = fullName;
            staff.Email = email;
            staff.Designation = designation;
            staff.HasLoginAccess = true;
            staff.IsActive = true;
            staff.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        var user = staff.UserId is null
            ? null
            : await userManager.FindByIdAsync(staff.UserId);

        if (user is null)
        {
            user = await userManager.Users
                .FirstOrDefaultAsync(u => u.LoginId != null && u.LoginId.ToLower() == loginId.ToLower(), ct);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    LoginId = loginId,
                    SchoolId = schoolId,
                    IsActive = true,
                    MustChangePassword = false
                };
                var create = await userManager.CreateAsync(user, DemoPassword);
                if (!create.Succeeded)
                    return null;
            }

            staff.UserId = user.Id;
            staff.HasLoginAccess = true;
            await db.SaveChangesAsync(ct);
        }
        else
        {
            user.FullName = fullName;
            user.Email = email;
            user.UserName = email;
            user.LoginId = loginId;
            user.SchoolId = schoolId;
            user.IsActive = true;
            user.MustChangePassword = false;
            await userManager.UpdateAsync(user);
        }

        await EnsureStaffRolesAsync(userManager, user, grantTeacherRole, ct);
        await EnsureSchoolClaimAsync(userManager, user, schoolId, ct);
        await FinalizeDemoUserAsync(userManager, loginId, ct);

        return staff;
    }

    private static async Task EnsureDemoTeacherAssignmentAsync(
        AppDbContext db, Guid schoolId, Guid staffMemberId, CancellationToken ct)
    {
        if (await db.TeacherAssignments.AnyAsync(
                a => a.SchoolId == schoolId && a.StaffMemberId == staffMemberId && a.IsActive, ct))
            return;

        var classId = await db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);
        if (classId == Guid.Empty)
            return;

        var sectionId = await db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.SchoolClassId == classId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);
        if (sectionId == Guid.Empty)
            return;

        var subjectId = await db.Subjects.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);
        if (subjectId == Guid.Empty)
            return;

        db.TeacherAssignments.Add(new TeacherAssignment
        {
            SchoolId = schoolId,
            StaffMemberId = staffMemberId,
            SchoolClassId = classId,
            SchoolSectionId = sectionId,
            SubjectId = subjectId,
            IsActive = true
        });
        await db.SaveChangesAsync(ct);
    }

    public static async Task<ApplicationUser> EnsurePlatformAdminsAsync(
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        Guid schoolId,
        CancellationToken ct = default)
    {
        await UpsertAdminAsync(
            userManager,
            DemoSuperAdminEmail,
            ["superadmin@platform.com"],
            DemoSuperAdminPassword,
            AppRoleNames.SuperAdmin,
            "Platform Super Admin",
            "PLATFORM-SA",
            schoolId: null,
            ct);

        var schoolAdmin = await UpsertAdminAsync(
            userManager,
            DemoSchoolAdminEmail,
            ["admin@brightfuture.academy"],
            DemoSchoolAdminPassword,
            AppRoleNames.SchoolAdmin,
            "School Administrator",
            "BFA-ADMIN",
            schoolId,
            ct);

        var hasProfile = await db.SchoolAdminProfiles
            .AnyAsync(p => p.UserId == schoolAdmin.Id && p.SchoolId == schoolId, ct);
        if (!hasProfile)
        {
            db.SchoolAdminProfiles.Add(new SchoolAdminProfile
            {
                UserId = schoolAdmin.Id,
                SchoolId = schoolId,
                AdminType = "School Admin",
                IsPrimary = true,
                IsActive = true
            });
            await db.SaveChangesAsync(ct);
        }

        return schoolAdmin;
    }

    private static async Task<ApplicationUser> UpsertAdminAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        IEnumerable<string> previousEmails,
        string password,
        string role,
        string fullName,
        string loginId,
        Guid? schoolId,
        CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            foreach (var old in previousEmails)
            {
                user = await userManager.FindByEmailAsync(old);
                if (user is not null)
                    break;
            }
        }

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                SchoolId = schoolId,
                IsActive = true,
                LoginId = loginId,
                MustChangePassword = false
            };
            var created = await userManager.CreateAsync(user);
            if (!created.Succeeded)
                throw new InvalidOperationException(
                    "Failed to create " + role + ": " + string.Join("; ", created.Errors.Select(e => e.Description)));
        }
        else
        {
            user.FullName = fullName;
            user.LoginId = loginId;
            user.SchoolId = schoolId;
            user.IsActive = true;
            user.MustChangePassword = false;
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                await userManager.SetEmailAsync(user, email);
                await userManager.SetUserNameAsync(user, email);
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
        user.MustChangePassword = false;
        await userManager.UpdateAsync(user);
        return user;
    }

    private static async Task EnsureStaffRolesAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser account,
        bool grantTeacherRole,
        CancellationToken ct)
    {
        if (grantTeacherRole)
        {
            if (!await userManager.IsInRoleAsync(account, AppRoleNames.Teacher))
                await userManager.AddToRoleAsync(account, AppRoleNames.Teacher);
            if (await userManager.IsInRoleAsync(account, AppRoleNames.Staff))
                await userManager.RemoveFromRoleAsync(account, AppRoleNames.Staff);
        }
        else
        {
            if (!await userManager.IsInRoleAsync(account, AppRoleNames.Staff))
                await userManager.AddToRoleAsync(account, AppRoleNames.Staff);
            if (await userManager.IsInRoleAsync(account, AppRoleNames.Teacher))
                await userManager.RemoveFromRoleAsync(account, AppRoleNames.Teacher);
        }
    }

    private static async Task EnsureSchoolClaimAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        Guid schoolId,
        CancellationToken ct)
    {
        var claims = await userManager.GetClaimsAsync(user);
        if (claims.Any(c => c.Type == "school_id"))
            return;

        await userManager.AddClaimAsync(user, new Claim("school_id", schoolId.ToString()));
    }

    private static async Task FinalizeDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        string loginId,
        CancellationToken ct)
    {
        var user = await userManager.Users
            .FirstOrDefaultAsync(u => u.LoginId != null && u.LoginId.ToLower() == loginId.ToLower(), ct);
        if (user is null)
            return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await userManager.ResetPasswordAsync(user, token, DemoPassword);
        user.IsActive = true;
        user.MustChangePassword = false;
        await userManager.UpdateAsync(user);
    }
}
