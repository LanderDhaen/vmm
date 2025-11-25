using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations {
	/// <inheritdoc />
	public partial class AddCalibrationIntervalDays : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropColumn(
				name: "DefaultMaintenanceIntervalDays",
				table: "MaintObjType");

			migrationBuilder.AddColumn<int>(
				name: "CalibrationIntervalDays",
				table: "MaintObjType",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "MaintenanceIntervalDays",
				table: "MaintObjType",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
				columns: new[] { "CalibrationIntervalDays", "MaintenanceIntervalDays" },
				values: new object[] { 21, 14 });

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
				columns: new[] { "CalibrationIntervalDays", "MaintenanceIntervalDays" },
				values: new object[] { 97, 90 });

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
				columns: new[] { "CalibrationIntervalDays", "MaintenanceIntervalDays" },
				values: new object[] { 187, 180 });

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
				columns: new[] { "CalibrationIntervalDays", "MaintenanceIntervalDays" },
				values: new object[] { 372, 365 });

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
				columns: new[] { "CalibrationIntervalDays", "MaintenanceIntervalDays" },
				values: new object[] { 372, 365 });
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropColumn(
				name: "CalibrationIntervalDays",
				table: "MaintObjType");

			migrationBuilder.DropColumn(
				name: "MaintenanceIntervalDays",
				table: "MaintObjType");

			migrationBuilder.AddColumn<int>(
				name: "DefaultMaintenanceIntervalDays",
				table: "MaintObjType",
				type: "int",
				nullable: true);

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
				column: "DefaultMaintenanceIntervalDays",
				value: 14);

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
				column: "DefaultMaintenanceIntervalDays",
				value: 90);

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
				column: "DefaultMaintenanceIntervalDays",
				value: 180);

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000004"),
				column: "DefaultMaintenanceIntervalDays",
				value: 365);

			migrationBuilder.UpdateData(
				table: "MaintObjType",
				keyColumn: "Id",
				keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
				column: "DefaultMaintenanceIntervalDays",
				value: 365);
		}
	}
}