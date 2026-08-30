namespace BrightStepsAcademy.Domain;

public class SchoolClass : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GradeLevel { get; set; }
    public int DisplayOrder { get; set; }

    public School School { get; set; } = null!;
    public ICollection<SchoolSection> Sections { get; set; } = new List<SchoolSection>();
}
