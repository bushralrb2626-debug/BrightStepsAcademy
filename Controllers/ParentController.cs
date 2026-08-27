using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public class ParentController(ISchoolData store) : DashboardController(store)
{
    public IActionResult Index() { Hydrate("Parent"); return View(); }
    public IActionResult Children() { Hydrate("Parent"); ViewData["Title"] = "My Children"; return View(); }
    public IActionResult Attendance() { Hydrate("Parent"); ViewData["Title"] = "Attendance"; return View(); }
    public IActionResult Homework() { Hydrate("Parent"); ViewData["Title"] = "Homework"; return View(); }
    public IActionResult Assignments() { Hydrate("Parent"); ViewData["Title"] = "Assignments"; return View("~/Views/Shared/Modules/Assignments.cshtml"); }
    public IActionResult Results() { Hydrate("Parent"); ViewData["Title"] = "Results"; return View("~/Views/Shared/Modules/Results.cshtml"); }
    public IActionResult Timetable() { Hydrate("Parent"); ViewData["Title"] = "Timetable"; return View("~/Views/Shared/Modules/Timetable.cshtml"); }
    public IActionResult Notices() { Hydrate("Parent"); ViewData["Title"] = "Notices"; return View("~/Views/Shared/Modules/Notices.cshtml"); }
    public IActionResult Events() { Hydrate("Parent"); ViewData["Title"] = "Events"; return View("~/Views/Shared/Modules/Events.cshtml"); }
    public IActionResult Messages() { Hydrate("Parent"); ViewData["Title"] = "Messages"; return View("~/Views/Shared/Modules/Messages.cshtml"); }
    public IActionResult Settings() { Hydrate("Parent"); ViewData["Title"] = "Settings"; return View("~/Views/Shared/Modules/Settings.cshtml"); }
    public IActionResult Profile() { Hydrate("Parent"); ViewData["Title"] = "Profile"; return View("~/Views/Shared/Modules/Profile.cshtml"); }
}
