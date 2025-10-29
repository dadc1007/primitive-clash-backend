using PrimitiveClash.Backend.Exceptions;

namespace PrimitiveClash.Backend.Models
{
    public class PlayerState(Guid id, List<PlayerCard> cards)
    {
        private readonly object _lock = new();
        
        public Guid Id { get; set; } = id;
        public bool IsConnected { get; set; } = true;
        public string? ConnectionId { get; set; }
        public List<PlayerCard> Cards { get; set; } = cards;
        public decimal CurrentElixir { get; set; } = 5.0m;

        public void PlayCard(Guid cardId)
        {
            List<PlayerCard> hand = GetHand();
            PlayerCard cardPlayed =
                hand.Find(c => c.Id == cardId) ?? throw new CardNotInHandException();
            Cards.Remove(cardPlayed);
            Cards.Add(cardPlayed);
        }

        public List<PlayerCard> GetHand()
        {
            return Cards.GetRange(0, 4);
        }

        public PlayerCard GetNextCard()
        {
            return Cards[4];
        }
        
        public object GetLock() => _lock;
    }
}
