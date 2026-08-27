namespace BrightStepsAcademy.Domain;

public class StaffMember : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid StaffCategoryId { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeId { get; set; }
    public string? Designation { get; set; }
    public string? Qualification { get; set; }
    public string? Department { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public string? Address { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool HasLoginAccess { get; set; }
    public string? UserId { get; set; }

    public School School { get; set; } = null!;
    public StaffCategory StaffCategory { get; set; } = null!;
}
