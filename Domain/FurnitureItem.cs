namespace BrightStepsAcademy.Domain;

public class FurnitureItem : AuditableEntity, ISchoolScoped
{
    public Guid SchoolId { get; set; }
    public Guid RoomId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public FurnitureCondition Condition { get; set; } = FurnitureCondition.Good;
    public string? Description { get; set; }
    public DateOnly? PurchaseDate { get; set; }

    public School School { get; set; } = null!;
    public Room Room { get; set; } = null!;
}
