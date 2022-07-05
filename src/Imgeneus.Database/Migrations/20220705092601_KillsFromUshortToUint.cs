using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class KillsFromUshortToUint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "Victories",
                table: "Characters",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<uint>(
                name: "Kills",
                table: "Characters",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<uint>(
                name: "Defeats",
                table: "Characters",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<uint>(
                name: "Deaths",
                table: "Characters",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ushort>(
                name: "Victories",
                table: "Characters",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<ushort>(
                name: "Kills",
                table: "Characters",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<ushort>(
                name: "Defeats",
                table: "Characters",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<ushort>(
                name: "Deaths",
                table: "Characters",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");
        }
    }
}
