using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultNote.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerConsultationConditionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualDriver",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractPeriod",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractType",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditStatus",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryRegion",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormFuelType",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormVehicleBrand",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormVehicleDetail",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormVehicleName",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBusinessExperienceOverOneYear",
                table: "Customers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDriverLicenseOverOneYear",
                table: "Customers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InitialCost",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceAge",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mileage",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerType",
                table: "Customers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialNote",
                table: "Customers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualDriver",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContractPeriod",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreditStatus",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeliveryRegion",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FormFuelType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FormVehicleBrand",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FormVehicleDetail",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FormVehicleName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "HasBusinessExperienceOverOneYear",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "HasDriverLicenseOverOneYear",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "InitialCost",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "InsuranceAge",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Mileage",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "SpecialNote",
                table: "Customers");
        }
    }
}
