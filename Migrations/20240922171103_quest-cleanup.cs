using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class questcleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BegsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "BegsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "CompletedWeeklies",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "FailedYoinks",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostBapsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostBapsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostBetsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostBetsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostFlipsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostFlipsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostRisksMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "LostRisksWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "QuestIndexes",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "SuccesfulYoinks",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonBapsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonBapsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonBetsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonBetsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonDuelsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonFlipChainOverFifty",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonFlipsMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonFlipsWeekly",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonRisksMN",
                table: "Jews");

            migrationBuilder.DropColumn(
                name: "WonRisksWeekly",
                table: "Jews");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BegsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BegsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompletedWeeklies",
                table: "Jews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedYoinks",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostBapsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostBapsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostBetsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostBetsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostFlipsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostFlipsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostRisksMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LostRisksWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "QuestIndexes",
                table: "Jews",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SuccesfulYoinks",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonBapsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonBapsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonBetsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonBetsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonDuelsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonFlipChainOverFifty",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonFlipsMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonFlipsWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonRisksMN",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WonRisksWeekly",
                table: "Jews",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
