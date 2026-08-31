namespace BrightStepsAcademy.Data;

public static class PermissionCatalog
{
    public sealed record Entry(string Code, string Module, string Name, string Description, int SortOrder);

    public static IReadOnlyList<Entry> All { get; } =
    [
        // Academic & Timetable
        new(PermissionCodes.TimetableView, "Academic & Timetable", "View Timetable", "View school timetables.", 10),
        new(PermissionCodes.TimetableCreate, "Academic & Timetable", "Add Timetable", "Create new timetable entries.", 11),
        new(PermissionCodes.TimetableEdit, "Academic & Timetable", "Edit Timetable", "Edit existing timetable entries.", 12),
        new(PermissionCodes.TimetableDelete, "Academic & Timetable", "Delete Timetable", "Delete timetable entries.", 13),
        new(PermissionCodes.TimetableClassManage, "Academic & Timetable", "Manage Class Timetable", "Manage class-level timetables.", 14),
        new(PermissionCodes.TimetableTeacherManage, "Academic & Timetable", "Manage Teacher Timetable", "Manage teacher timetables.", 15),
        new(PermissionCodes.TimetableRoomManage, "Academic & Timetable", "Manage Room Timetable", "Manage room timetables.", 16),

        // Fee Management
        new(PermissionCodes.FeesRecordsView, "Fee Management", "View Fee Records", "View fee records.", 20),
        new(PermissionCodes.FeesVouchersCreate, "Fee Management", "Create Fee Vouchers", "Create fee vouchers.", 21),
        new(PermissionCodes.FeesVouchersEdit, "Fee Management", "Edit Fee Vouchers", "Edit fee vouchers.", 22),
        new(PermissionCodes.FeesVouchersSend, "Fee Management", "Send Fee Vouchers to Parents", "Send fee vouchers to parents.", 23),
        new(PermissionCodes.FeesPaymentsView, "Fee Management", "View Payment Status", "View fee payment status.", 24),
        new(PermissionCodes.FeesStructureManage, "Fee Management", "Manage Fee Structure", "Manage school fee structure.", 25),
        new(PermissionCodes.FeesHistoryView, "Fee Management", "View Fee History", "View historical fee records.", 26),

        // Class & Student Management
        new(PermissionCodes.ClassesView, "Class & Student Management", "View Classes", "View classes and sections.", 30),
        new(PermissionCodes.ClassesManage, "Class & Student Management", "Manage Classes", "Create and edit classes.", 31),
        new(PermissionCodes.SectionsManage, "Class & Student Management", "Manage Sections", "Create and edit sections.", 32),
        new(PermissionCodes.ClassCapacitySet, "Class & Student Management", "Set Class Capacity", "Set class capacity limits.", 33),
        new(PermissionCodes.ClassStrengthManage, "Class & Student Management", "Manage Class Strength", "Manage class strength records.", 34),
        new(PermissionCodes.StudentAllocationView, "Class & Student Management", "View Student Allocation", "View student class/section allocation.", 35),
        new(PermissionCodes.StudentsAssign, "Class & Student Management", "Assign Students to Classes/Sections", "Assign students to classes and sections.", 36),
        new(PermissionCodes.StudentsTransfer, "Class & Student Management", "Transfer Students Between Sections", "Transfer students between sections.", 37),
        new(PermissionCodes.StudentsView, "Class & Student Management", "View Students", "View student records.", 38),
        new(PermissionCodes.StudentsManage, "Class & Student Management", "Manage Students", "Create and edit student records.", 39),

        // Staff Management
        new(PermissionCodes.StaffView, "Staff Management", "View Staff", "View staff members.", 40),
        new(PermissionCodes.TeachersView, "Staff Management", "View Teachers", "View teacher records.", 41),
        new(PermissionCodes.TeacherAssignmentsManage, "Staff Management", "Manage Teacher Assignments", "Manage teacher class/subject assignments.", 42),
        new(PermissionCodes.StaffCategoriesView, "Staff Management", "View Staff Categories", "View staff categories.", 43),
        new(PermissionCodes.StaffManage, "Staff Management", "Manage Staff", "Create and edit staff records.", 44),

        // Parent / Guardian Management
        new(PermissionCodes.GuardiansView, "Parent/Guardian Management", "View Guardian Information", "View guardian information.", 50),
        new(PermissionCodes.GuardiansManage, "Parent/Guardian Management", "Manage Guardian Information", "Create and edit guardian information.", 51),
        new(PermissionCodes.GuardiansAccountsView, "Parent/Guardian Management", "View Guardian Accounts", "View guardian portal accounts.", 52),

        // Attendance
        new(PermissionCodes.AttendanceView, "Attendance", "View Attendance", "View attendance records.", 60),
        new(PermissionCodes.AttendanceManage, "Attendance", "Manage Attendance", "Record and edit attendance.", 61),
        new(PermissionCodes.AttendanceReportsView, "Attendance", "View Attendance Reports", "View attendance reports.", 62),

        // Academic Records
        new(PermissionCodes.MarksView, "Academic Records", "View Marks", "View published student marks.", 70),
        new(PermissionCodes.GradebookView, "Academic Records", "View Grade Books", "View grade book records.", 71),
        new(PermissionCodes.AcademicReportsView, "Academic Records", "View Academic Reports", "View academic performance reports.", 72),

        // Announcements & Information
        new(PermissionCodes.AnnouncementsView, "Announcements & Information", "View Announcements", "View announcements.", 80),
        new(PermissionCodes.AnnouncementsCreate, "Announcements & Information", "Create Announcements", "Create announcements.", 81),
        new(PermissionCodes.AnnouncementsEdit, "Announcements & Information", "Edit Announcements", "Edit announcements.", 82),
        new(PermissionCodes.AnnouncementsDelete, "Announcements & Information", "Delete Announcements", "Delete announcements.", 83),
        new(PermissionCodes.ImportantInfoManage, "Announcements & Information", "Manage Important Information", "Manage important school information.", 84),

        // Course Material
        new(PermissionCodes.MaterialsView, "Course Material", "View Course Material", "View course materials.", 90),
        new(PermissionCodes.MaterialsUpload, "Course Material", "Upload Course Material", "Upload course materials.", 91),
        new(PermissionCodes.MaterialsEdit, "Course Material", "Edit Course Material", "Edit course materials.", 92),
        new(PermissionCodes.MaterialsDelete, "Course Material", "Delete Course Material", "Delete course materials.", 93),
        new(PermissionCodes.MaterialsAttachmentsManage, "Course Material", "Manage Attachments", "Manage course material attachments.", 94),

        // Reports
        new(PermissionCodes.ReportsView, "Reports", "View Reports Overview", "View the reports dashboard.", 100),
        new(PermissionCodes.ReportsStudents, "Reports", "View Student Reports", "View student reports.", 101),
        new(PermissionCodes.ReportsAttendance, "Reports", "View Attendance Reports", "View attendance summary reports.", 102),
        new(PermissionCodes.ReportsFees, "Reports", "View Fee Reports", "View fee reports.", 103),
        new(PermissionCodes.ReportsClassStrength, "Reports", "View Class Strength Reports", "View class strength reports.", 104),
        new(PermissionCodes.ReportsExport, "Reports", "Export Permitted Reports", "Export reports the admin can access.", 105),

        // Administration
        new(PermissionCodes.AdminsManage, "Administration", "Manage Administrators", "Create and manage additional admins.", 110),
        new(PermissionCodes.PermissionsManage, "Administration", "Manage Permissions", "Assign permissions to additional admins.", 111),

        // School
        new(PermissionCodes.SchoolProfile, "School", "Manage School Profile", "Edit school profile and branding.", 120),
        new(PermissionCodes.WebsiteManage, "School", "Manage Website", "Manage public website content.", 121),

        // Infrastructure
        new(PermissionCodes.BuildingsView, "Infrastructure", "View Buildings", "View buildings.", 130),
        new(PermissionCodes.BuildingsManage, "Infrastructure", "Manage Buildings", "Create and edit buildings.", 131),
        new(PermissionCodes.FloorsManage, "Infrastructure", "Manage Floors", "Manage building floors.", 132),
        new(PermissionCodes.RoomsView, "Infrastructure", "View Rooms", "View rooms.", 133),
        new(PermissionCodes.RoomsManage, "Infrastructure", "Manage Rooms", "Create and edit rooms.", 134),
        new(PermissionCodes.FurnitureManage, "Infrastructure", "Manage Furniture", "Manage room furniture.", 135),
    ];

