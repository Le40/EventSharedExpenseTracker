using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSharedExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "AmountOriginal");

            migrationBuilder.AddColumn<string>(
                name: "BaseCurrencyCode",
                table: "Trips",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountBase",
                table: "Payments",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BaseCurrencyCode",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                table: "Expenses",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateToBase",
                table: "Expenses",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseCurrencyCode",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "AmountBase",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BaseCurrencyCode",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ExchangeRateToBase",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "AmountOriginal",
                table: "Payments",
                newName: "Amount");
        }
    }
}
