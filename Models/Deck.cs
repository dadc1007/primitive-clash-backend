using PrimitiveClash.Backend.Exceptions;

namespace PrimitiveClashBackend.Models
{
    public class Deck
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public List<PlayerCard> PlayerCards { get; set; } = [];
        private readonly int _maxSizeDeck;

        public Deck() { }

        public Deck(int maxSizeDeck)
        {
            _maxSizeDeck = maxSizeDeck;
        }

        public void AddCard(PlayerCard card)
        {
            if (PlayerCards.Count >= _maxSizeDeck)
            {
                throw new InvalidDeckSizeException(_maxSizeDeck);
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