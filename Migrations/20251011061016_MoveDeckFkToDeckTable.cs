using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimitiveClash.Backend.Migrations
{
    /// <inheritdoc />
    public partial class MoveDeckFkToDeckTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Decks_DeckId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DeckId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Decks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Decks_UserId",
                table: "Decks",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Users_UserId",
                table: "Decks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Users_UserId",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_Decks_UserId",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Decks");

            migrationBuilder.AddColumn<Guid>(
                name: "DeckId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeckId",
                table: "Users",
                column: "DeckId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Decks_DeckId",
                table: "Users",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
