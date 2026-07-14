using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    [Migration("20260714093000_AddCustomerVehicleResourceLinks")]
    public partial class AddCustomerVehicleResourceLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS "CustomerVehicleResourceLinks" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_CustomerVehicleResourceLinks" PRIMARY KEY AUTOINCREMENT,
                    "CustomerId" INTEGER NOT NULL,
                    "VehicleResourceFileId" INTEGER NOT NULL,
                    "CustomerFileId" INTEGER NULL,
                    "Memo" TEXT NULL,
                    "CreatedAt" TEXT NOT NULL,
                    CONSTRAINT "FK_CustomerVehicleResourceLinks_Customers_CustomerId"
                        FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_CustomerVehicleResourceLinks_VehicleResourceFiles_VehicleResourceFileId"
                        FOREIGN KEY ("VehicleResourceFileId") REFERENCES "VehicleResourceFiles" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_CustomerVehicleResourceLinks_CustomerFiles_CustomerFileId"
                        FOREIGN KEY ("CustomerFileId") REFERENCES "CustomerFiles" ("Id") ON DELETE SET NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_CustomerId"
                ON "CustomerVehicleResourceLinks" ("CustomerId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_CustomerFileId"
                ON "CustomerVehicleResourceLinks" ("CustomerFileId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_VehicleResourceFileId"
                ON "CustomerVehicleResourceLinks" ("VehicleResourceFileId");
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_CustomerVehicleResourceLinks_CustomerId_VehicleResourceFileId"
                ON "CustomerVehicleResourceLinks" ("CustomerId", "VehicleResourceFileId");
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CustomerVehicleResourceLinks");
        }
    }
}
