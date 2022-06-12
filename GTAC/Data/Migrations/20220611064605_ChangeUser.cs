using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class ChangeUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentChangeRequests_Students_StudentId",
                table: "StudentChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_StudentChangeRequests_StudentId",
                table: "StudentChangeRequests");

            migrationBuilder.DropColumn(
                name: "isProfileApproved",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "StudentChangeRequests");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StudentChangeRequests",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileApproved",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentChangeRequests_UserId",
                table: "StudentChangeRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentChangeRequests_AspNetUsers_UserId",
                table: "StudentChangeRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentChangeRequests_AspNetUsers_UserId",
                table: "StudentChangeRequests");

            migrationBuilder.DropIndex(
                name: "IX_StudentChangeRequests_UserId",
                table: "StudentChangeRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StudentChangeRequests");

            migrationBuilder.DropColumn(
                name: "IsProfileApproved",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "isProfileApproved",
                table: "Students",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "StudentChangeRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StudentChangeRequests_StudentId",
                table: "StudentChangeRequests",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentChangeRequests_Students_StudentId",
                table: "StudentChangeRequests",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
