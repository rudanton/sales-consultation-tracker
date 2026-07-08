using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFavoriteAndDiscardReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscardReason",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscardReason",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Customers");
        }
    }
}
