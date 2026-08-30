namespace BrightStepsAcademy.Domain;

public class StudentRecord : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ParentName { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentPhone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public DateOnly? AdmissionDate { get; set; }
    public string? ClassName { get; set; }
    public string? Section { get; set; }
    public Guid? SchoolClassId { get; set; }
    public Guid? SchoolSectionId { get; set; }
    public string? RollNumber { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? ProfileImagePath { get; set; }
    public string? UserId { get; set; }

    public School School { get; set; } = null!;
    public SchoolClass? SchoolClass { get; set; }
    public SchoolSection? SchoolSection { get; set; }
    public StudentGuardianLink? GuardianLink { get; set; }
}
