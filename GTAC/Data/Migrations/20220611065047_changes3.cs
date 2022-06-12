using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class changes3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "StudentChangeRequests",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StudentChangeRequests",
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
                onDelete: ReferentialAction.Cascade);
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
                name: "Firstname",
                table: "StudentChangeRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StudentChangeRequests");
        }
    }
}
