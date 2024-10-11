using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class addquestflags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pos",
                table: "Quests",
                newName: "IsDaily");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Quests",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Quests");

            migrationBuilder.RenameColumn(
                name: "IsDaily",
                table: "Quests",
                newName: "Pos");
        }
    }
}
