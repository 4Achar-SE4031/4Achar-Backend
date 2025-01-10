using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConcertPriceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<int>>(
                name: "TicketPrice",
                table: "Concerts",
                type: "integer[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TicketPrice",
                table: "Concerts",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<int>),
                oldType: "integer[]");
        }
    }
}
