using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class RemoveItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TypeID = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackTime = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Attackfighter = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Attackmage = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Buy = table.Column<int>(type: "int", nullable: false),
                    ConstDex = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstHP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstInt = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstLuc = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstMP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstRec = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstSP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstStr = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ConstWis = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Count = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Country = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Effect3 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Defensefighter = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Defensemage = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Drop = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Duration = table.Column<uint>(type: "int unsigned", nullable: false),
                    Attrib = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Exp = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Grade = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Grow = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Effect1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Patrolrogue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Effect2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Quality = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Range = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReqDex = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReqIg = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReqInt = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReqOg = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReqRec = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReqStr = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReqVg = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReqWis = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Reqlevel = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Reqluc = table.Column<short>(type: "smallint", nullable: false),
                    Effect4 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Sell = table.Column<int>(type: "int", nullable: false),
                    Server = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Shootrogue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Slot = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Special = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Speed = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => new { x.Type, x.TypeID });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
