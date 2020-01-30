using Microsoft.EntityFrameworkCore.Migrations;

namespace CBS.Data.Migrations
{
    public partial class MembershipLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MemberNumber",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "CHAR(10)");

            migrationBuilder.AlterColumn<string>(
                name: "MemberName",
                table: "AspNetUsers",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(40)");

            migrationBuilder.AddColumn<string>(
                name: "MembershipLevel",
                table: "AspNetUsers",
                type: "CHAR(10)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_MembershipLevel",
                table: "AspNetUsers",
                principalTable: "MembershipLevels",
                column: "MembershipLevel",
                principalColumn: "MembershipLevel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_MembershipLevel",
                table: "AspNetUsers"
                );

            migrationBuilder.DropColumn(
                name: "MembershipLevel",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "MemberNumber",
                table: "AspNetUsers",
                type: "CHAR(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MemberName",
                table: "AspNetUsers",
                type: "VARCHAR(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
