namespace BrightStepsAcademy.Services;

public interface ITenantContext
{
    Guid? SchoolId { get; }
    string? UserId { get; }
    bool IsSuperAdmin { get; }
}
