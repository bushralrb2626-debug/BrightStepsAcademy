using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Buildings")]
public class SchoolBuildingsController : SchoolManageControllerBase
{
    public SchoolBuildingsController(
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
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsView) is { } deny)
            return deny;
        var items = await Db.Buildings.AsNoTracking()
            .Where(b => b.SchoolId == SchoolId)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
        return SchoolView("Buildings/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsManage) is { } deny)
            return deny;
        return SchoolView("Buildings/Create", new Building { SchoolId = SchoolId });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Building model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsManage) is { } deny)
            return deny;

        model.Id = Guid.NewGuid();
        model.SchoolId = SchoolId;
        model.Name = model.Name?.Trim() ?? "";
        model.CreatedByUserId = CurrentUserId;
        model.IsActive = true;
        Db.Buildings.Add(model);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A building with this name already exists.");
            return SchoolView("Buildings/Create", model);
        }

        await Audit.LogAsync("Create", "Buildings", SchoolId, "Building", model.Id.ToString(), model.Name, ct);
        SetFlash("Building created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsManage) is { } deny)
            return deny;
        var item = await Db.Buildings.FirstOrDefaultAsync(b => b.Id == id && b.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        return SchoolView("Buildings/Edit", item);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Building model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsManage) is { } deny)
            return deny;
        var item = await Db.Buildings.FirstOrDefaultAsync(b => b.Id == id && b.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.Name = model.Name?.Trim() ?? "";
        item.BuildingNumber = model.BuildingNumber?.Trim();
        item.Description = model.Description?.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.UpdatedByUserId = CurrentUserId;
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "A building with this name already exists.");
            return SchoolView("Buildings/Edit", item);
        }
        await Audit.LogAsync("Update", "Buildings", SchoolId, "Building", item.Id.ToString(), item.Name, ct);
        SetFlash("Building updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsManage) is { } deny)
            return deny;
        var item = await Db.Buildings.FirstOrDefaultAsync(b => b.Id == id && b.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        item.UpdatedByUserId = CurrentUserId;
        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Deactivate", "Buildings", SchoolId, "Building", item.Id.ToString(), item.Name, ct);
        SetFlash("Building deactivated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Activate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.BuildingsManage) is { } deny)
            return deny;
        var item = await Db.Buildings.FirstOrDefaultAsync(b => b.Id == id && b.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = true;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Building activated.");
        return RedirectToAction(nameof(Index));
    }
}
