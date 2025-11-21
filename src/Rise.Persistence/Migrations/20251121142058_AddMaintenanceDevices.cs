using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rise.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    Sonde = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Logger = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Modem = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Sim = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Voeding = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
