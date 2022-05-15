using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class CreateQuiz : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_AspNetUsers_UploaderId",
                table: "Modules");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestReschedule_Schedules_ScheduleId",
                table: "RequestReschedule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestReschedule",
                table: "RequestReschedule");

            migrationBuilder.RenameTable(
                name: "RequestReschedule",
                newName: "RequestReschedules");

            migrationBuilder.RenameIndex(
                name: "IX_RequestReschedule_ScheduleId",
                table: "RequestReschedules",
                newName: "IX_RequestReschedules_ScheduleId");

            migrationBuilder.AlterColumn<string>(
                name: "UploaderId",
                table: "Modules",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Modules",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestReschedules",
                table: "RequestReschedules",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Link = table.Column<string>(nullable: true),
                    AuthorId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quizzes_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Quiz_Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QuizId = table.Column<Guid>(nullable: false),
                    StudentId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quiz_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quiz_Students_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Quiz_Students_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_Students_QuizId",
                table: "Quiz_Students",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Quiz_Students_StudentId",
                table: "Quiz_Students",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_AuthorId",
                table: "Quizzes",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_AspNetUsers_UploaderId",
                table: "Modules",
                column: "UploaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestReschedules_Schedules_ScheduleId",
                table: "RequestReschedules",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_AspNetUsers_UploaderId",
                table: "Modules");

            migrationBuilder.DropForeignKey(
                name: "FK_RequestReschedules_Schedules_ScheduleId",
                table: "RequestReschedules");

            migrationBuilder.DropTable(
                name: "Quiz_Students");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestReschedules",
                table: "RequestReschedules");

            migrationBuilder.RenameTable(
                name: "RequestReschedules",
                newName: "RequestReschedule");

            migrationBuilder.RenameIndex(
                name: "IX_RequestReschedules_ScheduleId",
                table: "RequestReschedule",
                newName: "IX_RequestReschedule_ScheduleId");

            migrationBuilder.AlterColumn<string>(
                name: "UploaderId",
                table: "Modules",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Path",
                table: "Modules",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestReschedule",
                table: "RequestReschedule",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_AspNetUsers_UploaderId",
                table: "Modules",
                column: "UploaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RequestReschedule_Schedules_ScheduleId",
                table: "RequestReschedule",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
