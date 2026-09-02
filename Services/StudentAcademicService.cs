using BrightStepsAcademy.Data;
using BrightStepsAcademy.Domain;
using BrightStepsAcademy.Models;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Services;

public interface IStudentAcademicService
{
    Task<StudentRecord?> GetStudentForUserAsync(string userId, CancellationToken ct = default);
    Task<bool> OwnsStudentAsync(string userId, Guid studentId, CancellationToken ct = default);
    Task<StudentDashboardVm> BuildDashboardAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<StudentDiaryItemVm>> GetDiaryAsync(StudentRecord student, StudentDiaryFilterVm? filter, CancellationToken ct = default);
    Task<IReadOnlyList<StudentTimetableSlotVm>> GetTimetableAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<StudentTimetableSlotVm>> GetTodayTimetableAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<StudentAssignmentItemVm>> GetAssignmentsAsync(StudentRecord student, StudentAssignmentFilterVm? filter, CancellationToken ct = default);
    Task<StudentAssignmentDetailVm?> GetAssignmentAsync(StudentRecord student, Guid assignmentId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentMaterialItemVm>> GetMaterialsAsync(StudentRecord student, StudentMaterialFilterVm? filter, CancellationToken ct = default);
    Task<IReadOnlyList<StudentMarkItemVm>> GetMarksAsync(StudentRecord student, StudentMarkFilterVm? filter, CancellationToken ct = default);
    Task<IReadOnlyList<StudentPerformanceSubjectVm>> GetPerformanceAsync(StudentRecord student, CancellationToken ct = default);
    Task<StudentAttendanceSummaryVm> GetAttendanceAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<StudentAnnouncementItemVm>> GetAnnouncementsAsync(StudentRecord student, StudentAnnouncementFilterVm? filter, CancellationToken ct = default);
    Task<IReadOnlyList<StudentInfoItemVm>> GetImportantInformationAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<StudentExamItemVm>> GetExamsAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<StudentExamItemVm>> GetPreviousExamsAsync(StudentRecord student, CancellationToken ct = default);
    Task<StudentExamResultsVm?> GetExamResultsAsync(StudentRecord student, CancellationToken ct = default);
    Task<IReadOnlyList<AppNotification>> GetNotificationsAsync(string userId, Guid schoolId, CancellationToken ct = default);
    Task<StudentSearchResultsVm> SearchAsync(StudentRecord student, string query, CancellationToken ct = default);
    Task<IReadOnlyList<AcademicAttachment>> GetAttachmentsAsync(AcademicAttachmentOwnerType ownerType, Guid ownerId, CancellationToken ct = default);
}

public class StudentAcademicService(AppDbContext db) : IStudentAcademicService
{
    public Task<StudentRecord?> GetStudentForUserAsync(string userId, CancellationToken ct = default)
        => db.StudentRecords.AsNoTracking()
            .Include(s => s.SchoolClass)
            .Include(s => s.SchoolSection)
            .Include(s => s.School)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive, ct);

    public async Task<bool> OwnsStudentAsync(string userId, Guid studentId, CancellationToken ct = default)
    {
        var student = await GetStudentForUserAsync(userId, ct);
        return student is not null && student.Id == studentId;
    }

    public async Task<StudentDashboardVm> BuildDashboardAsync(StudentRecord student, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var vm = new StudentDashboardVm
        {
            StudentId = student.Id,
            FullName = student.FullName,
            StudentCode = student.StudentCode,
            RollNumber = student.RollNumber,
            ProfileImagePath = student.ProfileImagePath,
            ClassDisplay = FormatClassDisplay(student),
            SchoolName = student.School?.Name ?? ""
        };

        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return vm;

        vm.TodayTimetable = await GetTodayTimetableAsync(student, ct);
        vm.TodayDiary = (await GetDiaryAsync(student, new StudentDiaryFilterVm { Date = today }, ct)).Take(5).ToList();
        vm.UpcomingAssignments = (await GetAssignmentsAsync(student, null, ct))
            .Where(a => a.DisplayStatus is StudentAssignmentDisplayStatus.Pending or StudentAssignmentDisplayStatus.Upcoming)
            .Take(5).ToList();
        vm.PendingAssignmentCount = (await GetAssignmentsAsync(student, null, ct))
            .Count(a => a.DisplayStatus is StudentAssignmentDisplayStatus.Pending or StudentAssignmentDisplayStatus.Upcoming);
        vm.RecentAnnouncements = (await GetAnnouncementsAsync(student, null, ct)).Take(5).ToList();
        vm.RecentMaterials = (await GetMaterialsAsync(student, null, ct)).Take(5).ToList();

        var attendance = await GetAttendanceAsync(student, ct);
        vm.AttendancePercentage = attendance.OverallPercentage;
        vm.AttendancePresent = attendance.Present;
        vm.AttendanceAbsent = attendance.Absent;
        vm.AttendanceLate = attendance.Late;

        var marks = await GetMarksAsync(student, null, ct);
        vm.RecentMarks = marks.Take(5).ToList();
        vm.LatestResultPercentage = marks.FirstOrDefault()?.Percentage;
        vm.RecentAverage = marks.Where(m => m.Percentage.HasValue).Select(m => m.Percentage!.Value).DefaultIfEmpty()
            .Average();
        if (marks.Count == 0) vm.RecentAverage = null;

        vm.UpcomingExams = (await GetExamsAsync(student, ct)).Take(5).ToList();
        vm.UpcomingExamCount = vm.UpcomingExams.Count;

        vm.Notifications = await GetNotificationsAsync(student.UserId ?? "", student.SchoolId, ct);
        vm.UnreadNotificationCount = vm.Notifications.Count(n => !n.IsRead);

        return vm;
    }

