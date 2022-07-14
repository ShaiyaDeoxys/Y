using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class AddKillLevelAndDeathLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "DeathLevel",
                table: "Characters",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<byte>(
                name: "KillLevel",
                table: "Characters",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeathLevel",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "KillLevel",
                table: "Characters");
        }
    }
}
