using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Timetable")]
public class SchoolTimetableController : SchoolManageControllerBase
{
    public SchoolTimetableController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager)
        : base(db, tenant, permissions, audit, userManager)
    {
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? classId, Guid? sectionId, DayOfWeek? day, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TimetableView, PermissionCodes.TimetableClassManage) is { } deny)
            return deny;

        await SchoolBootstrap.EnsureSchoolBootstrappedAsync(Db, SchoolId, ct);

        var filter = new TimetableFilterVm
        {
            SchoolClassId = classId,
            SchoolSectionId = sectionId,
            DayOfWeek = day
        };
        await LoadFilterOptionsAsync(filter, ct);
        ViewBag.Filter = filter;

        var query = Db.ClassTimetableSlots.AsNoTracking().Where(t => t.SchoolId == SchoolId);
        if (classId.HasValue) query = query.Where(t => t.SchoolClassId == classId);
        if (sectionId.HasValue) query = query.Where(t => t.SchoolSectionId == sectionId);
        if (day.HasValue) query = query.Where(t => t.DayOfWeek == day);

        var items = await query
            .Join(Db.SchoolClasses.AsNoTracking(), t => t.SchoolClassId, c => c.Id, (t, c) => new { t, c })
            .Join(Db.SchoolSections.AsNoTracking(), x => x.t.SchoolSectionId, s => s.Id, (x, s) => new { x.t, x.c, s })
            .Join(Db.Subjects.AsNoTracking(), x => x.t.SubjectId, sub => sub.Id, (x, sub) => new { x.t, x.c, x.s, sub })
            .GroupJoin(Db.StaffMembers.AsNoTracking(), x => x.t.StaffMemberId, st => st.Id, (x, staff) => new { x.t, x.c, x.s, x.sub, staff })
            .SelectMany(x => x.staff.DefaultIfEmpty(), (x, st) => new { x.t, x.c, x.s, x.sub, st })
            .GroupJoin(Db.Rooms.AsNoTracking(), x => x.t.RoomId, r => r.Id, (x, rooms) => new { x.t, x.c, x.s, x.sub, x.st, rooms })
            .SelectMany(x => x.rooms.DefaultIfEmpty(), (x, room) => new TimetableSlotListVm
            {
                Id = x.t.Id,
                ClassName = x.c.Name,
                SectionName = x.s.Name,
                DayOfWeek = x.t.DayOfWeek,
                PeriodOrder = x.t.PeriodOrder,
                PeriodLabel = x.t.PeriodLabel,
                StartTime = x.t.StartTime,
                EndTime = x.t.EndTime,
                SubjectName = x.sub.Name,
                TeacherName = x.st != null ? x.st.FullName : null,
                RoomName = room != null ? (room.RoomName ?? room.RoomNumber) : null,
                Status = x.t.Status
            })
            .OrderBy(x => x.ClassName).ThenBy(x => x.SectionName).ThenBy(x => x.DayOfWeek).ThenBy(x => x.PeriodOrder)
            .ToListAsync(ct);

        ViewData["Title"] = "Timetable";
        return SchoolView("Timetable/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TimetableCreate, PermissionCodes.TimetableClassManage) is { } deny)
            return deny;
        var model = new TimetableSlotFormVm();
        await LoadFormOptionsAsync(model, ct);
        ViewData["Title"] = "Add Timetable Slot";
        return SchoolView("Timetable/Create", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TimetableSlotFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TimetableCreate, PermissionCodes.TimetableClassManage) is { } deny)
            return deny;
        ValidateSlot(model);
        NormalizeOptionalIds(model);
        await ValidateSlotReferencesAsync(model, ct);
        if (!ModelState.IsValid)
        {
            await LoadFormOptionsAsync(model, ct);
            ViewData["Title"] = "Add Timetable Slot";
            return SchoolView("Timetable/Create", model);
        }

        Db.ClassTimetableSlots.Add(new ClassTimetableSlot
        {
            SchoolId = SchoolId,
            SchoolClassId = model.SchoolClassId,
            SchoolSectionId = model.SchoolSectionId,
            DayOfWeek = model.DayOfWeek,
            PeriodOrder = model.PeriodOrder,
            PeriodLabel = model.PeriodLabel?.Trim(),
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            SubjectId = model.SubjectId,
            StaffMemberId = model.StaffMemberId,
            RoomId = model.RoomId,
            Status = model.Status,
            CreatedByUserId = CurrentUserId
        });
        await Db.SaveChangesAsync(ct);
        await Audit.LogAsync("Create", "Timetable", SchoolId, "ClassTimetableSlot", null, model.PeriodLabel, ct);
        SetFlash("Timetable slot added.");
        return RedirectToAction(nameof(Index), new { classId = model.SchoolClassId, sectionId = model.SchoolSectionId });
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TimetableEdit, PermissionCodes.TimetableClassManage) is { } deny)
            return deny;
        var slot = await Db.ClassTimetableSlots.FirstOrDefaultAsync(t => t.Id == id && t.SchoolId == SchoolId, ct);
        if (slot is null) return NotFound();

        var model = new TimetableSlotFormVm
        {
            Id = slot.Id,
            SchoolClassId = slot.SchoolClassId,
            SchoolSectionId = slot.SchoolSectionId,
            DayOfWeek = slot.DayOfWeek,
            PeriodOrder = slot.PeriodOrder,
            PeriodLabel = slot.PeriodLabel,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            SubjectId = slot.SubjectId,
            StaffMemberId = slot.StaffMemberId,
            RoomId = slot.RoomId,
            Status = slot.Status
        };
        await LoadFormOptionsAsync(model, ct);
        ViewData["Title"] = "Edit Timetable Slot";
        return SchoolView("Timetable/Edit", model);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, TimetableSlotFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TimetableEdit, PermissionCodes.TimetableClassManage) is { } deny)
            return deny;
        var slot = await Db.ClassTimetableSlots.FirstOrDefaultAsync(t => t.Id == id && t.SchoolId == SchoolId, ct);
        if (slot is null) return NotFound();

        ValidateSlot(model);
        NormalizeOptionalIds(model);
        await ValidateSlotReferencesAsync(model, ct);
        if (!ModelState.IsValid)
        {
            await LoadFormOptionsAsync(model, ct);
            ViewData["Title"] = "Edit Timetable Slot";
            return SchoolView("Timetable/Edit", model);
        }

        slot.SchoolClassId = model.SchoolClassId;
        slot.SchoolSectionId = model.SchoolSectionId;
        slot.DayOfWeek = model.DayOfWeek;
        slot.PeriodOrder = model.PeriodOrder;
        slot.PeriodLabel = model.PeriodLabel?.Trim();
        slot.StartTime = model.StartTime;
        slot.EndTime = model.EndTime;
        slot.SubjectId = model.SubjectId;
        slot.StaffMemberId = model.StaffMemberId;
        slot.RoomId = model.RoomId;
        slot.Status = model.Status;
        slot.UpdatedAt = DateTimeOffset.UtcNow;
        slot.UpdatedByUserId = CurrentUserId;
        await Db.SaveChangesAsync(ct);
        SetFlash("Timetable slot updated.");
        return RedirectToAction(nameof(Index), new { classId = model.SchoolClassId, sectionId = model.SchoolSectionId });
    }

    [HttpPost("Delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAnyAsync(PermissionCodes.TimetableDelete, PermissionCodes.TimetableClassManage) is { } deny)
            return deny;
        var slot = await Db.ClassTimetableSlots.FirstOrDefaultAsync(t => t.Id == id && t.SchoolId == SchoolId, ct);
        if (slot is null) return NotFound();
        Db.ClassTimetableSlots.Remove(slot);
        await Db.SaveChangesAsync(ct);
        SetFlash("Timetable slot deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("TeachersByClassSection")]
    public async Task<IActionResult> TeachersByClassSection(Guid? classId, Guid? sectionId, CancellationToken ct)
    {
        var teachersQ = Db.StaffMembers.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive);

        if (classId.HasValue && sectionId.HasValue)
        {
            var assignedIds = await Db.TeacherAssignments.AsNoTracking()
                .Where(a => a.SchoolId == SchoolId && a.IsActive
                            && a.SchoolClassId == classId && a.SchoolSectionId == sectionId)
                .Select(a => a.StaffMemberId)
                .Distinct()
                .ToListAsync(ct);
            if (assignedIds.Count > 0)
                teachersQ = teachersQ.Where(s => assignedIds.Contains(s.Id));
        }

        var teachers = await teachersQ
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName, s.StaffCode })
            .ToListAsync(ct);

        if (teachers.Count == 0 && classId.HasValue)
        {
            teachers = await Db.StaffMembers.AsNoTracking()
                .Where(s => s.SchoolId == SchoolId && s.IsActive)
                .OrderBy(s => s.FullName)
                .Select(s => new { s.Id, s.FullName, s.StaffCode })
                .ToListAsync(ct);
        }

        return Json(teachers.Select(t => new
        {
            t.Id,
            Label = t.FullName + (string.IsNullOrEmpty(t.StaffCode) ? "" : " (" + t.StaffCode + ")")
        }));
    }

    [HttpGet("RoomsForTimetable")]
    public async Task<IActionResult> RoomsForTimetable(Guid? classId, Guid? sectionId, CancellationToken ct)
    {
        var rooms = await Db.Rooms.AsNoTracking()
            .Where(r => r.SchoolId == SchoolId && r.IsActive)
            .Join(
                Db.Buildings.AsNoTracking().Where(b => b.SchoolId == SchoolId),
                r => r.BuildingId,
                b => b.Id,
                (r, b) => new { r.Id, r.SchoolClassId, r.SchoolSectionId, b.Name, r.RoomNumber, r.RoomName })
            .OrderBy(x => x.Name).ThenBy(x => x.RoomNumber)
            .ToListAsync(ct);

        var labeled = rooms.Select(x => new
        {
            x.Id,
            x.SchoolClassId,
            x.SchoolSectionId,
            Label = x.Name + " · " + x.RoomNumber + (string.IsNullOrWhiteSpace(x.RoomName) ? "" : " — " + x.RoomName)
        }).ToList();

        var filtered = labeled.Where(r =>
            (!classId.HasValue || r.SchoolClassId is null || r.SchoolClassId == classId)
            && (!sectionId.HasValue || r.SchoolSectionId is null || r.SchoolSectionId == sectionId))
            .Select(r => new { r.Id, r.Label })
            .ToList();

        return Json(filtered.Count > 0 ? filtered : labeled.Select(r => new { r.Id, r.Label }));
    }

    private void ValidateSlot(TimetableSlotFormVm model)
    {
        if (model.SchoolClassId == Guid.Empty)
            ModelState.AddModelError(nameof(model.SchoolClassId), "Class is required.");
        if (model.SchoolSectionId == Guid.Empty)
            ModelState.AddModelError(nameof(model.SchoolSectionId), "Section is required.");
        if (model.SubjectId == Guid.Empty)
            ModelState.AddModelError(nameof(model.SubjectId), "Subject is required.");
        if (model.EndTime <= model.StartTime)
            ModelState.AddModelError(nameof(model.EndTime), "End time must be after start time.");
    }

    private async Task ValidateSlotReferencesAsync(TimetableSlotFormVm model, CancellationToken ct)
    {
        if (model.StaffMemberId.HasValue && model.StaffMemberId != Guid.Empty)
        {
            var teacherOk = await Db.StaffMembers.AnyAsync(s => s.Id == model.StaffMemberId && s.SchoolId == SchoolId && s.IsActive, ct);
            if (!teacherOk)
                ModelState.AddModelError(nameof(model.StaffMemberId), "Select a valid teacher.");
        }

        if (model.RoomId.HasValue && model.RoomId != Guid.Empty)
        {
            var roomOk = await Db.Rooms.AnyAsync(r => r.Id == model.RoomId && r.SchoolId == SchoolId && r.IsActive, ct);
            if (!roomOk)
                ModelState.AddModelError(nameof(model.RoomId), "Select a valid room.");
        }
    }

    private static void NormalizeOptionalIds(TimetableSlotFormVm model)
    {
        if (model.StaffMemberId == Guid.Empty) model.StaffMemberId = null;
        if (model.RoomId == Guid.Empty) model.RoomId = null;
    }

    private async Task LoadFilterOptionsAsync(TimetableFilterVm filter, CancellationToken ct)
    {
        var classes = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        filter.ClassOptions = classes.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == filter.SchoolClassId)).ToList();
        filter.ClassOptions.Insert(0, new SelectListItem("All classes", ""));

        var sections = await Db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive
                        && (!filter.SchoolClassId.HasValue || s.SchoolClassId == filter.SchoolClassId))
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);
        filter.SectionOptions = sections.Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == filter.SchoolSectionId)).ToList();
        filter.SectionOptions.Insert(0, new SelectListItem("All sections", ""));
    }

    private async Task LoadFormOptionsAsync(TimetableSlotFormVm model, CancellationToken ct)
    {
        var classes = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        model.ClassOptions = classes.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.SchoolClassId)).ToList();

        var sections = await Db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive
                        && (model.SchoolClassId == Guid.Empty || s.SchoolClassId == model.SchoolClassId))
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);
        model.SectionOptions = sections.Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == model.SchoolSectionId)).ToList();

        var subjects = await Db.Subjects.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);
        model.SubjectOptions = subjects.Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == model.SubjectId)).ToList();

        var teachers = await LoadTeacherOptionsAsync(model.SchoolClassId, model.SchoolSectionId, model.StaffMemberId, ct);
        model.TeacherOptions = teachers;

        var rooms = await Db.Rooms.AsNoTracking()
            .Where(r => r.SchoolId == SchoolId && r.IsActive)
            .Join(
                Db.Buildings.AsNoTracking().Where(b => b.SchoolId == SchoolId),
                r => r.BuildingId,
                b => b.Id,
                (r, b) => new { r, b })
            .OrderBy(x => x.b.Name).ThenBy(x => x.r.RoomNumber)
            .ToListAsync(ct);

        var roomItems = rooms
            .Where(x =>
                (!model.SchoolClassId.Equals(Guid.Empty) && x.r.SchoolClassId is not null && x.r.SchoolClassId != model.SchoolClassId)
                    ? false
                    : (!model.SchoolSectionId.Equals(Guid.Empty) && x.r.SchoolSectionId is not null && x.r.SchoolSectionId != model.SchoolSectionId)
                        ? false
                        : true)
            .ToList();

        if (roomItems.Count == 0)
            roomItems = rooms;

        model.RoomOptions =
        [
            new SelectListItem("— Select room —", ""),
            .. roomItems.Select(x => new SelectListItem(
                $"{x.b.Name} · {x.r.RoomNumber}{(string.IsNullOrWhiteSpace(x.r.RoomName) ? "" : " — " + x.r.RoomName)}",
                x.r.Id.ToString(),
                x.r.Id == model.RoomId))
        ];
    }

    private async Task<List<SelectListItem>> LoadTeacherOptionsAsync(
        Guid classId, Guid sectionId, Guid? selectedId, CancellationToken ct)
    {
        var teachersQ = Db.StaffMembers.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive);

        if (classId != Guid.Empty && sectionId != Guid.Empty)
        {
            var assignedIds = await Db.TeacherAssignments.AsNoTracking()
                .Where(a => a.SchoolId == SchoolId && a.IsActive
                            && a.SchoolClassId == classId && a.SchoolSectionId == sectionId)
                .Select(a => a.StaffMemberId)
                .Distinct()
                .ToListAsync(ct);
            if (assignedIds.Count > 0)
                teachersQ = teachersQ.Where(s => assignedIds.Contains(s.Id));
        }

        var teachers = await teachersQ
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName, s.StaffCode })
            .ToListAsync(ct);

        if (teachers.Count == 0 && classId != Guid.Empty)
        {
            teachers = await Db.StaffMembers.AsNoTracking()
                .Where(s => s.SchoolId == SchoolId && s.IsActive)
                .OrderBy(s => s.FullName)
                .Select(s => new { s.Id, s.FullName, s.StaffCode })
                .ToListAsync(ct);
        }

        return
        [
            new SelectListItem("— Select teacher —", ""),
            .. teachers.Select(t => new SelectListItem(
                t.FullName + (string.IsNullOrEmpty(t.StaffCode) ? "" : " (" + t.StaffCode + ")"),
                t.Id.ToString(),
                t.Id == selectedId))
        ];
    }
}
