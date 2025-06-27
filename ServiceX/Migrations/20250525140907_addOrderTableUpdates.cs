using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceX.Migrations
{
    /// <inheritdoc />
    public partial class addOrderTableUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "date",
                table: "Orders",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "time",
                table: "Orders",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "date",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "time",
                table: "Orders");
        }
    }
}
