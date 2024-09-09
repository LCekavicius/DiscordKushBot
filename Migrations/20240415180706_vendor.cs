using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class vendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plots",
                table: "Jews");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastVendorPurchase",
                table: "Jews",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastVendorPurchase",
                table: "Jews");

            migrationBuilder.AddColumn<string>(
                name: "Plots",
                table: "Jews",
                type: "TEXT",
                nullable: true);
        }
    }
}
