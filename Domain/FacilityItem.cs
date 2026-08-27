namespace BrightStepsAcademy.Domain;

public class FacilityItem : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int DisplayOrder { get; set; }

    public School School { get; set; } = null!;
}
