using BrightStepsAcademy.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BrightStepsAcademy.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<FurnitureItem> FurnitureItems => Set<FurnitureItem>();
    public DbSet<StaffCategory> StaffCategories => Set<StaffCategory>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<StudentRecord> StudentRecords => Set<StudentRecord>();
    public DbSet<GuardianProfile> GuardianProfiles => Set<GuardianProfile>();
    public DbSet<StudentGuardianLink> StudentGuardianLinks => Set<StudentGuardianLink>();
    public DbSet<SchoolClass> SchoolClasses => Set<SchoolClass>();
    public DbSet<SchoolSection> SchoolSections => Set<SchoolSection>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherAssignment> TeacherAssignments => Set<TeacherAssignment>();
    public DbSet<GradingRule> GradingRules => Set<GradingRule>();
    public DbSet<DailyDiaryEntry> DailyDiaryEntries => Set<DailyDiaryEntry>();
    public DbSet<ImportantInformationItem> ImportantInformationItems => Set<ImportantInformationItem>();
    public DbSet<ClassAnnouncement> ClassAnnouncements => Set<ClassAnnouncement>();
    public DbSet<CourseMaterial> CourseMaterials => Set<CourseMaterial>();
    public DbSet<AcademicAttachment> AcademicAttachments => Set<AcademicAttachment>();
    public DbSet<AttendanceSession> AttendanceSessions => Set<AttendanceSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<AssessmentMark> AssessmentMarks => Set<AssessmentMark>();
    public DbSet<ClassTimetableSlot> ClassTimetableSlots => Set<ClassTimetableSlot>();
    public DbSet<ClassAssignmentItem> ClassAssignmentItems => Set<ClassAssignmentItem>();
    public DbSet<ClassAssignmentSubmission> ClassAssignmentSubmissions => Set<ClassAssignmentSubmission>();
    public DbSet<FeeStructureItem> FeeStructureItems => Set<FeeStructureItem>();
    public DbSet<FeeVoucher> FeeVouchers => Set<FeeVoucher>();
    public DbSet<FeePayment> FeePayments => Set<FeePayment>();
    public DbSet<AppPermission> AppPermissions => Set<AppPermission>();
    public DbSet<SchoolAdminProfile> SchoolAdminProfiles => Set<SchoolAdminProfile>();
    public DbSet<UserPermissionGrant> UserPermissionGrants => Set<UserPermissionGrant>();
    public DbSet<WebsiteSettings> WebsiteSettings => Set<WebsiteSettings>();
    public DbSet<HeroContent> HeroContents => Set<HeroContent>();
    public DbSet<AboutContent> AboutContents => Set<AboutContent>();
    public DbSet<HighlightItem> HighlightItems => Set<HighlightItem>();
    public DbSet<FacilityItem> FacilityItems => Set<FacilityItem>();
    public DbSet<GalleryItem> GalleryItems => Set<GalleryItem>();
    public DbSet<ContactContent> ContactContents => Set<ContactContent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<SchoolSubscription> SchoolSubscriptions => Set<SchoolSubscription>();
    public DbSet<SubscriptionChangeLog> SubscriptionChangeLogs => Set<SubscriptionChangeLog>();
    public DbSet<PlatformSettings> PlatformSettings => Set<PlatformSettings>();
    public DbSet<AccountEmailLog> AccountEmailLogs => Set<AccountEmailLog>();
    public DbSet<CampusVisit> CampusVisits => Set<CampusVisit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureSchool(builder);
        ConfigureBuildingsAndRooms(builder);
        ConfigureStaffAndStudents(builder);
        ConfigureAcademic(builder);
        ConfigurePermissions(builder);
        ConfigureWebsite(builder);
        ConfigureAuditAndNotifications(builder);
        ConfigureAccountEmailLogs(builder);
        ConfigureApplicationUser(builder);
        ConfigureSubscriptionsAndPlatform(builder);
        ConfigureCampusVisits(builder);
    }

    private static void ConfigureSubscriptionsAndPlatform(ModelBuilder builder)
    {
        builder.Entity<SchoolSubscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PlanCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.PlanName).HasMaxLength(128).IsRequired();
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasIndex(x => x.SchoolId).IsUnique();
            e.HasOne(x => x.School)
                .WithOne(s => s.Subscription)
                .HasForeignKey<SchoolSubscription>(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SubscriptionChangeLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Summary).HasMaxLength(512).IsRequired();
            e.Property(x => x.Details).HasMaxLength(4000);
            e.Property(x => x.ChangedByUserName).HasMaxLength(256);
            e.HasOne(x => x.Subscription)
                .WithMany(s => s.ChangeLogs)
                .HasForeignKey(x => x.SchoolSubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PlatformSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PlatformName).HasMaxLength(256).IsRequired();
            e.Property(x => x.SupportEmail).HasMaxLength(256);
            e.Property(x => x.SupportPhone).HasMaxLength(64);
            e.Property(x => x.Website).HasMaxLength(256);
            e.Property(x => x.LogoPath).HasMaxLength(512);
            e.Property(x => x.AvailablePlansJson).HasMaxLength(4000);
        });
    }

    private static void ConfigureSchool(ModelBuilder builder)
    {
        builder.Entity<School>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SchoolCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.ShortName).HasMaxLength(64);
            e.Property(x => x.Tagline).HasMaxLength(256);
            e.Property(x => x.RegistrationNumber).HasMaxLength(128);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.Website).HasMaxLength(256);
            e.Property(x => x.City).HasMaxLength(128);
            e.Property(x => x.StateProvince).HasMaxLength(128);
            e.Property(x => x.Country).HasMaxLength(128);
            e.Property(x => x.PostalCode).HasMaxLength(32);
            e.Property(x => x.PrincipalName).HasMaxLength(256);
            e.Property(x => x.SchoolType).HasMaxLength(128);
            e.Property(x => x.EmergencyContact).HasMaxLength(256);
            e.Property(x => x.PrimaryContactName).HasMaxLength(256);
            e.Property(x => x.PrimaryContactEmail).HasMaxLength(256);
            e.Property(x => x.PrimaryContactPhone).HasMaxLength(64);
            e.Property(x => x.LogoPath).HasMaxLength(512);
            e.Property(x => x.FaviconPath).HasMaxLength(512);
            e.HasIndex(x => x.SchoolCode).IsUnique();
            e.HasIndex(x => x.Status);
        });
    }

    private static void ConfigureBuildingsAndRooms(ModelBuilder builder)
    {
        builder.Entity<Building>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.BuildingNumber).HasMaxLength(64);
            e.HasIndex(x => new { x.SchoolId, x.Name }).IsUnique();

            e.HasOne(x => x.School)
                .WithMany(s => s.Buildings)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Floor>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FloorName).HasMaxLength(128);
            e.HasIndex(x => new { x.SchoolId, x.BuildingId, x.FloorNumber }).IsUnique();

            e.HasOne(x => x.School)
                .WithMany(s => s.Floors)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Building)
                .WithMany(b => b.Floors)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Room>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RoomNumber).HasMaxLength(64).IsRequired();
            e.Property(x => x.RoomName).HasMaxLength(256);
            e.Property(x => x.RoomType).HasMaxLength(64).IsRequired();

            // MANDATORY unique room number within school / building / floor
            e.HasIndex(x => new { x.SchoolId, x.BuildingId, x.FloorId, x.RoomNumber })
                .IsUnique()
                .HasDatabaseName("IX_Rooms_School_Building_Floor_RoomNumber");

            e.HasOne(x => x.School)
                .WithMany(s => s.Rooms)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Building)
                .WithMany(b => b.Rooms)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Floor)
                .WithMany(f => f.Rooms)
                .HasForeignKey(x => x.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SchoolClass)
                .WithMany()
                .HasForeignKey(x => x.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SchoolSection)
                .WithMany()
                .HasForeignKey(x => x.SchoolSectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FurnitureItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Category).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => new { x.SchoolId, x.RoomId, x.Name }).IsUnique();

            e.HasOne(x => x.School)
                .WithMany(s => s.FurnitureItems)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Room)
                .WithMany(r => r.FurnitureItems)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureStaffAndStudents(ModelBuilder builder)
    {
        builder.Entity<StaffCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.HasIndex(x => new { x.SchoolId, x.Name }).IsUnique();

            e.HasOne(x => x.School)
                .WithMany(s => s.StaffCategories)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StaffMember>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StaffCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.EmployeeId).HasMaxLength(64);
            e.Property(x => x.Designation).HasMaxLength(128);
            e.Property(x => x.Qualification).HasMaxLength(256);
            e.Property(x => x.Department).HasMaxLength(128);
            e.Property(x => x.ProfileImagePath).HasMaxLength(512);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.HasIndex(x => new { x.SchoolId, x.StaffCode }).IsUnique();
            e.HasIndex(x => x.UserId);

            e.HasOne(x => x.School)
                .WithMany(s => s.StaffMembers)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.StaffCategory)
                .WithMany(c => c.StaffMembers)
                .HasForeignKey(x => x.StaffCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentCode).HasMaxLength(64).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.ParentName).HasMaxLength(256);
            e.Property(x => x.ParentEmail).HasMaxLength(256);
            e.Property(x => x.ParentPhone).HasMaxLength(64);
            e.Property(x => x.Gender).HasMaxLength(32);
            e.Property(x => x.ClassName).HasMaxLength(64);
            e.Property(x => x.Section).HasMaxLength(32);
            e.Property(x => x.RollNumber).HasMaxLength(64);
            e.Property(x => x.EmergencyContact).HasMaxLength(256);
            e.Property(x => x.ProfileImagePath).HasMaxLength(512);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.HasIndex(x => new { x.SchoolId, x.StudentCode }).IsUnique();
            e.HasIndex(x => x.UserId);

            e.HasOne(x => x.SchoolClass)
                .WithMany()
                .HasForeignKey(x => x.SchoolClassId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SchoolSection)
                .WithMany()
                .HasForeignKey(x => x.SchoolSectionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.School)
                .WithMany(s => s.Students)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GuardianProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.LoginId).HasMaxLength(128);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.SchoolId, x.Email });

            e.HasOne(x => x.School)
                .WithMany(s => s.Guardians)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StudentGuardianLink>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Relationship).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.StudentId).IsUnique();
            e.HasIndex(x => x.GuardianProfileId);

            e.HasOne(x => x.School)
                .WithMany(s => s.StudentGuardianLinks)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Student)
                .WithOne(s => s.GuardianLink)
                .HasForeignKey<StudentGuardianLink>(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Guardian)
                .WithMany(g => g.StudentLinks)
                .HasForeignKey(x => x.GuardianProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAcademic(ModelBuilder builder)
    {
        builder.Entity<SchoolClass>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.GradeLevel).HasMaxLength(64);
            e.HasIndex(x => new { x.SchoolId, x.Name }).IsUnique();
            e.HasOne(x => x.School).WithMany(s => s.SchoolClasses).HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SchoolSection>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();
            e.HasIndex(x => new { x.SchoolClassId, x.Name }).IsUnique();
            e.HasOne(x => x.School).WithMany(s => s.SchoolSections).HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolClass).WithMany(c => c.Sections).HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Subject>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Code).HasMaxLength(32);
            e.HasIndex(x => new { x.SchoolId, x.Name }).IsUnique();
            e.HasOne(x => x.School).WithMany(s => s.Subjects).HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<TeacherAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ScheduleNotes).HasMaxLength(512);
            e.HasIndex(x => new { x.StaffMemberId, x.SchoolClassId, x.SchoolSectionId, x.SubjectId }).IsUnique();
            e.HasOne(x => x.School).WithMany(s => s.TeacherAssignments).HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StaffMember).WithMany().HasForeignKey(x => x.StaffMemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolClass).WithMany().HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolSection).WithMany().HasForeignKey(x => x.SchoolSectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GradingRule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.GradeLabel).HasMaxLength(16).IsRequired();
            e.Property(x => x.MinPercentage).HasPrecision(5, 2);
            e.Property(x => x.MaxPercentage).HasPrecision(5, 2);
            e.Property(x => x.GradePoint).HasPrecision(4, 2);
            e.HasIndex(x => new { x.SchoolId, x.GradeLabel }).IsUnique();
            e.HasOne(x => x.School).WithMany(s => s.GradingRules).HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
        });

        ConfigureClassScopedContent<DailyDiaryEntry>(builder, "DailyDiaryEntries");
        ConfigureClassScopedContent<ImportantInformationItem>(builder, "ImportantInformationItems");
        ConfigureClassScopedContent<ClassAnnouncement>(builder, "ClassAnnouncements");
        builder.Entity<ClassAnnouncement>().Property(x => x.Message).HasMaxLength(4000).IsRequired();

        builder.Entity<CourseMaterial>(e =>
        {
            ConfigureClassScopedContentCore(e);
            e.ToTable("CourseMaterials");
            e.Property(x => x.FilePath).HasMaxLength(512);
            e.Property(x => x.FileName).HasMaxLength(256);
            e.Property(x => x.FileContentType).HasMaxLength(128);
        });

        builder.Entity<AcademicAttachment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            e.Property(x => x.StoredPath).HasMaxLength(512).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            e.HasIndex(x => new { x.OwnerType, x.OwnerId });
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.UploadedByStaffMember).WithMany().HasForeignKey(x => x.UploadedByStaffMemberId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AttendanceSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PeriodLabel).HasMaxLength(64);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => new { x.SchoolClassId, x.SchoolSectionId, x.SubjectId, x.SessionDate, x.PeriodLabel }).IsUnique();
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StaffMember).WithMany().HasForeignKey(x => x.StaffMemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolClass).WithMany().HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolSection).WithMany().HasForeignKey(x => x.SchoolSectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TeacherAssignment).WithMany().HasForeignKey(x => x.TeacherAssignmentId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AttendanceRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Notes).HasMaxLength(512);
            e.HasIndex(x => new { x.AttendanceSessionId, x.StudentId }).IsUnique();
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AttendanceSession).WithMany(s => s.Records).HasForeignKey(x => x.AttendanceSessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Assessment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.ScoreColumnsJson).HasMaxLength(4000);
            e.Property(x => x.TotalMarks).HasPrecision(8, 2);
            e.Property(x => x.PassingMarks).HasPrecision(8, 2);
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StaffMember).WithMany().HasForeignKey(x => x.StaffMemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolClass).WithMany().HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolSection).WithMany().HasForeignKey(x => x.SchoolSectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TeacherAssignment).WithMany().HasForeignKey(x => x.TeacherAssignmentId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AssessmentMark>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ObtainedMarks).HasPrecision(8, 2);
            e.Property(x => x.Percentage).HasPrecision(5, 2);
            e.Property(x => x.GradeLabel).HasMaxLength(16);
            e.Property(x => x.ScoreBreakdownJson).HasMaxLength(4000);
            e.HasIndex(x => new { x.AssessmentId, x.StudentId }).IsUnique();
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Assessment).WithMany(a => a.Marks).HasForeignKey(x => x.AssessmentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DailyDiaryEntry>(e =>
        {
            e.Property(x => x.Topic).HasMaxLength(256);
            e.Property(x => x.Homework).HasMaxLength(2000);
            e.Property(x => x.Instructions).HasMaxLength(2000);
        });

        ConfigureClassScopedContent<ClassAssignmentItem>(builder, "ClassAssignmentItems");
        builder.Entity<ClassAssignmentItem>(e =>
        {
            e.Property(x => x.TotalMarks).HasPrecision(8, 2);
            e.Property(x => x.AttachmentPath).HasMaxLength(512);
            e.Property(x => x.AttachmentFileName).HasMaxLength(256);
            e.Property(x => x.AttachmentContentType).HasMaxLength(128);
        });

        builder.Entity<ClassAssignmentSubmission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TextResponse).HasMaxLength(8000);
            e.Property(x => x.FilePath).HasMaxLength(512);
            e.Property(x => x.FileName).HasMaxLength(256);
            e.Property(x => x.FileContentType).HasMaxLength(128);
            e.Property(x => x.TeacherFeedback).HasMaxLength(2000);
            e.Property(x => x.ObtainedMarks).HasPrecision(8, 2);
            e.HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique();
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Assignment).WithMany(a => a.Submissions).HasForeignKey(x => x.AssignmentId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ClassTimetableSlot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PeriodLabel).HasMaxLength(64);
            e.HasIndex(x => new { x.SchoolClassId, x.SchoolSectionId, x.DayOfWeek, x.PeriodOrder }).IsUnique();
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolClass).WithMany().HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolSection).WithMany().HasForeignKey(x => x.SchoolSectionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.StaffMember).WithMany().HasForeignKey(x => x.StaffMemberId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FeeStructureItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
            e.Property(x => x.Description).HasMaxLength(512);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.BillingFrequency).HasMaxLength(64);
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SchoolClass).WithMany().HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FeeVoucher>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.VoucherNumber).HasMaxLength(32).IsRequired();
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.Property(x => x.TotalAmount).HasPrecision(12, 2);
            e.Property(x => x.PaidAmount).HasPrecision(12, 2);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasIndex(x => new { x.SchoolId, x.VoucherNumber }).IsUnique();
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FeePayment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(12, 2);
            e.Property(x => x.PaymentMethod).HasMaxLength(64);
            e.Property(x => x.Reference).HasMaxLength(128);
            e.Property(x => x.Notes).HasMaxLength(512);
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.FeeVoucher).WithMany(v => v.Payments).HasForeignKey(x => x.FeeVoucherId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureClassScopedContent<T>(ModelBuilder builder, string tableName)
        where T : ClassScopedAcademicContent
    {
        builder.Entity<T>(e =>
        {
            ConfigureClassScopedContentCore(e);
            e.ToTable(tableName);
        });
    }

    private static void ConfigureClassScopedContentCore<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> e)
        where T : ClassScopedAcademicContent
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).HasMaxLength(256).IsRequired();
        e.Property(x => x.Description).HasMaxLength(4000);
        e.HasIndex(x => new { x.SchoolClassId, x.SchoolSectionId, x.SubjectId, x.ContentDate });
        e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.StaffMember).WithMany().HasForeignKey(x => x.StaffMemberId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.SchoolClass).WithMany().HasForeignKey(x => x.SchoolClassId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.SchoolSection).WithMany().HasForeignKey(x => x.SchoolSectionId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
        e.HasOne(x => x.TeacherAssignment).WithMany().HasForeignKey(x => x.TeacherAssignmentId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePermissions(ModelBuilder builder)
    {
        builder.Entity<AppPermission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.Module).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<SchoolAdminProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.AdminType).HasMaxLength(64).IsRequired();
            e.HasIndex(x => new { x.UserId, x.SchoolId }).IsUnique();

            e.HasOne(x => x.School)
                .WithMany(s => s.AdminProfiles)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserPermissionGrant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.PermissionCode).HasMaxLength(128).IsRequired();
            e.HasIndex(x => new { x.UserId, x.SchoolId, x.PermissionCode }).IsUnique();

            e.HasOne(x => x.School)
                .WithMany(s => s.PermissionGrants)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureWebsite(ModelBuilder builder)
    {
        builder.Entity<WebsiteSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PrimaryColor).HasMaxLength(32);
            e.Property(x => x.SecondaryColor).HasMaxLength(32);
            e.HasIndex(x => x.SchoolId).IsUnique();

            e.HasOne(x => x.School)
                .WithOne(s => s.WebsiteSettings)
                .HasForeignKey<WebsiteSettings>(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<HeroContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Heading).HasMaxLength(256).IsRequired();
            e.Property(x => x.ImagePath).HasMaxLength(512);
            e.Property(x => x.CtaText).HasMaxLength(128);
            e.Property(x => x.CtaLink).HasMaxLength(512);
            e.HasIndex(x => x.SchoolId).IsUnique();

            e.HasOne(x => x.School)
                .WithOne(s => s.HeroContent)
                .HasForeignKey<HeroContent>(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AboutContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Heading).HasMaxLength(256).IsRequired();
            e.Property(x => x.ImagePath).HasMaxLength(512);
            e.HasIndex(x => x.SchoolId).IsUnique();

            e.HasOne(x => x.School)
                .WithOne(s => s.AboutContent)
                .HasForeignKey<AboutContent>(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ContactContent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Phone).HasMaxLength(64);
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.OfficeHours).HasMaxLength(256);
            e.HasIndex(x => x.SchoolId).IsUnique();

            e.HasOne(x => x.School)
                .WithOne(s => s.ContactContent)
                .HasForeignKey<ContactContent>(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<HighlightItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.Property(x => x.ImageOrIcon).HasMaxLength(512);
            e.HasIndex(x => new { x.SchoolId, x.DisplayOrder });

            e.HasOne(x => x.School)
                .WithMany(s => s.HighlightItems)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FacilityItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.Property(x => x.ImagePath).HasMaxLength(512);
            e.HasIndex(x => new { x.SchoolId, x.DisplayOrder });

            e.HasOne(x => x.School)
                .WithMany(s => s.FacilityItems)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<GalleryItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ImagePath).HasMaxLength(512).IsRequired();
            e.Property(x => x.Title).HasMaxLength(256);
            e.Property(x => x.Category).HasMaxLength(128);
            e.HasIndex(x => new { x.SchoolId, x.DisplayOrder });

            e.HasOne(x => x.School)
                .WithMany(s => s.GalleryItems)
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAuditAndNotifications(ModelBuilder builder)
    {
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.UserName).HasMaxLength(256);
            e.Property(x => x.Action).HasMaxLength(128).IsRequired();
            e.Property(x => x.Module).HasMaxLength(128).IsRequired();
            e.Property(x => x.RecordType).HasMaxLength(128);
            e.Property(x => x.RecordId).HasMaxLength(128);
            e.HasIndex(x => x.Timestamp);
            e.HasIndex(x => new { x.SchoolId, x.Timestamp });

            e.HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AppNotification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Title).HasMaxLength(256).IsRequired();
            e.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });

            e.HasOne(x => x.School)
                .WithMany()
                .HasForeignKey(x => x.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAccountEmailLogs(ModelBuilder builder)
    {
        builder.Entity<AccountEmailLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.RecipientEmail).HasMaxLength(256).IsRequired();
            e.Property(x => x.FailureReason).HasMaxLength(500);
            e.HasIndex(x => new { x.UserId, x.EmailType, x.CreatedAt });
            e.HasIndex(x => new { x.SchoolId, x.Status });
        });
    }

    private static void ConfigureApplicationUser(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(256).IsRequired();
            e.Property(x => x.LoginId).HasMaxLength(128);
            e.Property(x => x.PhoneAlternate).HasMaxLength(64);
            e.Property(x => x.ProfileImagePath).HasMaxLength(512);
            e.HasIndex(x => x.LoginId);
            e.HasIndex(x => x.SchoolId);
        });
    }

    private static void ConfigureCampusVisits(ModelBuilder builder)
    {
        builder.Entity<CampusVisit>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.WhenText).HasMaxLength(200).IsRequired();
            e.Property(x => x.ChildAge).HasMaxLength(120).IsRequired();
            e.Property(x => x.Language).HasMaxLength(8).IsRequired();
            e.HasIndex(x => x.SchoolId);
            e.HasIndex(x => x.CreatedAt);
            e.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
