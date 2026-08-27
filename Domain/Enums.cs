namespace BrightStepsAcademy.Domain;

public enum AppRoles
{
    SuperAdmin = 0,
    SchoolAdmin = 1,
    CustomAdmin = 2,
    Staff = 3,
    Student = 4
}

public enum FurnitureCondition
{
    New = 0,
    Good = 1,
    Fair = 2,
    Damaged = 3,
    NeedsRepair = 4,
    Unusable = 5
}

public enum RoomTypeKind
{
    Classroom = 0,
    Laboratory = 1,
    ComputerLab = 2,
    Library = 3,
    StaffRoom = 4,
    PrincipalOffice = 5,
    AdminOffice = 6,
    MeetingRoom = 7,
    Auditorium = 8,
    ExaminationRoom = 9,
    MedicalRoom = 10,
    Reception = 11,
    Store = 12,
    ActivityRoom = 13,
    Other = 14
}

public enum RecordStatus
{
    Active = 0,
    Inactive = 1
}
