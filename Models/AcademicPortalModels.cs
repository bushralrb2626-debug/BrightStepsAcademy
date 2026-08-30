using System.ComponentModel.DataAnnotations;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BrightStepsAcademy.Models;

public class TeacherPortalContextVm
{
    public IReadOnlyList<TeacherAssignmentOptionVm> Assignments { get; set; } = Array.Empty<TeacherAssignmentOptionVm>();
    public Guid? SelectedAssignmentId { get; set; }
    public TeacherAssignmentOptionVm? SelectedAssignment =>
        Assignments.FirstOrDefault(a => a.Id == SelectedAssignmentId) ?? Assignments.FirstOrDefault();
}

public class TeacherAssignmentOptionVm
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
    public Guid SchoolClassId { get; set; }
    public Guid SchoolSectionId { get; set; }
    public Guid SubjectId { get; set; }
    public int StudentCount { get; set; }
}

public class TeacherDashboardVm
{
    public int AssignmentCount { get; set; }
    public int StudentCount { get; set; }
    public int DraftDiaryCount { get; set; }
    public int PublishedDiaryCount { get; set; }
    public IReadOnlyList<TeacherAssignmentOptionVm> Assignments { get; set; } = Array.Empty<TeacherAssignmentOptionVm>();
}

public class DiaryEntryFormVm
{
    public Guid? Id { get; set; }
    public Guid AssignmentId { get; set; }

    [Required, MaxLength(256)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }
    public string? Topic { get; set; }
    public string? Homework { get; set; }
    public string? Instructions { get; set; }
    public DateOnly ContentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? DueDate { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public List<IFormFile>? Attachments { get; set; }
}

public class DiaryEntryListItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public DateOnly ContentDate { get; set; }
    public PublishStatus Status { get; set; }
    public string? Topic { get; set; }
    public string AssignmentLabel { get; set; } = "";
}

public class AnnouncementFormVm
{
    public Guid? Id { get; set; }
    public Guid AssignmentId { get; set; }

    [Required, MaxLength(256)]
    public string Title { get; set; } = "";

    [Required, MaxLength(4000)]
    public string Message { get; set; } = "";

    public DateOnly ContentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public List<IFormFile>? Attachments { get; set; }
}

public class AnnouncementListItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateOnly ContentDate { get; set; }
    public PublishStatus Status { get; set; }
    public string AssignmentLabel { get; set; } = "";
}

public class CourseMaterialFormVm
{
    public Guid? Id { get; set; }
    public Guid AssignmentId { get; set; }

    [Required, MaxLength(256)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }
    public CourseMaterialCategory Category { get; set; } = CourseMaterialCategory.Other;
    public DateOnly ContentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public bool VisibleToParents { get; set; } = true;
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public IFormFile? File { get; set; }
}

public class CourseMaterialListItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public CourseMaterialCategory Category { get; set; }
    public DateOnly ContentDate { get; set; }
    public PublishStatus Status { get; set; }
    public bool VisibleToParents { get; set; }
    public string? FileName { get; set; }
    public string AssignmentLabel { get; set; } = "";
}

public class AttendanceSessionFormVm
{
    public Guid AssignmentId { get; set; }
    public DateOnly SessionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? PeriodLabel { get; set; }
    public string? Notes { get; set; }
    public List<AttendanceStudentRowVm> Students { get; set; } = new();
}

public class AttendanceStudentRowVm
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string? RollNumber { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
}

public class AssessmentFormVm
{
    public Guid? Id { get; set; }
    public Guid AssignmentId { get; set; }

    [Required, MaxLength(256)]
    public string Name { get; set; } = "";

    public AssessmentType AssessmentType { get; set; } = AssessmentType.Quiz;
    public DateOnly AssessmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal TotalMarks { get; set; } = 100;
    public decimal PassingMarks { get; set; } = 40;
    public string? Description { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
}

public class AssessmentMarksFormVm
{
    public Guid AssessmentId { get; set; }
    public string AssessmentName { get; set; } = "";
    public decimal TotalMarks { get; set; }
    public PublishStatus Status { get; set; }
    public List<AssessmentMarkRowVm> Marks { get; set; } = new();
}

public class AssessmentMarkRowVm
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string? RollNumber { get; set; }
    public decimal ObtainedMarks { get; set; }
    public string? Notes { get; set; }
}

public class AssessmentListItemVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public AssessmentType AssessmentType { get; set; }
    public DateOnly AssessmentDate { get; set; }
    public decimal TotalMarks { get; set; }
    public PublishStatus Status { get; set; }
    public string AssignmentLabel { get; set; } = "";
}

public class TeacherChangePasswordVm
{
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}

public class ParentStudentScopeVm
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string ClassDisplay { get; set; } = "";
    public IReadOnlyList<SelectListItem> Children { get; set; } = Array.Empty<SelectListItem>();
}

public class ParentDiaryItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Topic { get; set; }
    public string? Homework { get; set; }
    public string? Instructions { get; set; }
    public DateOnly ContentDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string SubjectName { get; set; } = "";
}

public class ParentAnnouncementItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public DateOnly ContentDate { get; set; }
    public string SubjectName { get; set; } = "";
}

public class ParentMaterialItemVm
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public CourseMaterialCategory Category { get; set; }
    public DateOnly ContentDate { get; set; }
    public string? FileName { get; set; }
    public string SubjectName { get; set; } = "";
}

public class ParentMarkItemVm
{
    public Guid AssessmentId { get; set; }
    public string AssessmentName { get; set; } = "";
    public AssessmentType AssessmentType { get; set; }
    public DateOnly AssessmentDate { get; set; }
    public decimal ObtainedMarks { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal? Percentage { get; set; }
    public string? GradeLabel { get; set; }
    public string SubjectName { get; set; } = "";
}

public class ParentAttendanceItemVm
{
    public DateOnly SessionDate { get; set; }
    public string? PeriodLabel { get; set; }
    public AttendanceStatus Status { get; set; }
    public string SubjectName { get; set; } = "";
}
