namespace BrightStepsAcademy.Domain;

public class HighlightItem : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageOrIcon { get; set; }
    public int DisplayOrder { get; set; }

    public School School { get; set; } = null!;
}
