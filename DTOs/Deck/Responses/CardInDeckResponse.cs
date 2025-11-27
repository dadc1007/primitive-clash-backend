using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.DTOs.Deck.Responses
{
    public record CardInDeckResponse(
        Guid PlayerCardId,
        Guid CardId,
        string CardName,
        CardRarity Rarity,
        int ElixirCost,
        int Level,
        string ImageUrl
    );
}