namespace BrightStepsAcademy.Domain;

public class Subject : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }

    public School School { get; set; } = null!;
}
