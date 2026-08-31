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

[Authorize(Roles = AppRoleNames.Teacher)]
public abstract class TeacherPortalControllerBase : Controller
{
    protected readonly ISchoolData Store;
    protected readonly ITenantContext Tenant;
    protected readonly ITeacherAccessService TeacherAccess;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly AppDbContext Db;

    protected TeacherPortalControllerBase(
        ISchoolData store,
        ITenantContext tenant,
        ITeacherAccessService teacherAccess,
        UserManager<ApplicationUser> userManager,
        AppDbContext db)
    {
        Store = store;
        Tenant = tenant;
        TeacherAccess = teacherAccess;
        UserManager = userManager;
        Db = db;
    }

    protected string CurrentUserId => Tenant.UserId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    protected IActionResult? RequireSchool(out Guid schoolId)
    {
        if (!Tenant.SchoolId.HasValue)
        {
            schoolId = Guid.Empty;
            return Forbid();
        }
        schoolId = Tenant.SchoolId.Value;
        return null;
    }

    protected async Task<(StaffMember? Staff, IActionResult? Error)> RequireStaffAsync(Guid schoolId, CancellationToken ct)
    {
        var staff = await TeacherAccess.GetStaffForUserAsync(CurrentUserId, schoolId, ct);
        if (staff is null)
            return (null, View("NoAccess"));
        return (staff, null);
    }

    protected async Task HydrateAsync(CancellationToken ct, Guid? selectedAssignmentId = null)
    {
        var user = await UserManager.GetUserAsync(User);
        var profile = Store.ProfileFor("Teacher");
        if (user is not null)
        {
            profile.DisplayName = user.FullName;
            profile.FirstName = user.FullName.Split(' ').FirstOrDefault() ?? user.FullName;
            profile.Email = user.Email ?? profile.Email;
            profile.UserId = user.LoginId ?? user.Email ?? profile.UserId;
        }

        ViewBag.RoleKey = "Teacher";
        ViewBag.Profile = profile;
        ViewBag.NavGroups = NavCatalog.For("Teacher");
        ViewBag.Notifications = Store.Notifications;
        ViewBag.Unread = Store.Notifications.Count;
        ViewBag.Messages = 0;
        ViewData["Title"] ??= $"{profile.Role} · Teacher Portal";
        ViewBag.Store = Store;
        ViewBag.MustChangePassword = user?.MustChangePassword == true;

        if (Tenant.SchoolId.HasValue)
        {
            var schoolId = Tenant.SchoolId.Value;
            var assignments = await TeacherAccess.GetAssignmentsAsync(CurrentUserId, schoolId, ct);
            if (assignments.Count == 0)
            {
                await SchoolBootstrap.EnsureSchoolBootstrappedAsync(Db, schoolId, ct);
                assignments = await TeacherAccess.GetAssignmentsAsync(CurrentUserId, schoolId, ct);
            }

            var options = assignments.Select(MapAssignment).ToList();
            ViewBag.TeacherAssignments = options;
            ViewBag.SelectedAssignmentId = selectedAssignmentId ?? options.FirstOrDefault()?.Id;
        }
        else
        {
            ViewBag.TeacherAssignments = Array.Empty<TeacherAssignmentOptionVm>();
        }
    }

    protected static TeacherAssignmentOptionVm MapAssignment(TeacherAssignmentVm a) => new()
    {
        Id = a.Id,
        Label = a.DisplayLabel,
        SchoolClassId = a.SchoolClassId,
        SchoolSectionId = a.SchoolSectionId,
        SubjectId = a.SubjectId,
        StudentCount = a.StudentCount
    };

    protected async Task<TeacherAssignmentVm?> GetOwnedAssignmentAsync(Guid schoolId, Guid assignmentId, CancellationToken ct)
        => await TeacherAccess.GetAssignmentAsync(CurrentUserId, schoolId, assignmentId, ct);

    protected List<SelectListItem> AssignmentSelectList(IReadOnlyList<TeacherAssignmentOptionVm> assignments, Guid? selectedId)
        => assignments.Select(a => new SelectListItem(a.Label, a.Id.ToString(), a.Id == selectedId)).ToList();

    protected static void ApplyAssignmentScope(ClassScopedAcademicContent content, TeacherAssignmentVm assignment, Guid schoolId, Guid staffMemberId)
    {
        content.SchoolId = schoolId;
        content.StaffMemberId = staffMemberId;
        content.SchoolClassId = assignment.SchoolClassId;
        content.SchoolSectionId = assignment.SchoolSectionId;
        content.SubjectId = assignment.SubjectId;
        content.TeacherAssignmentId = assignment.Id;
    }

    protected static void ApplyAssignmentScope(Assessment assessment, TeacherAssignmentVm assignment, Guid schoolId, Guid staffMemberId)
    {
        assessment.SchoolId = schoolId;
        assessment.StaffMemberId = staffMemberId;
        assessment.SchoolClassId = assignment.SchoolClassId;
        assessment.SchoolSectionId = assignment.SchoolSectionId;
        assessment.SubjectId = assignment.SubjectId;
        assessment.TeacherAssignmentId = assignment.Id;
    }
}
