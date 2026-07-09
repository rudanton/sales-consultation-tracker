using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    [Migration("20260709010000_AddVehicleResourceFiles")]
    public partial class AddVehicleResourceFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleResourceFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CustomFileType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    FileOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    VehicleBrand = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    VehicleName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    FuelType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Memo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleResourceFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleResourceFiles_DisplayName",
                table: "VehicleResourceFiles",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleResourceFiles_FileType",
                table: "VehicleResourceFiles",
                column: "FileType");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleResourceFiles_VehicleName",
                table: "VehicleResourceFiles",
                column: "VehicleName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleResourceFiles");
        }
    }
}
