using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class RemoveMobs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MobItems");

            migrationBuilder.DropTable(
                name: "Mobs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MobItems",
                columns: table => new
                {
                    MobId = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ItemOrder = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DropRate = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobItems", x => new { x.MobId, x.ItemOrder });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Mobs",
                columns: table => new
                {
                    MobID = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AI = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Attack1 = table.Column<short>(type: "smallint", nullable: false),
                    Attack2 = table.Column<short>(type: "smallint", nullable: false),
                    Attack3 = table.Column<short>(type: "smallint", nullable: false),
                    AttackAttrib1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackAttrib2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackOk1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackOk2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackOk3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackPlus1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttackPlus2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttackPlus3 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttackRange1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackRange2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackRange3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackSpecial1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackSpecial2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackSpecial3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackTime1 = table.Column<int>(type: "int", nullable: false),
                    AttackTime2 = table.Column<int>(type: "int", nullable: false),
                    AttackTime3 = table.Column<int>(type: "int", nullable: false),
                    AttackType1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttackType2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttackType3 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ChaseRange = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ChaseStep = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ChaseTime = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Defense = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Dex = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Element = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Exp = table.Column<short>(type: "smallint", nullable: false),
                    AttackAttrib3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HP = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Luc = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MP = table.Column<short>(type: "smallint", nullable: false),
                    Magic = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MobName = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Money2 = table.Column<short>(type: "smallint", nullable: false),
                    Money1 = table.Column<short>(type: "smallint", nullable: false),
                    NormalStep = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NormalTime = table.Column<int>(type: "int", nullable: false),
                    QuestItemId = table.Column<int>(type: "int", nullable: false),
                    ResistSkill1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistSkill2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistSkill3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistSkill4 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistSkill5 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistSkill6 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState10 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState11 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState12 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState13 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState14 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState15 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState4 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState5 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState6 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState7 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState8 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ResistState9 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SP = table.Column<short>(type: "smallint", nullable: false),
                    Size = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Wis = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mobs", x => x.MobID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
