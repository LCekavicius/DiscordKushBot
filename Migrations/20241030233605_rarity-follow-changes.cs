using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class rarityfollowchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fk_UserId",
                table: "RarityFollow",
                newName: "UserId");

            migrationBuilder.AlterColumn<int>(
                name: "Rarity",
                table: "RarityFollow",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RarityFollow",
                newName: "fk_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Rarity",
                table: "RarityFollow",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
