using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public sealed class TeacherAssignmentVm
{
    public Guid Id { get; init; }
    public Guid StaffMemberId { get; init; }
    public Guid SchoolClassId { get; init; }
    public Guid SchoolSectionId { get; init; }
    public Guid SubjectId { get; init; }
    public string ClassName { get; init; } = "";
    public string SectionName { get; init; } = "";
    public string SubjectName { get; init; } = "";
    public string? ScheduleNotes { get; init; }
    public int StudentCount { get; init; }

    public string DisplayLabel => $"{ClassName} — {SectionName} · {SubjectName}";
}

public interface ITeacherAccessService
{
    Task<StaffMember?> GetStaffForUserAsync(string userId, Guid schoolId, CancellationToken ct = default);
    Task<IReadOnlyList<TeacherAssignmentVm>> GetAssignmentsAsync(string userId, Guid schoolId, CancellationToken ct = default);
    Task<TeacherAssignmentVm?> GetAssignmentAsync(string userId, Guid schoolId, Guid assignmentId, CancellationToken ct = default);
    Task<bool> HasAssignmentAsync(string userId, Guid schoolId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentRecord>> GetStudentsForAssignmentAsync(string userId, Guid schoolId, Guid assignmentId, CancellationToken ct = default);
    Task<StudentRecord?> GetStudentForAssignmentAsync(string userId, Guid schoolId, Guid assignmentId, Guid studentId, CancellationToken ct = default);
}

public class TeacherAccessService(AppDbContext db) : ITeacherAccessService
{
    public Task<StaffMember?> GetStaffForUserAsync(string userId, Guid schoolId, CancellationToken ct = default)
        => db.StaffMembers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SchoolId == schoolId && s.IsActive, ct);

    public async Task<IReadOnlyList<TeacherAssignmentVm>> GetAssignmentsAsync(string userId, Guid schoolId, CancellationToken ct = default)
    {
        var staff = await GetStaffForUserAsync(userId, schoolId, ct);
        if (staff is null) return Array.Empty<TeacherAssignmentVm>();

        var assignments = await db.TeacherAssignments.AsNoTracking()
            .Where(a => a.StaffMemberId == staff.Id && a.SchoolId == schoolId && a.IsActive)
            .Join(db.SchoolClasses.AsNoTracking(), a => a.SchoolClassId, c => c.Id, (a, c) => new { a, c })
            .Join(db.SchoolSections.AsNoTracking(), x => x.a.SchoolSectionId, s => s.Id, (x, s) => new { x.a, x.c, s })
            .Join(db.Subjects.AsNoTracking(), x => x.a.SubjectId, sub => sub.Id, (x, sub) => new { x.a, x.c, x.s, sub })
            .OrderBy(x => x.c.DisplayOrder).ThenBy(x => x.c.Name).ThenBy(x => x.s.Name).ThenBy(x => x.sub.Name)
            .ToListAsync(ct);

        var result = new List<TeacherAssignmentVm>();
        foreach (var row in assignments)
        {
            var count = await db.StudentRecords.CountAsync(
                st => st.SchoolId == schoolId && st.IsActive
                      && st.SchoolClassId == row.a.SchoolClassId
                      && st.SchoolSectionId == row.a.SchoolSectionId, ct);
            result.Add(new TeacherAssignmentVm
            {
                Id = row.a.Id,
                StaffMemberId = row.a.StaffMemberId,
                SchoolClassId = row.a.SchoolClassId,
                SchoolSectionId = row.a.SchoolSectionId,
                SubjectId = row.a.SubjectId,
                ClassName = row.c.Name,
                SectionName = row.s.Name,
                SubjectName = row.sub.Name,
                ScheduleNotes = row.a.ScheduleNotes,
                StudentCount = count
            });
        }
        return result;
    }

    public async Task<TeacherAssignmentVm?> GetAssignmentAsync(string userId, Guid schoolId, Guid assignmentId, CancellationToken ct = default)
    {
        var list = await GetAssignmentsAsync(userId, schoolId, ct);
        return list.FirstOrDefault(a => a.Id == assignmentId);
    }

    public async Task<bool> HasAssignmentAsync(string userId, Guid schoolId, Guid classId, Guid sectionId, Guid subjectId, CancellationToken ct = default)
    {
        var staff = await GetStaffForUserAsync(userId, schoolId, ct);
        if (staff is null) return false;
        return await db.TeacherAssignments.AnyAsync(
            a => a.StaffMemberId == staff.Id && a.SchoolId == schoolId && a.IsActive
                 && a.SchoolClassId == classId && a.SchoolSectionId == sectionId && a.SubjectId == subjectId, ct);
    }

    public async Task<IReadOnlyList<StudentRecord>> GetStudentsForAssignmentAsync(string userId, Guid schoolId, Guid assignmentId, CancellationToken ct = default)
    {
        var assignment = await GetAssignmentAsync(userId, schoolId, assignmentId, ct);
        if (assignment is null) return Array.Empty<StudentRecord>();

        return await db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.IsActive
                        && s.SchoolClassId == assignment.SchoolClassId
                        && s.SchoolSectionId == assignment.SchoolSectionId)
            .OrderBy(s => s.RollNumber).ThenBy(s => s.FullName)
            .ToListAsync(ct);
    }

    public async Task<StudentRecord?> GetStudentForAssignmentAsync(string userId, Guid schoolId, Guid assignmentId, Guid studentId, CancellationToken ct = default)
    {
        var students = await GetStudentsForAssignmentAsync(userId, schoolId, assignmentId, ct);
        return students.FirstOrDefault(s => s.Id == studentId);
    }
}
