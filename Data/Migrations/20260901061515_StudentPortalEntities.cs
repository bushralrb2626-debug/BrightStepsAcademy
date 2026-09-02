using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrightStepsAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentPortalEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassAssignmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalMarks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    AllowSubmission = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AttachmentContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ContentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassAssignmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentItems_SchoolClasses_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "SchoolClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentItems_SchoolSections_SchoolSectionId",
                        column: x => x.SchoolSectionId,
                        principalTable: "SchoolSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentItems_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentItems_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentItems_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentItems_TeacherAssignments_TeacherAssignmentId",
                        column: x => x.TeacherAssignmentId,
                        principalTable: "TeacherAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassTimetableSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    PeriodOrder = table.Column<int>(type: "int", nullable: false),
                    PeriodLabel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTimetableSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTimetableSlots_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTimetableSlots_SchoolClasses_SchoolClassId",
                        column: x => x.SchoolClassId,
                        principalTable: "SchoolClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTimetableSlots_SchoolSections_SchoolSectionId",
                        column: x => x.SchoolSectionId,
                        principalTable: "SchoolSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTimetableSlots_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTimetableSlots_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTimetableSlots_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassAssignmentSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TextResponse = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FileContentType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    ReviewStatus = table.Column<int>(type: "int", nullable: false),
                    ObtainedMarks = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    TeacherFeedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassAssignmentSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentSubmissions_ClassAssignmentItems_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "ClassAssignmentItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentSubmissions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassAssignmentSubmissions_StudentRecords_StudentId",
                        column: x => x.StudentId,
                        principalTable: "StudentRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentItems_SchoolClassId_SchoolSectionId_SubjectId_ContentDate",
                table: "ClassAssignmentItems",
                columns: new[] { "SchoolClassId", "SchoolSectionId", "SubjectId", "ContentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentItems_SchoolId",
                table: "ClassAssignmentItems",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentItems_SchoolSectionId",
                table: "ClassAssignmentItems",
                column: "SchoolSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentItems_StaffMemberId",
                table: "ClassAssignmentItems",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentItems_SubjectId",
                table: "ClassAssignmentItems",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentItems_TeacherAssignmentId",
                table: "ClassAssignmentItems",
                column: "TeacherAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentSubmissions_AssignmentId_StudentId",
                table: "ClassAssignmentSubmissions",
                columns: new[] { "AssignmentId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentSubmissions_SchoolId",
                table: "ClassAssignmentSubmissions",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignmentSubmissions_StudentId",
                table: "ClassAssignmentSubmissions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableSlots_RoomId",
                table: "ClassTimetableSlots",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableSlots_SchoolClassId_SchoolSectionId_DayOfWeek_PeriodOrder",
                table: "ClassTimetableSlots",
                columns: new[] { "SchoolClassId", "SchoolSectionId", "DayOfWeek", "PeriodOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableSlots_SchoolId",
                table: "ClassTimetableSlots",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableSlots_SchoolSectionId",
                table: "ClassTimetableSlots",
                column: "SchoolSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableSlots_StaffMemberId",
                table: "ClassTimetableSlots",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableSlots_SubjectId",
                table: "ClassTimetableSlots",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassAssignmentSubmissions");

            migrationBuilder.DropTable(
                name: "ClassTimetableSlots");

            migrationBuilder.DropTable(
                name: "ClassAssignmentItems");
        }
    }
}
