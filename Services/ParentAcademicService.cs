using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IParentAcademicService
{
    Task<IReadOnlyList<StudentRecord>> GetLinkedStudentsAsync(string userId, CancellationToken ct = default);
    Task<StudentRecord?> GetLinkedStudentAsync(string userId, Guid studentId, CancellationToken ct = default);
    Task<bool> CanAccessStudentAsync(string userId, Guid studentId, CancellationToken ct = default);
}

public class ParentAcademicService(AppDbContext db, IGuardianService guardians) : IParentAcademicService
{
    public async Task<IReadOnlyList<StudentRecord>> GetLinkedStudentsAsync(string userId, CancellationToken ct = default)
    {
        var profile = await guardians.GetProfileForUserAsync(userId, ct);
        if (profile is null) return Array.Empty<StudentRecord>();

        return await db.StudentGuardianLinks.AsNoTracking()
            .Where(l => l.GuardianProfileId == profile.Id && l.IsActive)
            .Join(db.StudentRecords.AsNoTracking().Where(s => s.IsActive),
                l => l.StudentId, s => s.Id, (_, s) => s)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);
    }

    public async Task<StudentRecord?> GetLinkedStudentAsync(string userId, Guid studentId, CancellationToken ct = default)
    {
        if (!await CanAccessStudentAsync(userId, studentId, ct))
            return null;
        return await db.StudentRecords.AsNoTracking().FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive, ct);
    }

    public async Task<bool> CanAccessStudentAsync(string userId, Guid studentId, CancellationToken ct = default)
    {
        var profile = await guardians.GetProfileForUserAsync(userId, ct);
        if (profile is null) return false;
        return await db.StudentGuardianLinks.AnyAsync(
            l => l.GuardianProfileId == profile.Id && l.StudentId == studentId && l.IsActive, ct);
    }
}
