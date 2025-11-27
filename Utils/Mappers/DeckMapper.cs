using PrimitiveClash.Backend.DTOs.Deck.Responses;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Utils.Mappers
{
    public static class DeckMapper
    {
        public static DeckResponse ToDeckResponse(this Deck deck)
        {
            if (deck == null)
            {
                throw new InvalidOperationException("Deck is null");
            }

            return new DeckResponse(
                deck.Id,
                deck.Size(),
                deck.AverageElixirCost(),
                [.. deck.PlayerCards.Select(pc => pc.ToCardInDeckResponse())]
            );
        }

        public static CardInDeckResponse ToCardInDeckResponse(this PlayerCard playerCard)
        {
            if (playerCard == null)
            {
                throw new InvalidOperationException("PlayerCard or its Card property is null");
            }

            return new CardInDeckResponse(
                playerCard.Id,
                playerCard.Card.Id,
                playerCard.Card.Name,
                playerCard.Card.Rarity,
                playerCard.Card.ElixirCost,
                playerCard.Level,
                playerCard.Card.ImageUrl
            );
        }
    }
}