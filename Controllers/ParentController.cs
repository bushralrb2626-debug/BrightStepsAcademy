using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers;

[Authorize(Roles = AppRoleNames.Guardian)]
public class ParentController : Controller
{
    private readonly ISchoolData _store;
    private readonly IGuardianService _guardians;
    private readonly IParentAcademicService _academic;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public ParentController(
        ISchoolData store,
        IGuardianService guardians,
        IParentAcademicService academic,
        UserManager<ApplicationUser> userManager,
        AppDbContext db)
    {
        _store = store;
        _guardians = guardians;
        _academic = academic;
        _userManager = userManager;
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        return View(children);
    }

    public async Task<IActionResult> Children(CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        ViewData["Title"] = children.Count == 1 ? "My Child" : "My Children";
        return View(children);
    }

    public async Task<IActionResult> Diary(Guid? studentId, CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        var scope = await ResolveStudentScopeAsync(children, studentId, ct);
        if (scope is null) return View(Array.Empty<ParentDiaryItemVm>());

        var student = await _academic.GetLinkedStudentAsync(CurrentUserId(), scope.StudentId, ct);
        if (student?.SchoolClassId is null || student.SchoolSectionId is null)
            return View(Array.Empty<ParentDiaryItemVm>());

        var items = await _db.DailyDiaryEntries.AsNoTracking()
            .Where(d => d.Status == PublishStatus.Published
                        && d.SchoolClassId == student.SchoolClassId
                        && d.SchoolSectionId == student.SchoolSectionId)
            .Join(_db.Subjects.AsNoTracking(), d => d.SubjectId, sub => sub.Id, (d, sub) => new ParentDiaryItemVm
            {
                Id = d.Id,
                Title = d.Title,
                Topic = d.Topic,
                Homework = d.Homework,
                Instructions = d.Instructions,
                ContentDate = d.ContentDate,
                DueDate = d.DueDate,
                SubjectName = sub.Name
            })
            .OrderByDescending(d => d.ContentDate)
            .ToListAsync(ct);

        ViewBag.Scope = scope;
        ViewData["Title"] = "Daily Diary";
        return View(items);
    }

    public async Task<IActionResult> Attendance(Guid? studentId, CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        var scope = await ResolveStudentScopeAsync(children, studentId, ct);
        if (scope is null) return View(Array.Empty<ParentAttendanceItemVm>());

        var student = await _academic.GetLinkedStudentAsync(CurrentUserId(), scope.StudentId, ct);
        if (student?.SchoolClassId is null || student.SchoolSectionId is null)
            return View(Array.Empty<ParentAttendanceItemVm>());

        var items = await _db.AttendanceRecords.AsNoTracking()
            .Where(r => r.StudentId == student.Id)
            .Join(_db.AttendanceSessions.AsNoTracking(), r => r.AttendanceSessionId, s => s.Id, (r, s) => new { r, s })
            .Where(x => x.s.SchoolClassId == student.SchoolClassId && x.s.SchoolSectionId == student.SchoolSectionId)
            .Join(_db.Subjects.AsNoTracking(), x => x.s.SubjectId, sub => sub.Id, (x, sub) => new ParentAttendanceItemVm
            {
                SessionDate = x.s.SessionDate,
                PeriodLabel = x.s.PeriodLabel,
                Status = x.r.Status,
                SubjectName = sub.Name
            })
            .OrderByDescending(x => x.SessionDate)
            .Take(60)
            .ToListAsync(ct);

        ViewBag.Scope = scope;
        ViewData["Title"] = "Attendance";
        return View(items);
    }

    public async Task<IActionResult> Marks(Guid? studentId, CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        var scope = await ResolveStudentScopeAsync(children, studentId, ct);
        if (scope is null) return View(Array.Empty<ParentMarkItemVm>());

        var items = await _db.AssessmentMarks.AsNoTracking()
            .Where(m => m.StudentId == scope.StudentId)
            .Join(_db.Assessments.AsNoTracking().Where(a => a.Status == PublishStatus.Published),
                m => m.AssessmentId, a => a.Id, (m, a) => new { m, a })
            .Join(_db.Subjects.AsNoTracking(), x => x.a.SubjectId, sub => sub.Id, (x, sub) => new ParentMarkItemVm
            {
                AssessmentId = x.a.Id,
                AssessmentName = x.a.Name,
                AssessmentType = x.a.AssessmentType,
                AssessmentDate = x.a.AssessmentDate,
                ObtainedMarks = x.m.ObtainedMarks,
                TotalMarks = x.a.TotalMarks,
                Percentage = x.m.Percentage,
                GradeLabel = x.m.GradeLabel,
                SubjectName = sub.Name
            })
            .OrderByDescending(x => x.AssessmentDate)
            .ToListAsync(ct);

        ViewBag.Scope = scope;
        ViewData["Title"] = "Marks";
        return View(items);
    }

    public async Task<IActionResult> Announcements(Guid? studentId, CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        var scope = await ResolveStudentScopeAsync(children, studentId, ct);
        if (scope is null) return View(Array.Empty<ParentAnnouncementItemVm>());

        var student = await _academic.GetLinkedStudentAsync(CurrentUserId(), scope.StudentId, ct);
        if (student?.SchoolClassId is null || student.SchoolSectionId is null)
            return View(Array.Empty<ParentAnnouncementItemVm>());

        var items = await _db.ClassAnnouncements.AsNoTracking()
            .Where(a => a.Status == PublishStatus.Published
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId)
            .Join(_db.Subjects.AsNoTracking(), a => a.SubjectId, sub => sub.Id, (a, sub) => new ParentAnnouncementItemVm
            {
                Id = a.Id,
                Title = a.Title,
                Message = a.Message,
                ContentDate = a.ContentDate,
                SubjectName = sub.Name
            })
            .OrderByDescending(a => a.ContentDate)
            .ToListAsync(ct);

        ViewBag.Scope = scope;
        ViewData["Title"] = "Announcements";
        return View(items);
    }

