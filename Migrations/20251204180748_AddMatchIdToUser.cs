using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrimitiveClash.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MatchId",
                table: "Users",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "Users");
        }
    }
}
