namespace BrightStepsAcademy.Domain;

public class GradingRule : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string GradeLabel { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public decimal? GradePoint { get; set; }
    public int DisplayOrder { get; set; }

    public School School { get; set; } = null!;
}
