using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuraLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditDoctorAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "AvailableDate",
                table: "DoctorAvailabilities");

            migrationBuilder.AddColumn<int>(
                name: "DayOfWeek",
                table: "DoctorAvailabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "DoctorAvailabilities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "SlotDurationMinutes",
                table: "DoctorAvailabilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "DoctorAvailabilities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DoctorId_DayOfWeek",
                table: "DoctorAvailabilities",
                columns: new[] { "DoctorId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilities_DoctorId_DayOfWeek",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "SlotDurationMinutes",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "DoctorAvailabilities");

            migrationBuilder.AddColumn<DateTime>(
                name: "AvailableDate",
                table: "DoctorAvailabilities",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId");
        }
    }
}
