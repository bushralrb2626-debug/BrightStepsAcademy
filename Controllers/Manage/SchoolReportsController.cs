using BrightStepsAcademy.Data;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Reports")]
public class SchoolReportsController : SchoolManageControllerBase
{
    public SchoolReportsController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
        : base(db, tenant, permissions, audit, userManager)
    {
    }

    [HttpGet("")]
    [HttpGet("Overview")]
    public async Task<IActionResult> Overview(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.ReportsView) is { } deny)
            return deny;

        ViewBag.Buildings = await Db.Buildings.CountAsync(b => b.SchoolId == SchoolId && b.IsActive, ct);
        ViewBag.Floors = await Db.Floors.CountAsync(f => f.SchoolId == SchoolId && f.IsActive, ct);
        ViewBag.Rooms = await Db.Rooms.CountAsync(r => r.SchoolId == SchoolId && r.IsActive, ct);
        ViewBag.Furniture = await Db.FurnitureItems.CountAsync(f => f.SchoolId == SchoolId && f.IsActive, ct);
        ViewBag.Staff = await Db.StaffMembers.CountAsync(s => s.SchoolId == SchoolId && s.IsActive, ct);
        ViewBag.Students = await Db.StudentRecords.CountAsync(s => s.SchoolId == SchoolId && s.IsActive, ct);
        return SchoolView("Reports/Overview");
    }

    [HttpGet("Rooms")]
    public async Task<IActionResult> Rooms(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.ReportsView) is { } deny)
            return deny;
        var rows = await Db.Rooms.AsNoTracking()
            .Include(r => r.Building).Include(r => r.Floor)
            .Where(r => r.SchoolId == SchoolId)
            .OrderBy(r => r.Building.Name).ThenBy(r => r.RoomNumber)
            .ToListAsync(ct);
        return SchoolView("Reports/Rooms", rows);
    }

    [HttpGet("Furniture")]
    public async Task<IActionResult> Furniture(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.ReportsView) is { } deny)
            return deny;
        var rows = await Db.FurnitureItems.AsNoTracking()
            .Include(f => f.Room)
            .Where(f => f.SchoolId == SchoolId)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);
        return SchoolView("Reports/Furniture", rows);
    }

    [HttpGet("Staff")]
    public async Task<IActionResult> Staff(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.ReportsView) is { } deny)
            return deny;
        var rows = await Db.StaffMembers.AsNoTracking()
            .Include(s => s.StaffCategory)
            .Where(s => s.SchoolId == SchoolId)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);
        return SchoolView("Reports/Staff", rows);
    }

    [HttpGet("Students")]
    public async Task<IActionResult> Students(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.ReportsView) is { } deny)
            return deny;
        var rows = await Db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId)
            .OrderBy(s => s.ClassName).ThenBy(s => s.FullName)
            .ToListAsync(ct);
        return SchoolView("Reports/Students", rows);
    }
}
