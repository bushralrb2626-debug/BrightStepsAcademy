using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers;

[Authorize(Roles = AppRoleNames.Student)]
[Route("StudentPortal")]
public class StudentPortalController : Controller
{
    private readonly ISchoolData _store;
    private readonly IStudentAcademicService _academic;
    private readonly IReportCardService _reportCards;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IFileStorageService _files;

    public StudentPortalController(
        ISchoolData store,
        IStudentAcademicService academic,
        IReportCardService reportCards,
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IFileStorageService files)
    {
        _store = store;
        _academic = academic;
        _reportCards = reportCards;
        _userManager = userManager;
        _db = db;
        _files = files;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null)
        {
            ViewData["Title"] = "Student Dashboard";
            return await HydratedViewAsync(new StudentDashboardVm(), ct);
        }

        var dashboard = await _academic.BuildDashboardAsync(student, ct);
        ViewData["Title"] = "Student Dashboard";
        return await HydratedViewAsync(dashboard, ct);
    }

    [HttpGet("Profile")]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync((StudentProfileVm?)null, ct);

        var vm = new StudentProfileVm
        {
            FullName = student.FullName,
            StudentCode = student.StudentCode,
            ClassDisplay = FormatClassDisplay(student),
            RollNumber = student.RollNumber,
            DateOfBirth = student.DateOfBirth,
            ProfileImagePath = student.ProfileImagePath,
            AdmissionDate = student.AdmissionDate,
            SchoolName = student.School?.Name ?? ""
        };
        ViewData["Title"] = "My Profile";
        return await HydratedViewAsync(vm, ct);
    }

    [HttpGet("Timetable")]
    public async Task<IActionResult> Timetable(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentTimetableSlotVm>(), ct);

        ViewBag.TodaySlots = await _academic.GetTodayTimetableAsync(student, ct);
        ViewData["Title"] = "My Timetable";
        return await HydratedViewAsync(await _academic.GetTimetableAsync(student, ct), ct);
    }

    [HttpGet("Diary")]
    public async Task<IActionResult> Diary(Guid? subjectId, Guid? staffMemberId, DateOnly? date, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentDiaryItemVm>(), ct);

        var filter = new StudentDiaryFilterVm
        {
            SubjectId = subjectId,
            StaffMemberId = staffMemberId,
            Date = date,
            SubjectOptions = await LoadSubjectOptionsAsync(student, ct)
        };
        ViewBag.Filter = filter;
        ViewData["Title"] = "Daily Diary";
        return await HydratedViewAsync(await _academic.GetDiaryAsync(student, filter, ct), ct);
    }

    [HttpGet("Assignments")]
    public async Task<IActionResult> Assignments(Guid? subjectId, StudentAssignmentDisplayStatus? status, DateOnly? dueBefore, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentAssignmentItemVm>(), ct);

        var filter = new StudentAssignmentFilterVm
        {
            SubjectId = subjectId,
            Status = status,
            DueBefore = dueBefore,
            SubjectOptions = await LoadSubjectOptionsAsync(student, ct)
        };
        ViewBag.Filter = filter;
        ViewData["Title"] = "Assignments";
        return await HydratedViewAsync(await _academic.GetAssignmentsAsync(student, filter, ct), ct);
    }

    [HttpGet("Assignments/{id:guid}")]
    public async Task<IActionResult> AssignmentDetail(Guid id, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync((StudentAssignmentDetailVm?)null, ct);

        ViewData["Title"] = "Assignment";
        return await HydratedViewAsync(await _academic.GetAssignmentAsync(student, id, ct), ct);
    }

    [HttpPost("Assignments/{id:guid}/Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAssignment(Guid id, StudentAssignmentSubmitVm model, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return Forbid();

        var assignment = await _academic.GetAssignmentAsync(student, id, ct);
        if (assignment is null || !assignment.AllowSubmission)
            return NotFound();

        if (assignment.Submission is not null)
        {
            TempData["Flash"] = "You have already submitted this assignment.";
            return RedirectToAction(nameof(AssignmentDetail), new { id });
        }

        string? filePath = null;
        string? fileName = null;
        string? contentType = null;
        long? size = null;

        if (model.UploadFile is { Length: > 0 })
        {
            filePath = await _files.SaveAcademicAsync(model.UploadFile, student.SchoolId, "submissions", ct);
            fileName = model.UploadFile.FileName;
            contentType = model.UploadFile.ContentType;
            size = model.UploadFile.Length;
        }

        _db.ClassAssignmentSubmissions.Add(new ClassAssignmentSubmission
        {
            SchoolId = student.SchoolId,
            AssignmentId = id,
            StudentId = student.Id,
            SubmittedAt = DateTimeOffset.UtcNow,
            TextResponse = model.TextResponse?.Trim(),
            FilePath = filePath,
            FileName = fileName,
            FileContentType = contentType,
            FileSizeBytes = size,
            ReviewStatus = AssignmentSubmissionStatus.Submitted,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        TempData["Flash"] = "Assignment submitted successfully.";
        return RedirectToAction(nameof(AssignmentDetail), new { id });
    }

    [HttpGet("Materials")]
    public async Task<IActionResult> Materials(Guid? subjectId, CourseMaterialCategory? category, DateOnly? date, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentMaterialItemVm>(), ct);

        var filter = new StudentMaterialFilterVm
        {
            SubjectId = subjectId,
            Category = category,
            Date = date,
            SubjectOptions = await LoadSubjectOptionsAsync(student, ct)
        };
        ViewBag.Filter = filter;
        ViewData["Title"] = "Course Material";
        return await HydratedViewAsync(await _academic.GetMaterialsAsync(student, filter, ct), ct);
    }

    [HttpGet("Marks")]
    public async Task<IActionResult> Marks(Guid? subjectId, AssessmentType? assessmentType, DateOnly? date, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentMarkItemVm>(), ct);

        var filter = new StudentMarkFilterVm
        {
            SubjectId = subjectId,
            AssessmentType = assessmentType,
            Date = date,
            SubjectOptions = await LoadSubjectOptionsAsync(student, ct)
        };
        ViewBag.Filter = filter;
        ViewBag.Performance = await _academic.GetPerformanceAsync(student, ct);
        ViewData["Title"] = "Marks & Grades";
        return await HydratedViewAsync(await _academic.GetMarksAsync(student, filter, ct), ct);
    }

    [HttpGet("Performance")]
    public async Task<IActionResult> Performance(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentPerformanceSubjectVm>(), ct);

        ViewData["Title"] = "My Performance";
        return await HydratedViewAsync(await _academic.GetPerformanceAsync(student, ct), ct);
    }

    [HttpGet("Attendance")]
    public async Task<IActionResult> Attendance(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(new StudentAttendanceSummaryVm(), ct);

        ViewData["Title"] = "My Attendance";
        return await HydratedViewAsync(await _academic.GetAttendanceAsync(student, ct), ct);
    }

    [HttpGet("Announcements")]
    public async Task<IActionResult> Announcements(Guid? subjectId, DateOnly? date, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentAnnouncementItemVm>(), ct);

        var filter = new StudentAnnouncementFilterVm
        {
            SubjectId = subjectId,
            Date = date,
            SubjectOptions = await LoadSubjectOptionsAsync(student, ct)
        };
        ViewBag.Filter = filter;
        ViewData["Title"] = "Announcements";
        return await HydratedViewAsync(await _academic.GetAnnouncementsAsync(student, filter, ct), ct);
    }

    [HttpGet("ImportantInformation")]
    public async Task<IActionResult> ImportantInformation(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentInfoItemVm>(), ct);

        ViewData["Title"] = "Important Information";
        return await HydratedViewAsync(await _academic.GetImportantInformationAsync(student, ct), ct);
    }

    [HttpGet("Exams")]
    public async Task<IActionResult> Exams(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<StudentExamItemVm>(), ct);

        ViewBag.PreviousExams = await _academic.GetPreviousExamsAsync(student, ct);
        ViewData["Title"] = "Exams";
        return await HydratedViewAsync(await _academic.GetExamsAsync(student, ct), ct);
    }

    [HttpGet("ExamResults")]
    public async Task<IActionResult> ExamResults(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync((StudentExamResultsVm?)null, ct);

        ViewData["Title"] = "Exam Results";
        return await HydratedViewAsync(await _academic.GetExamResultsAsync(student, ct), ct);
    }

    [HttpGet("ReportCard")]
    public async Task<IActionResult> ReportCard(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync((ReportCardVm?)null, ct);

        var card = await _reportCards.BuildAsync(student.Id, ct);
        ViewData["Title"] = "Report Card";
        return await HydratedViewAsync(card, ct);
    }

    [HttpGet("Notifications")]
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(Array.Empty<AppNotification>(), ct);

        var items = await _academic.GetNotificationsAsync(CurrentUserId(), student.SchoolId, ct);
        ViewData["Title"] = "Notifications";
        return await HydratedViewAsync(items, ct);
    }

    [HttpPost("Notifications/MarkRead/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        var note = await _db.AppNotifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);
        if (note is null) return NotFound();
        note.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Notifications));
    }

    [HttpPost("Notifications/MarkAllRead")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllNotificationsRead(CancellationToken ct)
    {
        var userId = CurrentUserId();
        var unread = await _db.AppNotifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(ct);
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Notifications));
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(string? q, CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        if (student is null) return await HydratedViewAsync(new StudentSearchResultsVm(), ct);

        ViewData["Title"] = "Search";
        if (string.IsNullOrWhiteSpace(q))
            return await HydratedViewAsync(new StudentSearchResultsVm(), ct);

        return await HydratedViewAsync(await _academic.SearchAsync(student, q, ct), ct);
    }

    [HttpGet("Settings")]
    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        var student = await RequireStudentAsync(ct);
        var user = await _userManager.GetUserAsync(User);
        var vm = new StudentSettingsVm
        {
            FullName = student?.FullName ?? user?.FullName ?? "",
            LoginId = user?.LoginId ?? user?.UserName ?? "",
            Email = user?.Email,
            ClassDisplay = student is not null ? FormatClassDisplay(student) : ""
        };
        ViewData["Title"] = "Settings";
        return await HydratedViewAsync(vm, ct);
    }

    [HttpGet("Security")]
    public async Task<IActionResult> Security(CancellationToken ct)
    {
        await HydrateAsync(null, ct);
        ViewData["Title"] = "Security";
        return View(new StudentChangePasswordVm());
    }

    [HttpPost("Security")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Security(StudentChangePasswordVm model, CancellationToken ct)
    {
        await HydrateAsync(null, ct);
        ViewData["Title"] = "Security";

        if (model.NewPassword != model.ConfirmPassword)
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Passwords do not match.");

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);
            return View(model);
        }

        user.MustChangePassword = false;
        await _userManager.UpdateAsync(user);
        TempData["Flash"] = "Password updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private string CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    private async Task<StudentRecord?> RequireStudentAsync(CancellationToken ct)
        => await _academic.GetStudentForUserAsync(CurrentUserId(), ct);

    private async Task<IActionResult> HydratedViewAsync<T>(T model, CancellationToken ct)
    {
        await HydrateAsync(model, ct);
        return View(model);
    }

    private async Task HydrateAsync(object? model, CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        var student = await _academic.GetStudentForUserAsync(CurrentUserId(), ct);
        var profile = _store.ProfileFor("Student");
        if (user is not null)
        {
            profile.DisplayName = user.FullName;
            profile.FirstName = user.FullName.Split(' ').FirstOrDefault() ?? user.FullName;
            profile.Email = user.Email ?? profile.Email;
            profile.UserId = user.LoginId ?? user.Email ?? profile.UserId;
        }

        ViewBag.RoleKey = "StudentPortal";
        ViewBag.Profile = profile;
        ViewBag.NavGroups = NavCatalog.For("StudentPortal");
        ViewBag.Student = student;
        ViewBag.MustChangePassword = user?.MustChangePassword == true;

        if (student?.School is not null)
        {
            ViewBag.SchoolName = student.School.Name;
            ViewBag.SchoolLogo = student.School.LogoPath;
        }

        var notifications = student is not null
            ? await _academic.GetNotificationsAsync(CurrentUserId(), student.SchoolId, ct)
            : Array.Empty<AppNotification>();
        ViewBag.Notifications = notifications.Select(n => new NotificationItem
        {
            Title = n.Title,
            Body = n.Message,
            Time = n.CreatedAt.ToLocalTime().ToString("MMM d, h:mm tt"),
            Type = n.IsRead ? "read" : "info"
        }).ToList();
        ViewBag.Unread = notifications.Count(n => !n.IsRead);
        ViewBag.Messages = 0;
        ViewData["Title"] ??= $"{profile.Role} · Student Portal";
        ViewBag.Store = _store;
        ViewBag.Model = model;
    }

    private async Task<List<SelectListItem>> LoadSubjectOptionsAsync(StudentRecord student, CancellationToken ct)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return new List<SelectListItem>();

        return await _db.TeacherAssignments.AsNoTracking()
            .Where(a => a.SchoolId == student.SchoolId
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId
                        && a.IsActive)
            .Join(_db.Subjects.AsNoTracking(), a => a.SubjectId, s => s.Id, (a, s) => new { s.Id, s.Name })
            .Distinct()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToListAsync(ct);
    }

    private static string FormatClassDisplay(StudentRecord student)
    {
        var cls = student.SchoolClass?.Name ?? student.ClassName ?? "—";
        var sec = student.SchoolSection?.Name ?? student.Section ?? "—";
        return $"{cls} · {sec}";
    }
}
