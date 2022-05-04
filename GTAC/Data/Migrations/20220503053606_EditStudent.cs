using Microsoft.EntityFrameworkCore.Migrations;

namespace GTAC.Data.Migrations
{
    public partial class EditStudent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_InstructorId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "InstructorId",
                table: "Students",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_InstructorId",
                table: "Students",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_InstructorId",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "InstructorId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_InstructorId",
                table: "Students",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
