namespace BrightStepsAcademy.Domain;

public enum SchoolStatus
{
    /// <summary>Kept as 0 so existing Active schools migrate cleanly.</summary>
    Active = 0,
    Inactive = 1,
    Pending = 2,
    Suspended = 3,
    Expired = 4
}

public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    ExpiringSoon = 2,
    Expired = 3,
    Suspended = 4,
    Cancelled = 5
}

public enum BillingCycle
{
    Monthly = 0,
    Yearly = 1,
    Custom = 2
}
