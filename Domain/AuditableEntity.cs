namespace BrightStepsAcademy.Domain;

public abstract class AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
}
