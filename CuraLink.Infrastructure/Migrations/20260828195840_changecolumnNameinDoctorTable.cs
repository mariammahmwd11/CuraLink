using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuraLink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changecolumnNameinDoctorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VerifiedByAdminId",
                table: "Doctors",
                newName: "ChangeStatusByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ChangeStatusByAdminId",
                table: "Doctors",
                newName: "VerifiedByAdminId");
        }
    }
}
