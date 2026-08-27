using System.ComponentModel.DataAnnotations;
using BrightStepsAcademy.Domain;

namespace BrightStepsAcademy.Models.Manage;

public class SchoolAdminFormVm
{
    public Guid SchoolId { get; set; }
    public string SchoolName { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public bool HasAdmin { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(256, ErrorMessage = "Full name cannot exceed 256 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Login ID is required.")]
    [StringLength(128, ErrorMessage = "Login ID cannot exceed 128 characters.")]
    [Display(Name = "Login ID")]
    public string LoginId { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Temporary password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary password")]
    public string? TemporaryPassword { get; set; }

    [StringLength(64)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Status")]
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public bool IsActive => Status == RecordStatus.Active;
}

public class SchoolAdminResetPasswordVm
{
    public Guid SchoolId { get; set; }

    [Required(ErrorMessage = "New temporary password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "New temporary password")]
    public string TemporaryPassword { get; set; } = string.Empty;
}
