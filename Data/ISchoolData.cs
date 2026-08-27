using BrightStepsAcademy.Models;

namespace BrightStepsAcademy.Data;

/// <summary>
/// Frontend data contract. Swap MockSchoolData for a database-backed implementation later
/// without changing controllers or views.
/// </summary>
public interface ISchoolData
{
    IReadOnlyList<School> Schools { get; }
    IReadOnlyList<UserAccount> Users { get; }
    IReadOnlyList<Student> Students { get; }
    IReadOnlyList<Teacher> Teachers { get; }
    IReadOnlyList<Parent> Parents { get; }
    IReadOnlyList<SchoolClass> Classes { get; }
    IReadOnlyList<ProgramItem> Programs { get; }
    IReadOnlyList<Facility> Facilities { get; }
    IReadOnlyList<FeatureItem> Features { get; }
    IReadOnlyList<EventItem> Events { get; }
    IReadOnlyList<Notice> Notices { get; }
    IReadOnlyList<Assignment> Assignments { get; }
    IReadOnlyList<AttendanceRow> Attendance { get; }
    IReadOnlyList<ResultItem> Results { get; }
    IReadOnlyList<MessageThread> Threads { get; }
    IReadOnlyList<GalleryItem> Gallery { get; }
    IReadOnlyList<ActivityItem> Activities { get; }
    IReadOnlyList<NotificationItem> Notifications { get; }
    IReadOnlyList<ActivityLog> RecentActivity { get; }
    IReadOnlyList<TimelineItem> TodaySchedule { get; }
    IReadOnlyList<TimetableSlot> Timetable { get; }
    IReadOnlyList<string> Subjects { get; }
    DashboardProfile ProfileFor(string role);
}
