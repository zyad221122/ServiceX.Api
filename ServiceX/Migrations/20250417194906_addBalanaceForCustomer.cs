using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceX.Migrations
{
    /// <inheritdoc />
    public partial class addBalanaceForCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Balanace",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balanace",
                table: "Customers");
        }
    }
}
