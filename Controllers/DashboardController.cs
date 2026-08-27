using BrightStepsAcademy.Data;
using Microsoft.AspNetCore.Mvc;

namespace BrightStepsAcademy.Controllers;

public abstract class DashboardController : Controller
{
    protected readonly ISchoolData Store;

    protected DashboardController(ISchoolData store) => Store = store;

    protected void Hydrate(string roleKey)
    {
        var profile = Store.ProfileFor(roleKey);
        ViewBag.RoleKey = roleKey;
        ViewBag.Profile = profile;
        ViewBag.NavGroups = NavCatalog.For(roleKey);
        ViewBag.Notifications = Store.Notifications;
        ViewBag.Unread = Store.Notifications.Count;
        ViewBag.Messages = 3;
        ViewData["Title"] ??= $"{profile.Role} · Scuola Materna";
        ViewBag.Store = Store;
    }
}
