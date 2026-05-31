using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCPS_API_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddForecastTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ForecastModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValidTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Temperature = table.Column<int>(type: "int", nullable: true),
                    WindSpeed = table.Column<int>(type: "int", nullable: true),
                    WindDirection = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WxPhrase = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrecipChance = table.Column<int>(type: "int", nullable: true),
                    CloudCover = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastModel", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastModel");
        }
    }
}
