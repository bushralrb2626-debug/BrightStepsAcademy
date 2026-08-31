using System.ComponentModel.DataAnnotations;
using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

    [ValidateNever]
    [MaxLength(256)]
    public string? Name { get; set; }

    public AssessmentType AssessmentType { get; set; } = AssessmentType.Test;
    public DateOnly AssessmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal TotalMarks { get; set; } = 100;
    public decimal PassingMarks { get; set; } = 40;
    public string? ImportantInfo { get; set; }
    public PublishStatus Status { get; set; } = PublishStatus.Draft;
    public List<AssessmentScoreColumnVm> Columns { get; set; } = new();
    public List<AssessmentMarkRowVm> Marks { get; set; } = new();
}

public class AssessmentScoreColumnVm
{
    public string Key { get; set; } = "";

    public AssessmentType AssessmentType { get; set; } = AssessmentType.Test;

    [ValidateNever]
    public string? Name { get; set; }

    public decimal MaxMarks { get; set; } = 100;
}

public class AssessmentMarksFormVm
{
    public Guid AssessmentId { get; set; }

    [ValidateNever]
    [MaxLength(256)]
    public string? Name { get; set; }

    public AssessmentType AssessmentType { get; set; } = AssessmentType.Test;
    public DateOnly AssessmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public decimal TotalMarks { get; set; }
    public decimal PassingMarks { get; set; } = 40;
    public string? ImportantInfo { get; set; }
    public PublishStatus Status { get; set; }
    public List<AssessmentScoreColumnVm> Columns { get; set; } = new();
    public List<AssessmentMarkRowVm> Marks { get; set; } = new();
}

public class AssessmentMarkRowVm
{
    public Guid StudentId { get; set; }
    public string FullName { get; set; } = "";
    public string? RollNumber { get; set; }
    public decimal ObtainedMarks { get; set; }
    public List<decimal> ColumnScores { get; set; } = new();
    public string? Notes { get; set; }
}

public class AssessmentMarksGridVm
{
    public string? ImportantInfo { get; set; }
    public List<AssessmentScoreColumnVm> Columns { get; set; } = new();
    public List<AssessmentMarkRowVm> Marks { get; set; } = new();
    public string ColumnsFieldPrefix { get; set; } = "Columns";
    public string MarksFieldPrefix { get; set; } = "Marks";
    public bool AllowColumnEdit { get; set; } = true;
}

public class AssessmentListItemVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public AssessmentType AssessmentType { get; set; }
    public string AssessmentTypeLabel { get; set; } = "";
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

public class ReportCardVm
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = "";
    public string StudentCode { get; set; } = "";
    public string? RollNumber { get; set; }
    public string ClassDisplay { get; set; } = "";
    public string SchoolName { get; set; } = "";
    public string SessionLabel { get; set; } = "";
    public DateOnly GeneratedDate { get; set; }
    public int AttendancePresent { get; set; }
    public int AttendanceAbsent { get; set; }
    public int AttendanceLate { get; set; }
    public int AttendanceExcused { get; set; }
    public int AttendanceTotal { get; set; }
    public decimal AttendancePercentage { get; set; }
    public List<ReportCardTypeColumnVm> AssessmentTypes { get; set; } = new();
    public List<ReportCardSubjectRowVm> Subjects { get; set; } = new();
    public decimal OverallObtained { get; set; }
    public decimal OverallTotal { get; set; }
    public decimal OverallPercentage { get; set; }
    public string? OverallGrade { get; set; }
    public bool HasMarks => Subjects.Any(s => s.TotalMarks > 0);
}

public class ReportCardTypeColumnVm
{
    public AssessmentType AssessmentType { get; set; }
    public string Label { get; set; } = "";
}

public class ReportCardSubjectRowVm
{
    public string SubjectName { get; set; } = "";
    public List<ReportCardMarkCellVm?> Cells { get; set; } = new();
    public decimal ObtainedMarks { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal? Percentage { get; set; }
    public string? GradeLabel { get; set; }
}

public class ReportCardMarkCellVm
{
    public decimal ObtainedMarks { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal? Percentage { get; set; }
    public string? GradeLabel { get; set; }
}
