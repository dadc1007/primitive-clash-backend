using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.DTOs.Deck.Responses
{
    public record DeckResponse(
        Guid DeckId,
        int Size,
        double AverageElixirCost,
        [property: JsonPropertyName("cards")]
        List<CardInDeckResponse> Cards
    );
}