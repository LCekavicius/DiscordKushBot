using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class seasonparameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BossProgress = table.Column<int>(type: "INTEGER", nullable: false),
                    BlueRoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    OrangeRoleId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MutedRoleId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonParameters", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SeasonParameters",
                columns: new[] { "Id", "BlueRoleId", "BossProgress", "MutedRoleId", "OrangeRoleId" },
                values: new object[] { 1, 945785365292285963ul, 1, 513478497885356041ul, 1225482353003204719ul });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonParameters");
        }
    }
}
