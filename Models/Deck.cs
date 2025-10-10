using PrimitiveClash.Backend.Exceptions;

namespace PrimitiveClashBackend.Models
{
    public class Deck
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<PlayerCard> PlayerCards { get; set; } = [];

        public void AddCard(PlayerCard card)
        {
            if (PlayerCards.Count >= 8)
            {
                throw new LimitCardsInDeckException();
            }
            if (PlayerCards.Contains(card))
            {
                throw new CardAlreadyInDeckException();
            }
            PlayerCards.Add(card);
        }

        public void RemoveCard(PlayerCard card)
        {
            if (!PlayerCards.Remove(card))
            {
                throw new CardNotInDeckException();
            }
        }

        public double AverageElixirCost()
        {
            if (PlayerCards.Count == 0) return 0;
            return PlayerCards.Average(c => c.Card.ElixirCost);
        }
    }
}