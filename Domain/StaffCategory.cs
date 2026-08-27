namespace BrightStepsAcademy.Domain;

public class StaffCategory : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public School School { get; set; } = null!;
    public ICollection<StaffMember> StaffMembers { get; set; } = new List<StaffMember>();
}
