using BrightStepsAcademy.Domain;

namespace BrightStepsAcademy.Services;

public static class SubscriptionStatusHelper
{
    public static readonly string[] DefaultPlans = ["Basic", "Standard", "Premium", "Enterprise"];

    public static SubscriptionStatus Compute(DateTimeOffset start, DateTimeOffset expiry, int warningDays, SubscriptionStatus? forced = null)
    {
        if (forced is SubscriptionStatus.Suspended or SubscriptionStatus.Cancelled or SubscriptionStatus.Trial)
            return forced.Value;

        var now = DateTimeOffset.UtcNow;
        if (expiry.Date < now.Date)
            return SubscriptionStatus.Expired;

        var daysLeft = (expiry.Date - now.Date).TotalDays;
        if (daysLeft <= warningDays)
            return SubscriptionStatus.ExpiringSoon;

        if (start.Date > now.Date)
            return SubscriptionStatus.Trial;

        return SubscriptionStatus.Active;
    }

    public static void Refresh(SchoolSubscription sub, int warningDays = 30)
    {
        if (sub.Status is SubscriptionStatus.Suspended or SubscriptionStatus.Cancelled)
            return;

        sub.Status = Compute(sub.StartDate, sub.ExpiryDate, warningDays, null);
        sub.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static bool AllowsSchoolAccess(SchoolStatus schoolStatus, SubscriptionStatus? subStatus)
    {
        if (schoolStatus != SchoolStatus.Active)
            return false;

        if (subStatus is SubscriptionStatus.Expired or SubscriptionStatus.Suspended or SubscriptionStatus.Cancelled)
            return false;

        return true;
    }
}
