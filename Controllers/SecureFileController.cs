using System.Security.Claims;
using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Controllers;

[Authorize]
[Route("Files")]
public class SecureFileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _files;
    private readonly ITeacherAccessService _teacherAccess;
    private readonly IParentAcademicService _parentAccess;
    private readonly ITenantContext _tenant;

    public SecureFileController(
        AppDbContext db,
        IFileStorageService files,
        ITeacherAccessService teacherAccess,
        IParentAcademicService parentAccess,
        ITenantContext tenant)
    {
        _db = db;
        _files = files;
        _teacherAccess = teacherAccess;
        _parentAccess = parentAccess;
        _tenant = tenant;
    }

    [HttpGet("Academic/{id:guid}")]
    public async Task<IActionResult> Academic(Guid id, CancellationToken ct)
    {
        var attachment = await _db.AcademicAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);
        if (attachment is null) return NotFound();
        if (!await CanAccessAttachmentAsync(attachment, ct)) return Forbid();

        var opened = await _files.OpenReadAsync(attachment.StoredPath, ct);
        if (opened is null) return NotFound();
        return File(opened.Value.Stream, opened.Value.ContentType, attachment.FileName);
    }

    [HttpGet("Material/{id:guid}")]
    public async Task<IActionResult> Material(Guid id, CancellationToken ct)
    {
        var material = await _db.CourseMaterials.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);
        if (material is null || string.IsNullOrEmpty(material.FilePath)) return NotFound();
        if (!await CanAccessMaterialAsync(material, ct)) return Forbid();

        var opened = await _files.OpenReadAsync(material.FilePath, ct);
        if (opened is null) return NotFound();
        return File(opened.Value.Stream, opened.Value.ContentType, material.FileName ?? opened.Value.FileName);
    }

    private async Task<bool> CanAccessAttachmentAsync(AcademicAttachment attachment, CancellationToken ct)
    {
        if (User.IsInRole(AppRoleNames.SuperAdmin) || User.IsInRole(AppRoleNames.SchoolAdmin))
            return true;

        var userId = _tenant.UserId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        if (User.IsInRole(AppRoleNames.Teacher) && _tenant.SchoolId == attachment.SchoolId)
            return await _db.StaffMembers.AnyAsync(s => s.UserId == userId && s.SchoolId == attachment.SchoolId && s.IsActive, ct);

        if (User.IsInRole(AppRoleNames.Guardian))
            return await CanParentAccessOwnerAsync(userId, attachment.OwnerType, attachment.OwnerId, ct);

        return false;
    }

    private async Task<bool> CanAccessMaterialAsync(CourseMaterial material, CancellationToken ct)
    {
        if (User.IsInRole(AppRoleNames.SuperAdmin) || User.IsInRole(AppRoleNames.SchoolAdmin))
            return true;

        var userId = _tenant.UserId ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        if (User.IsInRole(AppRoleNames.Teacher) && _tenant.SchoolId == material.SchoolId)
            return await _db.StaffMembers.AnyAsync(s => s.UserId == userId && s.SchoolId == material.SchoolId && s.IsActive, ct);

        if (User.IsInRole(AppRoleNames.Guardian))
        {
            if (material.Status != PublishStatus.Published || !material.VisibleToParents)
                return false;
            return await CanParentAccessClassSectionAsync(userId, material.SchoolClassId, material.SchoolSectionId, ct);
        }

        return false;
    }

    private async Task<bool> CanParentAccessOwnerAsync(string userId, AcademicAttachmentOwnerType ownerType, Guid ownerId, CancellationToken ct)
    {
        return ownerType switch
        {
            AcademicAttachmentOwnerType.DailyDiary => await CanParentAccessDiaryAsync(userId, ownerId, ct),
            AcademicAttachmentOwnerType.Announcement => await CanParentAccessAnnouncementAsync(userId, ownerId, ct),
            AcademicAttachmentOwnerType.CourseMaterial => await CanParentAccessMaterialOwnerAsync(userId, ownerId, ct),
            _ => false
        };
    }

    private async Task<bool> CanParentAccessDiaryAsync(string userId, Guid id, CancellationToken ct)
    {
        var diary = await _db.DailyDiaryEntries.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
        return diary is not null && diary.Status == PublishStatus.Published
               && await CanParentAccessClassSectionAsync(userId, diary.SchoolClassId, diary.SchoolSectionId, ct);
    }

    private async Task<bool> CanParentAccessAnnouncementAsync(string userId, Guid id, CancellationToken ct)
    {
        var item = await _db.ClassAnnouncements.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        return item is not null && item.Status == PublishStatus.Published
               && await CanParentAccessClassSectionAsync(userId, item.SchoolClassId, item.SchoolSectionId, ct);
    }

    private async Task<bool> CanParentAccessMaterialOwnerAsync(string userId, Guid id, CancellationToken ct)
    {
        var item = await _db.CourseMaterials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, ct);
        return item is not null && item.Status == PublishStatus.Published && item.VisibleToParents
               && await CanParentAccessClassSectionAsync(userId, item.SchoolClassId, item.SchoolSectionId, ct);
    }

    private async Task<bool> CanParentAccessClassSectionAsync(string userId, Guid classId, Guid sectionId, CancellationToken ct)
    {
        var students = await _parentAccess.GetLinkedStudentsAsync(userId, ct);
        return students.Any(s => s.SchoolClassId == classId && s.SchoolSectionId == sectionId);
    }
}
