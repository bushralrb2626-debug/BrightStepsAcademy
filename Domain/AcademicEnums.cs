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
    Test = 1,
    Assignment = 2,
    Midterm = 3,
    FinalExam = 4,
    Project = 5,
    Presentation = 6,
    Practical = 7,
    Classwork = 8,
    Other = 9
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
