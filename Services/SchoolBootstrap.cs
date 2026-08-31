using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public static class SchoolBootstrap
{
    public static readonly string[] DefaultStaffCategoryNames =
    [
        "Teachers",
        "Helpers",
        "Accountants",
        "Security",
        "Reception"
    ];

    public static async Task EnsureStaffCategoriesAsync(AppDbContext db, Guid schoolId, CancellationToken ct = default)
    {
        if (await db.StaffCategories.AnyAsync(c => c.SchoolId == schoolId, ct))
            return;

        foreach (var name in DefaultStaffCategoryNames)
        {
            db.StaffCategories.Add(new StaffCategory
            {
                SchoolId = schoolId,
                Name = name,
                Description = $"{name} staff category",
                IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureAcademicStructureAsync(AppDbContext db, Guid schoolId, CancellationToken ct = default)
    {
        if (!await db.SchoolClasses.AnyAsync(c => c.SchoolId == schoolId, ct))
        {
            var grade1 = new SchoolClass { SchoolId = schoolId, Name = "Grade 1", GradeLevel = "1", DisplayOrder = 1, IsActive = true };
            var grade2 = new SchoolClass { SchoolId = schoolId, Name = "Grade 2", GradeLevel = "2", DisplayOrder = 2, IsActive = true };
            db.SchoolClasses.AddRange(grade1, grade2);
            await db.SaveChangesAsync(ct);

            db.SchoolSections.AddRange(
                new SchoolSection { SchoolId = schoolId, SchoolClassId = grade1.Id, Name = "A", IsActive = true },
                new SchoolSection { SchoolId = schoolId, SchoolClassId = grade1.Id, Name = "B", IsActive = true },
                new SchoolSection { SchoolId = schoolId, SchoolClassId = grade2.Id, Name = "A", IsActive = true });
            await db.SaveChangesAsync(ct);

            db.Subjects.AddRange(
                new Subject { SchoolId = schoolId, Name = "English", Code = "ENG", IsActive = true },
                new Subject { SchoolId = schoolId, Name = "Mathematics", Code = "MATH", IsActive = true },
                new Subject { SchoolId = schoolId, Name = "Science", Code = "SCI", IsActive = true });
            await db.SaveChangesAsync(ct);
        }

        await EnsureGradingRulesAsync(db, schoolId, ct);
    }

    public static async Task EnsureGradingRulesAsync(AppDbContext db, Guid schoolId, CancellationToken ct = default)
    {
        if (await db.GradingRules.AnyAsync(r => r.SchoolId == schoolId, ct))
            return;

        var defaults = new (string Grade, decimal Min, decimal Max, decimal Point)[]
        {
            ("A+", 90, 100, 4.0m),
            ("A", 80, 89.99m, 3.7m),
            ("B+", 70, 79.99m, 3.3m),
            ("B", 60, 69.99m, 3.0m),
            ("C", 50, 59.99m, 2.0m),
            ("D", 40, 49.99m, 1.0m),
            ("F", 0, 39.99m, 0m)
        };
        for (var i = 0; i < defaults.Length; i++)
        {
            var d = defaults[i];
            db.GradingRules.Add(new GradingRule
            {
                SchoolId = schoolId,
                GradeLabel = d.Grade,
                MinPercentage = d.Min,
                MaxPercentage = d.Max,
                GradePoint = d.Point,
                DisplayOrder = i,
                IsActive = true
            });
        }
        await db.SaveChangesAsync(ct);
    }

    public static async Task LinkStudentsToAcademicStructureAsync(AppDbContext db, Guid schoolId, CancellationToken ct = default)
    {
        var students = await db.StudentRecords
            .Where(s => s.SchoolId == schoolId && s.IsActive && (s.SchoolClassId == null || s.SchoolSectionId == null))
            .ToListAsync(ct);
        if (students.Count == 0)
            return;

        var classes = await db.SchoolClasses.Where(c => c.SchoolId == schoolId && c.IsActive).ToListAsync(ct);
        var sections = await db.SchoolSections.Where(s => s.SchoolId == schoolId && s.IsActive).ToListAsync(ct);
        if (classes.Count == 0 || sections.Count == 0)
            return;

        foreach (var student in students)
        {
            var matchedClass = MatchClass(classes, student.ClassName);
            if (matchedClass is null)
                continue;

            var matchedSection = sections
                .Where(s => s.SchoolClassId == matchedClass.Id)
                .FirstOrDefault(s => string.Equals(s.Name, student.Section, StringComparison.OrdinalIgnoreCase))
                ?? sections.Where(s => s.SchoolClassId == matchedClass.Id).OrderBy(s => s.Name).FirstOrDefault();
            if (matchedSection is null)
                continue;

            student.SchoolClassId = matchedClass.Id;
            student.SchoolSectionId = matchedSection.Id;
        }

        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureDefaultTeacherAssignmentsAsync(AppDbContext db, Guid schoolId, CancellationToken ct = default)
    {
        var classSection = await ResolveDefaultClassSectionAsync(db, schoolId, ct);
        if (classSection is null)
            return;

        var subject = await db.Subjects.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.IsActive)
            .OrderBy(s => s.Name)
            .FirstOrDefaultAsync(ct);
        if (subject is null)
            return;

        var teachersCategory = await db.StaffCategories.AsNoTracking()
            .FirstOrDefaultAsync(c => c.SchoolId == schoolId && c.Name == "Teachers" && c.IsActive, ct);
        if (teachersCategory is null)
            return;

        var teachers = await db.StaffMembers
            .Where(s => s.SchoolId == schoolId && s.IsActive && s.HasLoginAccess && s.UserId != null
                        && s.StaffCategoryId == teachersCategory.Id)
            .ToListAsync(ct);

        foreach (var teacher in teachers)
        {
            var hasAssignment = await db.TeacherAssignments.AnyAsync(
                a => a.SchoolId == schoolId && a.IsActive && a.StaffMemberId == teacher.Id, ct);
            if (hasAssignment)
                continue;

            db.TeacherAssignments.Add(new TeacherAssignment
            {
                SchoolId = schoolId,
                StaffMemberId = teacher.Id,
                SchoolClassId = classSection.Value.ClassId,
                SchoolSectionId = classSection.Value.SectionId,
                SubjectId = subject.Id,
                ScheduleNotes = "Auto-assigned — update in Academic setup",
                IsActive = true
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public static async Task EnsureSchoolBootstrappedAsync(AppDbContext db, Guid schoolId, CancellationToken ct = default)
    {
        await EnsureStaffCategoriesAsync(db, schoolId, ct);
        await EnsureAcademicStructureAsync(db, schoolId, ct);
        await LinkStudentsToAcademicStructureAsync(db, schoolId, ct);
        await EnsureDefaultTeacherAssignmentsAsync(db, schoolId, ct);
    }

    public static async Task EnsureAllSchoolsBootstrappedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var schoolIds = await db.Schools.AsNoTracking().Select(s => s.Id).ToListAsync(ct);
        foreach (var schoolId in schoolIds)
            await EnsureSchoolBootstrappedAsync(db, schoolId, ct);
    }

    private static SchoolClass? MatchClass(IReadOnlyList<SchoolClass> classes, string? className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return classes.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).FirstOrDefault();

        var trimmed = className.Trim();
        return classes.FirstOrDefault(c =>
                   string.Equals(c.Name, trimmed, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(c.GradeLevel, trimmed, StringComparison.OrdinalIgnoreCase)
                   || c.Name.Contains(trimmed, StringComparison.OrdinalIgnoreCase)
                   || trimmed.Contains(c.GradeLevel ?? "", StringComparison.OrdinalIgnoreCase))
               ?? classes.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).FirstOrDefault();
    }

    private static async Task<(Guid ClassId, Guid SectionId)?> ResolveDefaultClassSectionAsync(
        AppDbContext db, Guid schoolId, CancellationToken ct)
    {
        var linkedGroup = await db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.IsActive && s.SchoolClassId != null && s.SchoolSectionId != null)
            .GroupBy(s => new { ClassId = s.SchoolClassId!.Value, SectionId = s.SchoolSectionId!.Value })
            .Select(g => new { g.Key.ClassId, g.Key.SectionId, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .FirstOrDefaultAsync(ct);
        if (linkedGroup is not null)
            return (linkedGroup.ClassId, linkedGroup.SectionId);

        var unlinked = await db.StudentRecords.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.IsActive && s.SchoolClassId == null, ct);
        if (unlinked is not null)
        {
            var classes = await db.SchoolClasses.Where(c => c.SchoolId == schoolId && c.IsActive).ToListAsync(ct);
            var sections = await db.SchoolSections.Where(s => s.SchoolId == schoolId && s.IsActive).ToListAsync(ct);
            var matchedClass = MatchClass(classes, unlinked.ClassName);
            if (matchedClass is not null)
            {
                var section = sections
                    .Where(s => s.SchoolClassId == matchedClass.Id)
                    .FirstOrDefault(s => string.Equals(s.Name, unlinked.Section, StringComparison.OrdinalIgnoreCase))
                    ?? sections.Where(s => s.SchoolClassId == matchedClass.Id).OrderBy(s => s.Name).FirstOrDefault();
                if (section is not null)
                    return (matchedClass.Id, section.Id);
            }
        }

        var firstClass = await db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == schoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .FirstOrDefaultAsync(ct);
        if (firstClass is null)
            return null;

        var firstSection = await db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == schoolId && s.SchoolClassId == firstClass.Id && s.IsActive)
            .OrderBy(s => s.Name)
            .FirstOrDefaultAsync(ct);
        return firstSection is null ? null : (firstClass.Id, firstSection.Id);
    }
}
