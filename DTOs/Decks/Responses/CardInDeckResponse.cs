namespace PrimitiveClash.Backend.DTOs.Decks.Responses
{
    public record CardInDeckResponse(
        Guid PlayerCardId,
        string CardName,
        int ElixirCost,
        int Level
    );
}