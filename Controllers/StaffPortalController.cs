using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrightStepsAcademy.Data;

namespace BrightStepsAcademy.Controllers;

[Authorize(Roles = $"{AppRoleNames.Staff},{AppRoleNames.SchoolAdmin},{AppRoleNames.CustomAdmin}")]
[Route("StaffPortal")]
public class StaffPortalController : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Staff Portal";
        return View();
    }
}
