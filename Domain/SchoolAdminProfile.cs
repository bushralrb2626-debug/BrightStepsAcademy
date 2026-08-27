namespace BrightStepsAcademy.Domain;

public class SchoolAdminProfile : AuditableEntity, ISchoolScoped
{
    public string UserId { get; set; } = string.Empty;
    public Guid SchoolId { get; set; }
    public string AdminType { get; set; } = nameof(AppRoles.SchoolAdmin);
    public bool IsPrimary { get; set; }

    public School School { get; set; } = null!;
}
