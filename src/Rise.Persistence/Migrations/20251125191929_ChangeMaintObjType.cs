using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations {
	/// <inheritdoc />
	public partial class ChangeMaintObjType : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropForeignKey(
				name: "FK_MaintObjects_Location_LocationId",
				table: "MaintObjects");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintObjects_MaintObjTypes_MaintObjTypeId",
				table: "MaintObjects");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintObjects_MeasurementStations_MeasurementStationId",
				table: "MaintObjects");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintObjects_Technician_TechnicianId",
				table: "MaintObjects");

			migrationBuilder.DropPrimaryKey(
				name: "PK_MaintObjTypes",
				table: "MaintObjTypes");

			migrationBuilder.DropPrimaryKey(
				name: "PK_MaintObjects",
				table: "MaintObjects");

			migrationBuilder.RenameTable(
				name: "MaintObjTypes",
				newName: "MaintObjType");

			migrationBuilder.RenameTable(
				name: "MaintObjects",
				newName: "MaintObj");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObjTypes_Code",
				table: "MaintObjType",
				newName: "IX_MaintObjType_Code");

			migrationBuilder.RenameColumn(
				name: "MeasurementStationId",
				table: "MaintObj",
				newName: "StationId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObjects_TechnicianId",
				table: "MaintObj",
				newName: "IX_MaintObj_TechnicianId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObjects_MeasurementStationId",
				table: "MaintObj",
				newName: "IX_MaintObj_StationId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObjects_MaintObjTypeId",
				table: "MaintObj",
				newName: "IX_MaintObj_MaintObjTypeId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObjects_LocationId",
				table: "MaintObj",
				newName: "IX_MaintObj_LocationId");

			migrationBuilder.AlterColumn<DateTime>(
				name: "UpdatedAt",
				table: "MaintObjType",
				type: "datetime2",
				nullable: false,
				defaultValueSql: "CURRENT_TIMESTAMP",
				oldClrType: typeof(DateTime),
				oldType: "datetime2");

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "MaintObjType",
				type: "bit",
				nullable: false,
				defaultValue: false,
				oldClrType: typeof(bool),
				oldType: "bit");

			migrationBuilder.AlterColumn<DateTime>(
				name: "CreatedAt",
				table: "MaintObjType",
				type: "datetime2",
				nullable: false,
				defaultValueSql: "CURRENT_TIMESTAMP",
				oldClrType: typeof(DateTime),
				oldType: "datetime2");

			migrationBuilder.AlterColumn<DateTime>(
				name: "UpdatedAt",
				table: "MaintObj",
				type: "datetime2",
				nullable: false,
				defaultValueSql: "CURRENT_TIMESTAMP",
				oldClrType: typeof(DateTime),
				oldType: "datetime2");

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "MaintObj",
				type: "bit",
				nullable: false,
				defaultValue: false,
				oldClrType: typeof(bool),
				oldType: "bit");

			migrationBuilder.AlterColumn<DateTime>(
				name: "CreatedAt",
				table: "MaintObj",
				type: "datetime2",
				nullable: false,
				defaultValueSql: "CURRENT_TIMESTAMP",
				oldClrType: typeof(DateTime),
				oldType: "datetime2");

			migrationBuilder.AddPrimaryKey(
				name: "PK_MaintObjType",
				table: "MaintObjType",
				column: "Id");

			migrationBuilder.AddPrimaryKey(
				name: "PK_MaintObj",
				table: "MaintObj",
				column: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObj_Location_LocationId",
				table: "MaintObj",
				column: "LocationId",
				principalTable: "Location",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObj_MaintObjType_MaintObjTypeId",
				table: "MaintObj",
				column: "MaintObjTypeId",
				principalTable: "MaintObjType",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObj_MeasurementStations_StationId",
				table: "MaintObj",
				column: "StationId",
				principalTable: "MeasurementStations",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObj_Technician_TechnicianId",
				table: "MaintObj",
				column: "TechnicianId",
				principalTable: "Technician",
				principalColumn: "Id");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropForeignKey(
				name: "FK_MaintObj_Location_LocationId",
				table: "MaintObj");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintObj_MaintObjType_MaintObjTypeId",
				table: "MaintObj");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintObj_MeasurementStations_StationId",
				table: "MaintObj");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintObj_Technician_TechnicianId",
				table: "MaintObj");

			migrationBuilder.DropPrimaryKey(
				name: "PK_MaintObjType",
				table: "MaintObjType");

			migrationBuilder.DropPrimaryKey(
				name: "PK_MaintObj",
				table: "MaintObj");

			migrationBuilder.RenameTable(
				name: "MaintObjType",
				newName: "MaintObjTypes");

			migrationBuilder.RenameTable(
				name: "MaintObj",
				newName: "MaintObjects");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObjType_Code",
				table: "MaintObjTypes",
				newName: "IX_MaintObjTypes_Code");

			migrationBuilder.RenameColumn(
				name: "StationId",
				table: "MaintObjects",
				newName: "MeasurementStationId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObj_TechnicianId",
				table: "MaintObjects",
				newName: "IX_MaintObjects_TechnicianId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObj_StationId",
				table: "MaintObjects",
				newName: "IX_MaintObjects_MeasurementStationId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObj_MaintObjTypeId",
				table: "MaintObjects",
				newName: "IX_MaintObjects_MaintObjTypeId");

			migrationBuilder.RenameIndex(
				name: "IX_MaintObj_LocationId",
				table: "MaintObjects",
				newName: "IX_MaintObjects_LocationId");

			migrationBuilder.AlterColumn<DateTime>(
				name: "UpdatedAt",
				table: "MaintObjTypes",
				type: "datetime2",
				nullable: false,
				oldClrType: typeof(DateTime),
				oldType: "datetime2",
				oldDefaultValueSql: "CURRENT_TIMESTAMP");

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "MaintObjTypes",
				type: "bit",
				nullable: false,
				oldClrType: typeof(bool),
				oldType: "bit",
				oldDefaultValue: false);

			migrationBuilder.AlterColumn<DateTime>(
				name: "CreatedAt",
				table: "MaintObjTypes",
				type: "datetime2",
				nullable: false,
				oldClrType: typeof(DateTime),
				oldType: "datetime2",
				oldDefaultValueSql: "CURRENT_TIMESTAMP");

			migrationBuilder.AlterColumn<DateTime>(
				name: "UpdatedAt",
				table: "MaintObjects",
				type: "datetime2",
				nullable: false,
				oldClrType: typeof(DateTime),
				oldType: "datetime2",
				oldDefaultValueSql: "CURRENT_TIMESTAMP");

			migrationBuilder.AlterColumn<bool>(
				name: "IsDeleted",
				table: "MaintObjects",
				type: "bit",
				nullable: false,
				oldClrType: typeof(bool),
				oldType: "bit",
				oldDefaultValue: false);

			migrationBuilder.AlterColumn<DateTime>(
				name: "CreatedAt",
				table: "MaintObjects",
				type: "datetime2",
				nullable: false,
				oldClrType: typeof(DateTime),
				oldType: "datetime2",
				oldDefaultValueSql: "CURRENT_TIMESTAMP");

			migrationBuilder.AddPrimaryKey(
				name: "PK_MaintObjTypes",
				table: "MaintObjTypes",
				column: "Id");

			migrationBuilder.AddPrimaryKey(
				name: "PK_MaintObjects",
				table: "MaintObjects",
				column: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObjects_Location_LocationId",
				table: "MaintObjects",
				column: "LocationId",
				principalTable: "Location",
				principalColumn: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObjects_MaintObjTypes_MaintObjTypeId",
				table: "MaintObjects",
				column: "MaintObjTypeId",
				principalTable: "MaintObjTypes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObjects_MeasurementStations_MeasurementStationId",
				table: "MaintObjects",
				column: "MeasurementStationId",
				principalTable: "MeasurementStations",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObjects_Technician_TechnicianId",
				table: "MaintObjects",
				column: "TechnicianId",
				principalTable: "Technician",
				principalColumn: "Id");
		}
	}
}