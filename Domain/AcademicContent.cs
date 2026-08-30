namespace BrightStepsAcademy.Domain;

public abstract class ClassScopedAcademicContent : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StaffMemberId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherAssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly ContentDate { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }

    public School School { get; set; } = null!;
    public StaffMember StaffMember { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
    public SchoolSection SchoolSection { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public TeacherAssignment? TeacherAssignment { get; set; }
}

public class DailyDiaryEntry : ClassScopedAcademicContent
{
    public string? Topic { get; set; }
    public string? Homework { get; set; }
    public string? Instructions { get; set; }
    public DateOnly? DueDate { get; set; }
}

public class ImportantInformationItem : ClassScopedAcademicContent
{
}

public class ClassAnnouncement : ClassScopedAcademicContent
{
    public string Message { get; set; } = string.Empty;
}

public class CourseMaterial : ClassScopedAcademicContent
{
    public CourseMaterialCategory Category { get; set; } = CourseMaterialCategory.Other;
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? FileContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public bool VisibleToParents { get; set; } = true;
}
