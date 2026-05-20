using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSharedExpenseTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ParticipantDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "TripParticipants",
                newName: "DisplayName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "TripParticipants",
                newName: "UserName");
        }
    }
}
