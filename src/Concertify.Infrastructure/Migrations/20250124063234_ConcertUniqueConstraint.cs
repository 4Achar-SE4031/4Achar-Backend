using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConcertUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Concerts_Title_StartDateTime_City",
                table: "Concerts",
                columns: new[] { "Title", "StartDateTime", "City" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Concerts_Title_StartDateTime_City",
                table: "Concerts");
        }
    }
}
