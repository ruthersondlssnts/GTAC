using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class AddTimeSched : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Time",
                table: "Schedules",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Time",
                table: "Schedules");
        }
    }
}
