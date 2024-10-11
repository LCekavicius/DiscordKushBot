using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class channelperms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelPerms",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PermitsAirDrop = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermitsCore = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermitsMisc = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermitsNya = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermitsBoss = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermitsVendor = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPerms", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelPerms");
        }
    }
}
