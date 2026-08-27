using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Floors")]
public class SchoolFloorsController : SchoolManageControllerBase
{
    public SchoolFloorsController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
        : base(db, tenant, permissions, audit, userManager)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FloorsManage) is { } deny)
            return deny;
        var items = await Db.Floors.AsNoTracking()
            .Include(f => f.Building)
            .Where(f => f.SchoolId == SchoolId)
            .OrderBy(f => f.Building.Name).ThenBy(f => f.FloorNumber)
            .ToListAsync(ct);
        return SchoolView("Floors/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FloorsManage) is { } deny)
            return deny;
        await LoadBuildingsAsync(ct);
        return SchoolView("Floors/Create", new Floor { SchoolId = SchoolId });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Floor model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FloorsManage) is { } deny)
            return deny;

        var buildingOk = await Db.Buildings.AnyAsync(b => b.Id == model.BuildingId && b.SchoolId == SchoolId, ct);
        if (!buildingOk)
        {
            ModelState.AddModelError(nameof(model.BuildingId), "Select a valid building.");
            await LoadBuildingsAsync(ct);
            return SchoolView("Floors/Create", model);
        }

        model.Id = Guid.NewGuid();
        model.SchoolId = SchoolId;
        model.CreatedByUserId = CurrentUserId;
        model.IsActive = true;
        Db.Floors.Add(model);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "This floor number already exists in the selected building.");
            await LoadBuildingsAsync(ct);
            return SchoolView("Floors/Create", model);
        }

        await Audit.LogAsync("Create", "Floors", SchoolId, "Floor", model.Id.ToString(), model.FloorNumber.ToString(), ct);
        SetFlash("Floor created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FloorsManage) is { } deny)
            return deny;
        var item = await Db.Floors.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        await LoadBuildingsAsync(ct);
        return SchoolView("Floors/Edit", item);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Floor model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FloorsManage) is { } deny)
            return deny;
        var item = await Db.Floors.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();

        var buildingOk = await Db.Buildings.AnyAsync(b => b.Id == model.BuildingId && b.SchoolId == SchoolId, ct);
        if (!buildingOk)
        {
            ModelState.AddModelError(nameof(model.BuildingId), "Select a valid building.");
            await LoadBuildingsAsync(ct);
            return SchoolView("Floors/Edit", item);
        }

        item.BuildingId = model.BuildingId;
        item.FloorNumber = model.FloorNumber;
        item.FloorName = model.FloorName?.Trim();
        item.Description = model.Description?.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.UpdatedByUserId = CurrentUserId;
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "This floor number already exists in the selected building.");
            await LoadBuildingsAsync(ct);
            return SchoolView("Floors/Edit", item);
        }

        SetFlash("Floor updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FloorsManage) is { } deny)
            return deny;
        var item = await Db.Floors.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Floor deactivated.");
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadBuildingsAsync(CancellationToken ct)
    {
        ViewBag.Buildings = new SelectList(
            await Db.Buildings.AsNoTracking()
                .Where(b => b.SchoolId == SchoolId && b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new { b.Id, b.Name })
                .ToListAsync(ct),
            "Id", "Name");
    }
}
