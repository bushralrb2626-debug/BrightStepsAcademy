using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Data;

namespace BrightStepsAcademy.Models.Manage;

public class ChartSliceVm
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
}

public class SchoolDashboardVm
{
    public string SchoolName { get; set; } = "";
    public string? LogoPath { get; set; }
    public int Buildings { get; set; }
    public int Floors { get; set; }
    public int Rooms { get; set; }
    public int Furniture { get; set; }
    public int Staff { get; set; }
    public int Teachers { get; set; }
    public int Students { get; set; }
    public int Admins { get; set; }
    public int Facilities { get; set; }
    public List<ChartSliceVm> StaffByCategory { get; set; } = new();
    public List<ChartSliceVm> RoomsByType { get; set; } = new();
    public List<ChartSliceVm> FurnitureByCondition { get; set; } = new();
    public List<ChartSliceVm> StudentsByClass { get; set; } = new();
    public List<AuditLog> RecentActivity { get; set; } = new();
}

public class SchoolProfileVm
{
    public Guid Id { get; set; }
    public string SchoolCode { get; set; } = "";
    public string Name { get; set; } = "";
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
    public string? LogoPath { get; set; }
    public string? FaviconPath { get; set; }
}

public class SchoolBrandingVm
{
    public string Name { get; set; } = "";
    public string? ShortName { get; set; }
    public string? Tagline { get; set; }
    public string SchoolCode { get; set; } = "";
    public string? LogoPath { get; set; }
    public string? FaviconPath { get; set; }
}

public class ExplorerBuildingVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public List<ExplorerFloorVm> Floors { get; set; } = new();
}

public class ExplorerFloorVm
{
    public Guid Id { get; set; }
    public int FloorNumber { get; set; }
    public string? FloorName { get; set; }
    public bool IsActive { get; set; }
    public List<ExplorerRoomVm> Rooms { get; set; } = new();
}

public class ExplorerRoomVm
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = "";
    public string? RoomName { get; set; }
    public bool IsActive { get; set; }
    public int FurnitureCount { get; set; }
}

public class AdminListItemVm
{
    public string UserId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string AdminType { get; set; } = "";
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
}

public class PermissionsPageVm
{
    public List<AdminListItemVm> Admins { get; set; } = new();
    public List<AppPermission> Permissions { get; set; } = new();
    public string? SelectedUserId { get; set; }
    public HashSet<string> GrantedCodes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class CreateAdminVm
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? LoginId { get; set; }
    public List<string> PermissionCodes { get; set; } = new();
    public List<AppPermission> AllPermissions { get; set; } = new();
}

public class SearchResultVm
{
    public string Query { get; set; } = "";
    public List<SearchHitVm> Hits { get; set; } = new();
}

public class SearchHitVm
{
    public string Entity { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string Url { get; set; } = "";
}

public class RoomFormVm
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public Guid FloorId { get; set; }
    public string RoomNumber { get; set; } = "";
    public string? RoomName { get; set; }
    public string RoomType { get; set; } = nameof(RoomTypeKind.Classroom);
    public int? Capacity { get; set; }
    public string? Description { get; set; }
}

public class FurnitureFormVm
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string Category { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public FurnitureCondition Condition { get; set; } = FurnitureCondition.Good;
    public string? Description { get; set; }
    public DateOnly? PurchaseDate { get; set; }
}

public class StaffFormVm
{
    public Guid Id { get; set; }
    public Guid StaffCategoryId { get; set; }
    public string StaffCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? EmployeeId { get; set; }
    public string? Designation { get; set; }
    public string? Qualification { get; set; }
    public string? Department { get; set; }
    public DateOnly? DateOfJoining { get; set; }
    public string? Address { get; set; }
    public bool HasLoginAccess { get; set; }
    public string? LoginId { get; set; }
    public string? LoginPassword { get; set; }
}

public class StudentFormVm
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? ParentName { get; set; }
    public string? ParentEmail { get; set; }
    public string? ParentPhone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public DateOnly? AdmissionDate { get; set; }
    public string? ClassName { get; set; }
    public string? Section { get; set; }
    public string? RollNumber { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
}

public class WebsiteHubVm
{
    public BrightStepsAcademy.Domain.HeroContent? Hero { get; set; }
    public BrightStepsAcademy.Domain.AboutContent? About { get; set; }
    public BrightStepsAcademy.Domain.ContactContent? Contact { get; set; }
    public List<BrightStepsAcademy.Domain.HighlightItem> Highlights { get; set; } = new();
    public List<BrightStepsAcademy.Domain.FacilityItem> Facilities { get; set; } = new();
    public List<BrightStepsAcademy.Domain.GalleryItem> Gallery { get; set; } = new();
}
