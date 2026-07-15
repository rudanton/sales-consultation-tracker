using ConsultNote.Data;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260715000000_AddConsultationLogStatus")]
    public partial class AddConsultationLogStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ConsultationLogs",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "Consulting");

            migrationBuilder.Sql("""
                UPDATE ConsultationLogs
                SET Status = COALESCE((
                    SELECT Customers.Status
                    FROM Customers
                    WHERE Customers.Id = ConsultationLogs.CustomerId
                ), 'Consulting')
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ConsultationLogs");
        }
    }
}
