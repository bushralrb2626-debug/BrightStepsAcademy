using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Academic")]
public class SchoolAcademicController : SchoolManageControllerBase
{
    public SchoolAcademicController(
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
        if (await ForbidUnlessAsync(PermissionCodes.StudentsView) is { } deny)
            return deny;

        ViewBag.ClassCount = await Db.SchoolClasses.CountAsync(c => c.SchoolId == SchoolId && c.IsActive, ct);
        ViewBag.SubjectCount = await Db.Subjects.CountAsync(s => s.SchoolId == SchoolId && s.IsActive, ct);
        ViewBag.AssignmentCount = await Db.TeacherAssignments.CountAsync(a => a.SchoolId == SchoolId && a.IsActive, ct);
        return SchoolView("Academic/Index");
    }

    [HttpGet("Classes")]
    public async Task<IActionResult> Classes(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsView) is { } deny)
            return deny;
        var items = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);
        return SchoolView("Academic/Classes/Index", items);
    }

    [HttpGet("Classes/Create")]
    public async Task<IActionResult> CreateClass(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        return SchoolView("Academic/Classes/Create", new SchoolClassFormVm());
    }

    [HttpPost("Classes/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClass(SchoolClassFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (!ModelState.IsValid)
            return SchoolView("Academic/Classes/Create", model);

        var entity = new SchoolClass
        {
            SchoolId = SchoolId,
            Name = model.Name.Trim(),
            GradeLevel = model.GradeLevel?.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = true
        };
        Db.SchoolClasses.Add(entity);
        await Db.SaveChangesAsync(ct);
        SetFlash("Class created.");
        return RedirectToAction(nameof(Classes));
    }

    [HttpGet("Classes/Edit/{id:guid}")]
    public async Task<IActionResult> EditClass(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.SchoolClasses.FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        return SchoolView("Academic/Classes/Edit", new SchoolClassFormVm
        {
            Id = item.Id,
            Name = item.Name,
            GradeLevel = item.GradeLevel,
            DisplayOrder = item.DisplayOrder
        });
    }

    [HttpPost("Classes/Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClass(Guid id, SchoolClassFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.SchoolClasses.FirstOrDefaultAsync(c => c.Id == id && c.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (!ModelState.IsValid)
            return SchoolView("Academic/Classes/Edit", model);

        item.Name = model.Name.Trim();
        item.GradeLevel = model.GradeLevel?.Trim();
        item.DisplayOrder = model.DisplayOrder;
        await Db.SaveChangesAsync(ct);
        SetFlash("Class updated.");
        return RedirectToAction(nameof(Classes));
    }

    [HttpGet("Sections")]
    public async Task<IActionResult> Sections(Guid? classId, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsView) is { } deny)
            return deny;
        await LoadClassOptionsAsync(classId, ct);
        var query = Db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive);
        if (classId.HasValue)
            query = query.Where(s => s.SchoolClassId == classId.Value);
        var items = await query.OrderBy(s => s.Name).ToListAsync(ct);
        return SchoolView("Academic/Sections/Index", items);
    }

    [HttpGet("Sections/Create")]
    public async Task<IActionResult> CreateSection(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        await LoadClassOptionsAsync(null, ct);
        return SchoolView("Academic/Sections/Create", new SchoolSectionFormVm());
    }

    [HttpPost("Sections/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSection(SchoolSectionFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (model.SchoolClassId == Guid.Empty)
            ModelState.AddModelError(nameof(model.SchoolClassId), "Class is required.");
        if (!ModelState.IsValid)
        {
            await LoadClassOptionsAsync(model.SchoolClassId, ct);
            return SchoolView("Academic/Sections/Create", model);
        }

        Db.SchoolSections.Add(new SchoolSection
        {
            SchoolId = SchoolId,
            SchoolClassId = model.SchoolClassId,
            Name = model.Name.Trim(),
            IsActive = true
        });
        await Db.SaveChangesAsync(ct);
        SetFlash("Section created.");
        return RedirectToAction(nameof(Sections));
    }

    [HttpGet("Subjects")]
    public async Task<IActionResult> Subjects(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsView) is { } deny)
            return deny;
        var items = await Db.Subjects.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
        return SchoolView("Academic/Subjects/Index", items);
    }

    [HttpGet("Subjects/Create")]
    public async Task<IActionResult> CreateSubject(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        return SchoolView("Academic/Subjects/Create", new SubjectFormVm());
    }

    [HttpPost("Subjects/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubject(SubjectFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (!ModelState.IsValid)
            return SchoolView("Academic/Subjects/Create", model);

        Db.Subjects.Add(new Subject
        {
            SchoolId = SchoolId,
            Name = model.Name.Trim(),
            Code = model.Code?.Trim(),
            IsActive = true
        });
        await Db.SaveChangesAsync(ct);
        SetFlash("Subject created.");
        return RedirectToAction(nameof(Subjects));
    }

    [HttpGet("Assignments")]
    public async Task<IActionResult> Assignments(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsView) is { } deny)
            return deny;
        var items = await Db.TeacherAssignments.AsNoTracking()
            .Where(a => a.SchoolId == SchoolId && a.IsActive)
            .Join(Db.StaffMembers.AsNoTracking(), a => a.StaffMemberId, s => s.Id, (a, s) => new { a, s })
            .Join(Db.SchoolClasses.AsNoTracking(), x => x.a.SchoolClassId, c => c.Id, (x, c) => new { x.a, x.s, c })
            .Join(Db.SchoolSections.AsNoTracking(), x => x.a.SchoolSectionId, sec => sec.Id, (x, sec) => new { x.a, x.s, x.c, sec })
            .Join(Db.Subjects.AsNoTracking(), x => x.a.SubjectId, sub => sub.Id, (x, sub) => new TeacherAssignmentListVm
            {
                Id = x.a.Id,
                Teacher = x.s.FullName,
                Class = x.c.Name,
                Section = x.sec.Name,
                Subject = sub.Name,
                ScheduleNotes = x.a.ScheduleNotes
            })
            .OrderBy(x => x.Teacher).ThenBy(x => x.Class)
            .ToListAsync(ct);
        return SchoolView("Academic/Assignments/Index", items);
    }

    [HttpGet("Assignments/Create")]
    public async Task<IActionResult> CreateAssignment(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        await LoadAssignmentFormAsync(new TeacherAssignmentFormVm(), ct);
        return SchoolView("Academic/Assignments/Create", new TeacherAssignmentFormVm());
    }

    [HttpPost("Assignments/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssignment(TeacherAssignmentFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        if (model.StaffMemberId == Guid.Empty)
            ModelState.AddModelError(nameof(model.StaffMemberId), "Teacher is required.");
        if (model.SchoolClassId == Guid.Empty || model.SchoolSectionId == Guid.Empty || model.SubjectId == Guid.Empty)
            ModelState.AddModelError(string.Empty, "Class, section, and subject are required.");
        if (!ModelState.IsValid)
        {
            await LoadAssignmentFormAsync(model, ct);
            return SchoolView("Academic/Assignments/Create", model);
        }

        var exists = await Db.TeacherAssignments.AnyAsync(a =>
            a.SchoolId == SchoolId && a.IsActive
            && a.StaffMemberId == model.StaffMemberId
            && a.SchoolClassId == model.SchoolClassId
            && a.SchoolSectionId == model.SchoolSectionId
            && a.SubjectId == model.SubjectId, ct);
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "This teacher is already assigned to that class, section, and subject.");
            await LoadAssignmentFormAsync(model, ct);
            return SchoolView("Academic/Assignments/Create", model);
        }

        Db.TeacherAssignments.Add(new TeacherAssignment
        {
            SchoolId = SchoolId,
            StaffMemberId = model.StaffMemberId,
            SchoolClassId = model.SchoolClassId,
            SchoolSectionId = model.SchoolSectionId,
            SubjectId = model.SubjectId,
            ScheduleNotes = model.ScheduleNotes?.Trim(),
            IsActive = true
        });
        await Db.SaveChangesAsync(ct);

        var staff = await Db.StaffMembers.FirstOrDefaultAsync(s => s.Id == model.StaffMemberId, ct);
        if (staff?.UserId is not null)
        {
            var user = await UserManager.FindByIdAsync(staff.UserId);
            if (user is not null && !await UserManager.IsInRoleAsync(user, AppRoleNames.Teacher))
                await UserManager.AddToRoleAsync(user, AppRoleNames.Teacher);
        }

        SetFlash("Teacher assignment created.");
        return RedirectToAction(nameof(Assignments));
    }

    private async Task LoadClassOptionsAsync(Guid? selectedClassId, CancellationToken ct)
    {
        var classes = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        ViewBag.Classes = new SelectList(classes, "Id", "Name", selectedClassId);
    }

    private async Task LoadAssignmentFormAsync(TeacherAssignmentFormVm model, CancellationToken ct)
    {
        var staff = await Db.StaffMembers.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .OrderBy(s => s.FullName)
            .Select(s => new { s.Id, s.FullName })
            .ToListAsync(ct);
        var classes = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);
        var sections = await Db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.SchoolClassId })
            .ToListAsync(ct);
        var subjects = await Db.Subjects.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        ViewBag.Staff = new SelectList(staff, "Id", "FullName", model.StaffMemberId);
        ViewBag.Classes = new SelectList(classes, "Id", "Name", model.SchoolClassId);
        ViewBag.Sections = new SelectList(sections, "Id", "Name", model.SchoolSectionId);
        ViewBag.Subjects = new SelectList(subjects, "Id", "Name", model.SubjectId);
        ViewBag.SectionsJson = System.Text.Json.JsonSerializer.Serialize(sections);
    }
}
