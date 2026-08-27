using BrightStepsAcademy.Domain;

namespace BrightStepsAcademy.Models.Manage;

public class SuperAdminDashboardVm
{
    public int TotalSchools { get; set; }
    public int ActiveSchools { get; set; }
    public int InactiveSchools { get; set; }
    public int PendingSchools { get; set; }
    public int SuspendedSchools { get; set; }
    public int TotalSchoolAdmins { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public int ExpiringSoon { get; set; }
    public IReadOnlyList<SchoolListItemVm> RecentSchools { get; set; } = Array.Empty<SchoolListItemVm>();
    public IReadOnlyList<SchoolListItemVm> ExpiringSchools { get; set; } = Array.Empty<SchoolListItemVm>();
    public IReadOnlyList<AuditLog> RecentAuditLogs { get; set; } = Array.Empty<AuditLog>();
    public IReadOnlyList<AppNotification> RecentNotifications { get; set; } = Array.Empty<AppNotification>();
    public IReadOnlyList<GrowthPointVm> GrowthPoints { get; set; } = Array.Empty<GrowthPointVm>();
}

public class GrowthPointVm
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AuditLogListVm
{
    public string? Search { get; set; }
    public string? ModuleFilter { get; set; }
    public IReadOnlyList<AuditLog> Logs { get; set; } = Array.Empty<AuditLog>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class SubscriptionListVm
{
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public IReadOnlyList<SubscriptionListItemVm> Items { get; set; } = Array.Empty<SubscriptionListItemVm>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public string Title { get; set; } = "Subscriptions";
}

public class SubscriptionListItemVm
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public decimal? Price { get; set; }
}

public class SubscriptionEditVm
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(64)]
    public string PlanCode { get; set; } = "Standard";

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(128)]
    public string PlanName { get; set; } = "Standard";

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
    public DateTimeOffset StartDate { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
    public DateTimeOffset ExpiryDate { get; set; }

    public BillingCycle BillingCycle { get; set; } = BillingCycle.Yearly;
    public decimal? Price { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    [System.ComponentModel.DataAnnotations.StringLength(2000)]
    public string? Notes { get; set; }

    public bool RenewOneYear { get; set; }
}

public class PlatformSchoolAdminListVm
{
    public string? Search { get; set; }
    public IReadOnlyList<PlatformSchoolAdminItemVm> Admins { get; set; } = Array.Empty<PlatformSchoolAdminItemVm>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class PlatformSchoolAdminItemVm
{
    public string UserId { get; set; } = string.Empty;
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LoginId { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimary { get; set; }
}

public class AnalyticsPageVm
{
    public IReadOnlyList<GrowthPointVm> GrowthPoints { get; set; } = Array.Empty<GrowthPointVm>();
    public int TotalSchools { get; set; }
    public int ActiveSchools { get; set; }
    public int PendingSchools { get; set; }
    public int SuspendedSchools { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int ExpiringSoon { get; set; }
    public int ExpiredSubscriptions { get; set; }
    public string GrowthJson { get; set; } = "[]";
    public string StatusJson { get; set; } = "[]";
}

public class PlatformSettingsVm
{
    public Guid Id { get; set; }

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(256)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Platform name")]
    public string PlatformName { get; set; } = "BrightSteps Platform";

    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.StringLength(256)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Support email")]
    public string? SupportEmail { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(64)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Support phone")]
    public string? SupportPhone { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(256)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Website")]
    public string? Website { get; set; }

    [System.ComponentModel.DataAnnotations.Range(1, 120)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Default subscription months")]
    public int DefaultSubscriptionMonths { get; set; } = 12;

    [System.ComponentModel.DataAnnotations.Range(1, 365)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Expiry warning days")]
    public int ExpiryWarningDays { get; set; } = 30;

    [System.ComponentModel.DataAnnotations.Display(Name = "Available plans (comma-separated)")]
    public string? AvailablePlans { get; set; }

    public string? LogoPath { get; set; }
}

public class SuperAdminProfileVm
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(256)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.Display(Name = "Email")]
    public string? Email { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(64)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Phone")]
    public string? Phone { get; set; }

    public string? LoginId { get; set; }
}

public class ChangePasswordVm
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 8)]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    [System.ComponentModel.DataAnnotations.Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [System.ComponentModel.DataAnnotations.Display(Name = "Confirm new password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class NotificationListVm
{
    public IReadOnlyList<AppNotification> Items { get; set; } = Array.Empty<AppNotification>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
