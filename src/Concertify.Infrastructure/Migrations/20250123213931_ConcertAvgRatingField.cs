using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConcertAvgRatingField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "AverageRating",
                table: "Concerts",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Concerts");
        }
    }
}
