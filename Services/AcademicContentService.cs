using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Services;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IAcademicContentService
{
    Task PublishAsync(PublishStatus targetStatus, Guid schoolId, Guid contentId, AcademicContentKind kind, CancellationToken ct = default);
    Task SaveAttachmentsAsync(
        Guid schoolId,
        Guid staffMemberId,
        AcademicAttachmentOwnerType ownerType,
        Guid ownerId,
        IEnumerable<IFormFile> files,
        CancellationToken ct = default);
    Task<IReadOnlyList<AcademicAttachment>> GetAttachmentsAsync(
        AcademicAttachmentOwnerType ownerType,
        Guid ownerId,
        CancellationToken ct = default);
}

public enum AcademicContentKind
{
    DailyDiary,
    ImportantInformation,
    Announcement,
    CourseMaterial,
    Assessment,
    ClassAssignment
}

public class AcademicContentService(
    AppDbContext db,
    IFileStorageService fileStorage,
    IStudentNotificationService studentNotifications) : IAcademicContentService
{
    public async Task PublishAsync(
        PublishStatus targetStatus,
        Guid schoolId,
        Guid contentId,
        AcademicContentKind kind,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        Guid classId = default;
        Guid sectionId = default;
        string notifyTitle = "";
        string notifyMessage = "";

        switch (kind)
        {
            case AcademicContentKind.DailyDiary:
                var diary = await db.DailyDiaryEntries.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Diary entry not found.");
                diary.Status = targetStatus;
                diary.PublishedAt = targetStatus == PublishStatus.Published ? now : diary.PublishedAt;
                diary.UpdatedAt = now;
                classId = diary.SchoolClassId;
                sectionId = diary.SchoolSectionId;
                notifyTitle = "New Diary Entry";
                notifyMessage = $"{diary.Title} has been published.";
                break;
            case AcademicContentKind.ImportantInformation:
                var info = await db.ImportantInformationItems.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Information item not found.");
                info.Status = targetStatus;
                info.PublishedAt = targetStatus == PublishStatus.Published ? now : info.PublishedAt;
                info.UpdatedAt = now;
                classId = info.SchoolClassId;
                sectionId = info.SchoolSectionId;
                notifyTitle = "Important Information";
                notifyMessage = info.Title;
                break;
            case AcademicContentKind.Announcement:
                var ann = await db.ClassAnnouncements.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Announcement not found.");
                ann.Status = targetStatus;
                ann.PublishedAt = targetStatus == PublishStatus.Published ? now : ann.PublishedAt;
                ann.UpdatedAt = now;
                classId = ann.SchoolClassId;
                sectionId = ann.SchoolSectionId;
                notifyTitle = "New Announcement";
                notifyMessage = ann.Title;
                break;
            case AcademicContentKind.CourseMaterial:
                var mat = await db.CourseMaterials.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Course material not found.");
                mat.Status = targetStatus;
                mat.PublishedAt = targetStatus == PublishStatus.Published ? now : mat.PublishedAt;
                mat.UpdatedAt = now;
                classId = mat.SchoolClassId;
                sectionId = mat.SchoolSectionId;
                notifyTitle = "New Course Material";
                notifyMessage = mat.Title;
                break;
            case AcademicContentKind.Assessment:
                var assessment = await db.Assessments.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Assessment not found.");
                assessment.Status = targetStatus;
                assessment.PublishedAt = targetStatus == PublishStatus.Published ? now : assessment.PublishedAt;
                assessment.UpdatedAt = now;
                classId = assessment.SchoolClassId;
                sectionId = assessment.SchoolSectionId;
                notifyTitle = "Marks Published";
                notifyMessage = $"{assessment.Name} results are now available.";
                break;
            case AcademicContentKind.ClassAssignment:
                var assignment = await db.ClassAssignmentItems.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Assignment not found.");
                assignment.Status = targetStatus;
                assignment.PublishedAt = targetStatus == PublishStatus.Published ? now : assignment.PublishedAt;
                assignment.UpdatedAt = now;
                classId = assignment.SchoolClassId;
                sectionId = assignment.SchoolSectionId;
                notifyTitle = "New Assignment";
                notifyMessage = assignment.Title;
                break;
        }

        await db.SaveChangesAsync(ct);

        if (targetStatus == PublishStatus.Published && classId != default && sectionId != default)
        {
            await studentNotifications.NotifyClassSectionAsync(schoolId, classId, sectionId, notifyTitle, notifyMessage, ct);
        }
    }

    public async Task SaveAttachmentsAsync(
        Guid schoolId,
        Guid staffMemberId,
        AcademicAttachmentOwnerType ownerType,
        Guid ownerId,
        IEnumerable<IFormFile> files,
        CancellationToken ct = default)
    {
        foreach (var file in files.Where(f => f is { Length: > 0 }))
        {
            var path = await fileStorage.SaveAcademicAsync(file, schoolId, $"academic/{ownerType.ToString().ToLowerInvariant()}", ct);
            db.AcademicAttachments.Add(new AcademicAttachment
            {
                SchoolId = schoolId,
                OwnerType = ownerType,
                OwnerId = ownerId,
                FileName = file.FileName,
                StoredPath = path,
                ContentType = file.ContentType ?? "application/octet-stream",
                SizeBytes = file.Length,
                UploadedByStaffMemberId = staffMemberId,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AcademicAttachment>> GetAttachmentsAsync(
        AcademicAttachmentOwnerType ownerType,
        Guid ownerId,
        CancellationToken ct = default)
        => await db.AcademicAttachments.AsNoTracking()
            .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId)
            .OrderBy(a => a.FileName)
            .ToListAsync(ct);
}
