namespace PrimitiveClash.Backend.Models
{
    public class PlayerState(Guid id, List<PlayerCard> cards)
    {
        public Guid Id { get; set; } = id;
        public bool IsConnected { get; set; } = true;
        public string? ConnectionId { get; set; }
        public List<PlayerCard> Cards { get; set; } = cards;
        public decimal CurrentElixir { get; set; } = 5.0m;

        public List<PlayerCard> GetHand()
        {
            return Cards.GetRange(0, 4);
        }

        public PlayerCard GetNextCard()
        {
            return Cards[4];
        }
    }
}