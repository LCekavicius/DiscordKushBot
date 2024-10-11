using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class usertablerename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumableBuffs_Jews_OwnerId",
                table: "ConsumableBuffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_Jews_OwnerId",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_NyaClaims_Jews_OwnerId",
                table: "NyaClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_Quests_Jews_UserId",
                table: "Quests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEvents_Jews_UserId",
                table: "UserEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPets_Jews_UserId",
                table: "UserPets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jews",
                table: "Jews");

            migrationBuilder.RenameTable(
                name: "Jews",
                newName: "Users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumableBuffs_Users_OwnerId",
                table: "ConsumableBuffs",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Users_OwnerId",
                table: "Item",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NyaClaims_Users_OwnerId",
                table: "NyaClaims",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_Users_UserId",
                table: "Quests",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEvents_Users_UserId",
                table: "UserEvents",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPets_Users_UserId",
                table: "UserPets",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsumableBuffs_Users_OwnerId",
                table: "ConsumableBuffs");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_Users_OwnerId",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_NyaClaims_Users_OwnerId",
                table: "NyaClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_Quests_Users_UserId",
                table: "Quests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEvents_Users_UserId",
                table: "UserEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPets_Users_UserId",
                table: "UserPets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Jews");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jews",
                table: "Jews",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsumableBuffs_Jews_OwnerId",
                table: "ConsumableBuffs",
                column: "OwnerId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Item_Jews_OwnerId",
                table: "Item",
                column: "OwnerId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NyaClaims_Jews_OwnerId",
                table: "NyaClaims",
                column: "OwnerId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_Jews_UserId",
                table: "Quests",
                column: "UserId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEvents_Jews_UserId",
                table: "UserEvents",
                column: "UserId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPets_Jews_UserId",
                table: "UserPets",
                column: "UserId",
                principalTable: "Jews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
