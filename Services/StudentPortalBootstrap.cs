using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public static class StudentPortalBootstrap
{
    private const string DefaultPassword = "Demo@12345";

    /// <summary>
    /// Creates portal logins for active students that do not have one yet.
    /// Login ID format: STU_{StudentCode} (sanitized).
    /// </summary>
    public static async Task EnsurePortalLoginsAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var studentAccounts = scope.ServiceProvider.GetRequiredService<IStudentAccountService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var students = await db.StudentRecords
            .Where(s => s.IsActive && s.UserId == null)
            .ToListAsync(ct);

        foreach (var student in students)
        {
            if (string.Equals(student.StudentCode, DemoPortalAccountsBootstrap.DemoStudentCode, StringComparison.OrdinalIgnoreCase))
                continue;

            var code = SanitizeCode(student.StudentCode);
            if (string.IsNullOrWhiteSpace(code))
                code = student.Id.ToString("N")[..8];

            var loginId = $"STU_{code}";
            var result = await studentAccounts.ConfigureLoginAsync(new StudentAccountRequest
            {
                SchoolId = student.SchoolId,
                StudentId = student.Id,
                EnablePortal = true,
                LoginId = loginId,
                Password = DefaultPassword
            }, ct);

            if (result.Success)
            {
                var user = await userManager.Users
                    .FirstOrDefaultAsync(u => u.LoginId != null && u.LoginId.ToLower() == loginId.ToLower(), ct);
                if (user is not null)
                {
                    user.MustChangePassword = false;
                    await userManager.UpdateAsync(user);
                }
            }
        }
    }

    private static string SanitizeCode(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? ""
            : new string(code.Where(char.IsLetterOrDigit).ToArray());
}
