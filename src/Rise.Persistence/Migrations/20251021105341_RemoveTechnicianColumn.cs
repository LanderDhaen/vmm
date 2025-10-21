using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTechnicianColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Technician_TechnicianId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_TechnicianId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "TechnicianId",
                table: "Project");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TechnicianId",
                table: "Project",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_TechnicianId",
                table: "Project",
                column: "TechnicianId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Technician_TechnicianId",
                table: "Project",
                column: "TechnicianId",
                principalTable: "Technician",
                principalColumn: "Id");
        }
    }
}
