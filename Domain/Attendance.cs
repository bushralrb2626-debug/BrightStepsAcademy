namespace BrightStepsAcademy.Domain;

public class AttendanceSession : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StaffMemberId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherAssignmentId { get; set; }
    public DateOnly SessionDate { get; set; }
    public string? PeriodLabel { get; set; }
    public string? Notes { get; set; }

    public School School { get; set; } = null!;
    public StaffMember StaffMember { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
    public SchoolSection SchoolSection { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public TeacherAssignment? TeacherAssignment { get; set; }
    public ICollection<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
}

public class AttendanceRecord : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid AttendanceSessionId { get; set; }
    public Guid StudentId { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }

    public School School { get; set; } = null!;
    public AttendanceSession AttendanceSession { get; set; } = null!;
    public StudentRecord Student { get; set; } = null!;
}
