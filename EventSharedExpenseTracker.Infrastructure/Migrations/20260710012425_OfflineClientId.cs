using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSharedExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OfflineClientId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "OfflineClientId",
                table: "Expenses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_OfflineClientId",
                table: "Expenses",
                column: "OfflineClientId",
                unique: true,
                filter: "[OfflineClientId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Expenses_OfflineClientId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "OfflineClientId",
                table: "Expenses");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
