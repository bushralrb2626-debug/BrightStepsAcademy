using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Rooms")]
public class SchoolRoomsController : SchoolManageControllerBase
{
    public SchoolRoomsController(
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
        if (await ForbidUnlessAsync(PermissionCodes.RoomsView) is { } deny)
            return deny;
        var items = await Db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.Floor)
            .Where(r => r.SchoolId == SchoolId)
            .OrderBy(r => r.Building.Name).ThenBy(r => r.Floor.FloorNumber).ThenBy(r => r.RoomNumber)
            .ToListAsync(ct);
        return SchoolView("Rooms/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.RoomsManage) is { } deny)
            return deny;
        await LoadCascadeAsync(null, null, ct);
        return SchoolView("Rooms/Create", new RoomFormVm());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.RoomsManage) is { } deny)
            return deny;

        if (!await ValidateRoomParentAsync(model, ct))
        {
            await LoadCascadeAsync(model.BuildingId, model.FloorId, ct);
            return SchoolView("Rooms/Create", model);
        }

        var room = new Room
        {
            SchoolId = SchoolId,
            BuildingId = model.BuildingId,
            FloorId = model.FloorId,
            RoomNumber = model.RoomNumber?.Trim() ?? "",
            RoomName = model.RoomName?.Trim(),
            RoomType = string.IsNullOrWhiteSpace(model.RoomType) ? nameof(RoomTypeKind.Classroom) : model.RoomType,
            Capacity = model.Capacity,
            Description = model.Description?.Trim(),
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        Db.Rooms.Add(room);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            var buildingName = await Db.Buildings.AsNoTracking()
                .Where(b => b.Id == model.BuildingId && b.SchoolId == SchoolId)
                .Select(b => b.Name).FirstOrDefaultAsync(ct) ?? "the selected building";
            var floorLabel = await Db.Floors.AsNoTracking()
                .Where(f => f.Id == model.FloorId && f.SchoolId == SchoolId)
                .Select(f => f.FloorName ?? ("Floor " + f.FloorNumber))
                .FirstOrDefaultAsync(ct) ?? "the selected floor";
            ModelState.AddModelError(string.Empty,
                $"Room {model.RoomNumber?.Trim()} already exists on {floorLabel} of {buildingName}.");
            await LoadCascadeAsync(model.BuildingId, model.FloorId, ct);
            return SchoolView("Rooms/Create", model);
        }

        await Audit.LogAsync("Create", "Rooms", SchoolId, "Room", room.Id.ToString(), room.RoomNumber, ct);
        SetFlash("Room created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.RoomsManage) is { } deny)
            return deny;
        var room = await Db.Rooms.FirstOrDefaultAsync(r => r.Id == id && r.SchoolId == SchoolId, ct);
        if (room is null) return NotFound();
        await LoadCascadeAsync(room.BuildingId, room.FloorId, ct);
        return SchoolView("Rooms/Edit", new RoomFormVm
        {
            Id = room.Id,
            BuildingId = room.BuildingId,
            FloorId = room.FloorId,
            RoomNumber = room.RoomNumber,
            RoomName = room.RoomName,
            RoomType = room.RoomType,
            Capacity = room.Capacity,
            Description = room.Description
        });
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RoomFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.RoomsManage) is { } deny)
            return deny;
        var room = await Db.Rooms.FirstOrDefaultAsync(r => r.Id == id && r.SchoolId == SchoolId, ct);
        if (room is null) return NotFound();

        if (!await ValidateRoomParentAsync(model, ct))
        {
            await LoadCascadeAsync(model.BuildingId, model.FloorId, ct);
            return SchoolView("Rooms/Edit", model);
        }

        room.BuildingId = model.BuildingId;
        room.FloorId = model.FloorId;
        room.RoomNumber = model.RoomNumber?.Trim() ?? "";
        room.RoomName = model.RoomName?.Trim();
        room.RoomType = model.RoomType;
        room.Capacity = model.Capacity;
        room.Description = model.Description?.Trim();
        room.UpdatedAt = DateTimeOffset.UtcNow;
        room.UpdatedByUserId = CurrentUserId;
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,
                $"Room {model.RoomNumber?.Trim()} already exists on the selected floor of the selected building.");
            await LoadCascadeAsync(model.BuildingId, model.FloorId, ct);
            return SchoolView("Rooms/Edit", model);
        }

        SetFlash("Room updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Details/{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.RoomsView) is { } deny)
            return deny;
        var room = await Db.Rooms.AsNoTracking()
            .Include(r => r.Building)
            .Include(r => r.Floor)
            .Include(r => r.FurnitureItems.Where(f => f.IsActive))
            .FirstOrDefaultAsync(r => r.Id == id && r.SchoolId == SchoolId, ct);
        if (room is null) return NotFound();
        ViewBag.FurnitureForm = new FurnitureFormVm { RoomId = room.Id, Quantity = 1 };
        return SchoolView("Rooms/Details", room);
    }

    [HttpPost("Details/{id:guid}/Furniture")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFurniture(Guid id, FurnitureFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.FurnitureManage) is { } deny)
            return deny;

        var room = await Db.Rooms.FirstOrDefaultAsync(r => r.Id == id && r.SchoolId == SchoolId, ct);
        if (room is null) return NotFound();

        var name = model.Name?.Trim() ?? "";
        var existing = await Db.FurnitureItems
            .FirstOrDefaultAsync(f => f.SchoolId == SchoolId && f.RoomId == id && f.Name == name, ct);

        if (existing is not null)
        {
            existing.Quantity += Math.Max(1, model.Quantity);
            existing.Condition = model.Condition;
            if (!string.IsNullOrWhiteSpace(model.Category))
                existing.Category = model.Category.Trim();
            existing.IsActive = true;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedByUserId = CurrentUserId;
            await Db.SaveChangesAsync(ct);
            SetFlash($"Added quantity to existing \"{name}\".");
        }
        else
        {
            Db.FurnitureItems.Add(new FurnitureItem
            {
                SchoolId = SchoolId,
                RoomId = id,
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
                // Race: same Name inserted concurrently — merge quantity
                var raced = await Db.FurnitureItems
                    .FirstOrDefaultAsync(f => f.SchoolId == SchoolId && f.RoomId == id && f.Name == name, ct);
                if (raced is not null)
                {
                    raced.Quantity += Math.Max(1, model.Quantity);
                    raced.IsActive = true;
                    await Db.SaveChangesAsync(ct);
                    SetFlash($"Merged quantity into existing \"{name}\".");
                    return RedirectToAction(nameof(Details), new { id });
                }
                SetFlash("Could not save furniture (unique constraint).", "error");
                return RedirectToAction(nameof(Details), new { id });
            }
            SetFlash("Furniture added.");
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.RoomsManage) is { } deny)
            return deny;
        var room = await Db.Rooms.FirstOrDefaultAsync(r => r.Id == id && r.SchoolId == SchoolId, ct);
        if (room is null) return NotFound();
        room.IsActive = false;
        room.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Room deactivated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("FloorsByBuilding")]
    public async Task<IActionResult> FloorsByBuilding(Guid buildingId, CancellationToken ct)
    {
        var floors = await Db.Floors.AsNoTracking()
            .Where(f => f.SchoolId == SchoolId && f.BuildingId == buildingId && f.IsActive)
            .OrderBy(f => f.FloorNumber)
            .Select(f => new { f.Id, Label = f.FloorName != null ? $"{f.FloorNumber} — {f.FloorName}" : f.FloorNumber.ToString() })
            .ToListAsync(ct);
        return Json(floors);
    }

    private async Task<bool> ValidateRoomParentAsync(RoomFormVm model, CancellationToken ct)
    {
        var buildingOk = await Db.Buildings.AnyAsync(b => b.Id == model.BuildingId && b.SchoolId == SchoolId, ct);
        var floorOk = await Db.Floors.AnyAsync(f => f.Id == model.FloorId && f.SchoolId == SchoolId && f.BuildingId == model.BuildingId, ct);
        if (!buildingOk) ModelState.AddModelError(nameof(model.BuildingId), "Select a valid building.");
        if (!floorOk) ModelState.AddModelError(nameof(model.FloorId), "Select a valid floor for that building.");
        if (string.IsNullOrWhiteSpace(model.RoomNumber))
            ModelState.AddModelError(nameof(model.RoomNumber), "Room number is required.");
        return ModelState.IsValid;
    }

    private async Task LoadCascadeAsync(Guid? buildingId, Guid? floorId, CancellationToken ct)
    {
        ViewBag.Buildings = new SelectList(
            await Db.Buildings.AsNoTracking().Where(b => b.SchoolId == SchoolId && b.IsActive)
                .OrderBy(b => b.Name).Select(b => new { b.Id, b.Name }).ToListAsync(ct),
            "Id", "Name", buildingId);

        var floorsQ = Db.Floors.AsNoTracking().Where(f => f.SchoolId == SchoolId && f.IsActive);
        if (buildingId.HasValue)
            floorsQ = floorsQ.Where(f => f.BuildingId == buildingId);
        ViewBag.Floors = new SelectList(
            await floorsQ.OrderBy(f => f.FloorNumber)
                .Select(f => new { f.Id, Label = f.FloorName != null ? $"{f.FloorNumber} — {f.FloorName}" : f.FloorNumber.ToString() })
                .ToListAsync(ct),
            "Id", "Label", floorId);

        ViewBag.RoomTypes = new SelectList(Enum.GetNames(typeof(RoomTypeKind)));
    }
}
