using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarePulse.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAndFoodCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DoctorCategory",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FoodCategory",
                table: "MenuItems",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoctorCategory",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FoodCategory",
                table: "MenuItems");
        }
    }
}
