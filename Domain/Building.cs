namespace BrightStepsAcademy.Domain;

public class Building : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BuildingNumber { get; set; }
    public string? Description { get; set; }

    public School School { get; set; } = null!;
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
