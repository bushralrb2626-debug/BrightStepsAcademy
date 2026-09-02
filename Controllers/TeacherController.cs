using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;
using BrightStepsAcademy.Services;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers;

public class TeacherController : TeacherPortalControllerBase
{
    private readonly IAcademicContentService _content;
    private readonly IGradingService _grading;
    private readonly IFileStorageService _files;
    private readonly IAccountEmailNotificationService _accountEmails;

    public TeacherController(
        ISchoolData store,
        ITenantContext tenant,
        ITeacherAccessService teacherAccess,
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IAcademicContentService content,
        IGradingService grading,
        IFileStorageService files,
        IAccountEmailNotificationService accountEmails)
        : base(store, tenant, teacherAccess, userManager, db)
    {
        _content = content;
        _grading = grading;
        _files = files;
        _accountEmails = accountEmails;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct);
        var assignments = (IReadOnlyList<TeacherAssignmentOptionVm>)ViewBag.TeacherAssignments;
        var vm = new TeacherDashboardVm
        {
            AssignmentCount = assignments.Count,
            StudentCount = assignments.Sum(a => a.StudentCount),
            Assignments = assignments
        };
        if (assignments.Count > 0)
        {
            var classIds = assignments.Select(a => a.SchoolClassId).Distinct().ToList();
            var sectionIds = assignments.Select(a => a.SchoolSectionId).Distinct().ToList();
            vm.DraftDiaryCount = await Db.DailyDiaryEntries.CountAsync(
                d => d.SchoolId == schoolId && d.Status == PublishStatus.Draft
                     && classIds.Contains(d.SchoolClassId) && sectionIds.Contains(d.SchoolSectionId), ct);
            vm.PublishedDiaryCount = await Db.DailyDiaryEntries.CountAsync(
                d => d.SchoolId == schoolId && d.Status == PublishStatus.Published
                     && classIds.Contains(d.SchoolClassId) && sectionIds.Contains(d.SchoolSectionId), ct);
        }
        ViewData["Title"] = "Teacher Dashboard";
        return View(vm);
    }

    public async Task<IActionResult> Classes(CancellationToken ct)
    {
        if (RequireSchool(out _) is { } deny) return deny;
        await HydrateAsync(ct);
        ViewData["Title"] = "My Classes";
        return View(ViewBag.TeacherAssignments);
    }

    public async Task<IActionResult> Students(Guid? assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(Array.Empty<StudentRecord>());
        var students = await TeacherAccess.GetStudentsForAssignmentAsync(CurrentUserId, schoolId, selected.Value, ct);
        ViewData["Title"] = "Students";
        return View(students);
    }

    public async Task<IActionResult> Diary(Guid? assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(Array.Empty<DiaryEntryListItemVm>());

        var assignment = await GetOwnedAssignmentAsync(schoolId, selected.Value, ct);
        if (assignment is null) return Forbid();

        var items = await Db.DailyDiaryEntries.AsNoTracking()
            .Where(d => d.SchoolId == schoolId
                        && d.SchoolClassId == assignment.SchoolClassId
                        && d.SchoolSectionId == assignment.SchoolSectionId
                        && d.SubjectId == assignment.SubjectId)
            .OrderByDescending(d => d.ContentDate)
            .Select(d => new DiaryEntryListItemVm
            {
                Id = d.Id,
                Title = d.Title,
                ContentDate = d.ContentDate,
                Status = d.Status,
                Topic = d.Topic,
                AssignmentLabel = assignment.DisplayLabel
            })
            .ToListAsync(ct);
        ViewData["Title"] = "Daily Diary";
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDiary(Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await HydrateAsync(ct, assignmentId);
        ViewData["Title"] = "New diary entry";
        return View(new DiaryEntryFormVm { AssignmentId = assignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDiary(DiaryEntryFormVm model, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var (staff, staffErr) = await RequireStaffAsync(schoolId, ct);
        if (staffErr is not null) return staffErr;
        var assignment = await GetOwnedAssignmentAsync(schoolId, model.AssignmentId, ct);
        if (assignment is null || staff is null) return Forbid();

        if (!ModelState.IsValid)
        {
            await HydrateAsync(ct, model.AssignmentId);
            ViewData["Title"] = "New diary entry";
            return View(model);
        }

        var entry = new DailyDiaryEntry
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Topic = model.Topic?.Trim(),
            Homework = model.Homework?.Trim(),
            Instructions = model.Instructions?.Trim(),
            ContentDate = model.ContentDate,
            DueDate = model.DueDate,
            Status = model.Status,
            PublishedAt = model.Status == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        ApplyAssignmentScope(entry, assignment, schoolId, staff.Id);
        Db.DailyDiaryEntries.Add(entry);
        await Db.SaveChangesAsync(ct);

        if (model.Attachments?.Count > 0)
            await _content.SaveAttachmentsAsync(schoolId, staff.Id, AcademicAttachmentOwnerType.DailyDiary, entry.Id, model.Attachments, ct);

        TempData["Flash"] = model.Status == PublishStatus.Published ? "Diary entry published." : "Diary entry saved as draft.";
        return RedirectToAction(nameof(Diary), new { assignmentId = model.AssignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishDiary(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await _content.PublishAsync(PublishStatus.Published, schoolId, id, AcademicContentKind.DailyDiary, ct);
        TempData["Flash"] = "Diary entry published.";
        return RedirectToAction(nameof(Diary), new { assignmentId });
    }

    public async Task<IActionResult> Attendance(Guid? assignmentId, DateOnly? date, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(new AttendanceSessionFormVm());

        var assignment = await GetOwnedAssignmentAsync(schoolId, selected.Value, ct);
        if (assignment is null) return Forbid();

        var sessionDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        var students = await TeacherAccess.GetStudentsForAssignmentAsync(CurrentUserId, schoolId, selected.Value, ct);
        var session = await Db.AttendanceSessions
            .Include(s => s.Records)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId
                                      && s.SchoolClassId == assignment.SchoolClassId
                                      && s.SchoolSectionId == assignment.SchoolSectionId
                                      && s.SubjectId == assignment.SubjectId
                                      && s.SessionDate == sessionDate, ct);

        var vm = new AttendanceSessionFormVm
        {
            AssignmentId = selected.Value,
            SessionDate = sessionDate,
            PeriodLabel = session?.PeriodLabel,
            Notes = session?.Notes,
            Students = students.Select(s =>
            {
                var record = session?.Records.FirstOrDefault(r => r.StudentId == s.Id);
                return new AttendanceStudentRowVm
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    RollNumber = s.RollNumber,
                    Status = record?.Status ?? AttendanceStatus.Present
                };
            }).ToList()
        };
        ViewData["Title"] = "Attendance";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(AttendanceSessionFormVm model, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var (staff, staffErr) = await RequireStaffAsync(schoolId, ct);
        if (staffErr is not null) return staffErr;
        var assignment = await GetOwnedAssignmentAsync(schoolId, model.AssignmentId, ct);
        if (assignment is null || staff is null) return Forbid();

        var session = await Db.AttendanceSessions
            .Include(s => s.Records)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId
                                      && s.SchoolClassId == assignment.SchoolClassId
                                      && s.SchoolSectionId == assignment.SchoolSectionId
                                      && s.SubjectId == assignment.SubjectId
                                      && s.SessionDate == model.SessionDate, ct);

        if (session is null)
        {
            session = new AttendanceSession
            {
                SchoolId = schoolId,
                StaffMemberId = staff.Id,
                SchoolClassId = assignment.SchoolClassId,
                SchoolSectionId = assignment.SchoolSectionId,
                SubjectId = assignment.SubjectId,
                TeacherAssignmentId = assignment.Id,
                SessionDate = model.SessionDate,
                PeriodLabel = model.PeriodLabel?.Trim(),
                Notes = model.Notes?.Trim(),
                CreatedByUserId = CurrentUserId,
                IsActive = true
            };
            Db.AttendanceSessions.Add(session);
            await Db.SaveChangesAsync(ct);
        }
        else
        {
            session.PeriodLabel = model.PeriodLabel?.Trim();
            session.Notes = model.Notes?.Trim();
            session.UpdatedAt = DateTimeOffset.UtcNow;
        }

        foreach (var row in model.Students)
        {
            var existing = session.Records.FirstOrDefault(r => r.StudentId == row.StudentId);
            if (existing is null)
            {
                Db.AttendanceRecords.Add(new AttendanceRecord
                {
                    SchoolId = schoolId,
                    AttendanceSessionId = session.Id,
                    StudentId = row.StudentId,
                    Status = row.Status,
                    CreatedByUserId = CurrentUserId,
                    IsActive = true
                });
            }
            else
            {
                existing.Status = row.Status;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await Db.SaveChangesAsync(ct);
        TempData["Flash"] = "Attendance saved.";
        return RedirectToAction(nameof(Attendance), new { assignmentId = model.AssignmentId, date = model.SessionDate.ToString("yyyy-MM-dd") });
    }

    public async Task<IActionResult> GradeBook(Guid? assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(Array.Empty<AssessmentListItemVm>());

        var assignment = await GetOwnedAssignmentAsync(schoolId, selected.Value, ct);
        if (assignment is null) return Forbid();

        var rows = await Db.Assessments.AsNoTracking()
            .Where(a => a.SchoolId == schoolId
                        && a.SchoolClassId == assignment.SchoolClassId
                        && a.SchoolSectionId == assignment.SchoolSectionId
                        && a.SubjectId == assignment.SubjectId)
            .OrderByDescending(a => a.AssessmentDate)
            .ToListAsync(ct);
        var items = rows.Select(a => new AssessmentListItemVm
        {
            Id = a.Id,
            Name = a.Name,
            AssessmentType = a.AssessmentType,
            AssessmentTypeLabel = AssessmentTypeCatalog.Label(a.AssessmentType),
            AssessmentDate = a.AssessmentDate,
            TotalMarks = a.TotalMarks,
            Status = a.Status,
            AssignmentLabel = assignment.DisplayLabel
        }).ToList();
        ViewData["Title"] = "Grade Book";
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateAssessment(Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await HydrateAsync(ct, assignmentId);

        var students = await TeacherAccess.GetStudentsForAssignmentAsync(CurrentUserId, schoolId, assignmentId, ct);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var columns = AssessmentScoreColumns.Default();
        var vm = new AssessmentFormVm
        {
            AssignmentId = assignmentId,
            AssessmentType = AssessmentType.Test,
            AssessmentDate = today,
            Name = AssessmentTypeCatalog.DefaultName(AssessmentType.Test, today),
            TotalMarks = AssessmentScoreColumns.TotalMax(columns),
            Columns = columns,
            Marks = students.Select(s => new AssessmentMarkRowVm
            {
                StudentId = s.Id,
                FullName = s.FullName,
                RollNumber = s.RollNumber,
                ColumnScores = columns.Select(_ => 0m).ToList()
            }).ToList()
        };

        ViewData["Title"] = "New assessment";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssessment(AssessmentFormVm model, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var (staff, staffErr) = await RequireStaffAsync(schoolId, ct);
        if (staffErr is not null) return staffErr;
        var assignment = await GetOwnedAssignmentAsync(schoolId, model.AssignmentId, ct);
        if (assignment is null || staff is null) return Forbid();

        AssessmentScoreColumns.ApplyAssessmentDefaults(model);
        ClearAutoFieldValidation();
        PrepareAssessmentMarks(model.Columns, model.Marks, model.TotalMarks);
        ValidateAssessmentMarks(model.Columns, model.Marks, ModelState);

        if (!ModelState.IsValid)
        {
            await HydrateAsync(ct, model.AssignmentId);
            ViewData["Title"] = "New assessment";
            return View(model);
        }

        var assessment = new Assessment
        {
            Name = (model.Name ?? AssessmentScoreColumns.TitleFromColumns(model.Columns, model.AssessmentDate)).Trim(),
            AssessmentType = model.AssessmentType,
            AssessmentDate = model.AssessmentDate,
            TotalMarks = AssessmentScoreColumns.TotalMax(model.Columns),
            PassingMarks = model.PassingMarks,
            Description = model.ImportantInfo?.Trim(),
            Status = model.Status,
            PublishedAt = model.Status == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            ScoreColumnsJson = AssessmentScoreColumns.Serialize(model.Columns),
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        ApplyAssignmentScope(assessment, assignment, schoolId, staff.Id);
        Db.Assessments.Add(assessment);
        await Db.SaveChangesAsync(ct);

        await SaveAssessmentMarksAsync(schoolId, assessment, model.Columns, model.Marks, ct);

        TempData["Flash"] = model.Marks.Count > 0
            ? "Assessment created and marks saved for the whole class."
            : "Assessment created.";
        return RedirectToAction(nameof(GradeBook), new { assignmentId = model.AssignmentId });
    }

    [HttpGet]
    public async Task<IActionResult> Marks(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var assignment = await GetOwnedAssignmentAsync(schoolId, assignmentId, ct);
        if (assignment is null) return Forbid();
        var assessment = await Db.Assessments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.SchoolId == schoolId, ct);
        if (assessment is null) return NotFound();

        await HydrateAsync(ct, assignmentId);
        var students = await TeacherAccess.GetStudentsForAssignmentAsync(CurrentUserId, schoolId, assignmentId, ct);
        var marks = await Db.AssessmentMarks.AsNoTracking()
            .Where(m => m.AssessmentId == id)
            .ToDictionaryAsync(m => m.StudentId, ct);

        var columns = AssessmentScoreColumns.Deserialize(assessment.ScoreColumnsJson);
        var vm = new AssessmentMarksFormVm
        {
            AssessmentId = id,
            Name = assessment.Name,
            AssessmentType = assessment.AssessmentType,
            AssessmentDate = assessment.AssessmentDate,
            TotalMarks = assessment.TotalMarks,
            PassingMarks = assessment.PassingMarks,
            ImportantInfo = assessment.Description,
            Status = assessment.Status,
            Columns = columns,
            Marks = students.Select(s =>
            {
                marks.TryGetValue(s.Id, out var mark);
                var row = new AssessmentMarkRowVm
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    RollNumber = s.RollNumber,
                    ObtainedMarks = mark?.ObtainedMarks ?? 0,
                    Notes = mark?.Notes,
                    ColumnScores = AssessmentScoreColumns.BreakdownToScores(mark?.ScoreBreakdownJson, columns)
                };
                if (row.ColumnScores.Sum() == 0 && row.ObtainedMarks > 0)
                    row.ColumnScores[0] = row.ObtainedMarks;
                return row;
            }).ToList()
        };
        PrepareAssessmentMarks(vm.Columns, vm.Marks, vm.TotalMarks);
        ViewBag.AssignmentId = assignmentId;
        ViewData["Title"] = "Enter marks";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Marks(AssessmentMarksFormVm model, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        var assessment = await Db.Assessments.FirstOrDefaultAsync(a => a.Id == model.AssessmentId && a.SchoolId == schoolId, ct);
        if (assessment is null) return NotFound();

        AssessmentScoreColumns.ApplyAssessmentDefaults(model, assessment);
        ClearAutoFieldValidation();
        PrepareAssessmentMarks(model.Columns, model.Marks, assessment.TotalMarks);
        ValidateAssessmentMarks(model.Columns, model.Marks, ModelState);

        if (!ModelState.IsValid)
        {
            await HydrateAsync(ct, assignmentId);
            ViewBag.AssignmentId = assignmentId;
            ViewData["Title"] = "Enter marks";
            return View(model);
        }

        assessment.Name = (model.Name ?? AssessmentScoreColumns.TitleFromColumns(model.Columns, model.AssessmentDate)).Trim();
        assessment.AssessmentType = model.AssessmentType;
        assessment.AssessmentDate = model.AssessmentDate;
        assessment.PassingMarks = model.PassingMarks;
        assessment.Description = model.ImportantInfo?.Trim();
        assessment.ScoreColumnsJson = AssessmentScoreColumns.Serialize(model.Columns);
        assessment.TotalMarks = AssessmentScoreColumns.TotalMax(model.Columns);
        assessment.UpdatedAt = DateTimeOffset.UtcNow;

        await SaveAssessmentMarksAsync(schoolId, assessment, model.Columns, model.Marks, ct);

        TempData["Flash"] = "Marks saved.";
        return RedirectToAction(nameof(GradeBook), new { assignmentId });
    }

    private async Task SaveAssessmentMarksAsync(
        Guid schoolId,
        Assessment assessment,
        IReadOnlyList<AssessmentScoreColumnVm> columns,
        IReadOnlyList<AssessmentMarkRowVm> rows,
        CancellationToken ct)
    {
        var cols = columns.Count > 0 ? columns.ToList() : AssessmentScoreColumns.Default(assessment.TotalMarks);
        AssessmentScoreColumns.NormalizeKeys(cols);

        foreach (var row in rows)
        {
            AssessmentScoreColumns.EnsureRowScores(row, cols.Count);
            var obtained = AssessmentScoreColumns.TotalObtained(row.ColumnScores);
            var percentage = assessment.TotalMarks > 0
                ? Math.Round(obtained / assessment.TotalMarks * 100, 2)
                : 0;
            var grade = await _grading.CalculateGradeAsync(schoolId, percentage, ct);
            var breakdown = AssessmentScoreColumns.ScoresToBreakdown(row.ColumnScores, cols);
            var existing = await Db.AssessmentMarks.FirstOrDefaultAsync(
                m => m.AssessmentId == assessment.Id && m.StudentId == row.StudentId, ct);
            if (existing is null)
            {
                Db.AssessmentMarks.Add(new AssessmentMark
                {
                    SchoolId = schoolId,
                    AssessmentId = assessment.Id,
                    StudentId = row.StudentId,
                    ObtainedMarks = obtained,
                    Percentage = percentage,
                    GradeLabel = grade,
                    Notes = row.Notes?.Trim(),
                    ScoreBreakdownJson = breakdown,
                    CreatedByUserId = CurrentUserId,
                    IsActive = true
                });
            }
            else
            {
                existing.ObtainedMarks = obtained;
                existing.Percentage = percentage;
                existing.GradeLabel = grade;
                existing.Notes = row.Notes?.Trim();
                existing.ScoreBreakdownJson = breakdown;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await Db.SaveChangesAsync(ct);
    }

    private static void PrepareAssessmentMarks(
        List<AssessmentScoreColumnVm> columns,
        List<AssessmentMarkRowVm> marks,
        decimal totalMarks)
    {
        if (columns.Count == 0)
            columns.AddRange(AssessmentScoreColumns.Default(totalMarks));
        AssessmentScoreColumns.NormalizeKeys(columns);

        foreach (var row in marks)
            AssessmentScoreColumns.EnsureRowScores(row, columns.Count);
    }

    private static void ValidateAssessmentMarks(
        IReadOnlyList<AssessmentScoreColumnVm> columns,
        IReadOnlyList<AssessmentMarkRowVm> marks,
        Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var maxTotal = AssessmentScoreColumns.TotalMax(columns);
        foreach (var row in marks)
        {
            AssessmentScoreColumns.EnsureRowScores(row, columns.Count);
            for (var i = 0; i < columns.Count; i++)
            {
                if (row.ColumnScores[i] > columns[i].MaxMarks)
                {
                    modelState.AddModelError(string.Empty,
                        $"{row.FullName}: {AssessmentTypeCatalog.Label(columns[i].AssessmentType)} cannot exceed {columns[i].MaxMarks}.");
                }
            }

            var obtained = AssessmentScoreColumns.TotalObtained(row.ColumnScores);
            if (maxTotal > 0 && obtained > maxTotal)
            {
                modelState.AddModelError(string.Empty,
                    $"Total marks for {row.FullName} cannot exceed {maxTotal}.");
            }
        }
    }

    private void ClearAutoFieldValidation()
    {
        foreach (var key in ModelState.Keys.ToList())
        {
            if (key == "Name" || key.EndsWith(".Name", StringComparison.Ordinal))
                ModelState.Remove(key);
        }
    }

    private static List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> BuildAssessmentTypeSelectList(AssessmentType selected)
        => AssessmentTypeCatalog.GradeBookOptions
            .Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(
                AssessmentTypeCatalog.Label(t),
                ((int)t).ToString(),
                t == selected))
            .ToList();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishAssessment(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await _content.PublishAsync(PublishStatus.Published, schoolId, id, AcademicContentKind.Assessment, ct);
        TempData["Flash"] = "Assessment results published to parents.";
        return RedirectToAction(nameof(GradeBook), new { assignmentId });
    }

    public async Task<IActionResult> Announcements(Guid? assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(Array.Empty<AnnouncementListItemVm>());

        var assignment = await GetOwnedAssignmentAsync(schoolId, selected.Value, ct);
        if (assignment is null) return Forbid();

        var items = await Db.ClassAnnouncements.AsNoTracking()
            .Where(a => a.SchoolId == schoolId
                        && a.SchoolClassId == assignment.SchoolClassId
                        && a.SchoolSectionId == assignment.SchoolSectionId
                        && a.SubjectId == assignment.SubjectId)
            .OrderByDescending(a => a.ContentDate)
            .Select(a => new AnnouncementListItemVm
            {
                Id = a.Id,
                Title = a.Title,
                Message = a.Message,
                ContentDate = a.ContentDate,
                Status = a.Status,
                AssignmentLabel = assignment.DisplayLabel
            })
            .ToListAsync(ct);
        ViewData["Title"] = "Announcements";
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateAnnouncement(Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await HydrateAsync(ct, assignmentId);
        ViewData["Title"] = "New announcement";
        return View(new AnnouncementFormVm { AssignmentId = assignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAnnouncement(AnnouncementFormVm model, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var (staff, staffErr) = await RequireStaffAsync(schoolId, ct);
        if (staffErr is not null) return staffErr;
        var assignment = await GetOwnedAssignmentAsync(schoolId, model.AssignmentId, ct);
        if (assignment is null || staff is null) return Forbid();

        if (!ModelState.IsValid)
        {
            await HydrateAsync(ct, model.AssignmentId);
            ViewData["Title"] = "New announcement";
            return View(model);
        }

        var item = new ClassAnnouncement
        {
            Title = model.Title.Trim(),
            Message = model.Message.Trim(),
            ContentDate = model.ContentDate,
            Status = model.Status,
            PublishedAt = model.Status == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        ApplyAssignmentScope(item, assignment, schoolId, staff.Id);
        Db.ClassAnnouncements.Add(item);
        await Db.SaveChangesAsync(ct);

        if (model.Attachments?.Count > 0)
            await _content.SaveAttachmentsAsync(schoolId, staff.Id, AcademicAttachmentOwnerType.Announcement, item.Id, model.Attachments, ct);

        TempData["Flash"] = model.Status == PublishStatus.Published ? "Announcement published." : "Announcement saved as draft.";
        return RedirectToAction(nameof(Announcements), new { assignmentId = model.AssignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishAnnouncement(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await _content.PublishAsync(PublishStatus.Published, schoolId, id, AcademicContentKind.Announcement, ct);
        TempData["Flash"] = "Announcement published.";
        return RedirectToAction(nameof(Announcements), new { assignmentId });
    }

    public async Task<IActionResult> Materials(Guid? assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(Array.Empty<CourseMaterialListItemVm>());

        var assignment = await GetOwnedAssignmentAsync(schoolId, selected.Value, ct);
        if (assignment is null) return Forbid();

        var items = await Db.CourseMaterials.AsNoTracking()
            .Where(m => m.SchoolId == schoolId
                        && m.SchoolClassId == assignment.SchoolClassId
                        && m.SchoolSectionId == assignment.SchoolSectionId
                        && m.SubjectId == assignment.SubjectId)
            .OrderByDescending(m => m.ContentDate)
            .Select(m => new CourseMaterialListItemVm
            {
                Id = m.Id,
                Title = m.Title,
                Category = m.Category,
                ContentDate = m.ContentDate,
                Status = m.Status,
                VisibleToParents = m.VisibleToParents,
                FileName = m.FileName,
                AssignmentLabel = assignment.DisplayLabel
            })
            .ToListAsync(ct);
        ViewData["Title"] = "Course Materials";
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateMaterial(Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await HydrateAsync(ct, assignmentId);
        ViewData["Title"] = "Upload material";
        return View(new CourseMaterialFormVm { AssignmentId = assignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMaterial(CourseMaterialFormVm model, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var (staff, staffErr) = await RequireStaffAsync(schoolId, ct);
        if (staffErr is not null) return staffErr;
        var assignment = await GetOwnedAssignmentAsync(schoolId, model.AssignmentId, ct);
        if (assignment is null || staff is null) return Forbid();

        if (model.File is null || model.File.Length == 0)
            ModelState.AddModelError(nameof(model.File), "A file is required.");

        if (!ModelState.IsValid)
        {
            await HydrateAsync(ct, model.AssignmentId);
            ViewData["Title"] = "Upload material";
            return View(model);
        }

        var path = await _files.SaveAcademicAsync(model.File!, schoolId, "course-materials", ct);
        var item = new CourseMaterial
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Category = model.Category,
            ContentDate = model.ContentDate,
            VisibleToParents = model.VisibleToParents,
            Status = model.Status,
            PublishedAt = model.Status == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            FilePath = path,
            FileName = model.File!.FileName,
            FileContentType = model.File.ContentType,
            FileSizeBytes = model.File.Length,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        ApplyAssignmentScope(item, assignment, schoolId, staff.Id);
        Db.CourseMaterials.Add(item);
        await Db.SaveChangesAsync(ct);
        TempData["Flash"] = "Course material uploaded.";
        return RedirectToAction(nameof(Materials), new { assignmentId = model.AssignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishMaterial(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await _content.PublishAsync(PublishStatus.Published, schoolId, id, AcademicContentKind.CourseMaterial, ct);
        TempData["Flash"] = "Material published.";
        return RedirectToAction(nameof(Materials), new { assignmentId });
    }

    [HttpGet]
    public async Task<IActionResult> Security(CancellationToken ct)
    {
        await HydrateAsync(ct);
        ViewData["Title"] = "Security";
        return View(new TeacherChangePasswordVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Security(TeacherChangePasswordVm model, CancellationToken ct)
    {
        await HydrateAsync(ct);
        ViewData["Title"] = "Security";
        if (model.NewPassword != model.ConfirmPassword)
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Passwords do not match.");
        if (!ModelState.IsValid) return View(model);

        var user = await UserManager.GetUserAsync(User);
        if (user is null) return Challenge();
        var result = await UserManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        user.MustChangePassword = false;
        await UserManager.UpdateAsync(user);
        await _accountEmails.SendPasswordChangedEmailAsync(user.Id, ct);
        TempData["Flash"] = "Password updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        await HydrateAsync(ct);
        ViewData["Title"] = "Profile";
        var user = await UserManager.GetUserAsync(User);
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var staff = await TeacherAccess.GetStaffForUserAsync(CurrentUserId, schoolId, ct);
        ViewBag.Staff = staff;
        ViewBag.User = user;
        return View();
    }

    public async Task<IActionResult> ClassAssignments(Guid? assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        await HydrateAsync(ct, assignmentId);
        var selected = assignmentId ?? (Guid?)ViewBag.SelectedAssignmentId;
        if (!selected.HasValue) return View(Array.Empty<ClassAssignmentListItemVm>());

        var assignment = await GetOwnedAssignmentAsync(schoolId, selected.Value, ct);
        if (assignment is null) return Forbid();

        var items = await Db.ClassAssignmentItems.AsNoTracking()
            .Where(a => a.SchoolId == schoolId
                        && a.SchoolClassId == assignment.SchoolClassId
                        && a.SchoolSectionId == assignment.SchoolSectionId
                        && a.SubjectId == assignment.SubjectId)
            .OrderByDescending(a => a.ContentDate)
            .Select(a => new ClassAssignmentListItemVm
            {
                Id = a.Id,
                Title = a.Title,
                ContentDate = a.ContentDate,
                DueDate = a.DueDate,
                Status = a.Status,
                AllowSubmission = a.AllowSubmission,
                AssignmentLabel = assignment.DisplayLabel
            })
            .ToListAsync(ct);
        ViewData["Title"] = "Student Assignments";
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> CreateClassAssignment(Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await HydrateAsync(ct, assignmentId);
        ViewData["Title"] = "Create assignment";
        return View(new ClassAssignmentFormVm { AssignmentId = assignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClassAssignment(ClassAssignmentFormVm model, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        var (staff, staffErr) = await RequireStaffAsync(schoolId, ct);
        if (staffErr is not null) return staffErr;
        var assignment = await GetOwnedAssignmentAsync(schoolId, model.AssignmentId, ct);
        if (assignment is null || staff is null) return Forbid();

        if (!ModelState.IsValid)
        {
            await HydrateAsync(ct, model.AssignmentId);
            ViewData["Title"] = "Create assignment";
            return View(model);
        }

        string? path = null;
        string? fileName = null;
        string? contentType = null;
        long? size = null;
        if (model.Attachment is { Length: > 0 })
        {
            path = await _files.SaveAcademicAsync(model.Attachment, schoolId, "class-assignments", ct);
            fileName = model.Attachment.FileName;
            contentType = model.Attachment.ContentType;
            size = model.Attachment.Length;
        }

        var item = new ClassAssignmentItem
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            ContentDate = model.ContentDate,
            DueDate = model.DueDate,
            TotalMarks = model.TotalMarks,
            AllowSubmission = model.AllowSubmission,
            Status = model.Status,
            PublishedAt = model.Status == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            AttachmentPath = path,
            AttachmentFileName = fileName,
            AttachmentContentType = contentType,
            AttachmentSizeBytes = size,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        ApplyAssignmentScope(item, assignment, schoolId, staff.Id);
        Db.ClassAssignmentItems.Add(item);
        await Db.SaveChangesAsync(ct);

        if (model.Status == PublishStatus.Published)
            await _content.PublishAsync(PublishStatus.Published, schoolId, item.Id, AcademicContentKind.ClassAssignment, ct);

        TempData["Flash"] = "Assignment created.";
        return RedirectToAction(nameof(ClassAssignments), new { assignmentId = model.AssignmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishClassAssignment(Guid id, Guid assignmentId, CancellationToken ct)
    {
        if (RequireSchool(out var schoolId) is { } deny) return deny;
        if (await GetOwnedAssignmentAsync(schoolId, assignmentId, ct) is null) return Forbid();
        await _content.PublishAsync(PublishStatus.Published, schoolId, id, AcademicContentKind.ClassAssignment, ct);
        TempData["Flash"] = "Assignment published.";
        return RedirectToAction(nameof(ClassAssignments), new { assignmentId });
    }
}
