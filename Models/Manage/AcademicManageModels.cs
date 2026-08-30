namespace BrightStepsAcademy.Models.Manage;

public class SchoolClassFormVm
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = "";
    public string? GradeLevel { get; set; }
    public int DisplayOrder { get; set; }
}

public class SchoolSectionFormVm
{
    public Guid? Id { get; set; }
    public Guid SchoolClassId { get; set; }
    public string Name { get; set; } = "";
}

public class SubjectFormVm
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = "";
    public string? Code { get; set; }
}

public class TeacherAssignmentListVm
{
    public Guid Id { get; set; }
    public string Teacher { get; set; } = "";
    public string Class { get; set; } = "";
    public string Section { get; set; } = "";
    public string Subject { get; set; } = "";
    public string? ScheduleNotes { get; set; }
}

public class TeacherAssignmentFormVm
{
    public Guid? Id { get; set; }
    public Guid StaffMemberId { get; set; }
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public Guid SubjectId { get; set; }
    public string? ScheduleNotes { get; set; }
}
