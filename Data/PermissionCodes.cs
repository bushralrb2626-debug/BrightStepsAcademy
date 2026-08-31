namespace BrightStepsAcademy.Data;

public static class PermissionCodes
{
    // Infrastructure (existing)
    public const string BuildingsView = "buildings.view";
    public const string BuildingsManage = "buildings.manage";
    public const string FloorsManage = "floors.manage";
    public const string RoomsView = "rooms.view";
    public const string RoomsManage = "rooms.manage";
    public const string FurnitureManage = "furniture.manage";

    // School settings
    public const string SchoolProfile = "school.profile";
    public const string WebsiteManage = "website.manage";
    public const string AdminsManage = "admins.manage";
    public const string PermissionsManage = "permissions.manage";

    // Academic & Timetable
    public const string TimetableView = "timetable.view";
    public const string TimetableCreate = "timetable.create";
    public const string TimetableEdit = "timetable.edit";
    public const string TimetableDelete = "timetable.delete";
    public const string TimetableClassManage = "timetable.class.manage";
    public const string TimetableTeacherManage = "timetable.teacher.manage";
    public const string TimetableRoomManage = "timetable.room.manage";

    // Fee Management
    public const string FeesRecordsView = "fees.records.view";
    public const string FeesVouchersCreate = "fees.vouchers.create";
    public const string FeesVouchersEdit = "fees.vouchers.edit";
    public const string FeesVouchersSend = "fees.vouchers.send";
    public const string FeesPaymentsView = "fees.payments.view";
    public const string FeesStructureManage = "fees.structure.manage";
    public const string FeesHistoryView = "fees.history.view";

    // Class & Student Management
    public const string ClassesView = "classes.view";
    public const string ClassesManage = "classes.manage";
    public const string SectionsManage = "sections.manage";
    public const string ClassCapacitySet = "classes.capacity.set";
    public const string ClassStrengthManage = "classes.strength.manage";
    public const string StudentAllocationView = "students.allocation.view";
    public const string StudentsAssign = "students.assign";
    public const string StudentsTransfer = "students.transfer";
    public const string StudentsView = "students.view";
    public const string StudentsManage = "students.manage";

    // Staff Management
    public const string StaffView = "staff.view";
    public const string StaffManage = "staff.manage";
    public const string TeachersView = "teachers.view";
    public const string TeacherAssignmentsManage = "teacher.assignments.manage";
    public const string StaffCategoriesView = "staff.categories.view";

    // Parent / Guardian Management
    public const string GuardiansView = "guardians.view";
    public const string GuardiansManage = "guardians.manage";
    public const string GuardiansAccountsView = "guardians.accounts.view";

    // Attendance
    public const string AttendanceView = "attendance.view";
    public const string AttendanceManage = "attendance.manage";
    public const string AttendanceReportsView = "attendance.reports.view";

    // Academic Records
    public const string MarksView = "marks.view";
    public const string GradebookView = "gradebook.view";
    public const string AcademicReportsView = "academic.reports.view";

    // Announcements & Information
    public const string AnnouncementsView = "announcements.view";
    public const string AnnouncementsCreate = "announcements.create";
    public const string AnnouncementsEdit = "announcements.edit";
    public const string AnnouncementsDelete = "announcements.delete";
    public const string ImportantInfoManage = "important.info.manage";

    // Course Material
    public const string MaterialsView = "materials.view";
    public const string MaterialsUpload = "materials.upload";
    public const string MaterialsEdit = "materials.edit";
    public const string MaterialsDelete = "materials.delete";
    public const string MaterialsAttachmentsManage = "materials.attachments.manage";

    // Reports
    public const string ReportsView = "reports.view";
    public const string ReportsStudents = "reports.students";
    public const string ReportsAttendance = "reports.attendance";
    public const string ReportsFees = "reports.fees";
    public const string ReportsClassStrength = "reports.class-strength";
    public const string ReportsExport = "reports.export";
}
