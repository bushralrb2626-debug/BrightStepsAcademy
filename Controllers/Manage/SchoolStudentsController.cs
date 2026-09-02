using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models.Manage;
using BrightStepsAcademy.Services;
using BrightStepsAcademy.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers.Manage;

[Route("Manage/School/Students")]
public class SchoolStudentsController : SchoolManageControllerBase
{
    private readonly IGuardianService _guardians;
    private readonly IStudentAccountService _studentAccounts;
    private readonly IAccountEmailNotificationService _accountEmails;

    public SchoolStudentsController(
        AppDbContext db,
        ITenantContext tenant,
        IPermissionService permissions,
        IAuditService audit,
        UserManager<ApplicationUser> userManager,
        IGuardianService guardians,
        IStudentAccountService studentAccounts,
        IAccountEmailNotificationService accountEmails)
        : base(db, tenant, permissions, audit, userManager)
    {
        _guardians = guardians;
        _studentAccounts = studentAccounts;
        _accountEmails = accountEmails;
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
        ValidateStudentPortalFields(model, isEdit: false);
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
            await LoadClassSectionOptionsAsync(model, ct);
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
            await LoadClassSectionOptionsAsync(model, ct);
            return SchoolView("Students/Create", model);
        }

        if (model.EnableStudentPortal)
        {
            var studentLogin = await _studentAccounts.ConfigureLoginAsync(new StudentAccountRequest
            {
                SchoolId = SchoolId,
                StudentId = student.Id,
                UpdatedByUserId = CurrentUserId,
                EnablePortal = true,
                LoginId = model.StudentLoginId,
                Password = model.StudentPassword
            }, ct);

            if (!studentLogin.Success)
            {
                await RollbackCreatedStudentAsync(student.Id, ct);
                ModelState.AddModelError(string.Empty, studentLogin.Error ?? "Could not create student portal login.");
                await LoadExistingGuardiansAsync(model, ct);
                await LoadClassSectionOptionsAsync(model, ct);
                return SchoolView("Students/Create", model);
            }
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

        model.HasStudentLogin = !string.IsNullOrEmpty(item.UserId);
        if (model.HasStudentLogin && item.UserId is not null)
        {
            var studentUser = await UserManager.FindByIdAsync(item.UserId);
            model.StudentLoginId = studentUser?.LoginId;
            model.EnableStudentPortal = studentUser?.IsActive == true;
        }

        await LoadExistingGuardiansAsync(model, ct);
        await LoadClassSectionOptionsAsync(model, ct);
        await LoadStudentEmailStatusAsync(model, item, ct);
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
        ValidateStudentPortalFields(model, isEdit: true);
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
            await LoadClassSectionOptionsAsync(model, ct);
            return SchoolView("Students/Edit", model);
        }

