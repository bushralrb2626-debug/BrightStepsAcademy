namespace BrightStepsAcademy.Domain;

public class CampusVisit : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string WhenText { get; set; } = string.Empty;
    public string ChildAge { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public string? UserId { get; set; }

    public School School { get; set; } = null!;
}