    public static IEnumerable<IGrouping<string, Entry>> ByModule()
        => All.GroupBy(e => e.Module).OrderBy(g => g.Min(e => e.SortOrder));

    /// <summary>Broader legacy grants that imply finer-grained permissions.</summary>
    public static IReadOnlyDictionary<string, string[]> ImpliedBy { get; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [PermissionCodes.ClassesView] = [PermissionCodes.StudentsView],
            [PermissionCodes.ClassesManage] = [PermissionCodes.StudentsManage, PermissionCodes.ClassesView],
            [PermissionCodes.SectionsManage] = [PermissionCodes.StudentsManage],
            [PermissionCodes.StudentAllocationView] = [PermissionCodes.StudentsView],
            [PermissionCodes.StudentsAssign] = [PermissionCodes.StudentsManage],
            [PermissionCodes.StudentsTransfer] = [PermissionCodes.StudentsManage],
            [PermissionCodes.TeachersView] = [PermissionCodes.StaffView],
            [PermissionCodes.TeacherAssignmentsManage] = [PermissionCodes.StudentsManage],
            [PermissionCodes.StaffCategoriesView] = [PermissionCodes.StaffView],
            [PermissionCodes.GuardiansView] = [PermissionCodes.StudentsView],
            [PermissionCodes.GuardiansManage] = [PermissionCodes.StudentsManage],
            [PermissionCodes.ReportsStudents] = [PermissionCodes.ReportsView],
            [PermissionCodes.ReportsAttendance] = [PermissionCodes.ReportsView, PermissionCodes.AttendanceReportsView],
            [PermissionCodes.ReportsClassStrength] = [PermissionCodes.ReportsView],
        };
}
