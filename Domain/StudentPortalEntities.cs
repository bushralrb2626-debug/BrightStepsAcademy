namespace BrightStepsAcademy.Domain;

public class ClassTimetableSlot : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int PeriodOrder { get; set; }
    public string? PeriodLabel { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? StaffMemberId { get; set; }
    public Guid? RoomId { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Published;

    public School School { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
    public SchoolSection SchoolSection { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public StaffMember? StaffMember { get; set; }
    public Room? Room { get; set; }
}

public class ClassAssignmentItem : ClassScopedAcademicContent
{
    public DateOnly DueDate { get; set; }
    public decimal? TotalMarks { get; set; }
    public bool AllowSubmission { get; set; } = true;
    public string? AttachmentPath { get; set; }
    public string? AttachmentFileName { get; set; }
    public string? AttachmentContentType { get; set; }
    public long? AttachmentSizeBytes { get; set; }

    public ICollection<ClassAssignmentSubmission> Submissions { get; set; } = new List<ClassAssignmentSubmission>();
}

public class ClassAssignmentSubmission : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public string? TextResponse { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public string? FileContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public AssignmentSubmissionStatus ReviewStatus { get; set; } = AssignmentSubmissionStatus.Submitted;
    public decimal? ObtainedMarks { get; set; }
    public string? TeacherFeedback { get; set; }

    public School School { get; set; } = null!;
    public ClassAssignmentItem Assignment { get; set; } = null!;
    public StudentRecord Student { get; set; } = null!;
}
