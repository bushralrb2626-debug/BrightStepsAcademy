namespace BrightStepsAcademy.Domain;

public class AboutContent : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }

    public School School { get; set; } = null!;
}
