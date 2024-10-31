using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class channelpermstoenumflags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChannelPerms_PermitsBoss",
                table: "ChannelPerms");

            migrationBuilder.DropIndex(
                name: "IX_ChannelPerms_PermitsVendor",
                table: "ChannelPerms");

            migrationBuilder.DropColumn(
                name: "PermitsAirDrop",
                table: "ChannelPerms");

            migrationBuilder.DropColumn(
                name: "PermitsBoss",
                table: "ChannelPerms");

            migrationBuilder.DropColumn(
                name: "PermitsCore",
                table: "ChannelPerms");

            migrationBuilder.DropColumn(
                name: "PermitsMisc",
                table: "ChannelPerms");

            migrationBuilder.DropColumn(
                name: "PermitsNya",
                table: "ChannelPerms");

            migrationBuilder.RenameColumn(
                name: "PermitsVendor",
                table: "ChannelPerms",
                newName: "PermissionsValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PermissionsValue",
                table: "ChannelPerms",
                newName: "PermitsVendor");

            migrationBuilder.AddColumn<bool>(
                name: "PermitsAirDrop",
                table: "ChannelPerms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermitsBoss",
                table: "ChannelPerms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermitsCore",
                table: "ChannelPerms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermitsMisc",
                table: "ChannelPerms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PermitsNya",
                table: "ChannelPerms",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

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
        }
    }
}
