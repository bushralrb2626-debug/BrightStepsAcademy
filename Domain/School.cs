namespace BrightStepsAcademy.Domain;

public class School
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SchoolCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Tagline { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? PrincipalName { get; set; }
    public int? EstablishedYear { get; set; }
    public string? SchoolType { get; set; }
    public string? Description { get; set; }
    public string? EmergencyContact { get; set; }
    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactPhone { get; set; }
    public string? LogoPath { get; set; }
    public string? FaviconPath { get; set; }
    public SchoolStatus Status { get; set; } = SchoolStatus.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public SchoolSubscription? Subscription { get; set; }

    public ICollection<Building> Buildings { get; set; } = new List<Building>();
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<FurnitureItem> FurnitureItems { get; set; } = new List<FurnitureItem>();
    public ICollection<StaffCategory> StaffCategories { get; set; } = new List<StaffCategory>();
    public ICollection<StaffMember> StaffMembers { get; set; } = new List<StaffMember>();
    public ICollection<StudentRecord> Students { get; set; } = new List<StudentRecord>();
    public ICollection<SchoolAdminProfile> AdminProfiles { get; set; } = new List<SchoolAdminProfile>();
    public ICollection<UserPermissionGrant> PermissionGrants { get; set; } = new List<UserPermissionGrant>();
    public WebsiteSettings? WebsiteSettings { get; set; }
    public HeroContent? HeroContent { get; set; }
    public AboutContent? AboutContent { get; set; }
    public ContactContent? ContactContent { get; set; }
    public ICollection<HighlightItem> HighlightItems { get; set; } = new List<HighlightItem>();
    public ICollection<FacilityItem> FacilityItems { get; set; } = new List<FacilityItem>();
    public ICollection<GalleryItem> GalleryItems { get; set; } = new List<GalleryItem>();
}
