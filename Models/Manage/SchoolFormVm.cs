using System.ComponentModel.DataAnnotations;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Http;

namespace BrightStepsAcademy.Models.Manage;

public class SchoolFormVm
{
    public Guid? Id { get; set; }

    /// <summary>Wizard step: 1 Basic, 2 Location, 3 Branding, 4 Admin, 5 Subscription, 6 Review.</summary>
    public int Step { get; set; } = 1;

    [Required(ErrorMessage = "School name is required.")]
    [StringLength(256, ErrorMessage = "Name cannot exceed 256 characters.")]
    [Display(Name = "School name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "School code is required.")]
    [StringLength(64, ErrorMessage = "School code cannot exceed 64 characters.")]
    [Display(Name = "School code")]
    public string SchoolCode { get; set; } = string.Empty;

    [StringLength(64)]
    [Display(Name = "Short name")]
    public string? ShortName { get; set; }

    [StringLength(256)]
    [Display(Name = "Tagline")]
    public string? Tagline { get; set; }

    [StringLength(128, ErrorMessage = "Registration number cannot exceed 128 characters.")]
    [Display(Name = "Registration number")]
    public string? RegistrationNumber { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(64, ErrorMessage = "Phone cannot exceed 64 characters.")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Address")]
    public string? Address { get; set; }

    [StringLength(128)]
    [Display(Name = "City")]
    public string? City { get; set; }

    [StringLength(128)]
    [Display(Name = "State / Province")]
    public string? StateProvince { get; set; }

    [StringLength(128)]
    [Display(Name = "Country")]
    public string? Country { get; set; }

    [StringLength(32)]
    [Display(Name = "Postal code")]
    public string? PostalCode { get; set; }

    [StringLength(128)]
    [Display(Name = "School type")]
    public string? SchoolType { get; set; }

    [StringLength(256)]
    [Display(Name = "Principal name")]
    public string? PrincipalName { get; set; }

    [Display(Name = "Established year")]
    [Range(1800, 2100)]
    public int? EstablishedYear { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [StringLength(64)]
    [Display(Name = "Emergency contact")]
    public string? EmergencyContact { get; set; }

    [StringLength(256)]
    [Display(Name = "Primary contact name")]
    public string? PrimaryContactName { get; set; }

    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Primary contact email")]
    public string? PrimaryContactEmail { get; set; }

    [StringLength(64)]
    [Display(Name = "Primary contact phone")]
    public string? PrimaryContactPhone { get; set; }

    [StringLength(256)]
    [Url(ErrorMessage = "Enter a valid website URL.")]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [Display(Name = "Status")]
    public SchoolStatus Status { get; set; } = SchoolStatus.Active;

    public string? LogoPath { get; set; }

    [Display(Name = "Logo")]
    public IFormFile? LogoFile { get; set; }

    // ——— Admin (wizard) ———
    [Display(Name = "Create school admin now")]
    public bool CreateAdmin { get; set; } = true;

    [StringLength(256)]
    [Display(Name = "Admin full name")]
    public string? AdminFullName { get; set; }

    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "Admin email")]
    public string? AdminEmail { get; set; }

    [StringLength(128)]
    [Display(Name = "Admin login ID")]
    public string? AdminLoginId { get; set; }

    [StringLength(64)]
    [Display(Name = "Admin phone")]
    public string? AdminPhone { get; set; }

    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary password")]
    public string? AdminTemporaryPassword { get; set; }

    // ——— Subscription (wizard) ———
    [StringLength(64)]
    [Display(Name = "Plan code")]
    public string PlanCode { get; set; } = "Standard";

    [StringLength(128)]
    [Display(Name = "Plan name")]
    public string PlanName { get; set; } = "Standard";

    [Display(Name = "Billing cycle")]
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Yearly;

    [Display(Name = "Start date")]
    [DataType(DataType.Date)]
    public DateTimeOffset SubscriptionStart { get; set; } = DateTimeOffset.UtcNow.Date;

    [Display(Name = "Expiry date")]
    [DataType(DataType.Date)]
    public DateTimeOffset SubscriptionExpiry { get; set; } = DateTimeOffset.UtcNow.Date.AddYears(1);

    [Display(Name = "Price")]
    public decimal? SubscriptionPrice { get; set; }

    [StringLength(2000)]
    [Display(Name = "Subscription notes")]
    public string? SubscriptionNotes { get; set; }
}
