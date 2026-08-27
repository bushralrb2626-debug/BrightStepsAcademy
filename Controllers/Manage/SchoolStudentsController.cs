using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Students")]
public class SchoolStudentsController : SchoolManageControllerBase
{
    public SchoolStudentsController(
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
        var items = await Db.StudentRecords.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId)
            .OrderBy(s => s.ClassName).ThenBy(s => s.FullName)
            .ToListAsync(ct);
        return SchoolView("Students/Index", items);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        return SchoolView("Students/Create", new StudentFormVm());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;

        var student = new StudentRecord
        {
            SchoolId = SchoolId,
            StudentCode = model.StudentCode?.Trim() ?? "",
            FullName = model.FullName?.Trim() ?? "",
            Email = model.Email?.Trim(),
            ParentName = model.ParentName?.Trim(),
            ParentEmail = model.ParentEmail?.Trim(),
            ParentPhone = model.ParentPhone?.Trim(),
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender?.Trim(),
            AdmissionDate = model.AdmissionDate,
            ClassName = model.ClassName?.Trim(),
            Section = model.Section?.Trim(),
            RollNumber = model.RollNumber?.Trim(),
            Address = model.Address?.Trim(),
            EmergencyContact = model.EmergencyContact?.Trim(),
            CreatedByUserId = CurrentUserId,
            IsActive = true
        };
        Db.StudentRecords.Add(student);
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Student code must be unique within the school.");
            return SchoolView("Students/Create", model);
        }

        await Audit.LogAsync("Create", "Students", SchoolId, "Student", student.Id.ToString(), student.FullName, ct);
        SetFlash("Student created.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        return SchoolView("Students/Edit", new StudentFormVm
        {
            Id = item.Id,
            StudentCode = item.StudentCode,
            FullName = item.FullName,
            Email = item.Email,
            ParentName = item.ParentName,
            ParentEmail = item.ParentEmail,
            ParentPhone = item.ParentPhone,
            DateOfBirth = item.DateOfBirth,
            Gender = item.Gender,
            AdmissionDate = item.AdmissionDate,
            ClassName = item.ClassName,
            Section = item.Section,
            RollNumber = item.RollNumber,
            Address = item.Address,
            EmergencyContact = item.EmergencyContact
        });
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, StudentFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();

        item.StudentCode = model.StudentCode?.Trim() ?? "";
        item.FullName = model.FullName?.Trim() ?? "";
        item.Email = model.Email?.Trim();
        item.ParentName = model.ParentName?.Trim();
        item.ParentEmail = model.ParentEmail?.Trim();
        item.ParentPhone = model.ParentPhone?.Trim();
        item.DateOfBirth = model.DateOfBirth;
        item.Gender = model.Gender?.Trim();
        item.AdmissionDate = model.AdmissionDate;
        item.ClassName = model.ClassName?.Trim();
        item.Section = model.Section?.Trim();
        item.RollNumber = model.RollNumber?.Trim();
        item.Address = model.Address?.Trim();
        item.EmergencyContact = model.EmergencyContact?.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;
        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Student code must be unique within the school.");
            return SchoolView("Students/Edit", model);
        }

        SetFlash("Student updated.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Deactivate/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();
        item.IsActive = false;
        item.UpdatedAt = DateTimeOffset.UtcNow;
        await Db.SaveChangesAsync(ct);
        SetFlash("Student deactivated.");
        return RedirectToAction(nameof(Index));
    }
}
