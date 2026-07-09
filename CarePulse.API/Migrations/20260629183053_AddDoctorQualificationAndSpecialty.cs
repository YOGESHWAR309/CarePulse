using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarePulse.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorQualificationAndSpecialty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorQualification",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DoctorSpecialty",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorQualification",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DoctorSpecialty",
                table: "Users");
        }
    }
}
