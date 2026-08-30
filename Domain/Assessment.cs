namespace BrightStepsAcademy.Domain;

public class Assessment : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StaffMemberId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherAssignmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssessmentType AssessmentType { get; set; } = AssessmentType.Quiz;
    public DateOnly AssessmentDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public string? Description { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }

    public School School { get; set; } = null!;
    public StaffMember StaffMember { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
    public SchoolSection SchoolSection { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public TeacherAssignment? TeacherAssignment { get; set; }
    public ICollection<AssessmentMark> Marks { get; set; } = new List<AssessmentMark>();
}

public class AssessmentMark : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid AssessmentId { get; set; }
    public Guid StudentId { get; set; }
    public decimal ObtainedMarks { get; set; }
    public decimal? Percentage { get; set; }
    public string? GradeLabel { get; set; }
    public string? Notes { get; set; }

    public School School { get; set; } = null!;
    public Assessment Assessment { get; set; } = null!;
    public StudentRecord Student { get; set; } = null!;
}
