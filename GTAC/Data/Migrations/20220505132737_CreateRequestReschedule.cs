using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class CreateRequestReschedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestReschedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DayOne = table.Column<DateTime>(nullable: false),
                    DayTwo = table.Column<DateTime>(nullable: false),
                    DayThree = table.Column<DateTime>(nullable: false),
                    DayFour = table.Column<DateTime>(nullable: false),
                    ScheduleId = table.Column<Guid>(nullable: false),
                    ApprovedAt = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestReschedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestReschedule_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestReschedule_ScheduleId",
                table: "RequestReschedule",
                column: "ScheduleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestReschedule");
        }
    }
}
