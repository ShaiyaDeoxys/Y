using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class AddMarketItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "MarketItemId",
                table: "Market",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "MarketItems",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MarketId = table.Column<uint>(type: "int unsigned", nullable: false),
                    Type = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    TypeId = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Count = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Quality = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    GemTypeId1 = table.Column<int>(type: "int", nullable: false),
                    GemTypeId2 = table.Column<int>(type: "int", nullable: false),
                    GemTypeId3 = table.Column<int>(type: "int", nullable: false),
                    GemTypeId4 = table.Column<int>(type: "int", nullable: false),
                    GemTypeId5 = table.Column<int>(type: "int", nullable: false),
                    GemTypeId6 = table.Column<int>(type: "int", nullable: false),
                    Craftname = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasDyeColor = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DyeColorAlpha = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DyeColorSaturation = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DyeColorR = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DyeColorG = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    DyeColorB = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketItems_Market_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Market",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MarketItems_MarketId",
                table: "MarketItems",
                column: "MarketId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketItems");

            migrationBuilder.DropColumn(
                name: "MarketItemId",
                table: "Market");
        }
    }
}
