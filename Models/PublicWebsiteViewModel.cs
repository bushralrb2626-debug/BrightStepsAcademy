namespace BrightStepsAcademy.Models;

public class PublicWebsiteViewModel
{
    public Guid SchoolId { get; set; }
    public string SchoolCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Tagline { get; set; }
    public string LogoPath { get; set; } = string.Empty;
    public PublicHeroDto Hero { get; set; } = new();
    public PublicAboutDto About { get; set; } = new();
    public IReadOnlyList<PublicHighlightDto> Highlights { get; set; } = [];
    public IReadOnlyList<PublicFacilityDto> Facilities { get; set; } = [];
    public IReadOnlyList<PublicGalleryDto> Gallery { get; set; } = [];
    public PublicContactDto Contact { get; set; } = new();
}

public class PublicHeroDto
{
    public string? Eyebrow { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string? CtaText { get; set; }
    public string? CtaLink { get; set; }
}

public class PublicAboutDto
{
    public string Heading { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
}

public class PublicHighlightDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageOrIcon { get; set; }
    public int DisplayOrder { get; set; }
}

public class PublicFacilityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public int DisplayOrder { get; set; }
}

public class PublicGalleryDto
{
    public string ImagePath { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
}

public class PublicContactDto
{
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? OfficeHours { get; set; }
    public string? MapEmbed { get; set; }
}
