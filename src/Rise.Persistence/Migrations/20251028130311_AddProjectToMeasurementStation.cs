using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectToMeasurementStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeasurementStation_Location_LocationId",
                table: "MeasurementStation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeasurementStation",
                table: "MeasurementStation");

            migrationBuilder.RenameTable(
                name: "MeasurementStation",
                newName: "MeasurementStations");

            migrationBuilder.RenameIndex(
                name: "IX_MeasurementStation_LocationId",
                table: "MeasurementStations",
                newName: "IX_MeasurementStations_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_MeasurementStation_Code",
                table: "MeasurementStations",
                newName: "IX_MeasurementStations_Code");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MeasurementStations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "MeasurementStations",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeasurementStations",
                table: "MeasurementStations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementStations_ProjectId",
                table: "MeasurementStations",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeasurementStations_Location_LocationId",
                table: "MeasurementStations",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeasurementStations_Project_ProjectId",
                table: "MeasurementStations",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeasurementStations_Location_LocationId",
                table: "MeasurementStations");

            migrationBuilder.DropForeignKey(
                name: "FK_MeasurementStations_Project_ProjectId",
                table: "MeasurementStations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MeasurementStations",
                table: "MeasurementStations");

            migrationBuilder.DropIndex(
                name: "IX_MeasurementStations_ProjectId",
                table: "MeasurementStations");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "MeasurementStations");

            migrationBuilder.RenameTable(
                name: "MeasurementStations",
                newName: "MeasurementStation");

            migrationBuilder.RenameIndex(
                name: "IX_MeasurementStations_LocationId",
                table: "MeasurementStation",
                newName: "IX_MeasurementStation_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_MeasurementStations_Code",
                table: "MeasurementStation",
                newName: "IX_MeasurementStation_Code");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MeasurementStation",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MeasurementStation",
                table: "MeasurementStation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MeasurementStation_Location_LocationId",
                table: "MeasurementStation",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
