namespace BrightStepsAcademy.Domain;

public enum AccountEmailType
{
    NewAccountCreated = 0,
    PasswordChanged = 1,
    PasswordReset = 2,
    AccountActivated = 3,
    AccountDeactivated = 4
}

public enum AccountEmailDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}

public enum PortalAccountType
{
    Student = 0,
    Teacher = 1,
    Guardian = 2,
    Admin = 3,
    Staff = 4
}

public class AccountEmailLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? SchoolId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public AccountEmailType EmailType { get; set; }
    public PortalAccountType AccountType { get; set; }
    public AccountEmailDeliveryStatus Status { get; set; } = AccountEmailDeliveryStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
