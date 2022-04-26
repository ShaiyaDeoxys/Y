using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class RemoveQuests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterQuest_Quests_QuestId",
                table: "CharacterQuest");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_CharacterQuest_QuestId",
                table: "CharacterQuest");

            migrationBuilder.AlterColumn<short>(
                name: "QuestId",
                table: "CharacterQuest",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ushort>(
                name: "QuestId",
                table: "CharacterQuest",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AttackFighter = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AttackMage = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DefenseDefender = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DefenseMage = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemCount_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemCount_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemCount_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemTypeId_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemTypeId_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemTypeId_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemType_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemType_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FarmItemType_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    GiverType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    GiverTypeId = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    Gold = table.Column<uint>(type: "int unsigned", nullable: false),
                    Grow = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    InitItemType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    InitItemTypeId = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MaxLevel = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MinLevel = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MobCount_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MobCount_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MobId_1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MobId_2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MsgComplete = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgIncomplete = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgResponse_1 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgResponse_2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgResponse_3 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgSummary = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgTake = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MsgWelcome = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PatrolRogue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PrevQuestId_1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    PrevQuestId_2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    PrevQuestId_3 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    QuestTimer = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    QuestTypeGiver = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    QuestTypeReceiver = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    QuestUnlock_1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    QuestUnlock_2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReceivedItemCount_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemCount_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemCount_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemTypeId_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemTypeId_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemTypeId_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemType_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemType_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceivedItemType_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceiverType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReceiverTypeId = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    RevardItemCount_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemCount_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemCount_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemTypeId_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemTypeId_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemTypeId_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemType_1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemType_2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    RevardItemType_3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ShooterRogue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    XP = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterQuest_QuestId",
                table: "CharacterQuest",
                column: "QuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterQuest_Quests_QuestId",
                table: "CharacterQuest",
                column: "QuestId",
                principalTable: "Quests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
