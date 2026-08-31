using System.ComponentModel.DataAnnotations;

namespace BrightStepsAcademy.Domain;

public enum PublishStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    Excused = 3
}

public enum AssessmentType
{
    Quiz = 0,

    [Display(Name = "Class Test")]
    Test = 1,

    Assignment = 2,

    [Display(Name = "Mid Term")]
    Midterm = 3,

    [Display(Name = "Final Term")]
    FinalExam = 4,

    [Display(Name = "Bi-Monthly")]
    BiMonthly = 5,

    Project = 6,
    Presentation = 7,
    Practical = 8,
    Classwork = 9,
    Other = 10
}

public enum CourseMaterialCategory
{
    LectureNotes = 0,
    Presentations = 1,
    Worksheets = 2,
    Assignments = 3,
    ReadingMaterial = 4,
    ReferenceMaterial = 5,
    PastPapers = 6,
    StudyGuides = 7,
    VideosLinks = 8,
    Other = 9
}

public enum AcademicAttachmentOwnerType
{
    DailyDiary = 0,
    ImportantInformation = 1,
    Announcement = 2,
    CourseMaterial = 3,
    Assessment = 4
}
