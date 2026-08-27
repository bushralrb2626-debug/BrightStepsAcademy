namespace BrightStepsAcademy.Domain;

public class HeroContent : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string? CtaText { get; set; }
    public string? CtaLink { get; set; }

    public School School { get; set; } = null!;
}
