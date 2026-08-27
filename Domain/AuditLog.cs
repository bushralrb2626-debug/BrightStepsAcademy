namespace BrightStepsAcademy.Domain;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SchoolId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? RecordType { get; set; }
    public string? RecordId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public School? School { get; set; }
}
