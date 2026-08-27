namespace BrightStepsAcademy.Domain;

public class UserPermissionGrant : AuditableEntity, ISchoolScoped
{
    public string UserId { get; set; } = string.Empty;
    public Guid SchoolId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public bool Granted { get; set; } = true;

    public School School { get; set; } = null!;
}
