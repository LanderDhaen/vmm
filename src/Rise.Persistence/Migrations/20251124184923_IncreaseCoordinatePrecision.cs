using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations {
	/// <inheritdoc />
	public partial class IncreaseCoordinatePrecision : Migration {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder) {
			migrationBuilder.AlterColumn<double>(
				name: "Y",
				table: "Location",
				type: "float(18)",
				precision: 18,
				scale: 6,
				nullable: false,
				oldClrType: typeof(double),
				oldType: "float");

			migrationBuilder.AlterColumn<double>(
				name: "X",
				table: "Location",
				type: "float(18)",
				precision: 18,
				scale: 6,
				nullable: false,
				oldClrType: typeof(double),
				oldType: "float");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder) {
			migrationBuilder.AlterColumn<double>(
				name: "Y",
				table: "Location",
				type: "float",
				nullable: false,
				oldClrType: typeof(double),
				oldType: "float(18)",
				oldPrecision: 18,
				oldScale: 6);

			migrationBuilder.AlterColumn<double>(
				name: "X",
				table: "Location",
				type: "float",
				nullable: false,
				oldClrType: typeof(double),
				oldType: "float(18)",
				oldPrecision: 18,
				oldScale: 6);
		}
	}
}