        if (model.EnableStudentPortal && model.ResetStudentPassword && model.NewStudentPassword != model.NewStudentConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.NewStudentConfirmPassword), "Student passwords do not match.");
            await LoadExistingGuardiansAsync(model, ct);
            await LoadClassSectionOptionsAsync(model, ct);
            return SchoolView("Students/Edit", model);
        }

        if (model.EnableStudentPortal || model.HasStudentLogin || model.ResetStudentPassword)
        {
            var studentLogin = await _studentAccounts.ConfigureLoginAsync(new StudentAccountRequest
            {
                SchoolId = SchoolId,
                StudentId = id,
                UpdatedByUserId = CurrentUserId,
                EnablePortal = model.EnableStudentPortal,
                LoginId = model.StudentLoginId,
                Password = model.StudentPassword,
                ResetPassword = model.ResetStudentPassword,
                NewPassword = model.NewStudentPassword
            }, ct);

            if (!studentLogin.Success)
            {
                ModelState.AddModelError(string.Empty, studentLogin.Error ?? "Could not update student portal login.");
                await LoadExistingGuardiansAsync(model, ct);
                await LoadClassSectionOptionsAsync(model, ct);
                return SchoolView("Students/Edit", model);
            }
        }

        try
        {
            await Db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Student code must be unique within the school.");
            await LoadExistingGuardiansAsync(model, ct);
            await LoadClassSectionOptionsAsync(model, ct);
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

    [HttpPost("ResendCredentials/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendCredentials(Guid id, CancellationToken ct)
    {
        if (await ForbidUnlessAsync(PermissionCodes.StudentsManage) is { } deny)
            return deny;

        var student = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == id && s.SchoolId == SchoolId, ct);
        if (student is null || string.IsNullOrEmpty(student.UserId))
        {
            SetFlash("This student does not have portal login access.", "error");
            return RedirectToAction(nameof(Edit), new { id });
        }

        var log = await _accountEmails.ResendCredentialsEmailAsync(student.UserId, SchoolId, PortalAccountType.Student, ct);
        SetFlash(log.Status == AccountEmailDeliveryStatus.Sent
            ? "Credentials email sent with a new temporary password."
            : $"Could not send credentials email: {log.FailureReason ?? "Unknown error"}",
            log.Status == AccountEmailDeliveryStatus.Sent ? "success" : "error");
        return RedirectToAction(nameof(Edit), new { id });
    }

    private async Task LoadStudentEmailStatusAsync(StudentFormVm model, StudentRecord student, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(student.UserId))
            return;

        if (string.IsNullOrEmpty(student.UserId))
            return;

        var log = await _accountEmails.GetLatestStatusAsync(student.UserId, AccountEmailType.NewAccountCreated, ct);
        if (log is null)
        {
            model.CanResendCredentialsEmail = true;
            return;
        }

        model.CredentialsEmailStatus = log.Status.ToString();
        model.CredentialsEmailFailureReason = log.FailureReason;
        model.CanResendCredentialsEmail = log.Status != AccountEmailDeliveryStatus.Sent;
    }

    private async Task LoadClassSectionOptionsAsync(StudentFormVm model, CancellationToken ct)
    {
        var selectedClassId = model.SchoolClassId;
        var classes = await Db.SchoolClasses.AsNoTracking()
            .Where(c => c.SchoolId == SchoolId && c.IsActive)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        model.ClassOptions =
        [
            new SelectListItem("— Select class —", ""),
            .. classes.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == selectedClassId))
        ];

        var selectedSectionId = model.SchoolSectionId;
        var sections = await Db.SchoolSections.AsNoTracking()
            .Where(s => s.SchoolId == SchoolId && s.IsActive
                        && (!selectedClassId.HasValue || s.SchoolClassId == selectedClassId))
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        model.SectionOptions =
        [
            new SelectListItem("— Select section —", ""),
            .. sections.Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == selectedSectionId))
        ];
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
        var selectedGuardianId = model.ExistingGuardianId;
        model.ExistingGuardians =
        [
            new SelectListItem("— Select guardian —", ""),
            .. guardians.Select(g => new SelectListItem(
                $"{g.FullName} ({g.Email}){(g.PortalEnabled ? " · Portal" : "")}",
                g.Id.ToString(),
                g.Id == selectedGuardianId))
        ];
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

    private void ValidateStudentPortalFields(StudentFormVm model, bool isEdit)
    {
        if (!model.EnableStudentPortal)
            return;

        var creatingLogin = !isEdit || !model.HasStudentLogin;
        if (creatingLogin)
        {
            if (string.IsNullOrWhiteSpace(model.StudentPassword))
                ModelState.AddModelError(nameof(model.StudentPassword), "Initial password is required when student portal access is enabled.");
            else if (model.StudentPassword != model.StudentConfirmPassword)
                ModelState.AddModelError(nameof(model.StudentConfirmPassword), "Student passwords do not match.");
        }

        if (model.ResetStudentPassword)
        {
            if (string.IsNullOrWhiteSpace(model.NewStudentPassword))
                ModelState.AddModelError(nameof(model.NewStudentPassword), "Enter a new temporary password.");
            else if (model.NewStudentPassword != model.NewStudentConfirmPassword)
                ModelState.AddModelError(nameof(model.NewStudentConfirmPassword), "Student passwords do not match.");
        }
    }

    private async Task RollbackCreatedStudentAsync(Guid studentId, CancellationToken ct)
    {
        var link = await Db.StudentGuardianLinks.FirstOrDefaultAsync(l => l.StudentId == studentId && l.SchoolId == SchoolId, ct);
        if (link is not null)
            Db.StudentGuardianLinks.Remove(link);

        var student = await Db.StudentRecords.FirstOrDefaultAsync(s => s.Id == studentId && s.SchoolId == SchoolId, ct);
        if (student is not null)
            Db.StudentRecords.Remove(student);

        await Db.SaveChangesAsync(ct);
    }
}