    public async Task<IReadOnlyList<StudentDiaryItemVm>> GetDiaryAsync(StudentRecord student, StudentDiaryFilterVm? filter, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentDiaryItemVm>();

        var query = db.DailyDiaryEntries.AsNoTracking()
            .Where(d => d.Status == PublishStatus.Published
                        && d.SchoolClassId == student.SchoolClassId
                        && d.SchoolSectionId == student.SchoolSectionId);

        if (filter?.SubjectId is Guid subjectId)
            query = query.Where(d => d.SubjectId == subjectId);
        if (filter?.Date is DateOnly date)
            query = query.Where(d => d.ContentDate == date);
        if (filter?.StaffMemberId is Guid staffId)
            query = query.Where(d => d.StaffMemberId == staffId);

        return await query
            .Join(db.Subjects.AsNoTracking(), d => d.SubjectId, s => s.Id, (d, s) => new { d, s })
            .Join(db.StaffMembers.AsNoTracking(), x => x.d.StaffMemberId, st => st.Id, (x, st) => new StudentDiaryItemVm
            {
                Id = x.d.Id,
                Title = x.d.Title,
                Topic = x.d.Topic,
                Classwork = x.d.Description,
                Homework = x.d.Homework,
                Instructions = x.d.Instructions,
                ContentDate = x.d.ContentDate,
                DueDate = x.d.DueDate,
                SubjectName = x.s.Name,
                TeacherName = st.FullName
            })
            .OrderByDescending(x => x.ContentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentTimetableSlotVm>> GetTimetableAsync(StudentRecord student, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentTimetableSlotVm>();

        return await db.ClassTimetableSlots.AsNoTracking()
            .Where(t => t.Status == PublishStatus.Published
                        && t.SchoolClassId == student.SchoolClassId
                        && t.SchoolSectionId == student.SchoolSectionId)
            .Join(db.Subjects.AsNoTracking(), t => t.SubjectId, s => s.Id, (t, s) => new { t, s })
            .GroupJoin(db.StaffMembers.AsNoTracking(), x => x.t.StaffMemberId, st => st.Id, (x, staff) => new { x.t, x.s, staff })
            .SelectMany(x => x.staff.DefaultIfEmpty(), (x, st) => new { x.t, x.s, st })
            .GroupJoin(db.Rooms.AsNoTracking(), x => x.t.RoomId, r => r.Id, (x, rooms) => new { x.t, x.s, x.st, rooms })
            .SelectMany(x => x.rooms.DefaultIfEmpty(), (x, room) => new StudentTimetableSlotVm
            {
                DayOfWeek = x.t.DayOfWeek,
                PeriodOrder = x.t.PeriodOrder,
                PeriodLabel = x.t.PeriodLabel,
                StartTime = x.t.StartTime,
                EndTime = x.t.EndTime,
                SubjectName = x.s.Name,
                TeacherName = x.st != null ? x.st.FullName : null,
                RoomName = room != null ? (room.RoomName ?? room.RoomNumber) : null
            })
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.PeriodOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentTimetableSlotVm>> GetTodayTimetableAsync(StudentRecord student, CancellationToken ct = default)
    {
        var today = DateTime.Today.DayOfWeek;
        var slots = await GetTimetableAsync(student, ct);
        return slots.Where(s => s.DayOfWeek == today).ToList();
    }

    public async Task<IReadOnlyList<StudentAssignmentItemVm>> GetAssignmentsAsync(StudentRecord student, StudentAssignmentFilterVm? filter, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentAssignmentItemVm>();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var query = db.ClassAssignmentItems.AsNoTracking()
            .Where(a => a.Status == PublishStatus.Published
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId);

        if (filter?.SubjectId is Guid subjectId)
            query = query.Where(a => a.SubjectId == subjectId);

        var items = await query
            .Join(db.Subjects.AsNoTracking(), a => a.SubjectId, s => s.Id, (a, s) => new { a, s })
            .Join(db.StaffMembers.AsNoTracking(), x => x.a.StaffMemberId, st => st.Id, (x, st) => new { x.a, x.s, st })
            .ToListAsync(ct);

        var assignmentIds = items.Select(x => x.a.Id).ToList();
        var submissions = await db.ClassAssignmentSubmissions.AsNoTracking()
            .Where(s => s.StudentId == student.Id && assignmentIds.Contains(s.AssignmentId))
            .ToDictionaryAsync(s => s.AssignmentId, ct);

        var result = items.Select(x =>
        {
            submissions.TryGetValue(x.a.Id, out var submission);
            return MapAssignment(x.a, x.s.Name, x.st.FullName, submission, today);
        }).ToList();

        if (filter?.Status is StudentAssignmentDisplayStatus status)
            result = result.Where(a => a.DisplayStatus == status).ToList();
        if (filter?.DueBefore is DateOnly dueBefore)
            result = result.Where(a => a.DueDate <= dueBefore).ToList();

        return result.OrderByDescending(a => a.AssignedDate).ToList();
    }

    public async Task<StudentAssignmentDetailVm?> GetAssignmentAsync(StudentRecord student, Guid assignmentId, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return null;

        var row = await db.ClassAssignmentItems.AsNoTracking()
            .Where(a => a.Id == assignmentId
                        && a.Status == PublishStatus.Published
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId)
            .Join(db.Subjects.AsNoTracking(), a => a.SubjectId, s => s.Id, (a, s) => new { a, s })
            .Join(db.StaffMembers.AsNoTracking(), x => x.a.StaffMemberId, st => st.Id, (x, st) => new { x.a, x.s, st })
            .FirstOrDefaultAsync(ct);

        if (row is null) return null;

        var submission = await db.ClassAssignmentSubmissions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == student.Id, ct);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var item = MapAssignment(row.a, row.s.Name, row.st.FullName, submission, today);

        return new StudentAssignmentDetailVm
        {
            Id = item.Id,
            Title = item.Title,
            Description = row.a.Description,
            SubjectName = item.SubjectName,
            TeacherName = item.TeacherName,
            AssignedDate = item.AssignedDate,
            DueDate = item.DueDate,
            TotalMarks = item.TotalMarks,
            AttachmentFileName = row.a.AttachmentFileName,
            HasAttachment = !string.IsNullOrEmpty(row.a.AttachmentPath),
            AllowSubmission = row.a.AllowSubmission,
            DisplayStatus = item.DisplayStatus,
            Submission = submission is null ? null : new StudentSubmissionVm
            {
                SubmittedAt = submission.SubmittedAt,
                TextResponse = submission.TextResponse,
                FileName = submission.FileName,
                ReviewStatus = submission.ReviewStatus,
                ObtainedMarks = submission.ObtainedMarks,
                TeacherFeedback = submission.TeacherFeedback
            }
        };
    }

    public async Task<IReadOnlyList<StudentMaterialItemVm>> GetMaterialsAsync(StudentRecord student, StudentMaterialFilterVm? filter, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentMaterialItemVm>();

        var query = db.CourseMaterials.AsNoTracking()
            .Where(m => m.Status == PublishStatus.Published
                        && m.SchoolClassId == student.SchoolClassId
                        && m.SchoolSectionId == student.SchoolSectionId);

        if (filter?.SubjectId is Guid subjectId)
            query = query.Where(m => m.SubjectId == subjectId);
        if (filter?.Category is CourseMaterialCategory category)
            query = query.Where(m => m.Category == category);
        if (filter?.Date is DateOnly date)
            query = query.Where(m => m.ContentDate == date);

        return await query
            .Join(db.Subjects.AsNoTracking(), m => m.SubjectId, s => s.Id, (m, s) => new StudentMaterialItemVm
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Category = m.Category,
                ContentDate = m.ContentDate,
                FileName = m.FileName,
                SubjectName = s.Name,
                HasFile = !string.IsNullOrEmpty(m.FilePath)
            })
            .OrderByDescending(m => m.ContentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentMarkItemVm>> GetMarksAsync(StudentRecord student, StudentMarkFilterVm? filter, CancellationToken ct = default)
    {
        var query = db.AssessmentMarks.AsNoTracking()
            .Where(m => m.StudentId == student.Id)
            .Join(db.Assessments.AsNoTracking().Where(a => a.Status == PublishStatus.Published),
                m => m.AssessmentId, a => a.Id, (m, a) => new { m, a });

        if (filter?.SubjectId is Guid subjectId)
            query = query.Where(x => x.a.SubjectId == subjectId);
        if (filter?.AssessmentType is AssessmentType type)
            query = query.Where(x => x.a.AssessmentType == type);
        if (filter?.Date is DateOnly date)
            query = query.Where(x => x.a.AssessmentDate == date);

        return await query
            .Join(db.Subjects.AsNoTracking(), x => x.a.SubjectId, s => s.Id, (x, s) => new StudentMarkItemVm
            {
                AssessmentId = x.a.Id,
                AssessmentName = x.a.Name,
                AssessmentType = x.a.AssessmentType,
                AssessmentDate = x.a.AssessmentDate,
                ObtainedMarks = x.m.ObtainedMarks,
                TotalMarks = x.a.TotalMarks,
                Percentage = x.m.Percentage,
                GradeLabel = x.m.GradeLabel,
                SubjectName = s.Name
            })
            .OrderByDescending(x => x.AssessmentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentPerformanceSubjectVm>> GetPerformanceAsync(StudentRecord student, CancellationToken ct = default)
    {
        var marks = await GetMarksAsync(student, null, ct);
        return marks
            .Where(m => m.Percentage.HasValue)
            .GroupBy(m => m.SubjectName)
            .Select(g => new StudentPerformanceSubjectVm
            {
                SubjectName = g.Key,
                AveragePercentage = Math.Round(g.Average(x => x.Percentage!.Value), 1),
                AssessmentCount = g.Count()
            })
            .OrderByDescending(x => x.AveragePercentage)
            .ToList();
    }

    public async Task<StudentAttendanceSummaryVm> GetAttendanceAsync(StudentRecord student, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return new StudentAttendanceSummaryVm();

        var records = await db.AttendanceRecords.AsNoTracking()
            .Where(r => r.StudentId == student.Id)
            .Join(db.AttendanceSessions.AsNoTracking(), r => r.AttendanceSessionId, s => s.Id, (r, s) => new { r, s })
            .Where(x => x.s.SchoolClassId == student.SchoolClassId && x.s.SchoolSectionId == student.SchoolSectionId)
            .Select(x => new StudentAttendanceDayVm
            {
                SessionDate = x.s.SessionDate,
                PeriodLabel = x.s.PeriodLabel,
                Status = x.r.Status,
                SubjectName = db.Subjects.Where(sub => sub.Id == x.s.SubjectId).Select(sub => sub.Name).FirstOrDefault() ?? ""
            })
            .OrderByDescending(x => x.SessionDate)
            .ToListAsync(ct);

        var present = records.Count(r => r.Status == AttendanceStatus.Present);
        var absent = records.Count(r => r.Status == AttendanceStatus.Absent);
        var late = records.Count(r => r.Status == AttendanceStatus.Late);
        var excused = records.Count(r => r.Status == AttendanceStatus.Excused);
        var total = records.Count;
        var pct = total == 0 ? 0 : Math.Round((decimal)(present + late) / total * 100, 1);

        var currentMonth = DateTime.Today.Month;
        var currentYear = DateTime.Today.Year;
        var monthRecords = records.Where(r => r.SessionDate.Month == currentMonth && r.SessionDate.Year == currentYear).ToList();

        return new StudentAttendanceSummaryVm
        {
            Present = present,
            Absent = absent,
            Late = late,
            Excused = excused,
            OverallPercentage = pct,
            MonthPresent = monthRecords.Count(r => r.Status == AttendanceStatus.Present),
            MonthAbsent = monthRecords.Count(r => r.Status == AttendanceStatus.Absent),
            MonthLate = monthRecords.Count(r => r.Status == AttendanceStatus.Late),
            MonthLabel = DateTime.Today.ToString("MMMM yyyy"),
            DailyHistory = records.Take(90).ToList()
        };
    }

    public async Task<IReadOnlyList<StudentAnnouncementItemVm>> GetAnnouncementsAsync(StudentRecord student, StudentAnnouncementFilterVm? filter, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentAnnouncementItemVm>();

        var query = db.ClassAnnouncements.AsNoTracking()
            .Where(a => a.Status == PublishStatus.Published
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId);

        if (filter?.SubjectId is Guid subjectId)
            query = query.Where(a => a.SubjectId == subjectId);
        if (filter?.Date is DateOnly date)
            query = query.Where(a => a.ContentDate == date);

        return await query
            .Join(db.Subjects.AsNoTracking(), a => a.SubjectId, s => s.Id, (a, s) => new { a, s })
            .Join(db.StaffMembers.AsNoTracking(), x => x.a.StaffMemberId, st => st.Id, (x, st) => new StudentAnnouncementItemVm
            {
                Id = x.a.Id,
                Title = x.a.Title,
                Message = x.a.Message,
                ContentDate = x.a.ContentDate,
                SubjectName = x.s.Name,
                AuthorName = st.FullName
            })
            .OrderByDescending(x => x.ContentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentInfoItemVm>> GetImportantInformationAsync(StudentRecord student, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentInfoItemVm>();

        return await db.ImportantInformationItems.AsNoTracking()
            .Where(i => i.Status == PublishStatus.Published
                        && i.SchoolClassId == student.SchoolClassId
                        && i.SchoolSectionId == student.SchoolSectionId)
            .Join(db.Subjects.AsNoTracking(), i => i.SubjectId, s => s.Id, (i, s) => new { i, s })
            .Join(db.StaffMembers.AsNoTracking(), x => x.i.StaffMemberId, st => st.Id, (x, st) => new StudentInfoItemVm
            {
                Id = x.i.Id,
                Title = x.i.Title,
                Description = x.i.Description,
                ContentDate = x.i.ContentDate,
                SubjectName = x.s.Name,
                AuthorName = st.FullName
            })
            .OrderByDescending(x => x.ContentDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentExamItemVm>> GetExamsAsync(StudentRecord student, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentExamItemVm>();

        var examTypes = new[] { AssessmentType.Midterm, AssessmentType.FinalExam, AssessmentType.Test };
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await db.Assessments.AsNoTracking()
            .Where(a => a.Status == PublishStatus.Published
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId
                        && examTypes.Contains(a.AssessmentType)
                        && a.AssessmentDate >= today)
            .Join(db.Subjects.AsNoTracking(), a => a.SubjectId, s => s.Id, (a, s) => new StudentExamItemVm
            {
                Id = a.Id,
                Name = a.Name,
                AssessmentType = a.AssessmentType,
                ExamDate = a.AssessmentDate,
                TotalMarks = a.TotalMarks,
                Instructions = a.Description,
                SubjectName = s.Name
            })
            .OrderBy(x => x.ExamDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StudentExamItemVm>> GetPreviousExamsAsync(StudentRecord student, CancellationToken ct = default)
    {
        if (student.SchoolClassId is null || student.SchoolSectionId is null)
            return Array.Empty<StudentExamItemVm>();

        var examTypes = new[] { AssessmentType.Midterm, AssessmentType.FinalExam, AssessmentType.Test };
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await db.Assessments.AsNoTracking()
            .Where(a => a.Status == PublishStatus.Published
                        && a.SchoolClassId == student.SchoolClassId
                        && a.SchoolSectionId == student.SchoolSectionId
                        && examTypes.Contains(a.AssessmentType)
                        && a.AssessmentDate < today)
            .Join(db.Subjects.AsNoTracking(), a => a.SubjectId, s => s.Id, (a, s) => new StudentExamItemVm
            {
                Id = a.Id,
                Name = a.Name,
                AssessmentType = a.AssessmentType,
                ExamDate = a.AssessmentDate,
                TotalMarks = a.TotalMarks,
                Instructions = a.Description,
                SubjectName = s.Name
            })
            .OrderByDescending(x => x.ExamDate)
            .ToListAsync(ct);
    }

    public async Task<StudentExamResultsVm?> GetExamResultsAsync(StudentRecord student, CancellationToken ct = default)
    {
        var examTypes = new[] { AssessmentType.Midterm, AssessmentType.FinalExam };
        var marks = await GetMarksAsync(student, null, ct);
        var examMarks = marks.Where(m => examTypes.Contains(m.AssessmentType)).ToList();
        if (examMarks.Count == 0) return null;

        var overallPct = examMarks.Where(m => m.Percentage.HasValue).Select(m => m.Percentage!.Value).DefaultIfEmpty(0).Average();
        return new StudentExamResultsVm
        {
            SubjectResults = examMarks.Select(m => new StudentExamSubjectResultVm
            {
                SubjectName = m.SubjectName,
                AssessmentName = m.AssessmentName,
                Percentage = m.Percentage,
                GradeLabel = m.GradeLabel
            }).ToList(),
            OverallPercentage = Math.Round(overallPct, 1)
        };
    }

    public async Task<IReadOnlyList<AppNotification>> GetNotificationsAsync(string userId, Guid schoolId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Array.Empty<AppNotification>();

        return await db.AppNotifications.AsNoTracking()
            .Where(n => n.UserId == userId && (n.SchoolId == null || n.SchoolId == schoolId))
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<StudentSearchResultsVm> SearchAsync(StudentRecord student, string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new StudentSearchResultsVm();

        var q = query.Trim();
        var diary = (await GetDiaryAsync(student, null, ct))
            .Where(d => Contains(d.Title, q) || Contains(d.Topic, q) || Contains(d.Homework, q) || Contains(d.SubjectName, q))
            .Take(10).ToList();
        var assignments = (await GetAssignmentsAsync(student, null, ct))
            .Where(a => Contains(a.Title, q) || Contains(a.SubjectName, q))
            .Take(10).ToList();
        var materials = (await GetMaterialsAsync(student, null, ct))
            .Where(m => Contains(m.Title, q) || Contains(m.SubjectName, q))
            .Take(10).ToList();
        var announcements = (await GetAnnouncementsAsync(student, null, ct))
            .Where(a => Contains(a.Title, q) || Contains(a.Message, q))
            .Take(10).ToList();
        var info = (await GetImportantInformationAsync(student, ct))
            .Where(i => Contains(i.Title, q) || Contains(i.Description, q))
            .Take(10).ToList();

        return new StudentSearchResultsVm
        {
            Query = q,
            Diary = diary,
            Assignments = assignments,
            Materials = materials,
            Announcements = announcements,
            ImportantInformation = info
        };
    }

    public Task<IReadOnlyList<AcademicAttachment>> GetAttachmentsAsync(AcademicAttachmentOwnerType ownerType, Guid ownerId, CancellationToken ct = default)
        => db.AcademicAttachments.AsNoTracking()
            .Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId)
            .OrderBy(a => a.FileName)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<AcademicAttachment>)t.Result, ct);

    private static StudentAssignmentItemVm MapAssignment(
        ClassAssignmentItem a,
        string subjectName,
        string teacherName,
        ClassAssignmentSubmission? submission,
        DateOnly today)
    {
        var status = ResolveAssignmentStatus(a, submission, today);
        return new StudentAssignmentItemVm
        {
            Id = a.Id,
            Title = a.Title,
            SubjectName = subjectName,
            TeacherName = teacherName,
            Description = a.Description,
            AssignedDate = a.ContentDate,
            DueDate = a.DueDate,
            TotalMarks = a.TotalMarks,
            HasAttachment = !string.IsNullOrEmpty(a.AttachmentPath),
            AllowSubmission = a.AllowSubmission,
            DisplayStatus = status
        };
    }

    internal static StudentAssignmentDisplayStatus ResolveAssignmentStatus(
        ClassAssignmentItem a,
        ClassAssignmentSubmission? submission,
        DateOnly today)
    {
        if (submission is not null)
        {
            if (submission.ReviewStatus is AssignmentSubmissionStatus.Graded or AssignmentSubmissionStatus.Returned)
                return StudentAssignmentDisplayStatus.Completed;
            return StudentAssignmentDisplayStatus.Submitted;
        }

        if (a.DueDate > today)
            return StudentAssignmentDisplayStatus.Upcoming;
        if (a.DueDate == today)
            return StudentAssignmentDisplayStatus.Pending;
        return StudentAssignmentDisplayStatus.Late;
    }

    private static string FormatClassDisplay(StudentRecord student)
    {
        var cls = student.SchoolClass?.Name ?? student.ClassName ?? "—";
        var sec = student.SchoolSection?.Name ?? student.Section ?? "—";
        return $"{cls} · {sec}";
    }

    private static bool Contains(string? haystack, string needle)
        => !string.IsNullOrEmpty(haystack) && haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
