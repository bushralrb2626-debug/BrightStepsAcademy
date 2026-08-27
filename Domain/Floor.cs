namespace BrightStepsAcademy.Domain;

public class Floor : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid BuildingId { get; set; }
    public int FloorNumber { get; set; }
    public string? FloorName { get; set; }
    public string? Description { get; set; }

    public School School { get; set; } = null!;
    public Building Building { get; set; } = null!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
