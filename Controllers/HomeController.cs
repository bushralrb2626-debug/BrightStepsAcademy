using System.Diagnostics;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Models;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public class HomeController : Controller
{
    private readonly ISchoolData _store;
    private readonly IWebsiteContentService _website;

    public HomeController(ISchoolData store, IWebsiteContentService website)
    {
        _store = store;
        _website = website;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        PublicWebsiteViewModel? site = null;
        if (DatabaseStartup.IsReady)
        {
            try
            {
                site = await _website.GetPublicWebsiteAsync(cancellationToken: ct);
            }
            catch
            {
                site = null;
            }
        }

        ViewData["Title"] = "Home";
        ViewData["SchoolName"] = site?.ShortName ?? site?.Name ?? _store.Schools.FirstOrDefault()?.Name ?? "BrightSteps Academy";
        ViewData["Tagline"] = site?.Tagline ?? "Learn. Explore. Grow.";
        ViewData["LogoPath"] = string.IsNullOrWhiteSpace(site?.LogoPath) ? null : site.LogoPath;
        ViewBag.Store = _store;
        ViewBag.Site = site;
        return View();
    }

    public IActionResult About() => Public("About Our School", "About");
    public IActionResult Programs() => Public("Our Programs", "Programs");
    public IActionResult Facilities() => Public("Our Facilities", "Facilities");
    public IActionResult Teachers() => Public("Meet Our Teachers", "Teachers");
    public IActionResult Activities() => Public("School Activities", "Activities");
    public IActionResult Events() => Public("Upcoming Events", "Events");
    public IActionResult Achievements() => Public("Achievements", "Achievements");
    public IActionResult Gallery() => Public("School Gallery", "Gallery");
    public IActionResult Notices() => Public("Latest Notices", "Notices");
    public IActionResult Contact() => Public("Visit Us", "Contact");

    public IActionResult TeacherProfile(string id)
    {
        var teacher = _store.Teachers.FirstOrDefault(t => t.Id == id) ?? _store.Teachers[0];
        ViewData["Title"] = teacher.Name;
        ViewBag.Store = _store;
        return View(teacher);
    }

    public IActionResult Facility(string id)
    {
        var facility = _store.Facilities.FirstOrDefault(f => f.Id == id) ?? _store.Facilities[0];
        ViewData["Title"] = facility.Name;
        ViewBag.Store = _store;
        return View(facility);
    }

    public IActionResult Event(string id)
    {
        var item = _store.Events.FirstOrDefault(e => e.Id == id) ?? _store.Events[0];
        ViewData["Title"] = item.Title;
        ViewBag.Store = _store;
        return View(item);
    }

    public IActionResult Program(string name)
    {
        var item = _store.Programs.FirstOrDefault(p => p.Title == name) ?? _store.Programs[0];
        ViewData["Title"] = item.Title;
        ViewBag.Store = _store;
        return View(item);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    private IActionResult Public(string title, string view)
    {
        ViewData["Title"] = title;
        ViewBag.Store = _store;
        return View(view);
    }
}
