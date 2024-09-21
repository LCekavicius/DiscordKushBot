using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class UserBuffsIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ConsumableBuffs_OwnerId",
                table: "ConsumableBuffs",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumableBuffs_Jews_OwnerId",
                table: "ConsumableBuffs",
                column: "OwnerId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumableBuffs_Jews_OwnerId",
                table: "ConsumableBuffs");

            migrationBuilder.DropIndex(
                name: "IX_ConsumableBuffs_OwnerId",
                table: "ConsumableBuffs");
        }
    }
}
