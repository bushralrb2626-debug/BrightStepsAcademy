using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public class AdminController(ISchoolData store) : DashboardController(store)
{
    public IActionResult Index() { Hydrate("Admin"); return View(); }
    public IActionResult Students() { Hydrate("Admin"); ViewData["Title"] = "Students"; return View(); }
    public IActionResult Teachers() { Hydrate("Admin"); ViewData["Title"] = "Teachers"; return View(); }
    public IActionResult Parents() { Hydrate("Admin"); ViewData["Title"] = "Parents"; return View(); }
    public IActionResult Classes() { Hydrate("Admin"); ViewData["Title"] = "Classes"; return View(); }
    public IActionResult Assignments() { Hydrate("Admin"); ViewData["Title"] = "Assignments"; return View("~/Views/Shared/Modules/Assignments.cshtml"); }
    public IActionResult Attendance() { Hydrate("Admin"); ViewData["Title"] = "Attendance"; return View("~/Views/Shared/Modules/Attendance.cshtml"); }
    public IActionResult Results() { Hydrate("Admin"); ViewData["Title"] = "Results"; return View("~/Views/Shared/Modules/Results.cshtml"); }
    public IActionResult Timetable() { Hydrate("Admin"); ViewData["Title"] = "Timetable"; return View("~/Views/Shared/Modules/Timetable.cshtml"); }
    public IActionResult Notices() { Hydrate("Admin"); ViewData["Title"] = "Notices"; return View("~/Views/Shared/Modules/Notices.cshtml"); }
    public IActionResult Events() { Hydrate("Admin"); ViewData["Title"] = "Events"; return View("~/Views/Shared/Modules/Events.cshtml"); }
    public IActionResult Messages() { Hydrate("Admin"); ViewData["Title"] = "Messages"; return View("~/Views/Shared/Modules/Messages.cshtml"); }
    public IActionResult Reports() { Hydrate("Admin"); ViewData["Title"] = "Reports"; return View("~/Views/Shared/Modules/Reports.cshtml"); }
    public IActionResult Settings() { Hydrate("Admin"); ViewData["Title"] = "Settings"; return View("~/Views/Shared/Modules/Settings.cshtml"); }
    public IActionResult Profile() { Hydrate("Admin"); ViewData["Title"] = "Profile"; return View("~/Views/Shared/Modules/Profile.cshtml"); }
}
