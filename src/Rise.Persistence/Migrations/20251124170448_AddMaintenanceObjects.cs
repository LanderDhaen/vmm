using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MaintObjType = table.Column<int>(type: "int", nullable: false),
                    MaintObjStatus = table.Column<int>(type: "int", nullable: false),
                    InstalledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsInStock = table.Column<bool>(type: "bit", nullable: false),
                    MeasurementStationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TechnicianId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintObjects_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MaintObjects_MeasurementStations_MeasurementStationId",
                        column: x => x.MeasurementStationId,
                        principalTable: "MeasurementStations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintObjects_Technician_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technician",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintObjects_LocationId",
                table: "MaintObjects",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintObjects_MeasurementStationId",
                table: "MaintObjects",
                column: "MeasurementStationId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintObjects_TechnicianId",
                table: "MaintObjects",
                column: "TechnicianId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintObjects");
        }
    }
}
