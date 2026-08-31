using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Models;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

[Authorize(Roles = AppRoleNames.Student)]
[Route("StudentPortal")]
public class StudentPortalController : Controller
{
    private readonly ISchoolData _store;
    private readonly IStudentAcademicService _academic;
    private readonly IReportCardService _reportCards;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentPortalController(
        ISchoolData store,
        IStudentAcademicService academic,
        IReportCardService reportCards,
        UserManager<ApplicationUser> userManager)
    {
        _store = store;
        _academic = academic;
        _reportCards = reportCards;
        _userManager = userManager;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        await HydrateAsync(ct);
        var student = await _academic.GetStudentForUserAsync(CurrentUserId(), ct);
        ViewBag.Student = student;
        ViewData["Title"] = "Student Portal";
        return View();
    }

    [HttpGet("ReportCard")]
    public async Task<IActionResult> ReportCard(CancellationToken ct)
    {
        await HydrateAsync(ct);
        var student = await _academic.GetStudentForUserAsync(CurrentUserId(), ct);
        if (student is null)
        {
            ViewData["Title"] = "Report Card";
            return View((ReportCardVm?)null);
        }

        var card = await _reportCards.BuildAsync(student.Id, ct);
        ViewData["Title"] = "Report Card";
        return View(card);
    }

    private string CurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    private async Task HydrateAsync(CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
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
        ViewBag.Notifications = _store.Notifications;
        ViewBag.Unread = _store.Notifications.Count;
        ViewBag.Messages = 0;
        ViewData["Title"] ??= $"{profile.Role} · Student Portal";
        ViewBag.Store = _store;
    }
}
