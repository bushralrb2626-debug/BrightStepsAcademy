using Microsoft.AspNetCore.Identity;

namespace BrightStepsAcademy.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? LoginId { get; set; }
    public string? PhoneAlternate { get; set; }
    public Guid? SchoolId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public string? ProfileImagePath { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
