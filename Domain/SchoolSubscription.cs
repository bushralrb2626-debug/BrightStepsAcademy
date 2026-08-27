namespace BrightStepsAcademy.Domain;

public class SchoolSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SchoolId { get; set; }
    public School School { get; set; } = null!;

    public string PlanCode { get; set; } = "Standard";
    public string PlanName { get; set; } = "Standard";
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public decimal? Price { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Yearly;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<SubscriptionChangeLog> ChangeLogs { get; set; } = new List<SubscriptionChangeLog>();
}

public class SubscriptionChangeLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SchoolSubscriptionId { get; set; }
    public SchoolSubscription Subscription { get; set; } = null!;
    public Guid? SchoolId { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

public class PlatformSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PlatformName { get; set; } = "BrightSteps Platform";
    public string? LogoPath { get; set; }
    public string? SupportEmail { get; set; }
    public string? SupportPhone { get; set; }
    public string? Website { get; set; }
    public int DefaultSubscriptionMonths { get; set; } = 12;
    public int ExpiryWarningDays { get; set; } = 30;
    public string? AvailablePlansJson { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
