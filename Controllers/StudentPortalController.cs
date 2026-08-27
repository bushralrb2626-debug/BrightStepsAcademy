using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrightStepsAcademy.Data;

namespace BrightStepsAcademy.Controllers;

[Authorize(Roles = AppRoleNames.Student)]
[Route("StudentPortal")]
public class StudentPortalController : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Student Portal";
        return View();
    }
}
