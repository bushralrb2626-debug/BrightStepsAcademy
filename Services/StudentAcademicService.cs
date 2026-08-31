using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IStudentAcademicService
{
    Task<StudentRecord?> GetStudentForUserAsync(string userId, CancellationToken ct = default);
}

public class StudentAcademicService(AppDbContext db) : IStudentAcademicService
{
    public Task<StudentRecord?> GetStudentForUserAsync(string userId, CancellationToken ct = default)
        => db.StudentRecords.AsNoTracking()
            .Include(s => s.SchoolClass)
            .Include(s => s.SchoolSection)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, ct);
}
