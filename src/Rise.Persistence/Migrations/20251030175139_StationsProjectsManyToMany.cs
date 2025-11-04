using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations {
	/// <inheritdoc />
	public partial class StationsProjectsManyToMany : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropForeignKey(
				name: "FK_MeasurementStations_Project_ProjectId",
				table: "MeasurementStations");

			migrationBuilder.DropIndex(
				name: "IX_MeasurementStations_ProjectId",
				table: "MeasurementStations");

			migrationBuilder.DropColumn(
				name: "ProjectId",
				table: "MeasurementStations");

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

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropTable(
				name: "ProjectStations");

			migrationBuilder.AddColumn<Guid>(
				name: "ProjectId",
				table: "MeasurementStations",
				type: "uniqueidentifier",
				nullable: false,
				defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.CreateIndex(
				name: "IX_MeasurementStations_ProjectId",
				table: "MeasurementStations",
				column: "ProjectId");

			migrationBuilder.AddForeignKey(
				name: "FK_MeasurementStations_Project_ProjectId",
				table: "MeasurementStations",
				column: "ProjectId",
				principalTable: "Project",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}
	}
}