using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class bossentitiesfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BossBaseIndex",
                table: "Bosses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Bosses",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BossBaseIndex",
                table: "Bosses");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Bosses");
        }
    }
}
