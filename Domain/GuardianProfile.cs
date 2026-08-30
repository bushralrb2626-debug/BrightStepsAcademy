namespace BrightStepsAcademy.Domain;

public class GuardianProfile : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? LoginId { get; set; }
    public string? UserId { get; set; }
    public bool PortalEnabled { get; set; }

    public School School { get; set; } = null!;
    public ICollection<StudentGuardianLink> StudentLinks { get; set; } = new List<StudentGuardianLink>();
}
