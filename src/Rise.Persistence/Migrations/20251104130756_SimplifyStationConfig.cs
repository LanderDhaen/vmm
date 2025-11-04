using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations {
	/// <inheritdoc />
	public partial class SimplifyStationConfig : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropTable(
				name: "ProjectStations");

			migrationBuilder.CreateTable(
				name: "MeasurementStationProject",
				columns: table => new {
					ProjectsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					StationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_MeasurementStationProject", x => new { x.ProjectsId, x.StationsId });
					table.ForeignKey(
						name: "FK_MeasurementStationProject_MeasurementStations_StationsId",
						column: x => x.StationsId,
						principalTable: "MeasurementStations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_MeasurementStationProject_Project_ProjectsId",
						column: x => x.ProjectsId,
						principalTable: "Project",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_MeasurementStationProject_StationsId",
				table: "MeasurementStationProject",
				column: "StationsId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropTable(
				name: "MeasurementStationProject");

			migrationBuilder.CreateTable(
				name: "ProjectStations",
				columns: table => new {
					ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					StationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_ProjectStations", x => new { x.ProjectId, x.StationId });
					table.ForeignKey(
						name: "FK_ProjectStations_MeasurementStations_StationId",
						column: x => x.StationId,
						principalTable: "MeasurementStations",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_ProjectStations_Project_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Project",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_ProjectStations_StationId",
				table: "ProjectStations",
				column: "StationId");
		}
	}
}