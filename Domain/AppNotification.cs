namespace BrightStepsAcademy.Domain;

public class AppNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SchoolId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public School? School { get; set; }
}
