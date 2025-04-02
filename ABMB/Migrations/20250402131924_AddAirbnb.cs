using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ABMB.Migrations
{
    /// <inheritdoc />
    public partial class AddAirbnb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airbnbs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    Reviews = table.Column<int>(type: "integer", nullable: false),
                    HostName = table.Column<string>(type: "text", nullable: false),
                    HostId = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    Amenities = table.Column<string>(type: "text", nullable: false),
                    SafetyRules = table.Column<string>(type: "text", nullable: false),
                    HouseRules = table.Column<string>(type: "text", nullable: false),
                    ImgLinks = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Bathrooms = table.Column<int>(type: "integer", nullable: false),
                    Beds = table.Column<int>(type: "integer", nullable: false),
                    Guests = table.Column<int>(type: "integer", nullable: false),
                    Toilets = table.Column<int>(type: "integer", nullable: false),
                    Bedrooms = table.Column<int>(type: "integer", nullable: false),
                    Studios = table.Column<int>(type: "integer", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airbnbs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Airbnbs");
        }
    }
}
