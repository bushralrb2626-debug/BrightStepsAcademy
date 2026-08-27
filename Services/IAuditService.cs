namespace BrightStepsAcademy.Services;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string module,
        Guid? schoolId = null,
        string? recordType = null,
        string? recordId = null,
        string? details = null,
        CancellationToken cancellationToken = default);
}
