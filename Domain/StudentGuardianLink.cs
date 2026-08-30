namespace BrightStepsAcademy.Domain;

/// <summary>
/// Links exactly one guardian portal account to a student.
/// StudentId is unique — one student may only have one guardian portal.
/// </summary>
public class StudentGuardianLink : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public Guid GuardianProfileId { get; set; }
    public string Relationship { get; set; } = string.Empty;

    public School School { get; set; } = null!;
    public StudentRecord Student { get; set; } = null!;
    public GuardianProfile Guardian { get; set; } = null!;
}
