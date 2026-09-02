namespace BrightStepsAcademy.Domain;

public enum FeeVoucherStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public class FeeStructureItem : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? SchoolClassId { get; set; }
    public string? BillingFrequency { get; set; }
    public bool IsActive { get; set; } = true;

    public School School { get; set; } = null!;
    public SchoolClass? SchoolClass { get; set; }
}

public class FeeVoucher : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StudentId { get; set; }
    public string VoucherNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public FeeVoucherStatus Status { get; set; } = FeeVoucherStatus.Draft;
    public string? Notes { get; set; }
    public bool SentToGuardian { get; set; }

    public School School { get; set; } = null!;
    public StudentRecord Student { get; set; } = null!;
    public ICollection<FeePayment> Payments { get; set; } = new List<FeePayment>();
}

public class FeePayment : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid FeeVoucherId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public School School { get; set; } = null!;
    public FeeVoucher FeeVoucher { get; set; } = null!;
}
