namespace BrightStepsAcademy.Domain;

public class AcademicAttachment : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public AcademicAttachmentOwnerType OwnerType { get; set; }
    public Guid OwnerId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public Guid UploadedByStaffMemberId { get; set; }

    public School School { get; set; } = null!;
    public StaffMember UploadedByStaffMember { get; set; } = null!;
}
