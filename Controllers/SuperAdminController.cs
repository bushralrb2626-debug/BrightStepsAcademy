using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

/// <summary>Legacy mock portal — use /Manage/SuperAdmin for the real SaaS panel.</summary>
[Route("Legacy/SuperAdmin")]
public class LegacySuperAdminController(ISchoolData store) : DashboardController(store)
{
    public IActionResult Index() { Hydrate("SuperAdmin"); return View("~/Views/SuperAdmin/Index.cshtml"); }
    public IActionResult Users() { Hydrate("SuperAdmin"); ViewData["Title"] = "User Management"; return View("~/Views/SuperAdmin/Users.cshtml"); }
    public IActionResult Schools() { Hydrate("SuperAdmin"); ViewData["Title"] = "Schools"; return View("~/Views/SuperAdmin/Schools.cshtml"); }
    public IActionResult Messages() { Hydrate("SuperAdmin"); ViewData["Title"] = "Messages"; return View("~/Views/Shared/Modules/Messages.cshtml"); }
    public IActionResult Reports() { Hydrate("SuperAdmin"); ViewData["Title"] = "Reports"; return View("~/Views/Shared/Modules/Reports.cshtml"); }
    public IActionResult Settings() { Hydrate("SuperAdmin"); ViewData["Title"] = "Settings"; return View("~/Views/Shared/Modules/Settings.cshtml"); }
    public IActionResult Profile() { Hydrate("SuperAdmin"); ViewData["Title"] = "Profile"; return View("~/Views/Shared/Modules/Profile.cshtml"); }
}
