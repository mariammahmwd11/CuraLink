using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuraLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Doctorreviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailability_Doctors_DoctorId",
                table: "DoctorAvailability");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorReview_Doctors_DoctorId",
                table: "DoctorReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorReview",
                table: "DoctorReview");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorAvailability",
                table: "DoctorAvailability");

            migrationBuilder.RenameTable(
                name: "DoctorReview",
                newName: "DoctorReviews");

            migrationBuilder.RenameTable(
                name: "DoctorAvailability",
                newName: "DoctorAvailabilities");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorReview_DoctorId",
                table: "DoctorReviews",
                newName: "IX_DoctorReviews_DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorAvailability_DoctorId",
                table: "DoctorAvailabilities",
                newName: "IX_DoctorAvailabilities_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorReviews",
                table: "DoctorReviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorAvailabilities",
                table: "DoctorAvailabilities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorReviews_Doctors_DoctorId",
                table: "DoctorReviews",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorReviews_Doctors_DoctorId",
                table: "DoctorReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorReviews",
                table: "DoctorReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorAvailabilities",
                table: "DoctorAvailabilities");

            migrationBuilder.RenameTable(
                name: "DoctorReviews",
                newName: "DoctorReview");

            migrationBuilder.RenameTable(
                name: "DoctorAvailabilities",
                newName: "DoctorAvailability");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorReviews_DoctorId",
                table: "DoctorReview",
                newName: "IX_DoctorReview_DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailability",
                newName: "IX_DoctorAvailability_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorReview",
                table: "DoctorReview",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorAvailability",
                table: "DoctorAvailability",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailability_Doctors_DoctorId",
                table: "DoctorAvailability",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorReview_Doctors_DoctorId",
                table: "DoctorReview",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
