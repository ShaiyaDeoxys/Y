using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Imgeneus.Database.Migrations
{
    public partial class DeleteForeinKeyBankItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankItems_Items_Type_TypeId",
                table: "BankItems");

            migrationBuilder.DropIndex(
                name: "IX_BankItems_Type_TypeId",
                table: "BankItems");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BankItems_Type_TypeId",
                table: "BankItems",
                columns: new[] { "Type", "TypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BankItems_Items_Type_TypeId",
                table: "BankItems",
                columns: new[] { "Type", "TypeId" },
                principalTable: "Items",
                principalColumns: new[] { "Type", "TypeID" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
