using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KushBot.Migrations
{
    /// <inheritdoc />
    public partial class plottypediscrimination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Plots_UserId",
                table: "Plots",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plots_Users_UserId",
                table: "Plots",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plots_Users_UserId",
                table: "Plots");

            migrationBuilder.DropIndex(
                name: "IX_Plots_UserId",
                table: "Plots");
        }
    }
}
