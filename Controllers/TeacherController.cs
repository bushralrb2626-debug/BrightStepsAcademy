using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public class TeacherController(ISchoolData store) : DashboardController(store)
{
    public IActionResult Index() { Hydrate("Teacher"); return View(); }
    public IActionResult Classes() { Hydrate("Teacher"); ViewData["Title"] = "My Classes"; return View(); }
    public IActionResult Students() { Hydrate("Teacher"); ViewData["Title"] = "Students"; return View(); }
    public IActionResult Attendance() { Hydrate("Teacher"); ViewData["Title"] = "Attendance"; return View("~/Views/Shared/Modules/Attendance.cshtml"); }
    public IActionResult Assignments() { Hydrate("Teacher"); ViewData["Title"] = "Assignments"; return View("~/Views/Shared/Modules/Assignments.cshtml"); }
    public IActionResult Results() { Hydrate("Teacher"); ViewData["Title"] = "Results"; return View("~/Views/Shared/Modules/Results.cshtml"); }
    public IActionResult Timetable() { Hydrate("Teacher"); ViewData["Title"] = "Timetable"; return View("~/Views/Shared/Modules/Timetable.cshtml"); }
    public IActionResult Notices() { Hydrate("Teacher"); ViewData["Title"] = "Notices"; return View("~/Views/Shared/Modules/Notices.cshtml"); }
    public IActionResult Activities() { Hydrate("Teacher"); ViewData["Title"] = "Activities"; return View(); }
    public IActionResult Messages() { Hydrate("Teacher"); ViewData["Title"] = "Messages"; return View("~/Views/Shared/Modules/Messages.cshtml"); }
    public IActionResult Settings() { Hydrate("Teacher"); ViewData["Title"] = "Settings"; return View("~/Views/Shared/Modules/Settings.cshtml"); }
    public IActionResult Profile() { Hydrate("Teacher"); ViewData["Title"] = "Profile"; return View("~/Views/Shared/Modules/Profile.cshtml"); }
}
