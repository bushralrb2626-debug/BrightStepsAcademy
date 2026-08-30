using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Students")]
public class SchoolStudentsController : SchoolManageControllerBase
{
    private readonly IGuardianService _guardians;

    public SchoolStudentsController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager,
        IGuardianService guardians)
        : base(db, tenant, permissions, audit, userManager)
    {
        _guardians = guardians;
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
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var model = new StudentFormVm();
        await LoadExistingGuardiansAsync(model, ct);
        await LoadClassSectionOptionsAsync(model, ct);
        return SchoolView("Students/Create", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;

        ValidateGuardianFields(model, isEdit: false);
        if (!ModelState.IsValid)
        {
            await LoadExistingGuardiansAsync(model, ct);
            await LoadClassSectionOptionsAsync(model, ct);
            return SchoolView("Students/Create", model);
        }

        await SyncClassSectionTextAsync(model, ct);
        var student = new StudentRecord
        {
            SchoolId = SchoolId,
            StudentCode = model.StudentCode?.Trim() ?? "",
            FullName = model.FullName?.Trim() ?? "",
            Email = model.Email?.Trim(),
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender?.Trim(),
            AdmissionDate = model.AdmissionDate,
            ClassName = model.ClassName?.Trim(),
            Section = model.Section?.Trim(),
            SchoolClassId = model.SchoolClassId,
            SchoolSectionId = model.SchoolSectionId,
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
            await LoadExistingGuardiansAsync(model, ct);
            return SchoolView("Students/Create", model);
        }

        var guardianResult = await _guardians.AssignGuardianAsync(new GuardianAssignmentRequest
        {
            SchoolId = SchoolId,
            StudentId = student.Id,
            CreatedByUserId = CurrentUserId,
            UseExistingGuardian = model.GuardianMode == "existing",
            ExistingGuardianId = model.ExistingGuardianId,
            GuardianName = model.GuardianName,
            Relationship = model.GuardianRelationship,
            GuardianEmail = model.GuardianEmail,
            GuardianPhone = model.GuardianPhone,
            EnablePortal = model.EnableGuardianPortal,
            LoginId = model.GuardianLoginId,
            Password = model.GuardianPassword
        }, ct);

        if (!guardianResult.Success)
        {
            Db.StudentRecords.Remove(student);
            await Db.SaveChangesAsync(ct);
            ModelState.AddModelError(string.Empty, guardianResult.Error ?? "Could not assign guardian.");
            await LoadExistingGuardiansAsync(model, ct);
            return SchoolView("Students/Create", model);
        }

        await Audit.LogAsync("Create", "Students", SchoolId, "Student", student.Id.ToString(), student.FullName, ct);
        SetFlash("Student created with guardian information.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();

        var model = new StudentFormVm
        {
            Id = item.Id,
            StudentCode = item.StudentCode,
            FullName = item.FullName,
            Email = item.Email,
            DateOfBirth = item.DateOfBirth,
            Gender = item.Gender,
            AdmissionDate = item.AdmissionDate,
            ClassName = item.ClassName,
            Section = item.Section,
            SchoolClassId = item.SchoolClassId,
            SchoolSectionId = item.SchoolSectionId,
            RollNumber = item.RollNumber,
            Address = item.Address,
            EmergencyContact = item.EmergencyContact,
            ParentName = item.ParentName,
            ParentEmail = item.ParentEmail,
            ParentPhone = item.ParentPhone
        };

        var link = await _guardians.GetLinkForStudentAsync(id, SchoolId, ct);
        if (link?.Guardian is not null)
        {
            model.HasGuardianLink = true;
            model.CurrentGuardianProfileId = link.GuardianProfileId;
            model.GuardianName = link.Guardian.FullName;
            model.GuardianRelationship = link.Relationship;
            model.GuardianEmail = link.Guardian.Email;
            model.GuardianPhone = link.Guardian.Phone;
            model.GuardianLoginId = link.Guardian.LoginId;
            model.EnableGuardianPortal = link.Guardian.PortalEnabled;
            model.PortalWasEnabled = link.Guardian.PortalEnabled;
        }
        else
        {
            model.GuardianName = item.ParentName ?? "";
            model.GuardianEmail = item.ParentEmail;
            model.GuardianPhone = item.ParentPhone;
        }

        await LoadExistingGuardiansAsync(model, ct);
        await LoadClassSectionOptionsAsync(model, ct);
        return SchoolView("Students/Edit", model);
    }

    [HttpPost("Edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, StudentFormVm model, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;
        var item = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (item is null) return NotFound();

        ValidateGuardianFields(model, isEdit: true);
        if (!ModelState.IsValid)
        {
            await LoadExistingGuardiansAsync(model, ct);
            await LoadClassSectionOptionsAsync(model, ct);
            return SchoolView("Students/Edit", model);
        }

        await SyncClassSectionTextAsync(model, ct);
        item.StudentCode = model.StudentCode?.Trim() ?? "";
        item.FullName = model.FullName?.Trim() ?? "";
        item.Email = model.Email?.Trim();
        item.DateOfBirth = model.DateOfBirth;
        item.Gender = model.Gender?.Trim();
        item.AdmissionDate = model.AdmissionDate;
        item.ClassName = model.ClassName?.Trim();
        item.Section = model.Section?.Trim();
        item.SchoolClassId = model.SchoolClassId;
        item.SchoolSectionId = model.SchoolSectionId;
        item.RollNumber = model.RollNumber?.Trim();
        item.Address = model.Address?.Trim();
        item.EmergencyContact = model.EmergencyContact?.Trim();
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var guardianResult = await _guardians.UpdateGuardianAsync(new GuardianUpdateRequest
        {
            SchoolId = SchoolId,
            StudentId = id,
            UpdatedByUserId = CurrentUserId,
            ChangeGuardian = model.ChangeGuardian,
            UseExistingGuardian = model.GuardianMode == "existing",
            ExistingGuardianId = model.ExistingGuardianId,
            GuardianName = model.GuardianName,
            Relationship = model.GuardianRelationship,
            GuardianEmail = model.GuardianEmail,
            GuardianPhone = model.GuardianPhone,
            EnablePortal = model.EnableGuardianPortal,
            LoginId = model.GuardianLoginId,
            Password = model.GuardianPassword,
            ResetPassword = model.ResetGuardianPassword,
            NewPassword = model.NewGuardianPassword
        }, ct);

        if (!guardianResult.Success)
        {
            ModelState.AddModelError(string.Empty, guardianResult.Error ?? "Could not update guardian.");
            await LoadExistingGuardiansAsync(model, ct);
            return SchoolView("Students/Edit", model);
        }

        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Student code must be unique within the school.");
            await LoadExistingGuardiansAsync(model, ct);
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

    private async Task LoadClassSectionOptionsAsync(StudentFormVm model, CancellationToken ct)
    {
        model.ClassOptions = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.SchoolClassId))
            .Prepend(new SelectListItem("— Select class —", ""))
            .ToListAsync(ct);

        model.SectionOptions = await Db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive
                        && (!model.SchoolClassId.HasValue || s.SchoolClassId == model.SchoolClassId))
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == model.SchoolSectionId))
            .Prepend(new SelectListItem("— Select section —", ""))
            .ToListAsync(ct);
    }

    private async Task SyncClassSectionTextAsync(StudentFormVm model, CancellationToken ct)
    {
        if (model.SchoolClassId.HasValue)
        {
            var cls = await Db.SchoolClasses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == model.SchoolClassId && c.SchoolId == SchoolId, ct);
            if (cls is not null) model.ClassName = cls.Name;
        }
        if (model.SchoolSectionId.HasValue)
        {
            var sec = await Db.SchoolSections.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == model.SchoolSectionId && s.SchoolId == SchoolId, ct);
            if (sec is not null) model.Section = sec.Name;
        }
    }

    private async Task LoadExistingGuardiansAsync(StudentFormVm model, CancellationToken ct)
    {
        var guardians = await _guardians.ListGuardiansAsync(SchoolId, ct);
        model.ExistingGuardians = guardians
            .Select(g => new SelectListItem(
                $"{g.FullName} ({g.Email}){(g.PortalEnabled ? " · Portal" : "")}",
                g.Id.ToString(),
                model.ExistingGuardianId == g.Id))
            .Prepend(new SelectListItem("— Select guardian —", ""))
            .ToList();
    }

    private void ValidateGuardianFields(StudentFormVm model, bool isEdit)
    {
        if (isEdit && model.ChangeGuardian && model.GuardianMode == "existing" && !model.ExistingGuardianId.HasValue)
            ModelState.AddModelError(nameof(model.ExistingGuardianId), "Select an existing guardian.");

        if (!isEdit || model.ChangeGuardian || !model.HasGuardianLink)
        {
            if (string.IsNullOrWhiteSpace(model.GuardianRelationship))
                ModelState.AddModelError(nameof(model.GuardianRelationship), "Relationship is required.");

            if (model.GuardianMode != "existing")
            {
                if (string.IsNullOrWhiteSpace(model.GuardianName))
                    ModelState.AddModelError(nameof(model.GuardianName), "Guardian name is required.");
                if (string.IsNullOrWhiteSpace(model.GuardianEmail))
                    ModelState.AddModelError(nameof(model.GuardianEmail), "Guardian email is required.");
            }
            else if (!model.ExistingGuardianId.HasValue)
            {
                ModelState.AddModelError(nameof(model.ExistingGuardianId), "Select an existing guardian to link.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.GuardianName))
                ModelState.AddModelError(nameof(model.GuardianName), "Guardian name is required.");
            if (string.IsNullOrWhiteSpace(model.GuardianRelationship))
                ModelState.AddModelError(nameof(model.GuardianRelationship), "Relationship is required.");
            if (string.IsNullOrWhiteSpace(model.GuardianEmail))
                ModelState.AddModelError(nameof(model.GuardianEmail), "Guardian email is required.");
        }

        var needsPassword = model.EnableGuardianPortal && (
            (!isEdit && model.GuardianMode != "existing") ||
            (isEdit && model.ChangeGuardian && model.GuardianMode != "existing") ||
            (isEdit && !model.HasGuardianLink) ||
            (isEdit && model.HasGuardianLink && !model.PortalWasEnabled && !model.ChangeGuardian));

        if (needsPassword)
        {
            if (string.IsNullOrWhiteSpace(model.GuardianPassword))
                ModelState.AddModelError(nameof(model.GuardianPassword), "Initial password is required when portal access is enabled.");
            else if (model.GuardianPassword != model.GuardianConfirmPassword)
                ModelState.AddModelError(nameof(model.GuardianConfirmPassword), "Passwords do not match.");
        }

        if (model.ResetGuardianPassword)
        {
            if (string.IsNullOrWhiteSpace(model.NewGuardianPassword))
                ModelState.AddModelError(nameof(model.NewGuardianPassword), "Enter a new temporary password.");
            else if (model.NewGuardianPassword != model.NewGuardianConfirmPassword)
                ModelState.AddModelError(nameof(model.NewGuardianConfirmPassword), "Passwords do not match.");
        }
    }
}
