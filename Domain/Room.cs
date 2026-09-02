namespace BrightStepsAcademy.Domain;

public class Room : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid BuildingId { get; set; }
    public Guid FloorId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string? RoomName { get; set; }
    public string RoomType { get; set; } = nameof(RoomTypeKind.Classroom);
    public int? Capacity { get; set; }
    public string? Description { get; set; }
    public Guid? SchoolClassId { get; set; }
    public Guid? SchoolSectionId { get; set; }

    public School School { get; set; } = null!;
    public Building Building { get; set; } = null!;
    public Floor Floor { get; set; } = null!;
    public SchoolClass? SchoolClass { get; set; }
    public SchoolSection? SchoolSection { get; set; }
    public ICollection<FurnitureItem> FurnitureItems { get; set; } = new List<FurnitureItem>();
}
