namespace BrightStepsAcademy.Domain;

public class ContactContent : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? OfficeHours { get; set; }
    public string? MapEmbed { get; set; }

    public School School { get; set; } = null!;
}
