namespace BrightStepsAcademy.Domain;

public class GalleryItem : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }

    public School School { get; set; } = null!;
}
