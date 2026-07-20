using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyRoute.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPassengerSeatNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeatNumber",
                table: "PassengerDetails",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "PassengerDetails");
        }
    }
}
