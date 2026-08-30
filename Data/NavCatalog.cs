using BrightStepsAcademy.Models;

namespace BrightStepsAcademy.Data;

public static class NavCatalog
{
    public static IReadOnlyList<NavGroup> For(string role) => role switch
    {
        "SuperAdmin" =>
        [
            G("Main", I("Dashboard", "grid", "SuperAdmin", "Index")),
            G("Management",
                I("Users", "users", "SuperAdmin", "Users"),
                I("Schools", "school", "SuperAdmin", "Schools")),
            G("Communication", I("Messages", "chat", "SuperAdmin", "Messages")),
            G("System",
                I("Reports", "chart", "SuperAdmin", "Reports"),
                I("Settings", "cog", "SuperAdmin", "Settings"))
        ],
        "Admin" =>
        [
            G("Main", I("Dashboard", "grid", "Admin", "Index")),
            G("Management",
                I("Students", "backpack", "Admin", "Students"),
                I("Teachers", "apple", "Admin", "Teachers"),
                I("Parents", "family", "Admin", "Parents"),
                I("Classes", "board", "Admin", "Classes")),
            G("Academic",
                I("Assignments", "book", "Admin", "Assignments"),
                I("Attendance", "check", "Admin", "Attendance"),
                I("Results", "star", "Admin", "Results"),
                I("Timetable", "clock", "Admin", "Timetable")),
            G("Communication",
                I("Notices", "bell", "Admin", "Notices"),
                I("Events", "cal", "Admin", "Events"),
                I("Messages", "chat", "Admin", "Messages")),
            G("System",
                I("Reports", "chart", "Admin", "Reports"),
                I("Settings", "cog", "Admin", "Settings"))
        ],
        "Headmaster" =>
        [
            G("Main",
                I("Dashboard", "grid", "Headmaster", "Index"),
                I("School Overview", "school", "Headmaster", "Overview")),
            G("Management",
                I("Teachers", "apple", "Headmaster", "Teachers"),
                I("Students", "backpack", "Headmaster", "Students"),
                I("Classes", "board", "Headmaster", "Classes")),
            G("Academic",
                I("Attendance", "check", "Headmaster", "Attendance"),
                I("Performance", "star", "Headmaster", "Performance"),
                I("Assignments", "book", "Headmaster", "Assignments"),
                I("Timetable", "clock", "Headmaster", "Timetable")),
            G("Communication",
                I("Notices", "bell", "Headmaster", "Notices"),
                I("Approvals", "stamp", "Headmaster", "Approvals")),
            G("System",
                I("Reports", "chart", "Headmaster", "Reports"),
                I("Settings", "cog", "Headmaster", "Settings"))
        ],
        "TeacherPortal" or "Teacher" =>
        [
            G("Main", I("Dashboard", "grid", "Teacher", "Index")),
            G("Teaching",
                I("My Classes", "board", "Teacher", "Classes"),
                I("Students", "backpack", "Teacher", "Students"),
                I("Daily Diary", "book", "Teacher", "Diary"),
                I("Attendance", "check", "Teacher", "Attendance"),
                I("Grade Book", "star", "Teacher", "GradeBook")),
            G("Content",
                I("Announcements", "bell", "Teacher", "Announcements"),
                I("Course Material", "folder", "Teacher", "Materials")),
            G("Account",
                I("Profile", "user", "Teacher", "Profile"),
                I("Security", "lock", "Teacher", "Security"))
        ],
        "ParentPortal" or "Parent" =>
        [
            G("Main", I("Dashboard", "grid", "Parent", "Index")),
            G("Children",
                I("My Children", "family", "Parent", "Children"),
                I("Daily Diary", "book", "Parent", "Diary"),
                I("Attendance", "check", "Parent", "Attendance"),
                I("Marks", "star", "Parent", "Marks"),
                I("Announcements", "bell", "Parent", "Announcements"),
                I("Course Material", "folder", "Parent", "Materials")),
            G("Account", I("Change Password", "lock", "Parent", "ChangePassword"))
        ],
        _ =>
        [
            G("Main", I("Dashboard", "grid", "Student", "Index")),
            G("Learn",
                I("My Profile", "user", "Student", "Profile"),
                I("My Classes", "board", "Student", "Classes"),
                I("Homework", "pencil", "Student", "Homework"),
                I("Assignments", "book", "Student", "Assignments"),
                I("Attendance", "check", "Student", "Attendance"),
                I("Results", "star", "Student", "Results"),
                I("Timetable", "clock", "Student", "Timetable"),
                I("Achievements", "trophy", "Student", "Achievements")),
            G("School Life",
                I("Notices", "bell", "Student", "Notices"),
                I("Events", "cal", "Student", "Events")),
            G("System", I("Settings", "cog", "Student", "Settings"))
        ]
    };

    private static NavGroup G(string title, params NavItem[] items) =>
        new() { Title = title, Items = [.. items] };

    private static NavItem I(string label, string icon, string controller, string action) =>
        new() { Label = label, Icon = icon, Controller = controller, Action = action };
}
