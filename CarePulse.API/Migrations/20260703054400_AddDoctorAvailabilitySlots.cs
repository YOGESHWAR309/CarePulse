using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarePulse.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorAvailabilitySlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorAvailabilitySlots",
                columns: table => new
                {
                    DoctorAvailabilitySlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    SlotStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SlotEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsBooked = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAvailabilitySlots", x => x.DoctorAvailabilitySlotId);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilitySlots_Users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilitySlots_DoctorId",
                table: "DoctorAvailabilitySlots",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorAvailabilitySlots");
        }
    }
}
