using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BrightStepsAcademy.Models.Manage;

public class TimetableSlotListVm
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public DayOfWeek DayOfWeek { get; set; }
    public int PeriodOrder { get; set; }
    public string? PeriodLabel { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string SubjectName { get; set; } = "";
    public string? TeacherName { get; set; }
    public string? RoomName { get; set; }
    public PublishStatus Status { get; set; }
}

public class TimetableSlotFormVm
{
    public Guid? Id { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
    public int PeriodOrder { get; set; } = 1;
    public string? PeriodLabel { get; set; }
    public TimeOnly StartTime { get; set; } = new(8, 0);
    public TimeOnly EndTime { get; set; } = new(8, 45);
    public Guid SubjectId { get; set; }
    public Guid? StaffMemberId { get; set; }
    public Guid? RoomId { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Published;
    public List<SelectListItem> ClassOptions { get; set; } = new();
    public List<SelectListItem> SectionOptions { get; set; } = new();
    public List<SelectListItem> SubjectOptions { get; set; } = new();
    public List<SelectListItem> TeacherOptions { get; set; } = new();
    public List<SelectListItem> RoomOptions { get; set; } = new();
}

public class TimetableFilterVm
{
    public Guid? SchoolClassId { get; set; }
    public Guid? SchoolSectionId { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public List<SelectListItem> ClassOptions { get; set; } = new();
    public List<SelectListItem> SectionOptions { get; set; } = new();
}

public class FeeStructureListVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? ClassName { get; set; }
    public decimal Amount { get; set; }
    public string? BillingFrequency { get; set; }
    public bool IsActive { get; set; }
}

public class FeeStructureFormVm
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Guid? SchoolClassId { get; set; }
    public string? BillingFrequency { get; set; }
    public bool IsActive { get; set; } = true;
    public List<SelectListItem> ClassOptions { get; set; } = new();
}

public class FeeVoucherListVm
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public FeeVoucherStatus Status { get; set; }
    public bool SentToGuardian { get; set; }
}

public class FeeVoucherFormVm
{
    public Guid? Id { get; set; }
    public Guid StudentId { get; set; }
    public string Title { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(14));
    public string? Notes { get; set; }
    public List<SelectListItem> StudentOptions { get; set; } = new();
}

public class FeePaymentFormVm
{
    public Guid FeeVoucherId { get; set; }
    public string VoucherNumber { get; set; } = "";
    public string StudentName { get; set; } = "";
    public decimal Balance { get; set; }
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? PaymentMethod { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class FeeDashboardVm
{
    public int StructureCount { get; set; }
    public int VoucherCount { get; set; }
    public int PendingCount { get; set; }
    public int PaidCount { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalCollected { get; set; }
}
