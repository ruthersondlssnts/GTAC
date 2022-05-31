using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class Assessment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDayFourPassed",
                table: "Schedules",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDayOnePassed",
                table: "Schedules",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDayThreePassed",
                table: "Schedules",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDayTwoPassed",
                table: "Schedules",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDayFourPassed",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "isDayOnePassed",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "isDayThreePassed",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "isDayTwoPassed",
                table: "Schedules");
        }
    }
}
