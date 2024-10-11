using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class usereventchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "UserEvents",
                newName: "BapsInput");

            migrationBuilder.AddColumn<int>(
                name: "BapsChange",
                table: "UserEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BapsChange",
                table: "UserEvents");

            migrationBuilder.RenameColumn(
                name: "BapsInput",
                table: "UserEvents",
                newName: "Amount");
        }
    }
}
