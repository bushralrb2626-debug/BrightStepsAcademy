namespace BrightStepsAcademy.Models;

public class School
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public int Students { get; set; }
    public int Teachers { get; set; }
}

public class UserAccount
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "";
    public string School { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string Avatar { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Title { get; set; } = "";
}

public class Student
{
    public string Id { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Photo { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string Section { get; set; } = "";
    public string ParentName { get; set; } = "";
    public string ParentId { get; set; } = "";
    public int Attendance { get; set; }
    public string Status { get; set; } = "Active";
    public string Email { get; set; } = "";
    public string Gender { get; set; } = "";
    public int Age { get; set; }
}

public class Teacher
{
    public string Id { get; set; } = "";
    public string TeacherId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Photo { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Classes { get; set; } = "";
    public int ExperienceYears { get; set; }
    public string Status { get; set; } = "Active";
    public string Bio { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
}

public class Parent
{
    public string Id { get; set; } = "";
    public string ParentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Photo { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Children { get; set; } = "";
    public string Status { get; set; } = "Active";
}

public class SchoolClass
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Grade { get; set; } = "";
    public string Section { get; set; } = "";
    public string Teacher { get; set; } = "";
    public int Students { get; set; }
    public string Room { get; set; } = "";
    public string Schedule { get; set; } = "";
}

public class ProgramItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Accent { get; set; } = "";
}

public class Facility
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Accent { get; set; } = "";
    public bool Featured { get; set; }
    public string Size { get; set; } = "md";
}

public class EventItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public DateTime Date { get; set; }
    public string Location { get; set; } = "";
    public string Time { get; set; } = "";
}

public class Notice
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Category { get; set; } = "General";
    public DateTime Date { get; set; }
}

public class Assignment
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Subject { get; set; } = "";
    public string ClassName { get; set; } = "";
    public DateTime DueDate { get; set; }
    public string Description { get; set; } = "";
    public int SubmissionPercent { get; set; }
    public string Status { get; set; } = "Published";
}

public class AttendanceRow
{
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string Photo { get; set; } = "";
    public string Mark { get; set; } = "Present";
}

public class ResultItem
{
    public string Subject { get; set; } = "";
    public int Marks { get; set; }
    public int Total { get; set; } = 100;
    public string Grade { get; set; } = "";
    public string Performance { get; set; } = "";
}

public class MessageThread
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Avatar { get; set; } = "";
    public string Preview { get; set; } = "";
    public string Time { get; set; } = "";
    public int Unread { get; set; }
    public List<ChatMessage> Messages { get; set; } = [];
}

public class ChatMessage
{
    public string From { get; set; } = "";
    public string Text { get; set; } = "";
    public string Time { get; set; } = "";
    public bool Mine { get; set; }
}

public class GalleryItem
{
    public string Image { get; set; } = "";
    public string Caption { get; set; } = "";
    public string Category { get; set; } = "";
}

public class ActivityItem
{
    public string Title { get; set; } = "";
    public string Image { get; set; } = "";
    public string Description { get; set; } = "";
}

public class FeatureItem
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Accent { get; set; } = "";
}

public class TimelineItem
{
    public string Time { get; set; } = "";
    public string Title { get; set; } = "";
    public string Meta { get; set; } = "";
    public string Accent { get; set; } = "";
}

public class NotificationItem
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string Time { get; set; } = "";
    public string Type { get; set; } = "info";
}

public class ActivityLog
{
    public string Text { get; set; } = "";
    public string Time { get; set; } = "";
    public string Accent { get; set; } = "";
}

public class TimetableSlot
{
    public string Time { get; set; } = "";
    public string Monday { get; set; } = "";
    public string Tuesday { get; set; } = "";
    public string Wednesday { get; set; } = "";
    public string Thursday { get; set; } = "";
    public string Friday { get; set; } = "";
}

public class NavGroup
{
    public string Title { get; set; } = "";
    public List<NavItem> Items { get; set; } = [];
}

public class NavItem
{
    public string Label { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "Index";
}

public class EmptyStateVm
{
    public string Title { get; set; } = "Nothing here yet";
    public string Message { get; set; } = "Check back soon — something wonderful is on the way.";
    public string Emoji { get; set; } = "🎨";
    public string? ActionLabel { get; set; }
    public string? ActionAttr { get; set; }
}

public class DashboardProfile
{
    public string Role { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string Avatar { get; set; } = "";
    public string UserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "Active";
    public string Greeting { get; set; } = "";
    public string Subtitle { get; set; } = "";
}
