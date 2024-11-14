using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class bossentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bosses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Health = table.Column<int>(type: "INTEGER", nullable: false),
                    ParticipantSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    AbilitiesValue = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bosses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BossParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    BossId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BossParticipants_Bosses_BossId",
                        column: x => x.BossId,
                        principalTable: "Bosses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BossParticipants_BossId",
                table: "BossParticipants",
                column: "BossId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BossParticipants");

            migrationBuilder.DropTable(
                name: "Bosses");
        }
    }
}
