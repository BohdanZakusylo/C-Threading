using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABMB.Migrations
{
    /// <inheritdoc />
    public partial class ChangedHotelModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Address", table: "Hotels");

            migrationBuilder.RenameColumn(name: "Rating", table: "Hotels", newName: "countyName");

            migrationBuilder.RenameColumn(name: "Price", table: "Hotels", newName: "cityName");

            migrationBuilder.RenameColumn(name: "Name", table: "Hotels", newName: "PhoneNumber");

            migrationBuilder.RenameColumn(name: "City", table: "Hotels", newName: "HotelName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(name: "countyName", table: "Hotels", newName: "Rating");

            migrationBuilder.RenameColumn(name: "cityName", table: "Hotels", newName: "Price");

            migrationBuilder.RenameColumn(name: "PhoneNumber", table: "Hotels", newName: "Name");

            migrationBuilder.RenameColumn(name: "HotelName", table: "Hotels", newName: "City");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Hotels",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
