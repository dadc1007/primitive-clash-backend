using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimitiveClash.Backend.Migrations
{
    /// <inheritdoc />
    public partial class GameEntitiesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MovementType",
                table: "Cards",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisionRange",
                table: "Cards",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovementType",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "VisionRange",
                table: "Cards");
        }
    }
}
