using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    [Migration("20260709010000_AddVehicleResourceFiles")]
    public partial class AddVehicleResourceFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "VehicleResourceFiles" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_VehicleResourceFiles" PRIMARY KEY AUTOINCREMENT,
                    "OriginalFileName" TEXT NOT NULL,
                    "StoredFileName" TEXT NOT NULL,
                    "DisplayName" TEXT NOT NULL,
                    "FilePath" TEXT NOT NULL,
                    "FileType" TEXT NOT NULL,
                    "CustomFileType" TEXT NULL,
                    "FileOrder" INTEGER NOT NULL DEFAULT 1,
                    "VehicleBrand" TEXT NULL,
                    "VehicleName" TEXT NULL,
                    "FuelType" TEXT NULL,
                    "CapitalCompany" TEXT NULL,
                    "RentalCompany" TEXT NULL,
                    "Memo" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_VehicleResourceFiles_DisplayName"
                ON "VehicleResourceFiles" ("DisplayName");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_VehicleResourceFiles_FileType"
                ON "VehicleResourceFiles" ("FileType");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_VehicleResourceFiles_VehicleName"
                ON "VehicleResourceFiles" ("VehicleName");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleResourceFiles");
        }
    }
}
