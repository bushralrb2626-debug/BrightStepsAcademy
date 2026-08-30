using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
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
    Assessment
}

public class AcademicContentService(AppDbContext db, IFileStorageService fileStorage) : IAcademicContentService
{
    public async Task PublishAsync(
        PublishStatus targetStatus,
        Guid schoolId,
        Guid contentId,
        AcademicContentKind kind,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        switch (kind)
        {
            case AcademicContentKind.DailyDiary:
                var diary = await db.DailyDiaryEntries.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Diary entry not found.");
                diary.Status = targetStatus;
                diary.PublishedAt = targetStatus == PublishStatus.Published ? now : diary.PublishedAt;
                diary.UpdatedAt = now;
                break;
            case AcademicContentKind.ImportantInformation:
                var info = await db.ImportantInformationItems.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Information item not found.");
                info.Status = targetStatus;
                info.PublishedAt = targetStatus == PublishStatus.Published ? now : info.PublishedAt;
                info.UpdatedAt = now;
                break;
            case AcademicContentKind.Announcement:
                var ann = await db.ClassAnnouncements.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Announcement not found.");
                ann.Status = targetStatus;
                ann.PublishedAt = targetStatus == PublishStatus.Published ? now : ann.PublishedAt;
                ann.UpdatedAt = now;
                break;
            case AcademicContentKind.CourseMaterial:
                var mat = await db.CourseMaterials.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Course material not found.");
                mat.Status = targetStatus;
                mat.PublishedAt = targetStatus == PublishStatus.Published ? now : mat.PublishedAt;
                mat.UpdatedAt = now;
                break;
            case AcademicContentKind.Assessment:
                var assessment = await db.Assessments.FirstOrDefaultAsync(x => x.Id == contentId && x.SchoolId == schoolId, ct)
                    ?? throw new InvalidOperationException("Assessment not found.");
                assessment.Status = targetStatus;
                assessment.PublishedAt = targetStatus == PublishStatus.Published ? now : assessment.PublishedAt;
                assessment.UpdatedAt = now;
                break;
        }

        await db.SaveChangesAsync(ct);
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
