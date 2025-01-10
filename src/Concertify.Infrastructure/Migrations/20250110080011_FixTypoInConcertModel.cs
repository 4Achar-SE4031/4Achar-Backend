using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTypoInConcertModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "Concerts",
                newName: "Longtitude");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Longtitude",
                table: "Concerts",
                newName: "Longitude");
        }
    }
}
