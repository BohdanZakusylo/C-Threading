using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABMB.Migrations
{
    /// <inheritdoc />
    public partial class newFlightTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Airline",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Airport",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Flights");

            migrationBuilder.RenameColumn(
                name: "Time",
                table: "Flights",
                newName: "TimeHour");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Flights",
                newName: "TailNum");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Flights",
                newName: "Origin");

            migrationBuilder.RenameColumn(
                name: "Gate",
                table: "Flights",
                newName: "Destination");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Flights",
                newName: "Carrier");

            migrationBuilder.AddColumn<int>(
                name: "AirTime",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArrDelay",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ArrTime",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepDelay",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepTime",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Distance",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FlightNumber",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hour",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Minute",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchedArrTime",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SchedDepTime",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AirTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrDelay",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ArrTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepDelay",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "DepTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Hour",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Minute",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "SchedArrTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "SchedDepTime",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Flights");

            migrationBuilder.RenameColumn(
                name: "TimeHour",
                table: "Flights",
                newName: "Time");

            migrationBuilder.RenameColumn(
                name: "TailNum",
                table: "Flights",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "Origin",
                table: "Flights",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Destination",
                table: "Flights",
                newName: "Gate");

            migrationBuilder.RenameColumn(
                name: "Carrier",
                table: "Flights",
                newName: "Date");

            migrationBuilder.AddColumn<string>(
                name: "Airline",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Airport",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
