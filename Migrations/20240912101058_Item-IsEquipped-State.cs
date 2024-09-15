using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class ItemIsEquippedState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEquipped",
                table: "Item",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Item_OwnerId",
                table: "Item",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Jews_OwnerId",
                table: "Item",
                column: "OwnerId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_Jews_OwnerId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_OwnerId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "IsEquipped",
                table: "Item");
        }
    }
}
