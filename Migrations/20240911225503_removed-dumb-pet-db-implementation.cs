using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class removeddumbpetdbimplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PetDupes",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "PetLevels",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "Pets",
                table: "Jews");

            migrationBuilder.RenameColumn(
                name: "PetId",
                table: "ItemPetBonus",
                newName: "PetType");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_UserId",
                table: "UserPets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NyaClaims_OwnerId",
                table: "NyaClaims",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_NyaClaims_Jews_OwnerId",
                table: "NyaClaims",
                column: "OwnerId",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NyaClaims_Jews_OwnerId",
                table: "NyaClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPets_Jews_UserId",
                table: "UserPets");

            migrationBuilder.DropIndex(
                name: "IX_UserPets_UserId",
                table: "UserPets");

            migrationBuilder.DropIndex(
                name: "IX_NyaClaims_OwnerId",
                table: "NyaClaims");

            migrationBuilder.RenameColumn(
                name: "PetType",
                table: "ItemPetBonus",
                newName: "PetId");

            migrationBuilder.AddColumn<string>(
                name: "PetDupes",
                table: "Jews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PetLevels",
                table: "Jews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pets",
                table: "Jews",
                type: "TEXT",
                nullable: true);
        }
    }
}
