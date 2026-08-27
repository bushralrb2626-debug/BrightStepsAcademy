using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public class HeadmasterController(ISchoolData store) : DashboardController(store)
{
    public IActionResult Index() { Hydrate("Headmaster"); return View(); }
    public IActionResult Overview() { Hydrate("Headmaster"); ViewData["Title"] = "School Overview"; return View(); }
    public IActionResult Teachers() { Hydrate("Headmaster"); ViewData["Title"] = "Teachers"; return View(); }
    public IActionResult Students() { Hydrate("Headmaster"); ViewData["Title"] = "Students"; return View("~/Views/Admin/Students.cshtml"); }
    public IActionResult Classes() { Hydrate("Headmaster"); ViewData["Title"] = "Classes"; return View("~/Views/Admin/Classes.cshtml"); }
    public IActionResult Attendance() { Hydrate("Headmaster"); ViewData["Title"] = "Attendance"; return View("~/Views/Shared/Modules/Attendance.cshtml"); }
    public IActionResult Performance() { Hydrate("Headmaster"); ViewData["Title"] = "Performance"; return View(); }
    public IActionResult Assignments() { Hydrate("Headmaster"); ViewData["Title"] = "Assignments"; return View(); }
    public IActionResult Timetable() { Hydrate("Headmaster"); ViewData["Title"] = "Timetable"; return View("~/Views/Shared/Modules/Timetable.cshtml"); }
    public IActionResult Notices() { Hydrate("Headmaster"); ViewData["Title"] = "Notices"; return View("~/Views/Shared/Modules/Notices.cshtml"); }
    public IActionResult Approvals() { Hydrate("Headmaster"); ViewData["Title"] = "Approvals"; return View(); }
    public IActionResult Reports() { Hydrate("Headmaster"); ViewData["Title"] = "Reports"; return View("~/Views/Shared/Modules/Reports.cshtml"); }
    public IActionResult Settings() { Hydrate("Headmaster"); ViewData["Title"] = "Settings"; return View("~/Views/Shared/Modules/Settings.cshtml"); }
    public IActionResult Profile() { Hydrate("Headmaster"); ViewData["Title"] = "Profile"; return View("~/Views/Shared/Modules/Profile.cshtml"); }
    public IActionResult Messages() { Hydrate("Headmaster"); ViewData["Title"] = "Messages"; return View("~/Views/Shared/Modules/Messages.cshtml"); }
}
