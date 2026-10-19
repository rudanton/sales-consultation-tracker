using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuoteTermsAndRentalCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Older releases created this table during startup schema repair.
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

            migrationBuilder.AddColumn<string>(
                name: "QuoteType",
                table: "VehicleResourceFiles",
                type: "TEXT",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RentalCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false, collation: "NOCASE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalCompanies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalCompanies_Name",
                table: "RentalCompanies",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalCompanies");

            migrationBuilder.DropColumn(
                name: "QuoteType",
                table: "VehicleResourceFiles");
        }
    }
}
