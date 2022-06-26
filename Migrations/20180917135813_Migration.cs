using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KushBot.Migrations
{
    public partial class Migration : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.CreateTable(
                name: "Jews",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Balance = table.Column<int>(nullable: false),
                    LastBeg = table.Column<DateTime>(nullable: false),
                    HasEgg = table.Column<bool>(nullable: false),
                    Pets = table.Column<string>(),
                    PetLevels = table.Column<string>(),
                    PetDupes = table.Column<string>(),
                    LastDestroy = table.Column<DateTime>(nullable: false),
                    LastYoink = table.Column<DateTime>(nullable: false),
                    LastTylerRage = table.Column<DateTime>(nullable: false),
                    RageCash = table.Column<int>(nullable: false),
                    RageDuration = table.Column<int>(nullable: false),
                    QuestIndexes = table.Column<int>(nullable: false),
                    LostBapsMN = table.Column<int>(nullable: false),
                    WonBapsMN = table.Column<int>(nullable: false),
                    LostFlipsMN = table.Column<int>(nullable: false),
                    WonFlipsMN = table.Column<int>(nullable: false),
                    LostBetsMN = table.Column<int>(nullable: false),
                    WonBetsMN = table.Column<int>(nullable: false),
                    LostRisksMN = table.Column<int>(nullable: false),
                    WonRisksMN = table.Column<int>(nullable: false),
                    SuccesfulYoinks = table.Column<int>(nullable: false),
                    FailedYoinks = table.Column<int>(nullable: false),
                    WonFlipChainOverFifty = table.Column<int>(nullable: false),
                    BegsMN = table.Column<int>(nullable: false),
                    SetDigger = table.Column<DateTime>(),
                    LootedDigger = table.Column<DateTime>(),
                    DiggerState = table.Column<int>(),
                    WonDuelsMn = table.Column<int>(),
                    Pictures = table.Column<string>(),
                    SelectedPicture = table.Column<int>(),
                    Yiked = table.Column<int>(),
                    RedeemDate = table.Column<DateTime>(nullable: false),
                    YikeDate = table.Column<DateTime>(nullable: false),
                    Plots = table.Column<string>(),
                    LostBapsWeekly = table.Column<int>(nullable: false),
                    WonBapsWeekly = table.Column<int>(nullable: false),
                    LostFlipsWeekly = table.Column<int>(nullable: false),
                    WonFlipsWeekly = table.Column<int>(nullable: false),
                    LostBetsWeekly = table.Column<int>(nullable: false),
                    WonBetsWeekly = table.Column<int>(nullable: false),
                    LostRisksWeekly = table.Column<int>(nullable: false),
                    WonRisksWeekly = table.Column<int>(nullable: false),
                    BegsWeekly = table.Column<int>(nullable: false),
                    Tickets = table.Column<int>(nullable: false),
                    CompletedWeeklies = table.Column<string>(nullable: false),
                    DailyGive = table.Column<int>(nullable: false),
                    FirstItemId = table.Column<int>(),
                    SecondItemId = table.Column<int>(),
                    ThirdItemId = table.Column<int>(),
                    FourthItemId = table.Column<int>(),
                    Cheems = table.Column<int>(),
                    NyaMarryDate = table.Column<DateTime>(),
                    NyaMarry = table.Column<string>(),
                    TicketMultiplier = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jews", x => x.Id);
                });


            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnerId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    BossDmg = table.Column<int>(nullable: false),
                    AirDropFlat = table.Column<int>(),
                    AirDropPercent = table.Column<double>(),
                    QuestSlot = table.Column<int>(),
                    QuestBapsFlat = table.Column<int>(),
                    QuestBapsPercent = table.Column<double>(),
                    Rarity = table.Column<int>(),
                    Level = table.Column<int>(),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemPetBonus",
                columns: table => new
                {
                    Id = table.Column<int>(),
                    PetId = table.Column<int>(),
                    ItemId = table.Column<int>(),
                    LvlBonus = table.Column<string>(),
                },
            constraints: table =>
            {
                table.PrimaryKey("PK_ItemPetBonus", x => x.Id);
            });


            migrationBuilder.CreateTable(
                name: "RarityFollow",
                columns: table => new
                {
                    fk_UserId = table.Column<ulong>(),
                    Rarity = table.Column<string>(),
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jews");
        }
    }
}
