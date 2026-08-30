namespace BrightStepsAcademy.Models;

public class GuardianChildVm
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? ClassName { get; set; }
    public string? Section { get; set; }
    public Guid? SchoolClassId { get; set; }
    public Guid? SchoolSectionId { get; set; }
    public string Relationship { get; set; } = "";

    public string ClassDisplay =>
        string.IsNullOrWhiteSpace(ClassName) ? "—" :
        string.IsNullOrWhiteSpace(Section) ? ClassName! : $"{ClassName} — {Section}";
}

public class GuardianChangePasswordVm
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}
