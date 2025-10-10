using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimitiveClash.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDeckToSinglePlayerDeck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeckContent");

            migrationBuilder.AddColumn<Guid>(
                name: "DeckId",
                table: "PlayerCards",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_DeckId",
                table: "PlayerCards",
                column: "DeckId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCards_Decks_DeckId",
                table: "PlayerCards",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCards_Decks_DeckId",
                table: "PlayerCards");

            migrationBuilder.DropIndex(
                name: "IX_PlayerCards_DeckId",
                table: "PlayerCards");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "PlayerCards");

            migrationBuilder.CreateTable(
                name: "DeckContent",
                columns: table => new
                {
                    DeckId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerCardsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckContent", x => new { x.DeckId, x.PlayerCardsId });
                    table.ForeignKey(
                        name: "FK_DeckContent_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckContent_PlayerCards_PlayerCardsId",
                        column: x => x.PlayerCardsId,
                        principalTable: "PlayerCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeckContent_PlayerCardsId",
                table: "DeckContent",
                column: "PlayerCardsId");
        }
    }
}
