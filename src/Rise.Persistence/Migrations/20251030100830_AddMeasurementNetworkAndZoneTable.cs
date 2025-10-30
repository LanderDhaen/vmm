using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMeasurementNetworkAndZoneTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MeasurementNetworkId",
                table: "Project",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementNetworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasurementNetworks_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Project_MeasurementNetworkId",
                table: "Project",
                column: "MeasurementNetworkId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementNetworks_ZoneId",
                table: "MeasurementNetworks",
                column: "ZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_MeasurementNetworks_MeasurementNetworkId",
                table: "Project",
                column: "MeasurementNetworkId",
                principalTable: "MeasurementNetworks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_MeasurementNetworks_MeasurementNetworkId",
                table: "Project");

            migrationBuilder.DropTable(
                name: "MeasurementNetworks");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropIndex(
                name: "IX_Project_MeasurementNetworkId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "MeasurementNetworkId",
                table: "Project");
        }
    }
}
