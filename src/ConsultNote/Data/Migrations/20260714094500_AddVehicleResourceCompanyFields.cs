using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    [Migration("20260714094500_AddVehicleResourceCompanyFields")]
    public partial class AddVehicleResourceCompanyFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "VehicleResourceFiles"
                ADD COLUMN "CapitalCompany" TEXT NULL;
                """,
                suppressTransaction: true);

            migrationBuilder.Sql(
                """
                ALTER TABLE "VehicleResourceFiles"
                ADD COLUMN "RentalCompany" TEXT NULL;
                """,
                suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
