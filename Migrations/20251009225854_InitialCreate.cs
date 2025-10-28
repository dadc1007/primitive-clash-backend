using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimitiveClash.Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ElixirCost = table.Column<int>(type: "integer", nullable: false),
                    Rarity = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Damage = table.Column<int>(type: "integer", nullable: false),
                    Targets = table.Column<string[]>(type: "text[]", nullable: false),
                    CardType = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Duration = table.Column<float>(type: "real", nullable: true),
                    Hp = table.Column<int>(type: "integer", nullable: true),
                    Range = table.Column<int>(type: "integer", nullable: true),
                    DamageArea = table.Column<int>(type: "integer", nullable: true),
                    HitSpeed = table.Column<float>(type: "real", nullable: true),
                    SpellCard_Duration = table.Column<float>(type: "real", nullable: true),
                    Radius = table.Column<int>(type: "integer", nullable: true),
                    MovementSpeed = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name",
                table: "Cards",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_CardId",
                table: "PlayerCards",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerCards");

            migrationBuilder.DropTable(
                name: "Cards");
        }
    }
}
