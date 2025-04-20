using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceX.Migrations
{
    /// <inheritdoc />
    public partial class addPayByHourForTechnicianTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PayByHour",
                table: "Technicians",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayByHour",
                table: "Technicians");
        }
    }
}
