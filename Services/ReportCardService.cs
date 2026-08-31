using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IReportCardService
{
    Task<ReportCardVm?> BuildAsync(Guid studentId, CancellationToken ct = default);
}

public class ReportCardService(AppDbContext db, IGradingService grading) : IReportCardService
{
    public async Task<ReportCardVm?> BuildAsync(Guid studentId, CancellationToken ct = default)
    {
        var student = await db.StudentRecords.AsNoTracking()
            .Include(s => s.School)
            .Include(s => s.SchoolClass)
            .Include(s => s.SchoolSection)
            .FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive, ct);

        if (student is null) return null;

        var markRows = await db.AssessmentMarks.AsNoTracking()
            .Where(m => m.StudentId == studentId)
            .Join(
                db.Assessments.AsNoTracking().Where(a => a.Status == PublishStatus.Published),
                m => m.AssessmentId,
                a => a.Id,
                (m, a) => new { m, a })
            .Join(
                db.Subjects.AsNoTracking(),
                x => x.a.SubjectId,
                sub => sub.Id,
                (x, sub) => new MarkRow(
                    sub.Id,
                    sub.Name,
                    x.a.AssessmentType,
                    x.a.AssessmentDate,
                    x.m.ObtainedMarks,
                    x.a.TotalMarks,
                    x.m.GradeLabel,
                    x.m.Percentage))
            .ToListAsync(ct);

        var subjectRows = markRows
            .GroupBy(r => new { r.SubjectId, r.SubjectName })
            .OrderBy(g => g.Key.SubjectName)
            .Select(g => BuildSubjectRow(g.Key.SubjectName, g.ToList(), student.SchoolId, ct))
            .ToList();

        var resolvedSubjects = await Task.WhenAll(subjectRows);
        var subjects = resolvedSubjects.ToList();

        var attendanceCounts = await db.AttendanceRecords.AsNoTracking()
            .Where(r => r.StudentId == studentId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var present = attendanceCounts.FirstOrDefault(x => x.Status == AttendanceStatus.Present)?.Count ?? 0;
        var absent = attendanceCounts.FirstOrDefault(x => x.Status == AttendanceStatus.Absent)?.Count ?? 0;
        var late = attendanceCounts.FirstOrDefault(x => x.Status == AttendanceStatus.Late)?.Count ?? 0;
        var excused = attendanceCounts.FirstOrDefault(x => x.Status == AttendanceStatus.Excused)?.Count ?? 0;
        var attendanceTotal = present + absent + late + excused;

        var overallObtained = subjects.Sum(s => s.ObtainedMarks);
        var overallTotal = subjects.Sum(s => s.TotalMarks);
        var overallPct = overallTotal > 0
            ? Math.Round(overallObtained / overallTotal * 100m, 1)
            : 0m;
        var overallGrade = overallTotal > 0
            ? await grading.CalculateGradeAsync(student.SchoolId, overallPct, ct)
            : null;

        var className = student.SchoolClass?.Name ?? student.ClassName ?? "—";
        var section = student.SchoolSection?.Name ?? student.Section ?? "—";

        return new ReportCardVm
        {
            StudentId = student.Id,
            StudentName = student.FullName,
            StudentCode = student.StudentCode,
            RollNumber = student.RollNumber,
            ClassDisplay = $"{className} · Section {section}",
            SchoolName = student.School?.Name ?? "School",
            SessionLabel = BuildSessionLabel(),
            GeneratedDate = DateOnly.FromDateTime(DateTime.Today),
            AttendancePresent = present,
            AttendanceAbsent = absent,
            AttendanceLate = late,
            AttendanceExcused = excused,
            AttendanceTotal = attendanceTotal,
            AttendancePercentage = attendanceTotal > 0
                ? Math.Round((decimal)present / attendanceTotal * 100m, 1)
                : 0m,
            AssessmentTypes = AssessmentTypeCatalog.GradeBookOptions
                .Select(t => new ReportCardTypeColumnVm
                {
                    AssessmentType = t,
                    Label = AssessmentTypeCatalog.Label(t)
                })
                .ToList(),
            Subjects = subjects,
            OverallObtained = overallObtained,
            OverallTotal = overallTotal,
            OverallPercentage = overallPct,
            OverallGrade = overallGrade
        };
    }

    private async Task<ReportCardSubjectRowVm> BuildSubjectRow(
        string subjectName,
        IReadOnlyList<MarkRow> rows,
        Guid schoolId,
        CancellationToken ct)
    {
        var cells = AssessmentTypeCatalog.GradeBookOptions
            .Select(type =>
            {
                var latest = rows
                    .Where(r => r.AssessmentType == type)
                    .OrderByDescending(r => r.AssessmentDate)
                    .FirstOrDefault();

                if (latest is null) return null;

                return new ReportCardMarkCellVm
                {
                    ObtainedMarks = latest.ObtainedMarks,
                    TotalMarks = latest.TotalMarks,
                    GradeLabel = latest.GradeLabel,
                    Percentage = latest.Percentage
                };
            })
            .ToList();

        var obtained = cells.Where(c => c is not null).Sum(c => c!.ObtainedMarks);
        var total = cells.Where(c => c is not null).Sum(c => c!.TotalMarks);
        var pct = total > 0 ? Math.Round(obtained / total * 100m, 1) : (decimal?)null;
        var grade = pct.HasValue
            ? await grading.CalculateGradeAsync(schoolId, pct.Value, ct)
            : null;

        return new ReportCardSubjectRowVm
        {
            SubjectName = subjectName,
            Cells = cells,
            ObtainedMarks = obtained,
            TotalMarks = total,
            Percentage = pct,
            GradeLabel = grade
        };
    }

    private static string BuildSessionLabel()
    {
        var today = DateTime.Today;
        var startYear = today.Month >= 4 ? today.Year : today.Year - 1;
        return $"{startYear}–{startYear + 1}";
    }

    private sealed record MarkRow(
        Guid SubjectId,
        string SubjectName,
        AssessmentType AssessmentType,
        DateOnly AssessmentDate,
        decimal ObtainedMarks,
        decimal TotalMarks,
        string? GradeLabel,
        decimal? Percentage);
}
