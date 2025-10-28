using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Decks.Responses
{
    public record CardInDeckResponse(
        Guid PlayerCardId,
        string CardName,
        CardRarity Rarity,
        int ElixirCost,
        int Level
    );
}