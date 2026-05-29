using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCPS_API_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "jsonString",
                table: "WeatherModel",
                newName: "WxPhrase");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "WeatherModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkyCondition",
                table: "WeatherModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "WeatherModel",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "WindDirection",
                table: "WeatherModel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "WindSpeed",
                table: "WeatherModel",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "WeatherModel");

            migrationBuilder.DropColumn(
                name: "SkyCondition",
                table: "WeatherModel");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "WeatherModel");

            migrationBuilder.DropColumn(
                name: "WindDirection",
                table: "WeatherModel");

            migrationBuilder.DropColumn(
                name: "WindSpeed",
                table: "WeatherModel");

            migrationBuilder.RenameColumn(
                name: "WxPhrase",
                table: "WeatherModel",
                newName: "jsonString");
        }
    }
}
