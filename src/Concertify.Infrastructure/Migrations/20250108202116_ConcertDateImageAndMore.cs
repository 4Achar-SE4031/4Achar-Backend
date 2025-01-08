using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Concertify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConcertDateImageAndMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Province",
                table: "Concerts");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Concerts",
                newName: "Url");

            migrationBuilder.AddColumn<string>(
                name: "CardImage",
                table: "Concerts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverImage",
                table: "Concerts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Concerts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDateTime",
                table: "Concerts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardImage",
                table: "Concerts");

            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "Concerts");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Concerts");

            migrationBuilder.DropColumn(
                name: "StartDateTime",
                table: "Concerts");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Concerts",
                newName: "StartDate");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Concerts",
                type: "text",
                nullable: true);
        }
    }
}
