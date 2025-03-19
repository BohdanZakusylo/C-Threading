using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ABMB.Migrations
{
    /// <inheritdoc />
    public partial class RecreateFlightsTablev1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.CreateTable(
                name: "OldFlights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    DepTime = table.Column<int>(type: "integer", nullable: false),
                    SchedDepTime = table.Column<int>(type: "integer", nullable: false),
                    DepDelay = table.Column<int>(type: "integer", nullable: false),
                    ArrTime = table.Column<int>(type: "integer", nullable: false),
                    SchedArrTime = table.Column<int>(type: "integer", nullable: false),
                    ArrDelay = table.Column<int>(type: "integer", nullable: false),
                    Carrier = table.Column<string>(type: "text", nullable: false),
                    FlightNumber = table.Column<int>(type: "integer", nullable: false),
                    TailNum = table.Column<string>(type: "text", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: false),
                    Destination = table.Column<string>(type: "text", nullable: false),
                    AirTime = table.Column<int>(type: "integer", nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    Hour = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    TimeHour = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldFlights", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OldFlights");

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AirTime = table.Column<int>(type: "integer", nullable: false),
                    ArrDelay = table.Column<int>(type: "integer", nullable: false),
                    ArrTime = table.Column<int>(type: "integer", nullable: false),
                    Carrier = table.Column<string>(type: "text", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    DepDelay = table.Column<int>(type: "integer", nullable: false),
                    DepTime = table.Column<int>(type: "integer", nullable: false),
                    Destination = table.Column<string>(type: "text", nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    FlightNumber = table.Column<int>(type: "integer", nullable: false),
                    Hour = table.Column<int>(type: "integer", nullable: false),
                    Minute = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Origin = table.Column<string>(type: "text", nullable: false),
                    SchedArrTime = table.Column<int>(type: "integer", nullable: false),
                    SchedDepTime = table.Column<int>(type: "integer", nullable: false),
                    TailNum = table.Column<string>(type: "text", nullable: false),
                    TimeHour = table.Column<string>(type: "text", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                });
        }
    }
}
