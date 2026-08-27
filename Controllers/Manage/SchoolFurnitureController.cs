using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Furniture")]
public class SchoolFurnitureController : SchoolManageControllerBase
{
    public SchoolFurnitureController(
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
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;
        var items = await Db.FurnitureItems.AsNoTracking()
            .Include(f => f.Room).ThenInclude(r => r.Building)
            .Where(f => f.SchoolId == SchoolId)
            .OrderBy(f => f.Room.RoomNumber).ThenBy(f => f.Name)
            .ToListAsync(ct);
        return SchoolView("Furniture/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;
        await LoadRoomsAsync(ct);
        return SchoolView("Furniture/Create", new FurnitureFormVm { Quantity = 1 });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FurnitureFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;

        var roomOk = await Db.Rooms.AnyAsync(r => r.Id == model.RoomId && r.SchoolId == SchoolId, ct);
        if (!roomOk)
        {
            ModelState.AddModelError(nameof(model.RoomId), "Select a valid room.");
            await LoadRoomsAsync(ct);
            return SchoolView("Furniture/Create", model);
        }

        var name = model.Name?.Trim() ?? "";
        var existing = await Db.FurnitureItems
            .FirstOrDefaultAsync(f => f.SchoolId == SchoolId && f.RoomId == model.RoomId && f.Name == name, ct);

        if (existing is not null)
        {
            existing.Quantity += Math.Max(1, model.Quantity);
            existing.Condition = model.Condition;
            existing.Category = model.Category?.Trim() ?? existing.Category;
            existing.IsActive = true;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await Db.SaveChangesAsync(ct);
            SetFlash($"Merged quantity into existing \"{name}\" in that room.");
            return RedirectToAction(nameof(Index));
        }

        Db.FurnitureItems.Add(new FurnitureItem
        {
            SchoolId = SchoolId,
            RoomId = model.RoomId,
            Category = model.Category?.Trim() ?? "General",
            Name = name,
            Quantity = Math.Max(1, model.Quantity),
            Condition = model.Condition,
            Description = model.Description?.Trim(),
            PurchaseDate = model.PurchaseDate,
            CreatedByUserId = CurrentUserId,
            IsActive = true
        });
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            SetFlash("Could not create furniture due to a uniqueness conflict. Quantity may have been merged.", "error");
            return RedirectToAction(nameof(Index));
        }

        SetFlash("Furniture created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;
        var item = await Db.FurnitureItems.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        await LoadRoomsAsync(ct);
        return SchoolView("Furniture/Edit", new FurnitureFormVm
        {
            Id = item.Id,
            RoomId = item.RoomId,
            Category = item.Category,
            Name = item.Name,
            Quantity = item.Quantity,
            Condition = item.Condition,
            Description = item.Description,
            PurchaseDate = item.PurchaseDate
        });
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, FurnitureFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;
        var item = await Db.FurnitureItems.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();

        item.Category = model.Category?.Trim() ?? "";
        item.Name = model.Name?.Trim() ?? "";
        item.Quantity = Math.Max(0, model.Quantity);
        item.Condition = model.Condition;
        item.Description = model.Description?.Trim();
        item.PurchaseDate = model.PurchaseDate;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Another furniture item with this name already exists in the room.");
            await LoadRoomsAsync(ct);
            return SchoolView("Furniture/Edit", model);
        }

        SetFlash("Furniture updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;
        var item = await Db.FurnitureItems.FirstOrDefaultAsync(f => f.Id == id && f.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Furniture deactivated.");
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRoomsAsync(CancellationToken ct)
    {
        ViewBag.Rooms = new SelectList(
            await Db.Rooms.AsNoTracking()
                .Include(r => r.Building)
                .Where(r => r.SchoolId == SchoolId && r.IsActive)
                .OrderBy(r => r.Building.Name).ThenBy(r => r.RoomNumber)
                .Select(r => new { r.Id, Label = $"{r.Building.Name} · {r.RoomNumber}" })
                .ToListAsync(ct),
            "Id", "Label");
        ViewBag.Conditions = new SelectList(Enum.GetValues<FurnitureCondition>());
    }
}
