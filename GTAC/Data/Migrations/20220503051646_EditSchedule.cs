using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class EditSchedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Schedules_StudentId",
                table: "Schedules",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Students_StudentId",
                table: "Schedules",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Students_StudentId",
                table: "Schedules");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_StudentId",
                table: "Schedules");
        }
    }
}
