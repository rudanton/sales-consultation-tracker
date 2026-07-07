using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CustomFileType = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Memo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFiles_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFiles_CustomerId",
                table: "CustomerFiles",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFiles_DisplayName",
                table: "CustomerFiles",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFiles_FileType",
                table: "CustomerFiles",
                column: "FileType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFiles");
        }
    }
}
