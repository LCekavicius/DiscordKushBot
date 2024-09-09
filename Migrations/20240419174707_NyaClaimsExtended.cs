using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class NyaClaimsExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "NyaClaims",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Keys",
                table: "NyaClaims",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNyaClaim",
                table: "Jews",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "NyaClaims");

            migrationBuilder.DropColumn(
                name: "Keys",
                table: "NyaClaims");

            migrationBuilder.DropColumn(
                name: "LastNyaClaim",
                table: "Jews");
        }
    }
}