    public Task<IActionResult> CourseMaterial(Guid? studentId, CancellationToken ct)
        => Materials(studentId, ct);

    public async Task<IActionResult> Materials(Guid? studentId, CancellationToken ct)
    {
        var children = await LoadChildrenAsync(ct);
        await HydrateAsync(children, ct);
        var scope = await ResolveStudentScopeAsync(children, studentId, ct);
        if (scope is null) return View(Array.Empty<ParentMaterialItemVm>());

        var student = await _academic.GetLinkedStudentAsync(CurrentUserId(), scope.StudentId, ct);
        if (student?.SchoolClassId is null || student.SchoolSectionId is null)
            return View(Array.Empty<ParentMaterialItemVm>());

        var items = await _db.CourseMaterials.AsNoTracking()
            .Where(m => m.Status == PublishStatus.Published && m.VisibleToParents
                        && m.SchoolClassId == student.SchoolClassId
                        && m.SchoolSectionId == student.SchoolSectionId)
            .Join(_db.Subjects.AsNoTracking(), m => m.SubjectId, sub => sub.Id, (m, sub) => new ParentMaterialItemVm
            {
                Id = m.Id,
                Title = m.Title,
                Category = m.Category,
                ContentDate = m.ContentDate,
                FileName = m.FileName,
                SubjectName = sub.Name
            })
            .OrderByDescending(m => m.ContentDate)
            .ToListAsync(ct);

        ViewBag.Scope = scope;
        ViewData["Title"] = "Course Material";
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> ChangePassword(CancellationToken ct)
    {
        await HydrateAsync(await LoadChildrenAsync(ct), ct);
        ViewData["Title"] = "Change Password";
        return View(new GuardianChangePasswordVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(GuardianChangePasswordVm model, CancellationToken ct)
    {
        await HydrateAsync(await LoadChildrenAsync(ct), ct);
        ViewData["Title"] = "Change Password";

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

    private async Task<ParentStudentScopeVm?> ResolveStudentScopeAsync(
        IReadOnlyList<GuardianChildVm> children,
        Guid? studentId,
        CancellationToken ct)
    {
        if (children.Count == 0) return null;
        var selected = studentId.HasValue
            ? children.FirstOrDefault(c => c.Id == studentId.Value)
            : children[0];
        if (selected is null) return null;

        if (studentId.HasValue && !await _academic.CanAccessStudentAsync(CurrentUserId(), studentId.Value, ct))
            return null;

        return new ParentStudentScopeVm
        {
            StudentId = selected.Id,
            FullName = selected.FullName,
            ClassDisplay = selected.ClassDisplay,
            Children = children.Select(c => new SelectListItem(
                c.FullName,
                c.Id.ToString(),
                c.Id == selected.Id)).ToList()
        };
    }

    private async Task<IReadOnlyList<GuardianChildVm>> LoadChildrenAsync(CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Array.Empty<GuardianChildVm>();

        var profile = await _guardians.GetProfileForUserAsync(userId, ct);
        if (profile is null)
            return Array.Empty<GuardianChildVm>();

        return await _db.StudentGuardianLinks.AsNoTracking()
            .Where(l => l.GuardianProfileId == profile.Id && l.IsActive)
            .Join(_db.StudentRecords.AsNoTracking().Where(s => s.IsActive),
                l => l.StudentId,
                s => s.Id,
                (l, s) => new { l, s })
            .GroupJoin(_db.SchoolClasses.AsNoTracking(),
                x => x.s.SchoolClassId,
                c => c.Id,
                (x, classes) => new { x.l, x.s, classes })
            .SelectMany(x => x.classes.DefaultIfEmpty(), (x, c) => new { x.l, x.s, c })
            .GroupJoin(_db.SchoolSections.AsNoTracking(),
                x => x.s.SchoolSectionId,
                sec => sec.Id,
                (x, sections) => new { x.l, x.s, x.c, sections })
            .SelectMany(x => x.sections.DefaultIfEmpty(), (x, sec) => new GuardianChildVm
            {
                Id = x.s.Id,
                StudentCode = x.s.StudentCode,
                FullName = x.s.FullName,
                ClassName = x.c != null ? x.c.Name : x.s.ClassName,
                Section = sec != null ? sec.Name : x.s.Section,
                SchoolClassId = x.s.SchoolClassId,
                SchoolSectionId = x.s.SchoolSectionId,
                Relationship = x.l.Relationship
            })
            .OrderBy(c => c.FullName)
            .ToListAsync(ct);
    }

    private async Task HydrateAsync(IReadOnlyList<GuardianChildVm> children, CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        var profile = _store.ProfileFor("Parent");
        if (user is not null)
        {
            profile.DisplayName = user.FullName;
            profile.FirstName = user.FullName.Split(' ').FirstOrDefault() ?? user.FullName;
            profile.Email = user.Email ?? profile.Email;
            profile.UserId = user.LoginId ?? user.Email ?? profile.UserId;
        }

        ViewBag.RoleKey = "Parent";
        ViewBag.Profile = profile;
        ViewBag.NavGroups = NavCatalog.For("ParentPortal");
        ViewBag.Notifications = _store.Notifications;
        ViewBag.Unread = _store.Notifications.Count;
        ViewBag.Messages = 0;
        ViewData["Title"] ??= $"{profile.Role} · Guardian Portal";
        ViewBag.Store = _store;
        ViewBag.Children = children;
        ViewBag.MustChangePassword = user?.MustChangePassword == true;
    }
}
