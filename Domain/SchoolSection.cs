namespace BrightStepsAcademy.Domain;

public class SchoolSection : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid SchoolClassId { get; set; }
    public string Name { get; set; } = string.Empty;

    public School School { get; set; } = null!;
    public SchoolClass SchoolClass { get; set; } = null!;
}
