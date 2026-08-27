using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public class StudentController(ISchoolData store) : DashboardController(store)
{
    public IActionResult Index() { Hydrate("Student"); return View(); }
    public IActionResult Profile() { Hydrate("Student"); ViewData["Title"] = "My Profile"; return View("~/Views/Shared/Modules/Profile.cshtml"); }
    public IActionResult Classes() { Hydrate("Student"); ViewData["Title"] = "My Classes"; return View(); }
    public IActionResult Homework() { Hydrate("Student"); ViewData["Title"] = "Homework"; return View(); }
    public IActionResult Assignments() { Hydrate("Student"); ViewData["Title"] = "Assignments"; return View(); }
    public IActionResult Attendance() { Hydrate("Student"); ViewData["Title"] = "Attendance"; return View(); }
    public IActionResult Results() { Hydrate("Student"); ViewData["Title"] = "Results"; return View("~/Views/Shared/Modules/Results.cshtml"); }
    public IActionResult Timetable() { Hydrate("Student"); ViewData["Title"] = "Timetable"; return View("~/Views/Shared/Modules/Timetable.cshtml"); }
    public IActionResult Achievements() { Hydrate("Student"); ViewData["Title"] = "Achievements"; return View(); }
    public IActionResult Notices() { Hydrate("Student"); ViewData["Title"] = "Notices"; return View("~/Views/Shared/Modules/Notices.cshtml"); }
    public IActionResult Events() { Hydrate("Student"); ViewData["Title"] = "Events"; return View("~/Views/Shared/Modules/Events.cshtml"); }
    public IActionResult Settings() { Hydrate("Student"); ViewData["Title"] = "Settings"; return View("~/Views/Shared/Modules/Settings.cshtml"); }
    public IActionResult Messages() { Hydrate("Student"); ViewData["Title"] = "Messages"; return View("~/Views/Shared/Modules/Messages.cshtml"); }
}
