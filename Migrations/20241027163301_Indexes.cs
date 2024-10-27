using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class Indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserInfections_OwnerId",
                table: "UserInfections",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPerms_PermitsBoss",
                table: "ChannelPerms",
                column: "PermitsBoss",
                unique: true,
                filter: "[PermitsBoss] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPerms_PermitsVendor",
                table: "ChannelPerms",
                column: "PermitsVendor",
                unique: true,
                filter: "[PermitsVendor] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInfections_Users_OwnerId",
                table: "UserInfections",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserInfections_Users_OwnerId",
                table: "UserInfections");

            migrationBuilder.DropIndex(
                name: "IX_UserInfections_OwnerId",
                table: "UserInfections");

            migrationBuilder.DropIndex(
                name: "IX_ChannelPerms_PermitsBoss",
                table: "ChannelPerms");

            migrationBuilder.DropIndex(
                name: "IX_ChannelPerms_PermitsVendor",
                table: "ChannelPerms");
        }
    }
}
