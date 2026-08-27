using BrightStepsAcademy.Data;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Search")]
public class SchoolSearchController : SchoolManageControllerBase
{
    public SchoolSearchController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
        : base(db, tenant, permissions, audit, userManager)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, CancellationToken ct)
    {
        var vm = new SearchResultVm { Query = q?.Trim() ?? "" };
        if (string.IsNullOrWhiteSpace(vm.Query) || vm.Query.Length < 2)
            return SchoolView("Search/Index", vm);

        var term = vm.Query.ToLowerInvariant();

        if (await CanAsync(PermissionCodes.BuildingsView))
        {
            vm.Hits.AddRange(await Db.Buildings.AsNoTracking()
                .Where(b => b.SchoolId == SchoolId && b.Name.ToLower().Contains(term))
                .Take(10)
                .Select(b => new SearchHitVm
                {
                    Entity = "Building",
                    Title = b.Name,
                    Subtitle = b.BuildingNumber,
                    Url = "/Manage/School/Buildings"
                }).ToListAsync(ct));
        }

        if (await CanAsync(PermissionCodes.RoomsView))
        {
            vm.Hits.AddRange(await Db.Rooms.AsNoTracking()
                .Where(r => r.SchoolId == SchoolId &&
                            (r.RoomNumber.ToLower().Contains(term) ||
                             (r.RoomName != null && r.RoomName.ToLower().Contains(term))))
                .Take(10)
                .Select(r => new SearchHitVm
                {
                    Entity = "Room",
                    Title = r.RoomNumber + (r.RoomName != null ? " — " + r.RoomName : ""),
                    Subtitle = r.RoomType,
                    Url = "/Manage/School/Rooms/Details/" + r.Id
                }).ToListAsync(ct));
        }

        if (await CanAsync(PermissionCodes.FurnitureManage))
        {
            vm.Hits.AddRange(await Db.FurnitureItems.AsNoTracking()
                .Where(f => f.SchoolId == SchoolId && f.Name.ToLower().Contains(term))
                .Take(10)
                .Select(f => new SearchHitVm
                {
                    Entity = "Furniture",
                    Title = f.Name,
                    Subtitle = "Qty " + f.Quantity,
                    Url = "/Manage/School/Furniture"
                }).ToListAsync(ct));
        }

        if (await CanAsync(PermissionCodes.StaffView))
        {
            vm.Hits.AddRange(await Db.StaffMembers.AsNoTracking()
                .Where(s => s.SchoolId == SchoolId &&
                            (s.FullName.ToLower().Contains(term) || s.StaffCode.ToLower().Contains(term)))
                .Take(10)
                .Select(s => new SearchHitVm
                {
                    Entity = "Staff",
                    Title = s.FullName,
                    Subtitle = s.StaffCode,
                    Url = "/Manage/School/Staff"
                }).ToListAsync(ct));
        }

        if (await CanAsync(PermissionCodes.StudentsView))
        {
            vm.Hits.AddRange(await Db.StudentRecords.AsNoTracking()
                .Where(s => s.SchoolId == SchoolId &&
                            (s.FullName.ToLower().Contains(term) || s.StudentCode.ToLower().Contains(term)))
                .Take(10)
                .Select(s => new SearchHitVm
                {
                    Entity = "Student",
                    Title = s.FullName,
                    Subtitle = s.StudentCode,
                    Url = "/Manage/School/Students"
                }).ToListAsync(ct));
        }

        return SchoolView("Search/Index", vm);
    }
}
