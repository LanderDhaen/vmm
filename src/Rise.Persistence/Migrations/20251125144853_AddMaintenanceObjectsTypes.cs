using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rise.Persistence.Migrations {
	/// <inheritdoc />
	public partial class AddMaintenanceObjectsTypes : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropColumn(
				name: "MaintObjType",
				table: "MaintObjects");

			migrationBuilder.AddColumn<Guid>(
				name: "MaintObjTypeId",
				table: "MaintObjects",
				type: "uniqueidentifier",
				nullable: false,
				defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.CreateTable(
				name: "MaintObjTypes",
				columns: table => new {
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					Code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
					Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
					DefaultMaintenanceIntervalDays = table.Column<int>(type: "int", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					IsDeleted = table.Column<bool>(type: "bit", nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_MaintObjTypes", x => x.Id);
				});

			migrationBuilder.InsertData(
				table: "MaintObjTypes",
				columns: new[] { "Id", "Code", "CreatedAt", "DefaultMaintenanceIntervalDays", "IsDeleted", "Name", "UpdatedAt" },
				values: new object[,]
				{
					{ new Guid("00000000-0000-0000-0000-000000000001"), "Sonde", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 14, false, "Sonde", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
					{ new Guid("00000000-0000-0000-0000-000000000002"), "Logger", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 90, false, "Logger", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
					{ new Guid("00000000-0000-0000-0000-000000000003"), "Modem", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 180, false, "Modem", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
					{ new Guid("00000000-0000-0000-0000-000000000004"), "Sim", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 365, false, "SIM", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
					{ new Guid("00000000-0000-0000-0000-000000000005"), "Voeding", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 365, false, "Voeding", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
				});

			migrationBuilder.CreateIndex(
				name: "IX_MaintObjects_MaintObjTypeId",
				table: "MaintObjects",
				column: "MaintObjTypeId");

			migrationBuilder.CreateIndex(
				name: "IX_MaintObjTypes_Code",
				table: "MaintObjTypes",
				column: "Code",
				unique: true);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintObjects_MaintObjTypes_MaintObjTypeId",
				table: "MaintObjects",
				column: "MaintObjTypeId",
				principalTable: "MaintObjTypes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.DropForeignKey(
				name: "FK_MaintObjects_MaintObjTypes_MaintObjTypeId",
				table: "MaintObjects");

			migrationBuilder.DropTable(
				name: "MaintObjTypes");

			migrationBuilder.DropIndex(
				name: "IX_MaintObjects_MaintObjTypeId",
				table: "MaintObjects");

			migrationBuilder.DropColumn(
				name: "MaintObjTypeId",
				table: "MaintObjects");

			migrationBuilder.AddColumn<int>(
				name: "MaintObjType",
				table: "MaintObjects",
				type: "int",
				nullable: false,
				defaultValue: 0);
		}
	}
}