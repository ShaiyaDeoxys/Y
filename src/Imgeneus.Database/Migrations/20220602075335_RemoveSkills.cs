using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class RemoveSkills : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CharacterActiveBuff_Skills_SkillId",
                table: "CharacterActiveBuff");

            migrationBuilder.DropForeignKey(
                name: "FK_CharacterSkill_Skills_SkillId",
                table: "CharacterSkill");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_CharacterSkill_SkillId",
                table: "CharacterSkill");

            migrationBuilder.DropIndex(
                name: "IX_CharacterActiveBuff_SkillId",
                table: "CharacterActiveBuff");

            migrationBuilder.AlterColumn<ushort>(
                name: "SkillId",
                table: "CharacterSkill",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte>(
                name: "SkillLevel",
                table: "CharacterSkill",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<ushort>(
                name: "SkillId",
                table: "CharacterActiveBuff",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<byte>(
                name: "SkillLevel",
                table: "CharacterActiveBuff",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "CharacterSkill");

            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "CharacterActiveBuff");

            migrationBuilder.AlterColumn<int>(
                name: "SkillId",
                table: "CharacterSkill",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "SkillId",
                table: "CharacterActiveBuff",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AbilityType1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType10 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType4 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType5 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType6 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType7 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType8 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityType9 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    AbilityValue1 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue10 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue2 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue3 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue4 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue5 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue6 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue7 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue8 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AbilityValue9 = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AddDamageHP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AddDamageMP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AddDamageSP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ApplyRange = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Arrow = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttackRange = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Bag = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ChangeLevel = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ChangeType = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DamageHP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DamageMP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DamageSP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    DamageType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DefenceType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DefenceValue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DisabledSkill = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    AttrType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    FixRange = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Grow = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    HealHP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    HealMP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    HealSP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    KeepTime = table.Column<int>(type: "int", nullable: false),
                    LimitHP = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    MP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    MultiAttack = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedShield = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon10 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon11 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon12 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon13 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon14 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon15 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon3 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon4 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon5 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon6 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon7 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon8 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    NeedWeapon9 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    PreviousSkillId = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ReadyTime = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReqLevel = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    ResetTime = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    SP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    SkillId = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    SkillLevel = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SkillPoint = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SkillUtilizer = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    StateType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SuccessType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    SuccessValue = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TargetType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TimeDamageHP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TimeDamageMP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TimeDamageSP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TimeDamageType = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TimeHealHP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TimeHealMP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TimeHealSP = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TypeAttack = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TypeDetail = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    TypeEffect = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TypeShow = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    UsedByArcher = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UsedByDefender = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UsedByFighter = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UsedByMage = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UsedByPriest = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    UsedByRanger = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Weapon1 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Weapon2 = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Weaponvalue = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSkill_SkillId",
                table: "CharacterSkill",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterActiveBuff_SkillId",
                table: "CharacterActiveBuff",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_SkillId_SkillLevel",
                table: "Skills",
                columns: new[] { "SkillId", "SkillLevel" });

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterActiveBuff_Skills_SkillId",
                table: "CharacterActiveBuff",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CharacterSkill_Skills_SkillId",
                table: "CharacterSkill",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
