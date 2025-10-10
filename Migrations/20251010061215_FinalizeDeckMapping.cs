using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimitiveClash.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDeckMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckContent_PlayerCards_CardsId",
                table: "DeckContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeckContent",
                table: "DeckContent");

            migrationBuilder.DropIndex(
                name: "IX_DeckContent_DeckId",
                table: "DeckContent");

            migrationBuilder.RenameColumn(
                name: "CardsId",
                table: "DeckContent",
                newName: "PlayerCardsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeckContent",
                table: "DeckContent",
                columns: new[] { "DeckId", "PlayerCardsId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeckContent_PlayerCardsId",
                table: "DeckContent",
                column: "PlayerCardsId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckContent_PlayerCards_PlayerCardsId",
                table: "DeckContent",
                column: "PlayerCardsId",
                principalTable: "PlayerCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckContent_PlayerCards_PlayerCardsId",
                table: "DeckContent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeckContent",
                table: "DeckContent");

            migrationBuilder.DropIndex(
                name: "IX_DeckContent_PlayerCardsId",
                table: "DeckContent");

            migrationBuilder.RenameColumn(
                name: "PlayerCardsId",
                table: "DeckContent",
                newName: "CardsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeckContent",
                table: "DeckContent",
                columns: new[] { "CardsId", "DeckId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeckContent_DeckId",
                table: "DeckContent",
                column: "DeckId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckContent_PlayerCards_CardsId",
                table: "DeckContent",
                column: "CardsId",
                principalTable: "PlayerCards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
