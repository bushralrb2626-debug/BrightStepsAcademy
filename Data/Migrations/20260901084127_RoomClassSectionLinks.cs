using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrightStepsAcademy.Data.Migrations
{
    /// <inheritdoc />
    public partial class RoomClassSectionLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolClassId",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SchoolSectionId",
                table: "Rooms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_SchoolClassId",
                table: "Rooms",
                column: "SchoolClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_SchoolSectionId",
                table: "Rooms",
                column: "SchoolSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_SchoolClasses_SchoolClassId",
                table: "Rooms",
                column: "SchoolClassId",
                principalTable: "SchoolClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_SchoolSections_SchoolSectionId",
                table: "Rooms",
                column: "SchoolSectionId",
                principalTable: "SchoolSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_SchoolClasses_SchoolClassId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_SchoolSections_SchoolSectionId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_SchoolClassId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_SchoolSectionId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SchoolClassId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "SchoolSectionId",
                table: "Rooms");
        }
    }
}
