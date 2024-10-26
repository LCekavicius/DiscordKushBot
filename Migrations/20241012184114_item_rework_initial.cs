using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class item_rework_initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemPetBonus");

            migrationBuilder.DropColumn(
                name: "AirDropFlat",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "AirDropPercent",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "BossDmg",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "QuestBapsFlat",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "QuestBapsPercent",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "QuestSlot",
                table: "Item");

            migrationBuilder.CreateTable(
                name: "ItemStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    StatType = table.Column<int>(type: "INTEGER", nullable: false),
                    Bonus = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemStats_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemStats_ItemId",
                table: "ItemStats",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemStats");

            migrationBuilder.AddColumn<int>(
                name: "AirDropFlat",
                table: "Item",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AirDropPercent",
                table: "Item",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BossDmg",
                table: "Item",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuestBapsFlat",
                table: "Item",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "QuestBapsPercent",
                table: "Item",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "QuestSlot",
                table: "Item",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemPetBonus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    LvlBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    PetType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPetBonus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPetBonus_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPetBonus_ItemId",
                table: "ItemPetBonus",
                column: "ItemId");
        }
    }
}
