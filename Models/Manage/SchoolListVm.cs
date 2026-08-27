using BrightStepsAcademy.Domain;

namespace BrightStepsAcademy.Models.Manage;

public class SchoolListVm
{
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public string? SubscriptionFilter { get; set; }
    public string? DateFilter { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public IReadOnlyList<SchoolListItemVm> Schools { get; set; } = Array.Empty<SchoolListItemVm>();
}

public class SchoolListItemVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SchoolCode { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public SchoolStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool HasAdmin { get; set; }
    public string? AdminName { get; set; }
    public string? AdminEmail { get; set; }
    public string? PlanName { get; set; }
    public SubscriptionStatus? SubscriptionStatus { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
}

public class SchoolDetailsVm
{
    public BrightStepsAcademy.Domain.School School { get; set; } = null!;
    public SchoolAdminSummaryVm? PrimaryAdmin { get; set; }
    public SchoolSubscription? Subscription { get; set; }
    public int Buildings { get; set; }
    public int Floors { get; set; }
    public int Rooms { get; set; }
    public int Staff { get; set; }
    public int Students { get; set; }
    public int Administrators { get; set; }
    public int Furniture { get; set; }
}

public class SchoolAdminSummaryVm
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LoginId { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}
