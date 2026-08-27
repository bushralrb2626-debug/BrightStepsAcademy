using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrightStepsAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class SuperAdminPlatformSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Schools",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactEmail",
                table: "Schools",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactName",
                table: "Schools",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactPhone",
                table: "Schools",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StateProvince",
                table: "Schools",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "Schools",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlatformSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlatformName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LogoPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    SupportEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SupportPhone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DefaultSubscriptionMonths = table.Column<int>(type: "int", nullable: false),
                    ExpiryWarningDays = table.Column<int>(type: "int", nullable: false),
                    AvailablePlansJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BillingCycle = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolSubscriptions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionChangeLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedByUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionChangeLogs_SchoolSubscriptions_SchoolSubscriptionId",
                        column: x => x.SchoolSubscriptionId,
                        principalTable: "SchoolSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Status",
                table: "Schools",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolSubscriptions_SchoolId",
                table: "SchoolSubscriptions",
                column: "SchoolId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionChangeLogs_SchoolSubscriptionId",
                table: "SubscriptionChangeLogs",
                column: "SchoolSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformSettings");

            migrationBuilder.DropTable(
                name: "SubscriptionChangeLogs");

            migrationBuilder.DropTable(
                name: "SchoolSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Schools_Status",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PrimaryContactEmail",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PrimaryContactName",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PrimaryContactPhone",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "StateProvince",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Schools");
        }
    }
}
