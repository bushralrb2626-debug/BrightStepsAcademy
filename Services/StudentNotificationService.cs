using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IStudentNotificationService
{
    Task NotifyClassSectionAsync(
        Guid schoolId,
        Guid classId,
        Guid sectionId,
        string title,
        string message,
        CancellationToken ct = default);
}

public class StudentNotificationService(AppDbContext db) : IStudentNotificationService
{
    public async Task NotifyClassSectionAsync(
        Guid schoolId,
        Guid classId,
        Guid sectionId,
        string title,
        string message,
        CancellationToken ct = default)
    {
        var userIds = await db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == schoolId
                        && s.SchoolClassId == classId
                        && s.SchoolSectionId == sectionId
                        && s.IsActive
                        && s.UserId != null)
            .Select(s => s.UserId!)
            .Distinct()
            .ToListAsync(ct);

        if (userIds.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        foreach (var userId in userIds)
        {
            db.AppNotifications.Add(new AppNotification
            {
                SchoolId = schoolId,
                UserId = userId,
                Title = title,
                Message = message,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
