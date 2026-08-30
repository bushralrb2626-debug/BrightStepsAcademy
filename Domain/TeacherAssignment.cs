namespace BrightStepsAcademy.Domain;

public class TeacherAssignment : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StaffMemberId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public Guid SubjectId { get; set; }
    public string? ScheduleNotes { get; set; }

    public School School { get; set; } = null!;
    public StaffMember StaffMember { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
    public SchoolSection SchoolSection { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
