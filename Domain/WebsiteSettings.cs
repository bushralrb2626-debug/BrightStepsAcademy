namespace BrightStepsAcademy.Domain;

public class WebsiteSettings : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public bool IsPublished { get; set; }

    public School School { get; set; } = null!;
}
